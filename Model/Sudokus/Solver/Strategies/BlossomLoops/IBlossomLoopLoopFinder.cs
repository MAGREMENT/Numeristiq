﻿using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopLoopFinder
{
    List<Loop<ISudokuElement, LinkStrength>> Find(CellPossibility[] cps, IGraph<ISudokuElement, LinkStrength> graph);
}