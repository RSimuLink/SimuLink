using RocheLIT.Models;
using Xunit;

namespace RocheLIT.Core.Tests.Models;

public class ResultCodeTests
{
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
