using System;
using System.Collections.Generic;
using System.Linq;
using Model.Utility.BitSets;

namespace Model.Core;

public static class TruthAndLinksLogic
{
    public static IEnumerable<(TLink[], TLink[])> FindRank0<TElement, TLink>(ITruthAndLinkBank<TElement, TLink> bank,
        int maxSize, bool ignoreEqual, Construct<TElement> construct)
        where TLink : ITruthOrLink<TElement> where TElement : notnull
    {
        HashSet<Done> explored = new();
        List<(TLink[], TLink[])> result = new();

        foreach (var start in bank.EnumerateLinks())
        {
            var current = new Current<TElement, TLink>(construct);
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
            var last = current.TruthSet[^1];
            foreach (var element in last)
            {
                if(current.LinkElementSet.Contains(element)) continue;
                
                foreach (var link in bank.GetLinks(element))
                {
                    if(current.DoesOverlapWithLinksSet(link) || current.TruthSet.Contains(link)) continue;

                    var index = bank.GetIndex(link);
                    var a = current.AddLink(link, index);
                    if (explored.Contains(current.Done))
                    {
                        current.RemoveLastLink(a);
                        continue;
                    }
                    
                    explored.Add(current.Done.Copy());
                    switch (current.TruthElementSet.IsOneCoveredByTheOther(current.LinkElementSet))
                    {
                        case CoverResult.FirstCoveredBySecond :
                            result.Add((current.TruthSet.ToArray(), current.LinkSet.ToArray()));
                            break;
                        case CoverResult.SecondCoveredByFirst :
                            result.Add((current.LinkSet.ToArray(), current.TruthSet.ToArray()));
                            break;
                        case CoverResult.Equals :
                            if(!ignoreEqual) result.Add((current.TruthSet.ToArray(), current.LinkSet.ToArray()));
                            break;
                    }
                    
                    SearchRank0(bank, maxSize, ignoreEqual, current, explored, result);
                    current.RemoveLastLink(a);
                }
            }
        }
        else
        {
            if (current.TruthSet.Count == maxSize) return;
            
            var last = current.LinkSet[^1];
            foreach (var element in last)
            {
                if(current.TruthElementSet.Contains(element)) continue;
                
                foreach (var truth in bank.GetTruths(element))
                {
                    if(current.DoesOverlapWithTruthSet(truth) || current.LinkSet.Contains(truth)) continue;
                    
                    var index = bank.GetIndex(truth);
                    var a = current.AddTruth(truth, index);
                    if (explored.Contains(current.Done))
                    {
                        current.RemoveLastTruth(a);
                        continue;
                    }

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
        public IElementSet<TElement> TruthElementSet { get; }
        public IElementSet<TElement> LinkElementSet { get; }
        public Done Done { get; } = new();

        public Current(Construct<TElement> construct)
        {
            TruthElementSet = construct();
            LinkElementSet = construct();
        }

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
                if (TruthElementSet.Add(e)) result.Elements.Add(e);
            }
            return result;
        }

        public void RemoveLastTruth(AdditionResult<TElement> result)
        {
            TruthSet.RemoveAt(TruthSet.Count - 1);
            Done.TruthBitSet.Remove(result.Index);
            foreach (var e in result.Elements)
            {
                TruthElementSet.Remove(e);
            }
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
                if (LinkElementSet.Add(e)) result.Elements.Add(e);
            }
            return result;
        }

        public void RemoveLastLink(AdditionResult<TElement> result)
        {
            LinkSet.RemoveAt(LinkSet.Count - 1);
            Done.LinkBitSet.Remove(result.Index);
            foreach (var e in result.Elements)
            {
                LinkElementSet.Remove(e);
            }
        }
    }

    private class AdditionResult<TElement>
    {
        public int Index { get; }
        public List<TElement> Elements { get; } = new();

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
            return obj is Done ld && ld.TruthBitSet.Equals(TruthBitSet) && ld.LinkBitSet.Equals(LinkBitSet);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TruthBitSet.GetHashCode(), LinkBitSet.GetHashCode());
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

public delegate IElementSet<TElement> Construct<TElement>();

public interface IElementSet<TElement>
{
    bool Add(TElement element);
    void Remove(TElement element);
    bool Contains(TElement element);
    CoverResult IsOneCoveredByTheOther(IElementSet<TElement> set);
}

public enum CoverResult
{
    NoCover, FirstCoveredBySecond, SecondCoveredByFirst, Equals
}

public class DefaultTruthAndLinkBank<TElement, TLink> : ITruthAndLinkBank<TElement, TLink> where TElement : notnull
    where TLink : ITruthOrLink<TElement>
{
    private readonly Dictionary<TElement, List<TLink>> _links = new();
    private readonly Dictionary<TLink, int> _indexes = new();
    private int _current;


    public void Add(TLink link, bool isTruth)
    {
        if (!_indexes.TryAdd(link, _current)) return;

        _current++;
        foreach (var element in link)
        {
            if (!_links.TryGetValue(element, out var list))
            {
                list = new List<TLink>();
                _links.Add(element, list);
            }

            list.Add(link);
        }
    }

    public IEnumerable<TLink> GetTruths(TElement element)
    {
        return _links.TryGetValue(element, out var list) ? list : Enumerable.Empty<TLink>();
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