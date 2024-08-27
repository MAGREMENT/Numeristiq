using System.Linq;
using DesktopApplication.Presenter.Tectonics.Solve;
using Model.Utility;
using Model.Utility.Collections;
using Model.YourPuzzles;
using Model.YourPuzzles.Rules;

namespace DesktopApplication.Presenter.YourPuzzles;

public class YourPuzzlePresenter
{
    private readonly NumericYourPuzzle _puzzle = new(5, 5);
    private readonly NumericRuleBank _bank = new();
    private readonly UniqueList<Cell> _selected = new();
    private readonly IYourPuzzleView _view;

    public YourPuzzlePresenter(IYourPuzzleView view)
    {
        _view = view;
        FullUpdate();
    }

    public void SetRowCount(int diff)
    {
        _puzzle.ChangeSize(_puzzle.RowCount + diff, _puzzle.ColumnCount);
        FullUpdate();
    }

    public void SetColumnCount(int diff)
    {
        _puzzle.ChangeSize(_puzzle.RowCount, _puzzle.ColumnCount + diff);
        FullUpdate();
    }

    public void SelectCell(Cell cell)
    {
        if (_selected.Contains(cell))
        {
            _selected.Clear();
            _view.Drawer.ClearCursor();
        }
        else
        {
            _selected.Clear();
            _selected.Add(cell);
            _view.Drawer.PutCursorOn(_selected);
        }
        
        _view.Drawer.Refresh();
        UpdateBank();
    }

    public void AddCellToSelection(Cell cell)
    {
        _selected.Add(cell);
        _view.Drawer.PutCursorOn(_selected);
        _view.Drawer.Refresh();
        UpdateBank();
    }

    public void AddRule(int index, bool isGlobal)
    {
        if (isGlobal) _puzzle.AddRuleUnchecked(_bank.Craft(index));
        else _puzzle.AddRuleUnchecked(_bank.Craft(index, _selected));
        
        FullUpdate();
    }

    public void RemoveRule(int index, bool isGlobal)
    {
        _puzzle.RemoveRule(index, isGlobal);
        FullUpdate();
    }

    private void FullUpdate()
    {
        ShowPuzzle();
        UpdateRules();
        UpdateBank();
    }

    private void ShowPuzzle()
    {
        var drawer = _view.Drawer;

        drawer.RowCount = _puzzle.RowCount;
        drawer.ColumnCount = _puzzle.ColumnCount;
        
        drawer.ClearBorderDefinitions();
        drawer.ClearGreaterThanSigns();
        foreach (var rule in _puzzle.LocalRules)
        {
            switch (rule)
            {
                case UniqueBatchNumericPuzzleRule ub :
                    foreach (var cell in ub.Cells)
                    {
                        if (ub.Cells.Contains(new Cell(cell.Row + 1, cell.Column)))
                            drawer.AddBorderDefinition(cell.Row, cell.Column, BorderDirection.Horizontal, true);
                        if (ub.Cells.Contains(new Cell(cell.Row, cell.Column + 1)))
                            drawer.AddBorderDefinition(cell.Row, cell.Column, BorderDirection.Vertical, true);
                    }
                    break;
                case GreaterThanNumericPuzzleRule gt :
                    drawer.AddGreaterThanSign(gt.Smaller, gt.Greater);
                    break;
            }
        }
        
        drawer.Refresh();
    }
    
    private void UpdateRules()
    {
        _view.ClearCurrentRules();
        for (int i = 0; i < _puzzle.GlobalRules.Count; i++)
        {
            _view.AddCurrentRule(_puzzle.GlobalRules[i], i, true);
        }
        
        for (int i = 0; i < _puzzle.LocalRules.Count; i++)
        {
            _view.AddCurrentRule(_puzzle.LocalRules[i], i, false);
        }
    }

    private void UpdateBank()
    {
        _view.ClearRulesInBank();
        foreach (var result in _bank.SearchFor(_puzzle, _selected))
        {
            _view.AddRuleInBank(result);
        }
    }
}