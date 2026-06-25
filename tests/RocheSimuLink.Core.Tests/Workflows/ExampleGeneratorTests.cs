using RocheSimuLink.Models;
using RocheSimuLink.Models.Workflows;
using RocheSimuLink.Services;
using Xunit;

namespace RocheSimuLink.Core.Tests.Workflows;

/// <summary>
/// Verifies the Example Generator turns the main-UI inputs into the seven
/// LAB-27/28/29 example messages, with the right types, directions, and the
/// user's data embedded — all without a LIS connection.
/// </summary>
public class ExampleGeneratorTests
{
    private static ConnectionSettings Settings() => new()
    {
        SendingApplication = "X800DM",
        ReceivingApplication = "Host",
    };

    private static ExampleGeneratorInput Input() => new()
    {
        SampleId = "$0E0EYXDR",
        Test = new TestType
        {
            Name = "HIV",
            UniversalServiceIdentifier = "70241-5^HIV^LN",
            Targets =
            {
                new Target
                {
                    Name = "HIV",
                    ObservationIdentifier = "HIV^HIV^99ROC",
                    ObservationValues = { "130" },
                },
            },
        },
        SampleType = new SampleType
        {
            DisplayName = "Plasma",
            Hl7Code = "PLAS",
            SpecimenCode = "PLAS^plasma^HL70487",
        },
        SampleVolume = "500 uL",
        ResultValue = "130",
        ResultStatus = ResultStatus.Final,
        ResultFlag = ResultFlag.Normal,
    };

    private static readonly DateTimeOffset When =
        new(2024, 10, 30, 11, 0, 39, TimeSpan.FromHours(1));

    private static IReadOnlyList<GeneratedMessage> GenerateAll()
    {
        var gen = new ExampleGenerator(Settings());
        return gen.Generate(Input(), Enum.GetValues<ExampleWorkflow>(), When);
    }

    [Fact]
    public void Generate_AllSelected_ProducesSevenInCanonicalOrder()
    {
        var msgs = GenerateAll();

        Assert.Equal(7, msgs.Count);
        Assert.Collection(msgs,
            m => Assert.Equal(ExampleWorkflow.Lab27WorkOrderRequest, m.Workflow),
            m => Assert.Equal(ExampleWorkflow.Lab27RequestAcknowledge, m.Workflow),
            m => Assert.Equal(ExampleWorkflow.Lab28TestOrderSubmission, m.Workflow),
            m => Assert.Equal(ExampleWorkflow.Lab28TestOrderResponse, m.Workflow),
            m => Assert.Equal(ExampleWorkflow.Lab29TestResult, m.Workflow),
            m => Assert.Equal(ExampleWorkflow.Lab29ResultAccepted, m.Workflow),
            m => Assert.Equal(ExampleWorkflow.ControlTestResult, m.Workflow));
    }

    [Fact]
    public void Generate_RespectsSelectionAndOrdersCanonically()
    {
        var gen = new ExampleGenerator(Settings());

        // Request out of order; expect canonical order and only the two asked for.
        var msgs = gen.Generate(Input(), new[]
        {
            ExampleWorkflow.Lab29TestResult,
            ExampleWorkflow.Lab27WorkOrderRequest,
        }, When);

        Assert.Equal(2, msgs.Count);
        Assert.Equal(ExampleWorkflow.Lab27WorkOrderRequest, msgs[0].Workflow);
        Assert.Equal(ExampleWorkflow.Lab29TestResult, msgs[1].Workflow);
    }

    [Fact]
    public void Generate_EmptySelection_ProducesNothing()
    {
        var gen = new ExampleGenerator(Settings());
        var msgs = gen.Generate(Input(), Array.Empty<ExampleWorkflow>(), When);
        Assert.Empty(msgs);
    }

