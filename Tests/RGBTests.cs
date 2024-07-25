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
    }
}