using RocheSimuLink.HL7.Law;
using RocheSimuLink.HL7.Parsers;
using RocheSimuLink.Models.Law;
using Xunit;

namespace RocheSimuLink.Core.Tests.Law;

/// <summary>
/// Verifies the order flow (OML^O33 / ORL^O34) against the example messages in
/// the x800 Data Manager Host Interface Manual.
/// </summary>
public class LawOrderFlowTests
{
    private static string[] Segments(string raw) => raw.Split('\r');

    private static LawMessageHeader OrderHeader() => new()
    {
        SendingApplication = "HOST",
        ReceivingApplication = "X800 DM",
        MessageDateTime = "20210915123330+0200",
        MessageControlId = "0b070615-e2ec-4463-92db-3fd53b21daa3",
    };

    private static TestOrderMessage BuildSubmission() => new()
    {
        Header = OrderHeader(),
        Specimen = new Specimen
        {
            SampleId = "sample123",
            Namespace = "ROCHE",
            SpecimenType = new CodedElement("UR", "Urine", "HL70487"),
            Role = "P",
            CarrierId = "S54",
            CarrierPosition = "14",
        },
        Orders =
        {
            new TestOrder
            {
                OrderControl = "NW",
                TransactionDateTime = "20210915123330",
                PlacerOrderNumber = "3821",
                TestCode = new CodedElement("72828-7", "CT/NG", "LN"),
                ConsumptionVolume = "850^uL&&UCUM",
            },
        },
    };

    [Fact]
    public void Oml_Submission_MatchesTrace()
    {
        var msg = LawOmlO33Builder.Build(BuildSubmission());
        var segs = Segments(msg.RawMessage);

        Assert.Equal(
            "MSH|^~\\&|HOST||X800 DM||20210915123330+0200||OML^O33^OML_O33|" +
            "0b070615-e2ec-4463-92db-3fd53b21daa3|P|2.5.1|||NE|AL||UNICODE UTF-8|||LAB-28^IHE",
            segs[0]);
        Assert.Equal("SPM|1|sample123&ROCHE||UR^Urine^HL70487|||||||P^^HL70369", segs[1]);
        Assert.Equal("SAC|||sample123|||||||S54|14", segs[2]);
        Assert.Equal("ORC|NW||||||||20210915123330", segs[3]);
        Assert.Equal("OBR||3821||72828-7^CT/NG^LN", segs[4]);
        Assert.Equal("TCD|72828-7^CT/NG^LN||||||||850^uL&&UCUM", segs[5]);
    }

    [Fact]
    public void Oml_NoOrderAvailable_MatchesTrace()
    {
        var msg = LawOmlO33Builder.Build(new TestOrderMessage
        {
            Header = OrderHeader(),
            Specimen = new Specimen
            {
                SampleId = "sample123",
                SuppressSpmIdentity = true,
                SpecimenType = new CodedElement(),
                Role = "U",
                CarrierId = "S54",
                CarrierPosition = "14",
            },
            Orders =
            {
                new TestOrder
                {
                    OrderControl = "DC",
                    TransactionDateTime = "20210915123330",
                },
            },
        });
        var segs = Segments(msg.RawMessage);

        Assert.Equal("SPM|1||||||||||U^^HL70369", segs[1]);
        Assert.Equal("SAC|||sample123|||||||S54|14", segs[2]);
        Assert.Equal("ORC|DC||||||||20210915123330", segs[3]);
        // DC carries ORC only (no OBR/TCD).
        Assert.Equal(4, segs.Length);
    }

    [Fact]
    public void Oml_Cancel_MatchesTrace()
    {
        var submission = BuildSubmission();
        submission.Orders[0].OrderControl = "CA";
        submission.Orders[0].PlacerOrderNumber = "3141";

        var msg = LawOmlO33Builder.Build(submission);
        var segs = Segments(msg.RawMessage);

        Assert.Equal("ORC|CA||||||||20210915123330", segs[3]);
        Assert.Equal("OBR||3141||72828-7^CT/NG^LN", segs[4]);
        Assert.Equal("TCD|72828-7^CT/NG^LN||||||||850^uL&&UCUM", segs[5]);
    }

    [Fact]
    public void Oml_RoundTripsThroughParser()
    {
        var msg = LawOmlO33Builder.Build(BuildSubmission());
        var parsed = LawOrderParser.ParseOrder(msg.RawMessage);

        Assert.Equal("sample123", parsed.Specimen.SampleId);
        Assert.Equal("ROCHE", parsed.Specimen.Namespace);
        Assert.Equal("UR", parsed.Specimen.SpecimenType.Identifier);
        Assert.Equal("P", parsed.Specimen.Role);
        Assert.Equal("S54", parsed.Specimen.CarrierId);
        var order = Assert.Single(parsed.Orders);
        Assert.Equal("NW", order.OrderControl);
        Assert.Equal("3821", order.PlacerOrderNumber);
        Assert.Equal("72828-7", order.TestCode.Identifier);
        Assert.Equal("850^uL&&UCUM", order.ConsumptionVolume);
    }

