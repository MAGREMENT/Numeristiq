using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Helpers.Settings;
using Model.Helpers.Settings.Types;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.CellColoring;
using Model.Sudoku.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Sudoku.Solver.StrategiesUtility.Oddagons;

namespace Model.Sudoku.Solver.Strategies.ForcingNets;

public class OddagonForcingNetStrategy : SudokuStrategy
{
    public const string OfficialName = "Oddagon Forcing Net";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly IntSetting _maxNumberOfGuardians;
    
    public OddagonForcingNetStrategy(int maxNumberOfGuardians) : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _maxNumberOfGuardians = new IntSetting("Maximum number of guardians",
            new SliderInteractionInterface(1, 20, 1), maxNumberOfGuardians);
        AddSetting(_maxNumberOfGuardians);
    }
    
    public override void Apply(IStrategyUser strategyUser)
    {
        foreach (var oddagon in strategyUser.PreComputer.AlmostOddagons())
        {
            if (oddagon.Guardians.Length > _maxNumberOfGuardians.Value) continue;

            var colorings = new ColoringDictionary<ISudokuElement>[oddagon.Guardians.Length];
            for (int i = 0; i < oddagon.Guardians.Length; i++)
            {
                var current = oddagon.Guardians[i];
                colorings[i] = strategyUser.PreComputer.OnColoring(current.Row, current.Column, current.Possibility);
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
                    if (element.Value == Coloring.On) strategyUser.ChangeBuffer.ProposeSolutionAddition(cp);
                    else strategyUser.ChangeBuffer.ProposePossibilityRemoval(cp);

                    if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit( 
                            new OddagonForcingNetReportBuilder(colorings, element.Value, oddagon,
                                strategyUser.PreComputer.Graphs.ComplexLinkGraph, cp)) && OnCommitBehavior == OnCommitBehavior.Return) return;
                }
            }
        }
    }
}

public class OddagonForcingNetReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly ColoringDictionary<ISudokuElement>[] _colorings;
    private readonly Coloring _changeColoring;
    private readonly CellPossibility _change;
    private readonly AlmostOddagon _oddagon;
    private readonly ILinkGraph<ISudokuElement> _graph;

    public OddagonForcingNetReportBuilder(ColoringDictionary<ISudokuElement>[] colorings, Coloring changeColoring, AlmostOddagon oddagon, ILinkGraph<ISudokuElement> graph, CellPossibility change)
    {
        _colorings = colorings;
        _changeColoring = changeColoring;
        _oddagon = oddagon;
        _graph = graph;
        _change = change;
    }

    public ChangeReport<ISudokuHighlighter> Build(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        var highlights = new Highlight<ISudokuHighlighter>[_colorings.Length];
        for (int i = 0; i < _colorings.Length; i++)
        {
            var iForDelegate = i;
            highlights[i] = lighter =>
            {
                var paths = ForcingNetsUtility.FindEveryNeededPaths(_colorings[iForDelegate].History!.GetPathToRootWithGuessedLinks(_change,
                    _changeColoring), _colorings[iForDelegate], _graph, snapshot);

                ForcingNetsUtility.HighlightAllPaths(lighter, paths, Coloring.On);
                lighter.EncirclePossibility(_oddagon.Guardians[iForDelegate]);
                ChangeReportHelper.HighlightChanges(lighter, changes);
            };
        }

        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var element in _oddagon.Loop.Elements)
            {
                lighter.HighlightPossibility(element, ChangeColoration.CauseOffTwo);
            }
            
            _oddagon.Loop.ForEachLink((one, two)
                => lighter.CreateLink(one, two, LinkStrength.Strong), LinkStrength.Strong);
            _oddagon.Loop.ForEachLink((one, two)
                => lighter.CreateLink(one, two, LinkStrength.Weak), LinkStrength.Weak);

            foreach (var cp in _oddagon.Guardians)
            {
                lighter.EncirclePossibility(cp);
                lighter.HighlightPossibility(cp, ChangeColoration.CauseOnOne);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        }, highlights);
    }
}