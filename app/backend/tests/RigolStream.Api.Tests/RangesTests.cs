using RigolStream.Api.Dsp;
using Xunit;

namespace RigolStream.Api.Tests;

public class RangesTests
{
    [Theory]
    [InlineData(0.034, 0.05)]
    [InlineData(0.05, 0.05)]
    [InlineData(0.051, 0.1)]
    [InlineData(1.5, 2.0)]
    [InlineData(7.0, 10.0)]
    [InlineData(0, 0)]
    public void SnapUp125_RoundsToStandardStep(double input, double expected)
    {
        Assert.Equal(expected, Ranges.SnapUp125(input), 9);
    }
}
