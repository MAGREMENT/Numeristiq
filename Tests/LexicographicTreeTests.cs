using Model.Utility.Collections.Lexicography;
using Model.Utility.Collections.Lexicography.Nodes;

namespace Tests;

public class LexicographicTreeTests
{
    private readonly List<ConstructNode<int>> _constructs = new();

    [OneTimeSetUp]
    public void SetUp()
    {
        _constructs.Add(() => new ListNode<int>());
    }

    [Test]
    public void Test()
    {
        foreach (var c in _constructs)
        {
            var tree = new LexicographicTree<int>(c);

            tree.Add("aba", 1);
            Assert.That(tree.TryGet("aba", out var result), Is.True);
            Assert.That(result, Is.EqualTo(1));
        }
    }
}