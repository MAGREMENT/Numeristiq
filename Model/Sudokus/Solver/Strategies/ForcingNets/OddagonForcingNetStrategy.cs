﻿using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Graphs.Coloring;
using Model.Core.Graphs.Coloring.ColoringResults;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.Oddagons;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.ForcingNets;

public class OddagonForcingNetStrategy : SudokuStrategy
{
    public const string OfficialName = "Oddagon Forcing Net";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IntSetting _maxNumberOfGuardians;
    private readonly IntSetting _maxLength;
    
    public OddagonForcingNetStrategy(int maxLength, int maxNumberOfGuardians) : base(OfficialName, Difficulty.Inhuman, DefaultInstanceHandling)
    {
        _maxLength = new IntSetting("Maximum length",
            "The maximum length of the oddagon",
            new SliderInteractionInterface(3, 15, 2), maxLength);
        _maxNumberOfGuardians = new IntSetting("Maximum number of guardians", 
            "The maximum amount of guardians an oddagon can have",
            new SliderInteractionInterface(1, 7, 1), maxNumberOfGuardians);
    }
    
    public override IEnumerable<ISetting> EnumerateSettings()
    {
        yield return _maxLength;
        yield return _maxNumberOfGuardians;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        foreach (var oddagon in solverData.PreComputer.AlmostOddagons(_maxLength.Value, _maxNumberOfGuardians.Value))
        {
            var colorings = new ColoringDictionary<ISudokuElement>[oddagon.Guardians.Length];
            for (int i = 0; i < oddagon.Guardians.Length; i++)
            {
                var current = oddagon.Guardians[i];
                colorings[i] = solverData.PreComputer.OnColoring(current.Row, current.Column, current.Possibility);
            }

            foreach (var element in colorings[0])
            {
                if (element.Key is not CellPossibility cp) continue;
                
                bool ok = true;
                
                for (int i = 1; i < colorings.Length; i++)
                {
                    if (!colorings[i].TryGetColoredElement(element.Key, out var c) || c != element.Value)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    if (element.Value == ElementColor.On) solverData.ChangeBuffer.ProposeSolutionAddition(cp);
                    else solverData.ChangeBuffer.ProposePossibilityRemoval(cp);

                    if (solverData.ChangeBuffer.NeedCommit())
                    {
                        solverData.ChangeBuffer.Commit(new OddagonForcingNetReportBuilder(colorings, element.Value, oddagon,
                            ForcingNetsUtility.GetReportGraph(solverData), cp));
                        if (StopOnFirstCommit) return;
                    }
                }
            }
        }
    }
}

public class OddagonForcingNetReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly ColoringDictionary<ISudokuElement>[] _colorings;
    private readonly ElementColor _changeColoring;
    private readonly CellPossibility _change;
    private readonly AlmostOddagon _oddagon;
    private readonly IGraph<ISudokuElement, LinkStrength> _graph;

    public OddagonForcingNetReportBuilder(ColoringDictionary<ISudokuElement>[] colorings, ElementColor changeColoring, AlmostOddagon oddagon, IGraph<ISudokuElement, LinkStrength> graph, CellPossibility change)
    {
        _colorings = colorings;
        _changeColoring = changeColoring;
        _oddagon = oddagon;
        _graph = graph;
        _change = change;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var highlights = new Highlight<ISudokuHighlighter>[_colorings.Length];
        for (int i = 0; i < _colorings.Length; i++)
        {
            var iForDelegate = i;
            highlights[i] = lighter =>
            {
                var paths = ForcingNetsUtility.FindEveryNeededPaths(_colorings[iForDelegate].History!.GetPathToRootWithGuessedLinks(_change,
                    _changeColoring), _colorings[iForDelegate], _graph, snapshot);

                ForcingNetsUtility.HighlightAllPaths(lighter, paths, ElementColor.On);
                lighter.EncirclePossibility(_oddagon.Guardians[iForDelegate]);
                ChangeReportHelper.HighlightChanges(lighter, changes);
            };
        }

        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var element in _oddagon.Loop.Elements)
            {
                lighter.HighlightPossibility(element, StepColor.Cause2);
            }
            
            _oddagon.Loop.ForEachLink((one, two)
                => lighter.CreateLink(one, two, LinkStrength.Strong), LinkStrength.Strong);
            _oddagon.Loop.ForEachLink((one, two)
                => lighter.CreateLink(one, two, LinkStrength.Weak), LinkStrength.Weak);

            foreach (var cp in _oddagon.Guardians)
            {
                lighter.EncirclePossibility(cp);
                lighter.HighlightPossibility(cp, StepColor.On);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        }, highlights);
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}