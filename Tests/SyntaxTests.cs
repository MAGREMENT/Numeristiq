using Model.Core.Syntax;
using Model.Repositories;
using Repository.HardCoded;

namespace Tests;

public class SyntaxTests
{
    private static readonly ISyntaxParsablesRepository<ISyntaxElement> _repository = new HardCodedSyntaxParsablesRepository();
    private static readonly SyntaxParser<ISyntaxElement> _parser = new(_repository.GetParsables());
    
    [Test]
    public void SimpleEquationTest()
    {
        const string s = "12 + 4 = 10 + 6";
        var e = _parser.TryParseLine(s, out var b);
        Assert.That(e, Is.EqualTo(ParserError.None));
        Assert.That(b, Is.Not.Null);
        Assert.That(b!.ToString(), Is.EqualTo(s));
        AssertFirst3Operators(b, "=", "+", "+");

        /*var e2 = _ypInterpreter.TryInterpret(new ParsedLine[] { new(0, b) }, out var i);
        Assert.That(e2, Is.Null);*/
    }

    [Test]
    public void SimplePriorityEquationTest()
    {
        const string s1 = "12 + 2 * 2 = 10 + 6";
        var e = _parser.TryParseLine(s1, out var b);
        Assert.That(e, Is.EqualTo(ParserError.None));
        Assert.That(b, Is.Not.Null);
        Assert.That(b!.ToString(), Is.EqualTo(s1));
        AssertFirst3Operators(b, "=", "+", "+");
            
        const string s2 = "2 * 2 + 12 = 10 + 6";
        e = _parser.TryParseLine(s2, out b);
        Assert.That(e, Is.EqualTo(ParserError.None));
        Assert.That(b, Is.Not.Null);
        Assert.That(b!.ToString(), Is.EqualTo(s2));
        AssertFirst3Operators(b, "=", "+", "+");

        const string s3 = "3 * 4 = 10 ^ 3 * 5 + 6";
        e = _parser.TryParseLine(s3, out b);
        Assert.That(e, Is.EqualTo(ParserError.None));
        Assert.That(b, Is.Not.Null);
        Assert.That(b!.ToString(), Is.EqualTo(s3));
        AssertFirst3Operators(b, "=", "*", "+");

        const string s4 = "3 * 4 = 10 + 3 * 5 ^ 6";
        e = _parser.TryParseLine(s4, out b);
        Assert.That(e, Is.EqualTo(ParserError.None));
        Assert.That(b, Is.Not.Null);
        Assert.That(b!.ToString(), Is.EqualTo(s4));
        AssertFirst3Operators(b, "=", "*", "+");

        const string s5 = "3 * 4 = 10 + 3 ^ 5 * 6";
        e = _parser.TryParseLine(s5, out b);
        Assert.That(e, Is.EqualTo(ParserError.None));
        Assert.That(b, Is.Not.Null);
        Assert.That(b!.ToString(), Is.EqualTo(s5));
        AssertFirst3Operators(b, "=", "*", "+");
    }

    private static void AssertFirst3Operators(ISyntaxElement b, string main, string left, string right)
    {
        var op = b as SyntaxOperator;
        Assert.That(op, Is.Not.Null);
        if (op!.Left is not SyntaxOperator opl)
        {
            Assert.Fail();
            return;
        }

        if (op.Right is not SyntaxOperator opr)
        {
            Assert.Fail();
            return;
        }
        
        Assert.That(op.ToSyntaxString().value, Is.EqualTo(main));
        Assert.That(opl.ToSyntaxString().value, Is.EqualTo(left));
        Assert.That(opr.ToSyntaxString().value, Is.EqualTo(right));
    }

    [Test]
    public void ParenthesisTest()
    {
        const string s1 = "3 * (2 + 5)";
        var e = _parser.TryParseLine(s1, out var b);
        Assert.That(e, Is.EqualTo(ParserError.None));
        Assert.That(b, Is.Not.Null);
        Assert.That(b!.ToString(), Is.EqualTo(s1));
    }

    [Test]
    public void ParenthesisTest2()
    {
        const string s2 = "((2 + 3) * 4 + 5) * 6";
        var e = _parser.TryParseLine(s2, out var b);
        Assert.That(e, Is.EqualTo(ParserError.None));
        Assert.That(b, Is.Not.Null);
        Assert.That(b!.ToString(), Is.EqualTo(s2));
    }
}