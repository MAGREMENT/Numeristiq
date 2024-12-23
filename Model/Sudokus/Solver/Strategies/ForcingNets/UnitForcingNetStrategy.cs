﻿using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Graphs.Coloring;
using Model.Core.Graphs.Coloring.ColoringResults;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.ForcingNets;

public class UnitForcingNetStrategy : SudokuStrategy
{
    public const string OfficialName = "Unit Forcing Net";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly int _max;

    public UnitForcingNetStrategy(int maxPossibilities) : base(OfficialName, Difficulty.Inhuman, DefaultInstanceHandling)
    {
        _max = maxPossibilities;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = solverData.RowPositionsAt(row, number);
                if (ppir.Count < 2 || ppir.Count > _max) continue;
                
                var colorings = new ColoringDictionary<ISudokuElement>[ppir.Count];

                var cursor = 0;
                foreach (var col in ppir)
                {
                    colorings[cursor] = solverData.PreComputer.OnColoring(row, col, number);
                    cursor++;
                }

                if (Process(solverData, colorings)) return;
            }

            for (int col = 0; col < 9; col++)
            {
                var ppic = solverData.ColumnPositionsAt(col, number);
                if (ppic.Count < 2 || ppic.Count > _max) continue;
                
                var colorings = new ColoringDictionary<ISudokuElement>[ppic.Count];

                var cursor = 0;
                foreach (var row in ppic)
                {
                    colorings[cursor] = solverData.PreComputer.OnColoring(row, col, number);
                    cursor++;
                }

                if (Process(solverData, colorings)) return;
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = solverData.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimn.Count < 2 || ppimn.Count > _max) continue;
                
                    var colorings = new ColoringDictionary<ISudokuElement>[ppimn.Count];

                    var cursor = 0;
                    foreach (var pos in ppimn)
                    {
                        colorings[cursor] = solverData.PreComputer.OnColoring(pos.Row, pos.Column, number);
                        cursor++;
                    }

                    if (Process(solverData, colorings)) return;
                }
            }
        }
    }

    private bool Process(ISudokuSolverData solverData, ColoringDictionary<ISudokuElement>[] colorings)
    {
        foreach (var element in colorings[0])
        {
            if (element.Key is not CellPossibility current) continue;

            var sameInAll = true;
            var col = element.Value;

            for (int i = 1; i < colorings.Length && sameInAll; i++)
            {
                if (!colorings[i].TryGetValue(current, out var c) || c != col)
                {
                    sameInAll = false;
                    break;
                }
            }

            if (sameInAll)
            {
                if (col == ElementColor.On)
                {
                    solverData.ChangeBuffer.ProposeSolutionAddition(current.Possibility, current.Row, current.Column);
                    if (solverData.ChangeBuffer.NeedCommit())
                    {
                        solverData.ChangeBuffer.Commit(new UnitForcingNetReportBuilder(colorings, current, ElementColor.On,
                            ForcingNetsUtility.GetReportGraph(solverData)));
                        if (StopOnFirstCommit) return true;
                    }
                }
                else
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(current.Possibility, current.Row, current.Column);
                    if (solverData.ChangeBuffer.NeedCommit())
                    {
                        solverData.ChangeBuffer.Commit(new UnitForcingNetReportBuilder(colorings, current, ElementColor.Off,
                            ForcingNetsUtility.GetReportGraph(solverData)));
                        if (StopOnFirstCommit) return true;
                    }
                }
            }
        }

        return false;
    }
}

public class UnitForcingNetReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly ColoringDictionary<ISudokuElement>[] _colorings;
    private readonly CellPossibility _target;
    private readonly ElementColor _targetColoring;
    private readonly IGraph<ISudokuElement, LinkStrength> _graph;

    public UnitForcingNetReportBuilder(ColoringDictionary<ISudokuElement>[] colorings, CellPossibility target, ElementColor targetColoring, IGraph<ISudokuElement, LinkStrength> graph)
    {
        _colorings = colorings;
        _target = target;
        _targetColoring = targetColoring;
        _graph = graph;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var highlights = new Highlight<ISudokuHighlighter>[_colorings.Length];
        var paths = new List<Chain<ISudokuElement, LinkStrength>>[_colorings.Length];

        for (int i = 0; i < _colorings.Length; i++)
        {
            paths[i] = ForcingNetsUtility.FindEveryNeededPaths(_colorings[i].History!.GetPathToRootWithGuessedLinks(_target,
                _targetColoring), _colorings[i], _graph, snapshot);
            
            var iForDelegate = i;
            highlights[i] = lighter =>
            {
                ForcingNetsUtility.HighlightAllPaths(lighter, paths[iForDelegate], ElementColor.On);
                
                if (paths[iForDelegate][0].Elements[0] is CellPossibility start) lighter.EncirclePossibility(start);
                ChangeReportHelper.HighlightChanges(lighter, changes);
            };
        }
        
        return new ChangeReport<ISudokuHighlighter>( "", highlights);
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}