using Model.Utility;
using Model.XML;

namespace Tests;

public class XMLParserTests
{
    [Test]
    public void Test()
    {
        List<IXMLElement> expected = new();
        var a = new Tag("a");
        a.SetContent("Hi");
        var b = new Tag("b");
        b.AddToContent("Mmmh");
        b.AddToContent(new Tag("c"));
        expected.Add(a);
        expected.Add(b);
        expected.Add(new Text("Test"));
        Test(@"Data\XML\Tests\Simple.xml", expected);
        
        expected.Clear();
        a = new Tag("a");
        a.AddAttribute("cool", "yes");
        b = new Tag("b");
        var c = new Tag("c");
        c.AddAttribute("height", "32");
        c.AddAttribute("width", "69");
        b.AddToContent(c);
        b.AddToContent("YES");
        a.SetContent(b);
        expected.Add(a);
        Test(@"Data\XML\Tests\Attribute.xml", expected);
    }
    
    private static void Test(string fileName, List<IXMLElement> expected)
    {
        var s = PathFinder.Find(fileName, true, false);
        Assert.That(s, Is.Not.Null);

        var result = XMLParser.Parse(s!);

        int n = 0;
        foreach (var element in result)
        {
            Assert.That(expected, Has.Count.GreaterThan(n));
            Assert.That(element.Equals(expected[n++]), Is.True);
        }

        Assert.That(expected, Has.Count.EqualTo(n));
    }
}