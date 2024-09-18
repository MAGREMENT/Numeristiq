using Model.Core.Highlighting;
using Model.Utility;

namespace Tests;

public class HighlightInstructionTests
{
    [Test]
    public void Base16Test()
    {
        //0000 0010 0001 0010 0011 0000 0000 0000
        //a    c    b    c    d    a    a    a
        var instruction = HighlightInstruction.EncirclePossibility(1, 2, 3);
        var s = HighlightInstruction.ToBase16(instruction, DefaultBase16Alphabet.Instance);
        Assert.That(s, Is.EqualTo("acbcdaaa"));
        
        var back = HighlightInstruction.FromBase16(s, DefaultBase16Alphabet.Instance);
        Assert.That(instruction, Is.EqualTo(back));
    }
}