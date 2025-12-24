using System;
using Model.Core.Graphs.Implementations;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Core.Graphs;

public enum GraphCollectionType
{
    HashSet, UniqueList
}

public static class GraphCreator
{
    public static IGraph<TElement, TEdge> Create<TElement, TEdge>(GraphCollectionType type) 
        where TElement : notnull where TEdge : notnull
    {
        switch (type)
        {
            case GraphCollectionType.HashSet :
                return new DictionaryGraph<TElement, TEdge, ContainingHashSet<EdgeTo<TEdge, TElement>>>();
            case GraphCollectionType.UniqueList :
                return new DictionaryGraph<TElement, TEdge, UniqueList<EdgeTo<TEdge, TElement>>>();
        }

        throw new ArgumentException();
    }

    public static IGraph<CellPossibility, LinkStrength> CreateForCellPossibilityLink(GraphCollectionType type, 
        int rowCount, int colCount, int posCount)
    {
        switch (type)
        {
            case GraphCollectionType.HashSet :
                return new CellPossibilityArrayLinkGraph<ContainingHashSet<CellPossibility>>(rowCount, colCount,
                    posCount);
            case GraphCollectionType.UniqueList :
                return new CellPossibilityArrayLinkGraph<UniqueList<CellPossibility>>(rowCount, colCount,
                    posCount);
        }
        
        throw new ArgumentException();
    }
    
    public static IGraph<TElement, LinkStrength> CreateForLink<TElement>(GraphCollectionType type, bool divide) 
        where TElement : notnull
    {
        if (divide)
        {
            switch (type)
            {
                case GraphCollectionType.HashSet :
                    return new DoubleDictionaryLinkGraph<TElement, ContainingHashSet<TElement>>();
                case GraphCollectionType.UniqueList :
                    return new DoubleDictionaryLinkGraph<TElement, UniqueList<TElement>>();
            }
        }
        else
        {
            switch (type)
            {
                case GraphCollectionType.HashSet :
                    return new DictionaryLinkGraph<TElement, ContainingHashSet<TElement>>();
                case GraphCollectionType.UniqueList :
                    return new DictionaryLinkGraph<TElement, UniqueList<TElement>>();
            }
        }

        throw new ArgumentException();
    }
    
    public static IConditionalGraph<TElement, LinkStrength, TValue> CreateForConditionalLink<TElement, TValue>(GraphCollectionType type) 
        where TElement : notnull
    {
        switch (type)
        {
            case GraphCollectionType.HashSet :
                return new DictionaryConditionalLinkGraph<TElement, TValue, ContainingHashSet<TElement>, ContainingHashSet<(TElement, ICondition<TElement, TValue>)>>();
            case GraphCollectionType.UniqueList :
                return new DictionaryConditionalLinkGraph<TElement, TValue, UniqueList<TElement>, UniqueList<(TElement, ICondition<TElement, TValue>)>>();
        }

        throw new ArgumentException();
    }
}