using RocheSimuLink.HL7.Parsers;
using Xunit;

namespace RocheSimuLink.Core.Tests.Parsers;

public class Hl7ParserTests
{
    private const string SampleOrder =
        "MSH|^~\\&|LIS|Hospital|SimuLink|Roche|20260624120000||OML^O33|MSGID123|P|2.5.1\r" +
        "PID|1||SID999^^^LIS||Smith^Jane\r" +
        "SPM|1|SID999||SER\r" +
        "OBR|1|SID999||GLU^Glucose^L";

    [Fact]
    public void Parse_ReadsMessageTypeFromMsh9()
    {
        var msg = Hl7Parser.Parse(SampleOrder);

        Assert.Equal("OML^O33", msg.MessageType);
    }

    [Fact]
    public void Parse_PreservesSegmentOrder()
    {
        var msg = Hl7Parser.Parse(SampleOrder);

        Assert.Collection(msg.Segments,
            s => Assert.Equal("MSH", s.Name),
            s => Assert.Equal("PID", s.Name),
            s => Assert.Equal("SPM", s.Name),
            s => Assert.Equal("OBR", s.Name));
    }

    [Fact]
    public void Parse_MshFieldsAreOneBasedWithSeparatorAndEncoding()
    {
        var msg = Hl7Parser.Parse(SampleOrder);
        var msh = msg.Segment("MSH")!;

        Assert.Equal("|", msh.Field(1));        // MSH-1 field separator
        Assert.Equal("^~\\&", msh.Field(2));    // MSH-2 encoding characters
        Assert.Equal("LIS", msh.Field(3));      // MSH-3 sending application
        Assert.Equal("MSGID123", msh.Field(10));// MSH-10 message control id
    }

    [Fact]
    public void Parse_ReadsNonMshFields()
    {
        var msg = Hl7Parser.Parse(SampleOrder);
        var obr = msg.Segment("OBR")!;

        Assert.Equal("1", obr.Field(1));
        Assert.Equal("SID999", obr.Field(2));
        Assert.Equal("GLU^Glucose^L", obr.Field(4));
    }

    [Fact]
    public void Component_SplitsOnComponentSeparator()
    {
        var msg = Hl7Parser.Parse(SampleOrder);
        var obr = msg.Segment("OBR")!;

        Assert.Equal("GLU", obr.Component(4, 1));
        Assert.Equal("Glucose", obr.Component(4, 2));
        Assert.Equal("L", obr.Component(4, 3));
    }

    [Fact]
    public void Parse_ToleratesCrLfAndLfSeparators()
    {
        var crlf = SampleOrder.Replace("\r", "\r\n");
        var lf = SampleOrder.Replace("\r", "\n");

        Assert.Equal(4, Hl7Parser.Parse(crlf).Segments.Count);
        Assert.Equal(4, Hl7Parser.Parse(lf).Segments.Count);
    }

    [Fact]
    public void Field_ReturnsEmptyForMissingField()
    {
        var msg = Hl7Parser.Parse(SampleOrder);
        var obr = msg.Segment("OBR")!;

        Assert.Equal(string.Empty, obr.Field(99));
    }

    [Fact]
    public void Parse_ThrowsWhenNotStartingWithMsh()
    {
        Assert.Throws<FormatException>(() => Hl7Parser.Parse("PID|1||X"));
    }

    [Fact]
    public void Parse_ThrowsOnEmptyMessage()
    {
        Assert.Throws<FormatException>(() => Hl7Parser.Parse("   "));
    }

    [Fact]
    public void AllSegments_ReturnsEveryMatchingSegment()
    {
        var withTwoObx =
            "MSH|^~\\&|LIS|H|S|R|20260624120000||OUL^R22|ID|P|2.5.1\r" +
            "OBX|1|ST|A||1\r" +
            "OBX|2|ST|B||2";

        var msg = Hl7Parser.Parse(withTwoObx);

        Assert.Equal(2, msg.AllSegments("OBX").Count());
    }
}
