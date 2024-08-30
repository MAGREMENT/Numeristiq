using System.Collections.Generic;
using System.Linq;
using Model.Utility.BitSets;

namespace Model.Core;

public static class TruthAndLinksLogic
{
    public static IEnumerable<(TLink[], TLink[])> FindRank0<TElement, TLink>(ITruthAndLinkBank<TElement, TLink> bank,
        int maxSize, bool ignoreEqual)
        where TLink : ITruthOrLink<TElement> where TElement : notnull
    {
        HashSet<Done> explored = new();
        List<(TLink[], TLink[])> result = new();

        foreach (var start in bank.EnumerateLinks())
        {
            var current = new Current<TElement, TLink>();
            current.AddTruth(start, bank.GetIndex(start));
            SearchRank0(bank, maxSize, ignoreEqual, current, explored, result);
        }

        return result;
    }

    private static void SearchRank0<TElement, TLink>(ITruthAndLinkBank<TElement, TLink> bank, int maxSize, 
        bool ignoreEqual, Current<TElement, TLink> current, HashSet<Done> explored, List<(TLink[], TLink[])> result)
        where TLink : ITruthOrLink<TElement> where TElement : notnull
    {
        if (current.TruthSet.Count > current.LinkSet.Count)
        {
            foreach (var element in current.TruthsToCover.ToArray())
            {
                foreach (var link in bank.GetLinks(element))
                {
                    if(current.DoesOverlapWithLinksSet(link) || current.TruthSet.Contains(link)) continue;

                    var index = bank.GetIndex(link);
                    current.Done.LinkBitSet.Add(index);
                    if (explored.Contains(current.Done))
                    {
                        current.Done.LinkBitSet.Remove(index);
                        continue;
                    }
                    
                    var a = current.AddLink(link, index);
                    explored.Add(current.Done.Copy());
                    
                    if (current.TruthsToCover.Count == 0)
                    {
                        if (current.LinksToCover.Count == 0)
                        {
                            if(!ignoreEqual) result.Add((current.TruthSet.ToArray(), current.LinkSet.ToArray()));
                        }
                        else result.Add((current.TruthSet.ToArray(), current.LinkSet.ToArray()));
                    }
                    else if (current.LinksToCover.Count == 0) 
                        result.Add((current.LinkSet.ToArray(), current.TruthSet.ToArray()));
                    
                    SearchRank0(bank, maxSize, ignoreEqual, current, explored, result);
                    current.RemoveLastLink(a);
                }
            }
        }
        else
        {
            if (current.TruthSet.Count == maxSize) return;
            
            foreach (var element in current.LinksToCover.ToArray())
            {
                foreach (var truth in bank.GetTruths(element))
                {
                    if(current.DoesOverlapWithTruthSet(truth) || current.LinkSet.Contains(truth)) continue;
                    
                    var index = bank.GetIndex(truth);
                    current.Done.TruthBitSet.Add(index);
                    if (explored.Contains(current.Done))
                    {
                        current.Done.TruthBitSet.Remove(index);
                        continue;
                    }

                    var a = current.AddTruth(truth, index);
                    explored.Add(current.Done.Copy());
                    
                    SearchRank0(bank, maxSize, ignoreEqual, current, explored, result);
                    current.RemoveLastTruth(a);
                }
            }
        }
    }
    
    public static bool DefaultDoesOverlap<TElement>(ITruthOrLink<TElement> t1, ITruthOrLink<TElement> t2)
    {
        foreach (var cp in t2)
        {
            if (t1.Contains(cp)) return true;
        }

        return false;
    }

    private class Current<TElement, TLink> where TLink : ITruthOrLink<TElement>
    {
        public List<TLink> TruthSet { get; } = new();
        public List<TLink> LinkSet { get; } = new();
        public List<TElement> TruthsToCover { get; } = new();
        public List<TElement> LinksToCover { get; } = new();
        public Done Done { get; } = new();

        public bool DoesOverlapWithTruthSet(TLink link)
        {
            foreach (var l in TruthSet)
            {
                if (l.DoesOverlap(link)) return true;
            }

            return false;
        }

        public AdditionResult<TElement> AddTruth(TLink link, int index)
        {
            TruthSet.Add(link);
            Done.TruthBitSet.Add(index);
            
            var result = new AdditionResult<TElement>(index);
            foreach (var e in link)
            {
                var i = LinksToCover.IndexOf(e);

                if (i == -1)
                {
                    TruthsToCover.Add(e);
                    result.ElementsAdded.Add(e);
                }
                else
                {
                    LinksToCover.RemoveAt(i);
                    result.ElementsRemoved.Add(e);
                }
            }
            return result;
        }

        public void RemoveLastTruth(AdditionResult<TElement> result)
        {
            TruthSet.RemoveAt(TruthSet.Count - 1);
            Done.TruthBitSet.Remove(result.Index);
            
            TruthsToCover.RemoveAll(e => result.ElementsAdded.Contains(e));
            LinksToCover.AddRange(result.ElementsRemoved);
        }
        
        public bool DoesOverlapWithLinksSet(TLink link)
        {
            foreach (var l in LinkSet)
            {
                if (l.DoesOverlap(link)) return true;
            }

            return false;
        }

