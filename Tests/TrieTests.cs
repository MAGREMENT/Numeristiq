using Model.Utility.Collections.Lexicography;

namespace Tests;

public class TrieTests
{
    private readonly List<Trie<int>> _tries = new();

    [OneTimeSetUp]
    public void SetUp()
    {
        _tries.Add(new ListTrie<int>());
    }

    [Test]
    public void Test()
    {
        foreach (var trie in _tries)
        {
            trie.Add("aba", 1);
            Assert.That(trie.Add("aba", 2), Is.EqualTo(false));
            Assert.That(trie.TryGet("aba", out var result), Is.True);
            Assert.That(result, Is.EqualTo(1));
        }
    }
}