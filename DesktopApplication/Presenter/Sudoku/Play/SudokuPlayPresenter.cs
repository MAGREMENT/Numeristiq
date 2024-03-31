using System;
using System.Collections.Generic;
using System.Linq;
using Model.Sudoku.Player;
using Model.Sudoku.Player.Actions;
using Model.Utility;

namespace DesktopApplication.Presenter.Sudoku.Play;

public class SudokuPlayPresenter
{
    private readonly ISudokuPlayView _view;
    private readonly Settings _settings;
    private readonly SudokuPlayer _player;

    private readonly HashSet<Cell> _selectedCells = new();
    private ChangeLevel _changeLevel = ChangeLevel.Solution;
    
    public SettingsPresenter SettingsPresenter { get; }

    public SudokuPlayPresenter(ISudokuPlayView view, Settings settings)
    {
        _view = view;
        _settings = settings;
        _player = new SudokuPlayer();

        SettingsPresenter = new SettingsPresenter(_settings, SettingCollections.SudokuPlayPage);
    }

    public void SelectCell(Cell cell)
    {
        var wasContained = _selectedCells.Contains(cell);
        
        _selectedCells.Clear();
        if(!wasContained) _selectedCells.Add(cell);
        
        RefreshCursor();
    }

    public void AddCellToSelection(Cell cell)
    {
        if (_selectedCells.Add(cell)) RefreshCursor();
    }

    public void ActOnCurrentCells(int n)
    {
        if (_selectedCells.Count == 0) return;

        var action = _changeLevel switch
        {
            ChangeLevel.Solution => new SolutionSetAction(n),
            _ => throw new Exception()
        };
        
        if(_player.Execute(action, _selectedCells)) RefreshNumbers();
    }

    public void RemoveCurrentCells()
    {
        if (_selectedCells.Count == 0) return;

        var action = _changeLevel switch
        {
            ChangeLevel.Solution => new SolutionSetAction(0),
            _ => throw new Exception()
        };
        
        if(_player.Execute(action, _selectedCells)) RefreshNumbers();
    }

    private void RefreshCursor()
    {
        var drawer = _view.Drawer;
        switch (_selectedCells.Count)
        {
            case 0 : drawer.ClearCursor();
                break;
            case 1 : drawer.PutCursorOn(_selectedCells.First());
                break;
            default : drawer.PutCursorOn(_selectedCells);
                break;
        }
        
        drawer.Refresh();
    }

    private void RefreshNumbers()
    {
        var drawer = _view.Drawer;
        drawer.ClearNumbers();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var pc = _player[row, col];
                if(pc.IsNumber()) drawer.ShowSolution(row, col, pc.Number());
                else
                {
                    if(pc.PossibilitiesCount(PossibilitiesLocation.Top) > 0) drawer.ShowLinePossibilities(row,
                        col, pc.Possibilities(PossibilitiesLocation.Top), PossibilitiesLocation.Top);
                    
                    if(pc.PossibilitiesCount(PossibilitiesLocation.Middle) > 0) drawer.ShowLinePossibilities(row,
                        col, pc.Possibilities(PossibilitiesLocation.Middle), PossibilitiesLocation.Middle);
                    
                    if(pc.PossibilitiesCount(PossibilitiesLocation.Bottom) > 0) drawer.ShowLinePossibilities(row,
                        col, pc.Possibilities(PossibilitiesLocation.Bottom), PossibilitiesLocation.Bottom);
                }
            }
        }
        
        drawer.Refresh();
    }
}

public enum ChangeLevel
{
    Solution, PossibilitiesTop, PossibilitiesMiddle, PossibilitiesBottom
}