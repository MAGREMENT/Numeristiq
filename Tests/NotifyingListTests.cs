using NewView;
using NewView.Tectonic;

namespace Tests;

public class NotifyingListTests
{
    [Test]
    public void AddTest()
    {
        var n = 0;
        
        var list = new NotifyingList<int>();
        list.ElementAdded += () => n++;
        
        list.Add(0);
        list.Add(1);

        Assert.That(n, Is.EqualTo(2));
    }
}