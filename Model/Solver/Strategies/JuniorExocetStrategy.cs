using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class JuniorExocetStrategy : IStrategy //TODO for columns & other elims
{
    public const string OfficialName = "Junior Exocet";
    
    public string Name => OfficialName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Hard;
    public StatisticsTracker Tracker { get; } = new();

    private readonly int _max;

    public JuniorExocetStrategy(int max)
    {
        _max = max;
    }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        //Rows
        for (int row = 0; row < 9; row++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                for (int m1 = 0; m1 < 3; m1++)
                {
                    int col1 = gridCol * 3 + m1;
                    var poss1 = strategyManager.PossibilitiesAt(row, col1);
                    if (poss1.Count < 2 || poss1.Count > _max) continue;

                    for (int m2 = m1 + 1; m2 < 3; m2++)
                    {
                        int col2 = gridCol * 3 + m2;
                        var poss2 = strategyManager.PossibilitiesAt(row, col2);
                        if (poss2.Count < 2 || poss1.Count > _max) continue; //TODO look into removing PeekAny()

                        var or = poss1.Or(poss2);
                        if(or.Count > _max) continue;
                        
                        //Base cells found, search for target cells
                        var gridCols = GridUnitsExcept(gridCol);
                        var rows = UnitsInGridExcept(row);

                        for (int m3 = 0; m3 < 3; m3++)
                        {
                            var col3 = gridCols[0] * 3 + m3;
                            var pos3 = strategyManager.PossibilitiesAt(rows[0], col3);
                            if(pos3.Count == 0) continue;

                            for (int m4 = 0; m4 < 3; m4++)
                            {
                                var col4 = gridCols[1] * 3 + m4;
                                var pos4 = strategyManager.PossibilitiesAt(rows[1], col4);
                                if (pos4.Count == 0 || !pos3.Or(pos4).PeekAll(or)) continue;
                                
                                ConfirmPatternInRow(strategyManager, new Cell(row, col1),
                                    new Cell(row, col2), new Cell(rows[0], col3),
                                    new Cell(rows[1], col4), or);
                            }
                        }

                        for (int m3 = 0; m3 < 3; m3++)
                        {
                            var col3 = gridCols[1] * 3 + m3;
                            var pos3 = strategyManager.PossibilitiesAt(rows[0], col3);
                            if(pos3.Count == 0) continue;

                            for (int m4 = 0; m4 < 3; m4++)
                            {
                                var col4 = gridCols[0] * 3 + m4;
                                var pos4 = strategyManager.PossibilitiesAt(rows[1], col4);
                                if (pos4.Count == 0 || !pos3.Or(pos4).PeekAll(or)) continue;
                                
                                ConfirmPatternInRow(strategyManager, new Cell(row, col1),
                                    new Cell(row, col2), new Cell(rows[0], col3),
                                    new Cell(rows[1], col4), or);
                            }
                        }
                    }
                }
            }
        }
    }

    private int[] GridUnitsExcept(int i)
    {
        var cursor = 0;
        int[] result = new int[2];

        for (int n = 0; n < 3; n++)
        {
            if (n == i) continue;
            
            result[cursor] = n;
            cursor++;
        }

        return result;
    }

    private int[] UnitsInGridExcept(int i)
    {
        var start = i / 3 * 3;
        var cursor = 0;
        int[] result = new int[2];
        
        for (int n = 0; n < 3; n++)
        {
            var u = start + n;
            if (u == i) continue;
            
            result[cursor] = u;
            cursor++;
        }

        return result;
    }

    private int UnitInGridExcept(int i1, int i2)
    {
        int start = i1 / 3 * 3;

        for (int n = 0; n < 3; n++)
        {
            var u = start + n;
            if (u == i1 || u == i2) continue;

            return u;
        }

        return -1;
    }

    private int SolvedRowPositionsAt(IStrategyManager strategyManager, int col, int n)
    {
        for (int row = 0; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] == n) return row;
        }

        return -1;
    }

    private int SolvedColumnPositionsAt(IStrategyManager strategyManager, int row, int n)
    {
        for (int col = 0; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] == n) return col;
        }

        return -1;
    }

    private void ConfirmPatternInRow(IStrategyManager strategyManager, Cell base1, Cell base2, Cell target1,
        Cell target2, IPossibilities or)
    {
        //Check if companions doesn't have base candidates
        if (strategyManager.PossibilitiesAt(target1.Row, target2.Col).PeekAny(or)) return;
        if (strategyManager.PossibilitiesAt(target2.Row, target1.Col).PeekAny(or)) return;
        
        //Check if mirror cells can contain at least one base candidate
        var startCol1 = target1.Col / 3 * 3;
        var startCol2 = target2.Col / 3 * 3;
        bool ok1 = false;
        bool ok2 = false;

        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            var col1 = startCol1 + gridCol;
            var col2 = startCol2 + gridCol;

            if (col1 != target1.Col)
            {
                var solved = strategyManager.Sudoku[target1.Row, col1];
                if (solved != 0)
                {
                    if (or.Peek(solved)) ok1 = true;
                } 
                else if(strategyManager.PossibilitiesAt(target1.Row, col1).PeekAny(or)) ok1 = true;
            }

            if (col2 != target2.Col)
            {
                var solved = strategyManager.Sudoku[target2.Row, col2];
                if (solved != 0)
                {
                    if (or.Peek(solved)) ok2 = true;
                }
                else if(strategyManager.PossibilitiesAt(target2.Row, col2).PeekAny(or)) ok2 = true;
            }
        }

        if (!ok1 || !ok2) return;
        
        //Check that each base digit is in max 2 cover houses (2 rows, 2 cols, 1 row & 1 col) in the S cells
        var miniRow = base1.Row / 3;
        var eCol = UnitInGridExcept(base1.Col, base2.Col);
        foreach (var possibility in or)
        {
            var s1 = strategyManager.ColumnPositionsAt(target1.Col, possibility).Copy();
            s1.VoidMiniGrid(miniRow);
            var solved = SolvedRowPositionsAt(strategyManager, target1.Col, possibility);
            if(solved != -1) s1.Add(solved);
            
            var s2 = strategyManager.ColumnPositionsAt(target2.Col, possibility).Copy();
            s2.VoidMiniGrid(miniRow);
            solved = SolvedRowPositionsAt(strategyManager, target2.Col, possibility);
            if(solved != -1) s2.Add(solved);
            
            var se = strategyManager.ColumnPositionsAt(eCol, possibility).Copy();
            se.VoidMiniGrid(miniRow);
            solved = SolvedRowPositionsAt(strategyManager, eCol, possibility);
            if(solved != -1) se.Add(solved);
            
            //2 cols
            if (se.Count == 0 || s1.Count == 0 || s2.Count == 0) continue;
            
            //2 rows
            if(s1.Or(s2).Or(se).Count <= 2) continue;
            
            //1 row & 1 col
            if((se.Count <= 1 && s1.Count <= 1 && se.Or(s1).Count == 1) 
               || (s1.Count <= 1 && s2.Count <= 1 && s1.Or(s2).Count == 1) 
               || (se.Count <= 1 && s2.Count <= 1 && se.Or(s2).Count == 1)) continue;

            return;
        }
        
        //Valid pattern found
        Process(strategyManager, base1, base2, target1, target2, or, eCol, Unit.Row);
    }

    private void Process(IStrategyManager strategyManager, Cell base1, Cell base2, Cell target1,
        Cell target2, IPossibilities or, int eCol, Unit unit)
    {
        //Rule 1
        var miniRow = base1.Row / 3;
        foreach (var possibility in or)
        {
            var s1 = strategyManager.ColumnPositionsAt(target1.Col, possibility).Copy();
            s1.VoidMiniGrid(miniRow);
            var solved = SolvedRowPositionsAt(strategyManager, target1.Col, possibility);
            if(solved != -1) s1.Add(solved);
            
            var s2 = strategyManager.ColumnPositionsAt(target2.Col, possibility).Copy();
            s2.VoidMiniGrid(miniRow);
            solved = SolvedRowPositionsAt(strategyManager, target2.Col, possibility);
            if(solved != -1) s2.Add(solved);
            
            var se = strategyManager.ColumnPositionsAt(eCol, possibility).Copy();
            se.VoidMiniGrid(miniRow);
            solved = SolvedRowPositionsAt(strategyManager, eCol, possibility);
            if(solved != -1) se.Add(solved);

            if ((s1.Count > 0 && s2.Count == 0 && se.Count == 0)
                || (s1.Count == 0 && s2.Count > 0 && se.Count == 0)
                || (s1.Count == 0 && s2.Count == 0 && se.Count > 0)
                || s1.Or(s2).Or(se).Count == 1)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, target1.Row, target1.Col);
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, target2.Row, target2.Col);
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, base1.Row, base1.Col);
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, base2.Row, base2.Col);
                if (unit == Unit.Row) strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, base1.Row,
                        UnitInGridExcept(base1.Col, base2.Col));
                else strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility,
                        UnitInGridExcept(base1.Row, base2.Row), base1.Col);
            }
        }
        
        //Rule 3
        for (int possibility = 1; possibility <= 9; possibility++)
        {
            if(or.Peek(possibility)) continue;
            
            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, target1.Row, target1.Col);
            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, target2.Row, target2.Col);
        }

        if (strategyManager.ChangeBuffer.NotEmpty())
            strategyManager.ChangeBuffer.Push(this,
                new ExocetReportBuilder(base1, base2, target1, target2, or, eCol, unit));
    }
}

