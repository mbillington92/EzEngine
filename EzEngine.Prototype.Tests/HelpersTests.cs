using EzEngine.ContentManagement.Mono.Interop;

namespace EzEngine.Prototype.Tests;

public class HelpersTests
{
    [Fact]
    public void PointDirection_64x0_Returns0Degrees()
    {
        var result = double.RadiansToDegrees(Helpers.PointDirection(0, 0, 64, 0));

        Assert.Equal(0.0D, Math.Round(result));
    }

    [Fact]
    public void PointDirection_64x32_Returns22_5ishDegrees()
    {
        var result = double.RadiansToDegrees(Helpers.PointDirection(0, 0, 64, 32));

        Assert.Equal(26.0D, Math.Floor(result));
    }

    [Fact]
    public void PointDirection_64x64_Returns45Degrees()
    {
        var result = double.RadiansToDegrees(Helpers.PointDirection(0, 0, 64, 64));

        Assert.Equal(45.0D, Math.Round(result));
    }

    [Fact]
    public void PointDirection_0x64_Returns90Degrees()
    {
        var result = double.RadiansToDegrees(Helpers.PointDirection(0, 0, 0, 64));

        Assert.Equal(90.0D, Math.Round(result));
    }

    [Fact]
    public void PointDirection_Negative32x64_Returns112ishDegrees()
    {
        var result = double.RadiansToDegrees(Helpers.PointDirection(0, 0, -32, 64));

        Assert.Equal(112.0D, Math.Round(result));
    }

    [Fact]
    public void PointDirection_Negative64x64_Returns135Degrees()
    {
        var result = double.RadiansToDegrees(Helpers.PointDirection(0, 0, -64, 64));

        Assert.Equal(135.0D, Math.Round(result));
    }

    [Fact]
    public void PointDirection_Negative64x0_Returns180Degrees()
    {
        var result = double.RadiansToDegrees(Helpers.PointDirection(0, 0, -64, 0));

        Assert.Equal(180.0D, Math.Round(result));
    }

    [Theory]
    [InlineData(-32, -32, -96, -64)]
    [InlineData(-96, -64, -32, -32)]
    public void PointDirection_RealExample(double x1, double y1, double x2, double y2)
    {
        var result = double.RadiansToDegrees(Helpers.PointDirection(x1, y1, x2, y2));
    }

    [Theory]
    [InlineData("7F7F7F", 127, 127, 127)]
    [InlineData("1F1F1F", 31, 31, 31)]
    public void ConvertFromHex_7F7F7F_Returns127127127(string hexInput, int expectedR, int expectedG, int expectedB)
    {
        var result = Converters.ConvertFromHex([hexInput]);

        Assert.Equal(expectedR, result[0].R);
        Assert.Equal(expectedG, result[0].G);
        Assert.Equal(expectedB, result[0].B);
    }
}
