using RocheSimuLink.HL7.Parsers;
using Xunit;

namespace RocheSimuLink.Core.Tests.Parsers;

public class OrderParserTests
{
    private const string Order =
        "MSH|^~\\&|LIS|Hospital|SimuLink|Roche|20260624120000||OML^O33|CTRL-7|P|2.5.1\r" +
        "PID|1||789456123^^^LIS||Johnson^Emily||19850825|F\r" +
        "ORC|NW|123987654|||||S\r" +
        "SPM|1|789456123||PLAS\r" +
        "OBR|1|789456123||HPV^HPV Typing^L|||||||||||||||||||||||R\r" +
        "OBR|2|789456123||HCV^Hepatitis C^L|||||||||||||||||||||||S";

    [Fact]
    public void Parse_MapsOrderNumberAndSampleId()
    {
        var order = OrderParser.Parse(Order);

        Assert.Equal("123987654", order.OrderNumber);
        Assert.Equal("789456123", order.SampleId);
        Assert.Equal("OML^O33", order.MessageType);
    }

    [Fact]
    public void Parse_MapsPatientDemographics()
    {
        var order = OrderParser.Parse(Order);

        Assert.Equal("789456123", order.Patient.PatientId);
        Assert.Equal("Emily Johnson", order.Patient.FullName);
        Assert.Equal(new DateOnly(1985, 8, 25), order.Patient.DateOfBirth);
        Assert.Equal("F", order.Patient.Sex);
    }

    [Fact]
    public void Parse_MapsEachOrderedTestWithPriority()
    {
        var order = OrderParser.Parse(Order);

        Assert.Equal(2, order.Tests.Count);

        Assert.Equal("HPV", order.Tests[0].TestCode);
        Assert.Equal("HPV Typing", order.Tests[0].TestName);
        Assert.Equal("Routine", order.Tests[0].Priority);

        Assert.Equal("HCV", order.Tests[1].TestCode);
        Assert.Equal("Hepatitis C", order.Tests[1].TestName);
        Assert.Equal("STAT", order.Tests[1].Priority);
    }

    [Fact]
    public void Parse_FallsBackToOrcPriorityWhenObrLacksOne()
    {
        const string noObrPriority =
            "MSH|^~\\&|LIS|H|S|R|20260624120000||OML^O33|ID|P|2.5.1\r" +
            "ORC|NW|ORD1|||||S\r" +
            "OBR|1|SID||COVID^COVID-19^L";

        var order = OrderParser.Parse(noObrPriority);

        Assert.Equal("STAT", order.Tests[0].Priority);
    }

    [Fact]
    public void Parse_HandlesMissingPidGracefully()
    {
        const string noPid =
            "MSH|^~\\&|LIS|H|S|R|20260624120000||OML^O33|ID|P|2.5.1\r" +
            "OBR|1|SID||GLU^Glucose^L";

        var order = OrderParser.Parse(noPid);

        Assert.Equal(string.Empty, order.Patient.FullName);
        Assert.Null(order.Patient.DateOfBirth);
        Assert.Single(order.Tests);
    }

    [Fact]
    public void Parse_RetainsRawMessage()
    {
        var order = OrderParser.Parse(Order);

        Assert.Equal(Order, order.RawMessage);
    }
}
