using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.Possibility;

namespace Model.Sudoku.Solver.Strategies;

public class HiddenSetStrategy : AbstractStrategy
{
    public const string OfficialNameForType2 = "Hidden Double";
    public const string OfficialNameForType3 = "Hidden Triple";
    public const string OfficialNameForType4 = "Hidden Quad";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly int _type;

    public HiddenSetStrategy(int type) : base("", StrategyDifficulty.None, DefaultBehavior)
    {
        _type = type;
        switch (type)
        {
            case 2 : Name = OfficialNameForType2;
                Difficulty = StrategyDifficulty.Easy;
                break;
            case 3 : Name = OfficialNameForType3;
                Difficulty = StrategyDifficulty.Easy;
                break;
            case 4 : Name = OfficialNameForType4;
                Difficulty = StrategyDifficulty.Easy;
                break;
            default : throw new ArgumentException("Type not valid");
        }
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            if (RecursiveRowMashing(strategyManager, 1, new LinePositions(), Possibilities.NewEmpty(), row)) return;
        }

        for (int col = 0; col < 9; col++)
        {
            if (RecursiveColumnMashing(strategyManager, 1, new LinePositions(), Possibilities.NewEmpty(), col)) return;
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                if (RecursiveMiniGridMashing(strategyManager, 1, new MiniGridPositions(miniRow, miniCol),
                    Possibilities.NewEmpty(), miniRow, miniCol)) return;
            }
        }
    }

    private bool RecursiveRowMashing(IStrategyManager strategyManager, int start, LinePositions mashed,
        Possibilities visited, int row)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = strategyManager.RowPositionsAt(row, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Or(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var col in newMashed)
                {
                    RemoveAllPossibilitiesExcept(row, col, newVisited, strategyManager);
                }

                if (strategyManager.ChangeBuffer.Commit(this,
                        new LineHiddenPossibilitiesReportBuilder(newVisited, newMashed, row, Unit.Row))
                    && OnCommitBehavior == OnCommitBehavior.Return) return true;
            }
            else if (newVisited.Count < _type &&
                     RecursiveRowMashing(strategyManager, i + 1, newMashed, newVisited, row)) return true;
        }

        return false;
    }
    
    private bool RecursiveColumnMashing(IStrategyManager strategyManager, int start, LinePositions mashed,
        Possibilities visited, int col)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = strategyManager.ColumnPositionsAt(col, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Or(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var row in newMashed)
                {
                    RemoveAllPossibilitiesExcept(row, col, newVisited, strategyManager);
                }

                if (strategyManager.ChangeBuffer.Commit(this,
                        new LineHiddenPossibilitiesReportBuilder(newVisited, newMashed, col, Unit.Column))
                    && OnCommitBehavior == OnCommitBehavior.Return) return true;
            }
            else if (newVisited.Count < _type &&
                     RecursiveColumnMashing(strategyManager, i + 1, newMashed, newVisited, col)) return true;
        }

        return false;
    }
    
    private bool RecursiveMiniGridMashing(IStrategyManager strategyManager, int start, MiniGridPositions mashed,
        Possibilities visited, int miniRow, int miniCol)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = strategyManager.MiniGridPositionsAt(miniRow, miniCol, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Or(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var position in newMashed)
                {
                    RemoveAllPossibilitiesExcept(position.Row, position.Column, newVisited, strategyManager);
                }

                if (strategyManager.ChangeBuffer.Commit(this,
                        new MiniGridHiddenPossibilitiesReportBuilder(newVisited, newMashed))
                    && OnCommitBehavior == OnCommitBehavior.Return) return true;
            }
            else if (newVisited.Count < _type && RecursiveMiniGridMashing(strategyManager, i + 1, newMashed,
                         newVisited, miniRow, miniCol)) return true;
        }

        return false;
    }

    private void RemoveAllPossibilitiesExcept(int row, int col, Possibilities except,
        IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            if (!except.Peek(number))
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
            }
        }
    }
}

public class LineHiddenPossibilitiesReportBuilder : IChangeReportBuilder
{
    private readonly Possibilities _possibilities;
    private readonly LinePositions _linePos;
    private readonly int _unitNumber;
    private readonly Unit _unit;


    public LineHiddenPossibilitiesReportBuilder(Possibilities possibilities, LinePositions linePos, int unitNumber, Unit unit)
    {
        _possibilities = possibilities;
        _linePos = linePos;
        _unitNumber = unitNumber;
        _unit = unit;
    }
    
    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( Explanation(), lighter =>
        {
            foreach (var possibility in _possibilities)
            {
                foreach (var other in _linePos)
                {
                    switch (_unit)
                    {
                        case Unit.Row :
                            if(snapshot.PossibilitiesAt(_unitNumber, other).Peek(possibility))
                                lighter.HighlightPossibility(possibility, _unitNumber, other, ChangeColoration.CauseOffOne);
                            break;
                        case Unit.Column :
                            if(snapshot.PossibilitiesAt(other, _unitNumber).Peek(possibility))
                                lighter.HighlightPossibility(possibility, other, _unitNumber, ChangeColoration.CauseOffOne);
                            break;
                    }  
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        return $"The possibilities {_possibilities} are limited to the cells {_linePos.ToString(_unit, _unitNumber)} in" +
               $" {_unit.ToString().ToLower()} {_unitNumber + 1}, so any other candidates in those cells can be removed";
    }
}

public class MiniGridHiddenPossibilitiesReportBuilder : IChangeReportBuilder
{
    private readonly Possibilities _possibilities;
    private readonly MiniGridPositions _miniPos;

    public MiniGridHiddenPossibilitiesReportBuilder(Possibilities possibilities, MiniGridPositions miniPos)
    {
        _possibilities = possibilities;
        _miniPos = miniPos;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( Explanation(), lighter =>
        {
            foreach (var possibility in _possibilities)
            {
                foreach (var pos in _miniPos)
                {
                    if (snapshot.PossibilitiesAt(pos.Row, pos.Column).Peek(possibility))
                        lighter.HighlightPossibility(possibility, pos.Row, pos.Column, ChangeColoration.CauseOffOne);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
    
    private string Explanation()
    {
        return $"The possibilities {_possibilities} are limited to the cells {_miniPos} in" +
               $" mini grid {_miniPos.MiniGridNumber() + 1}, so any other candidates in those cells can be removed";
    }
}