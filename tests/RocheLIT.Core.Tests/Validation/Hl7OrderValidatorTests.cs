using RocheLIT.HL7.Parsers;
using RocheLIT.HL7.Validation;
using Xunit;

namespace RocheLIT.Core.Tests.Validation;

public class Hl7OrderValidatorTests
{
    private const string ValidOrder =
        "MSH|^~\\&|LIS|Hospital|LIT|Roche|20260624120000||OML^O33^OML_O33|CTRL-7|P|2.5.1|||NE|AL||UNICODE UTF-8|||LAB-28^IHE\r" +
        "PID|1||789456123^^^LIS||Johnson^Emily||19850825|F\r" +
        "SPM|1|789456123&ROCHE||PLAS^plasma^HL70487|||||||P^^HL70369\r" +
        "SAC|||789456123|||||||1897|5\r" +
        "ORC|NW||||||||20260624120000\r" +
        "OBR||789456123||HPV^HPV Typing^L\r" +
        "TCD|HPV^HPV Typing^L||||||||500^uL&&UCUM";

    [Fact]
    public void Validate_ReturnsNoIssuesForCompleteInboundOrder()
    {
        var issues = Hl7OrderValidator.Validate(Hl7Parser.Parse(ValidOrder));

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_FlagsMissingPipePositions()
    {
        var invalidOrder = ValidOrder.Replace(
            "SAC|||789456123|||||||1897|5",
            "SAC|||789456123|||||1897|5",
            StringComparison.Ordinal);

        var issues = Hl7OrderValidator.Validate(Hl7Parser.Parse(invalidOrder));

        var issue = Assert.Single(issues);
        Assert.Equal("SAC", issue.SegmentName);
        Assert.Equal(1, issue.SegmentOccurrence);
        Assert.Equal(10, issue.FieldPosition);
        Assert.Contains("possible missing '|'", issue.Message);
    }

    [Fact]
    public void Validate_FlagsMissingRequiredSegment()
    {
        var invalidOrder = ValidOrder.Replace(
            "SAC|||789456123|||||||1897|5\r",
            string.Empty,
            StringComparison.Ordinal);

        var issues = Hl7OrderValidator.Validate(Hl7Parser.Parse(invalidOrder));

        var issue = Assert.Single(issues);
        Assert.Equal("SAC", issue.SegmentName);
        Assert.Equal(0, issue.FieldPosition);
        Assert.Contains("required segment is missing", issue.Message);
    }

    [Fact]
    public void Validate_FlagsRackIdOverMaximumLength()
    {
        var tooLongRackId = new string('R', 81);
        var invalidOrder = ValidOrder.Replace(
            "SAC|||789456123|||||||1897|5",
            $"SAC|||789456123|||||||{tooLongRackId}|5",
            StringComparison.Ordinal);

        var issues = Hl7OrderValidator.Validate(Hl7Parser.Parse(invalidOrder));

        var issue = Assert.Single(issues);
        Assert.Equal("SAC", issue.SegmentName);
        Assert.Equal(10, issue.FieldPosition);
        Assert.Contains("maximum allowed is 80", issue.Message);
    }

    [Fact]
    public void Validate_FlagsCarrierPositionOverMaximumLength()
    {
        var tooLongCarrierPosition = new string('9', 17);
        var invalidOrder = ValidOrder.Replace(
            "SAC|||789456123|||||||1897|5",
            $"SAC|||789456123|||||||1897|{tooLongCarrierPosition}",
            StringComparison.Ordinal);

        var issues = Hl7OrderValidator.Validate(Hl7Parser.Parse(invalidOrder));

        var issue = Assert.Single(issues);
        Assert.Equal("SAC", issue.SegmentName);
        Assert.Equal(11, issue.FieldPosition);
        Assert.Contains("maximum allowed is 16", issue.Message);
    }
}
