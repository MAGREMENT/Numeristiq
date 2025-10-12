using Model.Repositories;
using Model.YourPuzzles.Syntax;
using Repository.HardCoded;

namespace Tests.YourPuzzles;

public class SyntaxParseTests
{
    private static readonly ISyntaxParsablesRepository _repository = new HardCodedSyntaxParsablesRepository();
    private static readonly SyntaxParser _parser = new(_repository.GetParsables());
    
    [Test]
    public void SimpleEquationTest()
    {
        const string s = "12 + 4 = 10 + 6";
        var e = _parser.TryParseLine(s, out var b);
        Assert.That(e, Is.EqualTo(ParserError.None));
        Assert.That(b, Is.Not.Null);
        Assert.That(b!.ToString(), Is.EqualTo(s));
    }

    [Test]
    public void SimplePriorityEquationTest()
    {
        const string s1 = "12 + 2 * 2 = 10 + 6";
        var e = _parser.TryParseLine(s1, out var b);
        Assert.That(e, Is.EqualTo(ParserError.None));
        Assert.That(b, Is.Not.Null);
        Assert.That(b!.Value.ToString(), Is.EqualTo("="));
        Assert.That(b.Left!.Value.ToString(), Is.EqualTo("+"));
        Assert.That(b.ToString(), Is.EqualTo(s1));
        
        const string s2 = "2 * 2 + 12 = 10 + 6";
        e = _parser.TryParseLine(s2, out b);
        Assert.That(e, Is.EqualTo(ParserError.None));
        Assert.That(b, Is.Not.Null);
        Assert.That(b!.Value.ToString(), Is.EqualTo("="));
        Assert.That(b.Left!.Value.ToString(), Is.EqualTo("+"));
        Assert.That(b.ToString(), Is.EqualTo(s2));
    }
}