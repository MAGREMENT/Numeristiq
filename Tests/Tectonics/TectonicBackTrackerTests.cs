using Model.Core.BackTracking;
using Model.Tectonics;

namespace Tests.Tectonics;

public class TectonicBackTrackerTests
{
    [Test]
    public void Test()
    {
        var t = TectonicTranslator.TranslateRdFormat("6.5:0d0r0r4rd00r4d0d00d1d0d00d0000r0r00rd3d0rd0d00r00r4r5");
        var r = TectonicTranslator.TranslateRdFormat("6.5:1d3r2r4rd12r4d1d52d1d3d23d1251r4r21rd3d2rd3d12r41r4r5");

        var backTracker = new TectonicBackTracker(t, new TectonicPossibilitiesGiver(t));
        backTracker.Fill();

        Assert.That(t.SameDigits(r), Is.True);
    }
}