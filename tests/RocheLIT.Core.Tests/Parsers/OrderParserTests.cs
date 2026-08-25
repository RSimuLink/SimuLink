using RocheLIT.HL7.Parsers;
using Xunit;

namespace RocheLIT.Core.Tests.Parsers;

public class OrderParserTests
{
    private const string Order =
        "MSH|^~\\&|LIS|Hospital|LIT|Roche|20260624120000||OML^O33|CTRL-7|P|2.5.1\r" +
        "PID|1||789456123^^^LIS||Johnson^Emily||19850825|F\r" +
        "ORC|NW|123987654|||||S\r" +
        "SPM|1|789456123||PLAS\r" +
        "OBR|1|789456123||HPV^HPV Typing^L|||||||||||||||||||||||R\r" +
        "TCD|HPV^HPV Typing^L||||||||500^uL&&UCUM\r" +
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
    public void Parse_MapsReceivedOrderDetails()
    {
        var order = OrderParser.Parse(Order);

        Assert.Equal("HPV Typing, Hepatitis C", order.TestType);
        Assert.Equal("PLAS", order.SampleType);
        Assert.Equal("500^uL&&UCUM", order.SampleVolume);
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

        Assert.Equal("Glucose", order.TestType);
        Assert.Equal(string.Empty, order.SampleType);
        Assert.Equal(string.Empty, order.SampleVolume);
        Assert.Single(order.Tests);
    }
}
