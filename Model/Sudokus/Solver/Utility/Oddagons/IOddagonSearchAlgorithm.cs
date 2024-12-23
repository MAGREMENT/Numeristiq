﻿using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Oddagons;

public interface IOddagonSearchAlgorithm
{
    public int MaxLength { set; }
    public int MaxGuardians { set; }
    
    List<AlmostOddagon> Search(ISudokuSolverData solverData, IGraph<CellPossibility, LinkStrength> graph);
}