    [Theory]
    [InlineData(ExampleWorkflow.Lab27WorkOrderRequest, "QBP^Q11", "Instrument to LIS")]
    [InlineData(ExampleWorkflow.Lab27RequestAcknowledge, "RSP^K11", "LIS to Instrument")]
    [InlineData(ExampleWorkflow.Lab28TestOrderSubmission, "OML^O33", "LIS to Instrument")]
    [InlineData(ExampleWorkflow.Lab28TestOrderResponse, "ORL^O34", "Instrument to LIS")]
    [InlineData(ExampleWorkflow.Lab29TestResult, "OUL^R22", "Instrument to LIS")]
    [InlineData(ExampleWorkflow.Lab29ResultAccepted, "ACK^R22", "LIS to Instrument")]
    [InlineData(ExampleWorkflow.ControlTestResult, "OUL^R22", "Instrument to LIS")]
    public void Generate_EachWorkflow_HasExpectedTypeAndDirection(
        ExampleWorkflow wf, string type, string direction)
    {
        var gen = new ExampleGenerator(Settings());
        var msg = Assert.Single(gen.Generate(Input(), new[] { wf }, When));

        Assert.Equal(type, msg.MessageType);
        Assert.Equal(direction, msg.Direction);
        Assert.StartsWith("MSH|", msg.RawMessage);
    }

    [Fact]
    public void WorkOrderRequest_EmbedsSampleId()
    {
        var gen = new ExampleGenerator(Settings());
        var msg = Assert.Single(
            gen.Generate(Input(), new[] { ExampleWorkflow.Lab27WorkOrderRequest }, When));

        Assert.Contains("QBP^Q11^QBP_Q11", msg.RawMessage);
        Assert.Contains("$0E0EYXDR", msg.RawMessage);
    }

    [Fact]
    public void TestResult_EmbedsResultValueSampleTypeAndVolume()
    {
        var gen = new ExampleGenerator(Settings());
        var msg = Assert.Single(
            gen.Generate(Input(), new[] { ExampleWorkflow.Lab29TestResult }, When));
        var segs = msg.RawMessage.Split('\r');

        Assert.Contains("OUL^R22^OUL_R22", msg.RawMessage);
        // SPM-4 carries the full coded element, not just the identifier.
        Assert.Contains(segs, s => s.StartsWith("SPM") && s.Contains("PLAS^plasma^HL70487"));
        Assert.Contains(segs, s => s.StartsWith("OBX") && s.Contains("130"));
        Assert.Contains(segs, s => s.StartsWith("TCD") && s.Contains("500^uL&&UCUM"));
        // Patient role on the standard result.
        Assert.Contains(segs, s => s.StartsWith("SPM") && s.Contains("P^^HL70369"));
    }

    [Fact]
    public void ControlResult_UsesQcSpecimenRole()
    {
        var gen = new ExampleGenerator(Settings());
        var msg = Assert.Single(
            gen.Generate(Input(), new[] { ExampleWorkflow.ControlTestResult }, When));
        var spm = msg.RawMessage.Split('\r').First(s => s.StartsWith("SPM"));

        Assert.Contains("Q^^HL70369", spm);
    }

    [Fact]
    public void ResponsesAcknowledgeTheirRequests()
    {
        var msgs = GenerateAll();

        string ControlId(string raw) => raw.Split('\r')[0].Split('|')[9];
        string MsaRef(GeneratedMessage m) =>
            m.RawMessage.Split('\r').First(s => s.StartsWith("MSA")).Split('|')[2];

        var oml = msgs.First(m => m.Workflow == ExampleWorkflow.Lab28TestOrderSubmission);
        var orl = msgs.First(m => m.Workflow == ExampleWorkflow.Lab28TestOrderResponse);
        var oul = msgs.First(m => m.Workflow == ExampleWorkflow.Lab29TestResult);
        var ack = msgs.First(m => m.Workflow == ExampleWorkflow.Lab29ResultAccepted);

        // ORL MSA-2 references the OML control id; ACK MSA-2 references the OUL.
        Assert.Equal(ControlId(oml.RawMessage), MsaRef(orl));
        Assert.Equal(ControlId(oul.RawMessage), MsaRef(ack));
    }

    [Fact]
    public void Render_LabelsEachMessageAndSplitsSegments()
    {
        var msgs = GenerateAll();
        var text = ExampleGenerator.Render(msgs);

        Assert.Contains("LAB-27 - Work order request", text);
        Assert.Contains("[QBP^Q11]", text);
        Assert.Contains("(Instrument to LIS)", text);
        // Segments are on their own lines (no raw carriage-return joins remain).
        Assert.DoesNotContain('\r', text);
        Assert.Contains("\nMSH|", "\n" + text);
    }
}
