using Model.Utility;

namespace Tests;

public class RGBTests
{
    [Test]
    public void HSLConversionTest()
    {
        var magenta = new RGB(255, 0, 255);
        var hsl = magenta.ToHSL();
        
        Assert.That(hsl.Hue, Is.EqualTo(300));
        Assert.That(Math.Round(hsl.Saturation * 100, 0), Is.EqualTo(100));
        Assert.That(Math.Round(hsl.Lightness * 100, 0), Is.EqualTo(50));

        var white = new RGB(255, 255, 255);
        hsl = white.ToHSL();
        Assert.That(hsl.Hue, Is.EqualTo(0));
        Assert.That(Math.Round(hsl.Saturation * 100, 0), Is.EqualTo(0));
        Assert.That(Math.Round(hsl.Lightness * 100, 0), Is.EqualTo(100));

        var black = new RGB(0, 0, 0);
        hsl = black.ToHSL();
        Assert.That(hsl.Hue, Is.EqualTo(0));
        Assert.That(Math.Round(hsl.Saturation * 100, 0), Is.EqualTo(0));
        Assert.That(Math.Round(hsl.Lightness * 100, 0), Is.EqualTo(0));
    }

    [Test]
    public void StringTest()
    {
        var color = new RGB(14, 14, 22);
        var s = color.ToHexString();
        
        Assert.That(s, Is.EqualTo("#0E0E16"));

        var back = RGB.FromHexString(s);
        Assert.That(color, Is.EqualTo(back));
    }
}