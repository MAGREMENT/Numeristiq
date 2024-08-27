using System;
using System.Collections.Generic;
using System.Linq;
using Model.Utility.BitSets;

namespace Model.Core;

public static class TruthAndLinksLogic //TODO complete and test
{
    public static IEnumerable<(TLink[], TLink[])> FindRank0<TElement, TLink>(ITruthBank<TElement, TLink> bank,
        int maxSize, Construct<TElement> construct)
        where TLink : ITruthLink<TElement> where TElement : notnull
    {
        HashSet<Done> explored = new();
        List<(TLink[], TLink[])> result = new();

        foreach (var start in bank.EnumerateLinks())
        {
            var current = new Current<TElement, TLink>(construct);
            current.AddTruth(start, bank.GetIndex(start));
            SearchRank0(bank, maxSize, current, explored, result);
        }

        return result;
    }

    private static void SearchRank0<TElement, TLink>(ITruthBank<TElement, TLink> bank, int maxSize, 
        Current<TElement, TLink> current,
        HashSet<Done> explored, List<(TLink[], TLink[])> result)
        where TLink : ITruthLink<TElement> where TElement : notnull
    {
        if (current.TruthSet.Count > current.LinksSets.Count)
        {
            var last = current.TruthSet[^1];
            foreach (var element in last)
            {
                if(current.LinksElementSet.Contains(element)) continue;
                
                foreach (var link in bank.GetLinks(element))
                {
                    if(current.DoesOverlapWithLinksSet(link)) continue;
                    
                    var a = current.AddLink(link, bank.GetIndex(link));
                    if(explored.Contains(current.Done)) continue;

                    explored.Add(current.Done.Copy());
                    switch (current.TruthElementSet.IsOneCoveredByTheOther(current.LinksElementSet))
                    {
                        case 0 :
                            result.Add((current.TruthSet.ToArray(), current.LinksSets.ToArray()));
                            break;
                        case 1 :
                            result.Add((current.LinksSets.ToArray(), current.TruthSet.ToArray()));
                            break;
                    }
                    
                    SearchRank0(bank, maxSize, current, explored, result);
                    current.RemoveLastLink(a);
                }
            }
        }
        else
        {
            if (current.TruthSet.Count == maxSize) return;
            
            var last = current.LinksSets[^1];
            foreach (var element in last)
            {
                if(current.TruthElementSet.Contains(element)) continue;
                
                foreach (var truth in bank.GetTruths(element))
                {
                    if(current.DoesOverlapWithTruthSet(truth)) continue;
                    
                    var a = current.AddTruth(truth, bank.GetIndex(truth));
                    if(explored.Contains(current.Done)) continue;

                    explored.Add(current.Done.Copy());
                    SearchRank0(bank, maxSize, current, explored, result);

                    current.RemoveLastTruth(a);
                }
            }
        }
    }

    private class Current<TElement, TLink> where TLink : ITruthLink<TElement>
    {
        public List<TLink> TruthSet { get; } = new();
        public List<TLink> LinksSets { get; } = new();
        public IElementSet<TElement> TruthElementSet { get; }
        public IElementSet<TElement> LinksElementSet { get; }
        public Done Done { get; } = new();

        public Current(Construct<TElement> construct)
        {
            TruthElementSet = construct();
            LinksElementSet = construct();
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
            foreach (var l in LinksSets)
            {
                if (l.DoesOverlap(link)) return true;
            }

            return false;
        }

        public AdditionResult<TElement> AddLink(TLink link, int index)
        {
            LinksSets.Add(link);
            Done.LinksBitSet.Add(index);
            
            var result = new AdditionResult<TElement>(index);
            foreach (var e in link)
            {
                if (LinksElementSet.Add(e)) result.Elements.Add(e);
            }
            return result;
        }

        public void RemoveLastLink(AdditionResult<TElement> result)
        {
            LinksSets.RemoveAt(LinksSets.Count - 1);
            Done.LinksBitSet.Remove(result.Index);
            foreach (var e in result.Elements)
            {
                LinksElementSet.Remove(e);
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
        public InfiniteBitSet LinksBitSet { get; }

        public Done()
        {
            TruthBitSet = new InfiniteBitSet();
            LinksBitSet = new InfiniteBitSet();
        }

        private Done(InfiniteBitSet truthBitSet, InfiniteBitSet linksBitSet)
        {
            TruthBitSet = truthBitSet;
            LinksBitSet = linksBitSet;
        }

        public override bool Equals(object? obj)
        {
            return obj is Done ld && ld.TruthBitSet.Equals(TruthBitSet) && ld.LinksBitSet.Equals(LinksBitSet);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TruthBitSet.GetHashCode(), LinksBitSet.GetHashCode());
        }

        public Done Copy() => new(TruthBitSet.Copy(), LinksBitSet.Copy());
    }
}

public interface ITruthLink<TElement> : IEnumerable<TElement>
{
    public bool DoesOverlap(ITruthLink<TElement> link);
}

public interface ITruthBank<in TElement, TLink> where TElement : notnull where TLink : ITruthLink<TElement>
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
    int IsOneCoveredByTheOther(IElementSet<TElement> set);
}

public class DefaultTruthBank<TElement, TLink> : ITruthBank<TElement, TLink> where TElement : notnull
    where TLink : ITruthLink<TElement>
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