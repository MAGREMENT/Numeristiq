using System.Collections.Generic;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class AvoidableRectangleStrategy : IOriginalBoardNeededStrategy //TODO : do reports builders
{
    public string Name => "Avoidable rectangle";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }

    private Sudoku? _originalBoard;

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        if (_originalBoard is null) return;

        for (int row = 0; row < 9; row++)
        {
            SearchForRowMatch(strategyManager, RowSolvedNumberPairs(strategyManager, row), row);
        }

        for (int col = 0; col < 9; col++)
        {
            SearchForColumnMatch(strategyManager, ColumnSolvedNumberPairs(strategyManager, col), col);
        }
    }

    private void SearchForRowMatch(IStrategyManager strategyManager, List<SolvedNumber[]> pairs, int except)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row == except) continue;

            foreach (var pair in pairs)
            {
                bool pairInSameBox = pair[0].Column / 3 == pair[1].Column / 3;
                bool rowInSameBox = row / 3 == except / 3;

                if (!(pairInSameBox ^ rowInSameBox)) continue;

                //Type 1
                if (strategyManager.Sudoku[row, pair[0].Column] == pair[1].Number &&
                    _originalBoard![row, pair[0].Column] == 0)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(pair[0].Number, row, pair[1].Column);
                    if(strategyManager.ChangeBuffer.NotEmpty())strategyManager.ChangeBuffer.Push(this,
                        new AvoidableRectanglesReportBuilder(pair, row, pair[0].Column));
                }


                if (strategyManager.Sudoku[row, pair[1].Column] == pair[0].Number &&
                    _originalBoard![row, pair[1].Column] == 0)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(pair[1].Number, row, pair[0].Column);
                    if(strategyManager.ChangeBuffer.NotEmpty())strategyManager.ChangeBuffer.Push(this,
                        new AvoidableRectanglesReportBuilder(pair, row, pair[1].Column));
                }
                
                //Type 2 and 3
                var possOne = strategyManager.Possibilities[row, pair[0].Column];
                var possTwo = strategyManager.Possibilities[row, pair[1].Column];
                if(!possOne.Peek(pair[1].Number) || !possTwo.Peek(pair[0].Number)) continue;

                //Type 2
                if (possOne.Count == 2 && possTwo.Count == 2)
                {
                    foreach (var possibility in possOne)
                    {
                        if (!possTwo.Peek(possibility)) continue;

                        foreach (var coord in CoordinateUtils.SharedSeenCells(row, pair[0].Column,
                                     row, pair[1].Column))
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
                        }

                        if (strategyManager.ChangeBuffer.NotEmpty()){
                            strategyManager.ChangeBuffer.Push(this, new AvoidableRectanglesWithBiValuesReportBuilder(pair));
                            return;
                        }
                    }
                }
                
                //Type 3
                var shared = new List<Coordinate>(CoordinateUtils.SharedSeenEmptyCells(strategyManager, row, pair[0].Column,
                    row, pair[1].Column));
                var mashed = possOne.Mash(possTwo);
                mashed.Remove(pair[0].Number);
                mashed.Remove(pair[1].Number);

                foreach (var als in AlmostLockedSet.SearchForAls(strategyManager, shared, 4))
                {
                    if (als.Possibilities.Equals(mashed))
                        RemovePossibilitiesInAllExcept(strategyManager, mashed, shared, als);
                    if (strategyManager.ChangeBuffer.NotEmpty())
                        strategyManager.ChangeBuffer.Push(this,
                            new AvoidableRectanglesWithBiValuesReportBuilder(pair));
                }
            }
        }
    }

    private List<SolvedNumber[]> RowSolvedNumberPairs(IStrategyManager strategyManager, int row)
    {
        List<SolvedNumber[]> result = new();

        for (int col = 0; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] != 0 && _originalBoard![row, col] == 0) 
                SearchRowForPairs(strategyManager, row, col + 1,
                    new SolvedNumber(strategyManager.Sudoku[row, col], row, col), result);
        }

        return result;
    }

    private void SearchRowForPairs(IStrategyManager strategyManager, int row, int start,
        SolvedNumber current, List<SolvedNumber[]> result)
    {
        for (int col = start; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] != 0 && _originalBoard![row, col] == 0)
            {
                result.Add(new []{current, new SolvedNumber(strategyManager.Sudoku[row, col], row, col)});
            }
        }
    }
    
    private void SearchForColumnMatch(IStrategyManager strategyManager, List<SolvedNumber[]> pairs, int except)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col == except) continue;

            foreach (var pair in pairs)
            {
                bool pairInSameBox = pair[0].Row / 3 == pair[1].Row / 3;
                bool colInSameBox = col / 3 == except / 3;

                if (!(pairInSameBox ^ colInSameBox)) continue;

                //Type 1
                if (strategyManager.Sudoku[pair[0].Row, col] == pair[1].Number &&
                    _originalBoard![pair[0].Row, col] == 0)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(pair[0].Number, pair[1].Row, col);
                    if(strategyManager.ChangeBuffer.NotEmpty())strategyManager.ChangeBuffer.Push(this,
                        new AvoidableRectanglesReportBuilder(pair, pair[0].Row, col));
                }


                if (strategyManager.Sudoku[pair[1].Row, col] == pair[0].Number &&
                    _originalBoard![pair[1].Row, col] == 0)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(pair[1].Number, pair[0].Row, col);
                    if(strategyManager.ChangeBuffer.NotEmpty())strategyManager.ChangeBuffer.Push(this,
                        new AvoidableRectanglesReportBuilder(pair, pair[1].Row, col));
                }
                
                //Type 2 and 3
                var possOne = strategyManager.Possibilities[pair[0].Row, col];
                var possTwo = strategyManager.Possibilities[pair[1].Row, col];
                if (!possOne.Peek(pair[1].Number) || !possTwo.Peek(pair[0].Number)) continue;

                //Type 2
                if (possOne.Count == 2 && possTwo.Count == 2)
                {
                    foreach (var possibility in possOne)
                    {
                        if (!possTwo.Peek(possibility)) continue;

                        foreach (var coord in CoordinateUtils.SharedSeenCells(pair[0].Row, col,
                                     pair[1].Row, col))
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
                        }

                        if (strategyManager.ChangeBuffer.NotEmpty()){
                            strategyManager.ChangeBuffer.Push(this, new AvoidableRectanglesWithBiValuesReportBuilder(pair));
                            return;
                        }
                    }
                }
                
                //Type 3
                var shared = new List<Coordinate>(CoordinateUtils.SharedSeenEmptyCells(strategyManager, pair[0].Row, col,
                    pair[1].Row, col));
                var mashed = possOne.Mash(possTwo);
                mashed.Remove(pair[0].Number);
                mashed.Remove(pair[1].Number);

                foreach (var als in AlmostLockedSet.SearchForAls(strategyManager, shared, 4))
                {
                    if (als.Possibilities.Equals(mashed))
                        RemovePossibilitiesInAllExcept(strategyManager, mashed, shared, als);
                    if (strategyManager.ChangeBuffer.NotEmpty())
                        strategyManager.ChangeBuffer.Push(this,
                            new AvoidableRectanglesWithBiValuesReportBuilder(pair));
                }
            }
        }
    }

    private List<SolvedNumber[]> ColumnSolvedNumberPairs(IStrategyManager strategyManager, int col)
    {
        List<SolvedNumber[]> result = new();

        for (int row = 0; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] != 0 && _originalBoard![row, col] == 0) 
                SearchColumnForPairs(strategyManager, col, row + 1,
                    new SolvedNumber(strategyManager.Sudoku[row, col], row, col), result);
        }

        return result;
    }

    private void SearchColumnForPairs(IStrategyManager strategyManager, int col, int start,
        SolvedNumber current, List<SolvedNumber[]> result)
    {
        for (int row = start; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] != 0 && _originalBoard![row, col] == 0)
            {
                result.Add(new []{current, new SolvedNumber(strategyManager.Sudoku[row, col], row, col)});
            }
        }
    }

    public void SetOriginalBoard(Sudoku board)
    {
        _originalBoard = board;
    }
    
    private void RemovePossibilitiesInAllExcept(IStrategyManager view, IPossibilities poss, List<Coordinate> coords,
        AlmostLockedSet except)
    {
        foreach (var coord in coords)
        {
            if(except.Contains(coord) || !except.ShareAUnit(coord)) continue;
            foreach (var possibility in poss)
            {
                view.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
            }
        }
    }
}

