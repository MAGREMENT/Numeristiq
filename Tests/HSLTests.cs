using Model.Utility;

namespace Tests;

public class HSLTests
{
    [Test]
    public void RGBConversionTest()
    {
        var prettyColor = new HSL(240, 18, 69);
        var rgb = prettyColor.ToRGB();

        Assert.That(rgb.Red, Is.EqualTo(162));
        Assert.That(rgb.Green, Is.EqualTo(162));
        Assert.That(rgb.Blue, Is.EqualTo(190));
    }
}