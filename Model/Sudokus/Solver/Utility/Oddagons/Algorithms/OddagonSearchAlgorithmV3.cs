﻿using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Oddagons.Algorithms;

public class OddagonSearchAlgorithmV3 : IOddagonSearchAlgorithm
{
    public int MaxLength { get; set; }
    public int MaxGuardians { get; set; }
    
    public List<AlmostOddagon> Search(ISudokuSolverData solverData, IGraph<CellPossibility, LinkStrength> graph)
    {
        var raw = CycleBasis.Find(graph, CycleBasis.DefaultConstructLoop);
        raw = new List<Loop<CellPossibility, LinkStrength>>(CycleBasis.Multiply(raw,
            CycleBasis.DefaultCombineLoops, 5));
        List<AlmostOddagon> result = new();

        foreach (var loop in raw)
        {
            if(loop.Elements.Count % 2 != 1 || loop.Elements.Count > MaxLength) continue;

            var guardians = OddagonSearcher.FindGuardians(solverData, loop);
            if (guardians is not null && guardians.Length <= MaxGuardians) result.Add(new AlmostOddagon(loop, guardians));
        }

        return result;
    }
}