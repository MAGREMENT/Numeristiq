using Model.Helpers.Changes;

namespace Tests;

public class SolverChangeTests
{
    [Test]
    public void Test()
    {
        var change = new SolverProgress(ProgressType.SolutionAddition, 5, 3, 6);
        
        Assert.Multiple(() =>
        {
            Assert.That(change.ProgressType, Is.EqualTo(ProgressType.SolutionAddition));
            Assert.That(change.Number, Is.EqualTo(5));
            Assert.That(change.Row, Is.EqualTo(3));
            Assert.That(change.Column, Is.EqualTo(6));
        });
    }
}