    private static TestOrderResponse BuildAcceptResponse() => new()
    {
        Header = new LawMessageHeader
        {
            SendingApplication = "X800 DM",
            ReceivingApplication = "HOST",
            MessageDateTime = "20210915103324+0200",
            MessageControlId = "63ed2cb1-493f-4d9d-89e9-f40dbb9bd4b3",
        },
        AcknowledgmentCode = "AA",
        AcknowledgedControlId = "0b070615-e2ec-4463-92db-3fd53b21daa3",
        Specimen = new Specimen
        {
            SampleId = "sample123",
            Namespace = "ROCHE",
            SpecimenType = new CodedElement("UR", "Urine", "HL70487"),
            Role = "P",
            CarrierId = "S54",
            CarrierPosition = "14",
        },
        Orders =
        {
            new TestOrderResponseItem
            {
                OrderControl = "OK",
                PlacerOrderNumber = "1",
                OrderStatus = "SC",
            },
        },
    };

    [Fact]
    public void Orl_Accept_MatchesTrace()
    {
        var msg = LawOrlO34Builder.Build(BuildAcceptResponse());
        var segs = Segments(msg.RawMessage);

        Assert.Equal(
            "MSH|^~\\&|X800 DM||HOST||20210915103324+0200||ORL^O34^ORL_O42|" +
            "63ed2cb1-493f-4d9d-89e9-f40dbb9bd4b3|P|2.5.1||||||UNICODE UTF-8|||LAB-28^IHE",
            segs[0]);
        Assert.Equal("MSA|AA|0b070615-e2ec-4463-92db-3fd53b21daa3", segs[1]);
        Assert.Equal("SPM|1|sample123&ROCHE||UR^Urine^HL70487|||||||P^^HL70369", segs[2]);
        Assert.Equal("SAC|||sample123|||||||S54|14", segs[3]);
        Assert.Equal("ORC|OK|1|||SC", segs[4]);
    }

    [Fact]
    public void Orl_AlreadyExists_MatchesTrace()
    {
        var response = BuildAcceptResponse();
        response.Orders[0] = new TestOrderResponseItem
        {
            OrderControl = "UA",
            PlacerOrderNumber = "01",
            OrderStatus = "CA",
        };

        var msg = LawOrlO34Builder.Build(response);
        var orc = Segments(msg.RawMessage).First(s => s.StartsWith("ORC"));

        Assert.Equal("ORC|UA|01|||CA", orc);
    }

    [Fact]
    public void Orl_Unavailable_IsMshMsaOnly()
    {
        var msg = LawOrlO34Builder.Build(new TestOrderResponse
        {
            Header = new LawMessageHeader
            {
                SendingApplication = "X800 DM",
                ReceivingApplication = "HOST",
                MessageDateTime = "20210915103324+0200",
                MessageControlId = "63ed2cb1-493f-4d9d-89e9-f40dbb9bd4b3",
            },
            AcknowledgmentCode = "AA",
            AcknowledgedControlId = "0b070615-e2ec-4463-92db-3fd53b21daa3",
            Specimen = null,
        });
        var segs = Segments(msg.RawMessage);

        Assert.Equal(2, segs.Length);
        Assert.StartsWith("MSH|", segs[0]);
        Assert.Equal("MSA|AA|0b070615-e2ec-4463-92db-3fd53b21daa3", segs[1]);
    }

    [Fact]
    public void Orl_RoundTripsThroughParser()
    {
        var msg = LawOrlO34Builder.Build(BuildAcceptResponse());
        var parsed = LawOrderParser.ParseResponse(msg.RawMessage);

        Assert.Equal("AA", parsed.AcknowledgmentCode);
        Assert.NotNull(parsed.Specimen);
        Assert.Equal("sample123", parsed.Specimen!.SampleId);
        var order = Assert.Single(parsed.Orders);
        Assert.Equal("OK", order.OrderControl);
        Assert.Equal("1", order.PlacerOrderNumber);
        Assert.Equal("SC", order.OrderStatus);
    }

    [Fact]
    public void Orl_Unavailable_ParsesWithNullSpecimen()
    {
        var msg = LawOrlO34Builder.Build(new TestOrderResponse
        {
            AcknowledgmentCode = "AR",
            AcknowledgedControlId = "abc",
            Specimen = null,
        });
        var parsed = LawOrderParser.ParseResponse(msg.RawMessage);

        Assert.Equal("AR", parsed.AcknowledgmentCode);
        Assert.Null(parsed.Specimen);
        Assert.Empty(parsed.Orders);
    }
}
