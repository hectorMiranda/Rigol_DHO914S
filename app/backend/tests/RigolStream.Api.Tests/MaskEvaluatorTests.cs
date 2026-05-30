using RigolStream.Api.Dsp;
using Xunit;

namespace RigolStream.Api.Tests;

public class MaskEvaluatorTests
{
    [Fact]
    public void Evaluate_CountsSamplesOutsideBand()
    {
        double[] v = { -2, -0.5, 0, 0.5, 2 };
        var (violations, total) = MaskEvaluator.Evaluate(v, -1, 1);
        Assert.Equal(5, total);
        Assert.Equal(2, violations); // -2 and 2
    }

    [Fact]
    public void Evaluate_PassesWhenAllInside()
    {
        double[] v = { -0.9, 0, 0.9 };
        var (violations, _) = MaskEvaluator.Evaluate(v, -1, 1);
        Assert.Equal(0, violations);
    }

    [Fact]
    public void Evaluate_NormalisesSwappedBounds()
    {
        double[] v = { -2, 0, 2 };
        var (violations, _) = MaskEvaluator.Evaluate(v, 1, -1); // swapped
        Assert.Equal(2, violations);
    }
}
