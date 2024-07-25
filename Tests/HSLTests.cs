using Model.Utility;

namespace Tests;

public class HSLTests
{
    [Test]
    public void RGBConversionTest()
    {
        var prettyColor = new HSL(240, 0.18, 0.69);
        var rgb = prettyColor.ToRGB();
        Assert.Multiple(() =>
        {
            Assert.That(rgb.Red, Is.EqualTo(162));
            Assert.That(rgb.Green, Is.EqualTo(162));
            Assert.That(rgb.Blue, Is.EqualTo(190));
        });
        
        prettyColor = new HSL(210, 0.79, 0.3);
        rgb = prettyColor.ToRGB();
        Assert.Multiple(() =>
        {
            Assert.That(rgb.Red, Is.EqualTo(16));
            Assert.That(rgb.Green, Is.EqualTo(76));
            Assert.That(rgb.Blue, Is.EqualTo(137));
        });

        prettyColor = new HSL(4, 0.5, 0.5);
        rgb = prettyColor.ToRGB();
        Assert.Multiple(() =>
        {
            Assert.That(rgb.Red, Is.EqualTo(191));
            Assert.That(rgb.Green, Is.EqualTo(72));
            Assert.That(rgb.Blue, Is.EqualTo(64));
        });
    }
}