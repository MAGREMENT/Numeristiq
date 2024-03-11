using DesktopApplication.View.Tectonic.Controls;

namespace Tests;

public class NotifyingListTests
{
    [Test]
    public void AddTest()
    {
        var n = 0;
        
        var list = new NotifyingList<int>();
        list.ElementAdded += _ => n++;
        
        list.Add(0);
        list.Add(1);
        Assert.Multiple(() =>
        {
            Assert.That(n, Is.EqualTo(2));
            Assert.That(list, Has.Count.EqualTo(2));
            Assert.That(list[0], Is.EqualTo(0));
            Assert.That(list[1], Is.EqualTo(1));
            Assert.That(list, Does.Contain(0));
            Assert.That(list, Does.Contain(1));
            Assert.That(list, Does.Not.Contain(2));
        });
    }
}