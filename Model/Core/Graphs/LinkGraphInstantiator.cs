﻿using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Core.Graphs;

public static class LinkGraphInstantiator
{
    public static ILinkGraph<T> For<T>() where T : notnull
    {
        return new DictionaryLinkGraph<T>();
    }
}