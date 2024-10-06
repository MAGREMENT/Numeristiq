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
        Test(@"Data\XML\Tests\Simple.txt", expected);
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
            Assert.That(element, Is.EqualTo(expected[n++]));
        }

        Assert.That(expected, Has.Count.EqualTo(n));
    }
}