public readonly struct SolvedNumber
{
    public int Number { get; }
    public int Row { get; }
    public int Column { get; }
    
    public SolvedNumber(int number, int row, int column)
    {
        Number = number;
        Row = row;
        Column = column;
    }
}

public class AvoidableRectanglesReportBuilder : IChangeReportBuilder
{
    private readonly SolvedNumber[] _pair;
    private readonly int _row;
    private readonly int _col;

    public AvoidableRectanglesReportBuilder(SolvedNumber[] pair, int row, int col)
    {
        _pair = pair;
        _row = row;
        _col = col;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes),
            lighter =>
            {
                foreach (var single in _pair)
                {
                    lighter.HighlightCell(single.Row, single.Column, ChangeColoration.CauseOffOne);
                }
                lighter.HighlightCell(_row, _col, ChangeColoration.CauseOffOne);
                
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            }, "");
    }
}

public class AvoidableRectanglesWithBiValuesReportBuilder : IChangeReportBuilder
{
    private readonly SolvedNumber[] _pair;

    public AvoidableRectanglesWithBiValuesReportBuilder(SolvedNumber[] pair)
    {
        _pair = pair;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes),
            lighter =>
            {
                foreach (var single in _pair)
                {
                    lighter.HighlightCell(single.Row, single.Column, ChangeColoration.CauseOffOne);
                }
                
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            }, "");
    }
}