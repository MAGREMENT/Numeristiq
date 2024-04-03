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
        
        _view.SetChangeLevelOptions(EnumConverter.ToStringArray<ChangeLevel>(new SpaceConverter()), (int)_changeLevel);
        
        _settings.AddEvent(SpecificSettings.StartAngle, _ => RefreshHighlights());
        _settings.AddEvent(SpecificSettings.RotationDirection, _ => RefreshHighlights());

        SettingsPresenter = new SettingsPresenter(_settings, SettingCollections.SudokuPlayPage);

        _player.Timer.TimeElapsed += _view.SetTimeElapsed;
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

        IPlayerAction action = _changeLevel switch
        {
            ChangeLevel.Solution => new SolutionChangeAction(n),
            _ => new PossibilityChangeAction(n, ToLocation(_changeLevel))
        };

        if (_player.Execute(action, _selectedCells))
        {
            RefreshNumbers();
            _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
        }
    }

    public void RemoveCurrentCells()
    {
        if (_selectedCells.Count == 0) return;

        IPlayerAction action = _changeLevel switch
        {
            ChangeLevel.Solution => new SolutionChangeAction(0),
            _ => new PossibilityRemovalAction(ToLocation(_changeLevel))
        };

        if (_player.Execute(action, _selectedCells))
        {
            RefreshNumbers();
            _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
        }
    }

    public void HighlightCurrentCells(HighlightColor color)
    {
        if (_selectedCells.Count == 0) return;

        if (_player.Execute(new HighlightChangeAction(color), _selectedCells))
        {
            RefreshHighlights();
            _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
        }
    }

    public void ClearHighlightsFromCurrentCells()
    {
        if (_selectedCells.Count == 0) return;

        if (_player.Execute(new HighlightClearAction(), _selectedCells))
        {
            RefreshHighlights();
            _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
        }
    }

    public void SetChangeLevel(int index)
    {
        _changeLevel = (ChangeLevel)index;
    }

    public void Start()
    {
        _player.StartTimer();
        RefreshNumbers();
        _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
        _view.SetIsPlaying(_player.Timer.IsPlaying);
    }

    public void PlayOrPause()
    {
        if(_player.Timer.IsPlaying) _player.PauseTimer();
        else if (_player.PlayTimer())
        {
            RefreshNumbers();
            _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
        }
        _view.SetIsPlaying(_player.Timer.IsPlaying);
    }

    public void Stop()
    {
        _player.StopTimer();
        RefreshNumbers();
        _view.SetIsPlaying(_player.Timer.IsPlaying);
    }

    public void MoveBack()
    {
        _player.MoveBack();
        RefreshNumbers();
        RefreshHighlights();
        _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
    }

    public void MoveForward()
    {
        _player.MoveForward();
        RefreshNumbers();
        RefreshHighlights();
        _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
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
                drawer.SetClue(row, col, !pc.Editable);
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

    private void RefreshHighlights()
    {
        var drawer = _view.Drawer;
        drawer.ClearHighlights();

        foreach (var entry in _player.EnumerateHighlights())
        {
            drawer.FillCell(entry.Key.Row, entry.Key.Column, View.Utility.MathUtility.ToRadians(
                _settings.StartAngle), (int)_settings.RotationDirection, entry.Value.ToColorArray());
        }
        
        drawer.Refresh();
    }

    private static PossibilitiesLocation ToLocation(ChangeLevel level)
    {
        return level switch
        {
            ChangeLevel.PossibilitiesBottom => PossibilitiesLocation.Bottom,
            ChangeLevel.PossibilitiesMiddle => PossibilitiesLocation.Middle,
            ChangeLevel.PossibilitiesTop => PossibilitiesLocation.Top,
            _ => throw new Exception()
        };
    }
}

public enum ChangeLevel
{
    Solution, PossibilitiesTop, PossibilitiesMiddle, PossibilitiesBottom
}