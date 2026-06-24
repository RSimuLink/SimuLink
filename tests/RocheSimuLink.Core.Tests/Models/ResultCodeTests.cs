using RocheSimuLink.Models;
using Xunit;

namespace RocheSimuLink.Core.Tests.Models;

public class ResultCodeTests
{
    [Theory]
    [InlineData(ResultStatus.Preliminary, "P")]
    [InlineData(ResultStatus.Final, "F")]
    [InlineData(ResultStatus.Corrected, "C")]
    [InlineData(ResultStatus.CannotObtain, "X")]
    public void ResultStatus_MapsToHl7Code(ResultStatus status, string expected)
    {
        Assert.Equal(expected, status.ToHl7Code());
    }

    [Theory]
    [InlineData(ResultFlag.Normal, "N")]
    [InlineData(ResultFlag.High, "H")]
    [InlineData(ResultFlag.Low, "L")]
    [InlineData(ResultFlag.Critical, "AA")]
    public void ResultFlag_MapsToHl7Code(ResultFlag flag, string expected)
    {
        Assert.Equal(expected, flag.ToHl7Code());
    }
}
