using System;
using System.Collections.Generic;
using System.Text;
using Model.Core.Graphs;

namespace Model.Sudokus.Solver.Utility.Graphs;

public static class SudokuChainExtensions
{
    public static int MaxRank<TElement, TLink>(this Chain<TElement, TLink> chain) where TLink : notnull
        where TElement : ISudokuElement
    {
        var result = 0;
        foreach (var element in chain.Elements)
        {
            result = Math.Max(result, element.DifficultyRank);
        }

        return result;
    }
}