        public AdditionResult<TElement> AddLink(TLink link, int index)
        {
            LinkSet.Add(link);
            Done.LinkBitSet.Add(index);
            
            var result = new AdditionResult<TElement>(index);
            foreach (var e in link)
            {
                var i = TruthsToCover.IndexOf(e);

                if (i == -1)
                {
                    LinksToCover.Add(e);
                    result.ElementsAdded.Add(e);
                }
                else
                {
                    TruthsToCover.RemoveAt(i);
                    result.ElementsRemoved.Add(e);
                }
            }
            return result;
        }

        public void RemoveLastLink(AdditionResult<TElement> result)
        {
            LinkSet.RemoveAt(LinkSet.Count - 1);
            Done.LinkBitSet.Remove(result.Index);
            
            LinksToCover.RemoveAll(e => result.ElementsAdded.Contains(e));
            TruthsToCover.AddRange(result.ElementsRemoved);
        }
    }

    private class AdditionResult<TElement>
    {
        public int Index { get; }
        public List<TElement> ElementsAdded { get; } = new();
        public List<TElement> ElementsRemoved { get; } = new();

        public AdditionResult(int index)
        {
            Index = index;
        }
    }
    
    private class Done
    {
        public InfiniteBitSet TruthBitSet { get; }
        public InfiniteBitSet LinkBitSet { get; }

        public Done()
        {
            TruthBitSet = new InfiniteBitSet();
            LinkBitSet = new InfiniteBitSet();
        }

        private Done(InfiniteBitSet truthBitSet, InfiniteBitSet linkBitSet)
        {
            TruthBitSet = truthBitSet;
            LinkBitSet = linkBitSet;
        }

        public override bool Equals(object? obj)
        {
            return obj is Done ld && ((ld.TruthBitSet.Equals(TruthBitSet) && ld.LinkBitSet.Equals(LinkBitSet))
                    || (ld.TruthBitSet.Equals(LinkBitSet) && ld.LinkBitSet.Equals(TruthBitSet)));
        }

        public override int GetHashCode()
        {
            return TruthBitSet.GetHashCode() ^ LinkBitSet.GetHashCode();
        }

        public Done Copy() => new(TruthBitSet.Copy(), LinkBitSet.Copy());
    }
}

public interface ITruthOrLink<TElement> : IEnumerable<TElement>
{
    public bool DoesOverlap(ITruthOrLink<TElement> link);
    public bool Contains(TElement element);
}

public interface ITruthAndLinkBank<in TElement, TLink> where TElement : notnull where TLink : ITruthOrLink<TElement>
{
    public void Add(TLink link, bool isTruth);
    public IEnumerable<TLink> GetTruths(TElement element);
    public IEnumerable<TLink> GetLinks(TElement element);
    public IEnumerable<TLink> EnumerateLinks();
    public int GetIndex(TLink link);
}

public enum CoverResult
{
    NoCover, FirstCoveredBySecond, SecondCoveredByFirst, Equals
}

public class MergedTruthAndLinkBank<TElement, TLink> : ITruthAndLinkBank<TElement, TLink> where TElement : notnull
    where TLink : ITruthOrLink<TElement>
{
    private readonly Dictionary<TElement, List<TLink>> _truthsOrLinks = new();
    private readonly Dictionary<TLink, int> _indexes = new();
    private int _current;


    public void Add(TLink link, bool isTruth)
    {
        if (!_indexes.TryAdd(link, _current)) return;

        _current++;
        foreach (var element in link)
        {
            if (!_truthsOrLinks.TryGetValue(element, out var list))
            {
                list = new List<TLink>();
                _truthsOrLinks.Add(element, list);
            }

            list.Add(link);
        }
    }

    public IEnumerable<TLink> GetTruths(TElement element)
    {
        return _truthsOrLinks.TryGetValue(element, out var list) ? list : Enumerable.Empty<TLink>();
    }

    public IEnumerable<TLink> GetLinks(TElement element)
    {
        return GetTruths(element);
    }

    public IEnumerable<TLink> EnumerateLinks()
    {
        return _indexes.Keys;
    }

    public int GetIndex(TLink link)
    {
        return _indexes[link];
    }
}

public class SeparatedTruthAndLinkBank<TElement, TLink> : ITruthAndLinkBank<TElement, TLink> where TElement : notnull
    where TLink : ITruthOrLink<TElement>
{
    private readonly Dictionary<TElement, List<TLink>> _truths = new();
    private readonly Dictionary<TElement, List<TLink>> _links = new();
    private readonly Dictionary<TLink, int> _indexes = new();
    private int _current;


    public void Add(TLink link, bool isTruth)
    {
        if (!_indexes.TryAdd(link, _current)) return;

        _current++;
        var dic = isTruth ? _truths : _links;
        foreach (var element in link)
        {
            if (!dic.TryGetValue(element, out var list))
            {
                list = new List<TLink>();
                dic.Add(element, list);
            }

            list.Add(link);
        }
    }

    public IEnumerable<TLink> GetTruths(TElement element)
    {
        return _truths.TryGetValue(element, out var list) ? list : Enumerable.Empty<TLink>();
    }

    public IEnumerable<TLink> GetLinks(TElement element)
    {
        return _links.TryGetValue(element, out var list) ? list : Enumerable.Empty<TLink>();
    }

    public IEnumerable<TLink> EnumerateLinks()
    {
        return _indexes.Keys;
    }

    public int GetIndex(TLink link)
    {
        return _indexes[link];
    }
}