public class ExocetReportBuilder : IChangeReportBuilder
{
    private readonly Cell _base1;
    private readonly Cell _base2;
    private readonly Cell _target1;
    private readonly Cell _target2;
    private readonly IPossibilities _or;
    private readonly int _eUnit;
    private readonly Unit _unit;

    public ExocetReportBuilder(Cell base1, Cell base2, Cell target1, Cell target2, IPossibilities or, int eUnit, Unit unit)
    {
        _base1 = base1;
        _base2 = base2;
        _target1 = target1;
        _target2 = target2;
        _or = or;
        _eUnit = eUnit;
        _unit = unit;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot) //Add see solved possibility in s cells
    {
        List<Cell> sCells = new();
        if (_unit == Unit.Row)
        {
            for (int row = 0; row < 9; row++)
            {
                if (row / 3 == _base1.Row / 3) continue;
                
                sCells.Add(new Cell(row, _target1.Col));
                sCells.Add(new Cell(row, _target2.Col));
                sCells.Add(new Cell(row, _eUnit));
            }
        }
        else
        {
            for (int col = 0; col < 9; col++)
            {
                if (col / 3 == _base1.Col / 3) continue;
                
                sCells.Add(new Cell(_target1.Row, col));
                sCells.Add(new Cell(_target2.Row, col));
                sCells.Add(new Cell(_eUnit, col));
            }
        }

        List<CellPossibility> sPossibilities = new();
        foreach (var cell in sCells)
        {
            foreach (var possibility in _or)
            {
                if(snapshot.PossibilitiesAt(cell).Peek(possibility)) sPossibilities.Add(new CellPossibility(cell, possibility));
            }
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_base1, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_base2, ChangeColoration.CauseOffOne);
            
            lighter.HighlightCell(_target1, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_target2, ChangeColoration.CauseOffTwo);

            foreach (var cell in sCells)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffThree);
            }

            foreach (var cp in sPossibilities)
            {
                lighter.HighlightPossibility(cp, ChangeColoration.Neutral);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}