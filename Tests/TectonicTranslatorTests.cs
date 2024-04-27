using Model.Tectonics;

namespace Tests;

public class TectonicTranslatorTests
{
    [Test]
    public void TranslateCodeFormatTest()
    {
        const string s = "5.4:2.0;.0;4.0;.0;.1;.1;.1;.1;.1;.2;.2;.2;.3;.4;5.4;.4;.3;3.3;.4;3.4;";

        var t = TectonicTranslator.TranslateCodeFormat(s);
        Console.WriteLine(t);
        
        Assert.That(t.Zones, Has.Count.EqualTo(5));
    }
    
    [Test]
    public void TranslateRdFormatTest()
    {
        const string s1 = "5.4:0rd5rd00d0r00d00d0r0r00r0r0r01r0r4r0";
        const string s2 = "5.4:4rd0d0rd2d0r0d3r00d00rd0d0rd04r0d00r03";

        var t = TectonicTranslator.TranslateRdFormat(s1);
        Console.WriteLine(t);
        Assert.That(t.Zones, Has.Count.EqualTo(5));

        t = TectonicTranslator.TranslateRdFormat(s2);
        Console.WriteLine(t);
        Assert.Multiple(() =>
        {
            Assert.That(t.Zones, Has.Count.EqualTo(5));
            foreach (var zone in t.Zones)
            {
                Assert.That(zone, Has.Count.LessThanOrEqualTo(5));
            }
        });
    }
}