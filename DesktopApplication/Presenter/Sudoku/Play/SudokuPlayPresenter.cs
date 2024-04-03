using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.Helpers;
using Model.Sudoku;
using Model.Sudoku.Player;
using Model.Sudoku.Player.Actions;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace DesktopApplication.Presenter.Sudoku.Play;

public class SudokuPlayPresenter
{
    private readonly ISudokuPlayView _view;
    private readonly SudokuSolver _solver;
    private readonly Settings _settings;
    private readonly SudokuPlayer _player;

    private readonly HashSet<Cell> _selectedCells = new();
    private ChangeLevel _changeLevel = ChangeLevel.Solution;
    
    public SettingsPresenter SettingsPresenter { get; }

    public SudokuPlayPresenter(ISudokuPlayView view, SudokuSolver solver, Settings settings)
    {
        _view = view;
        _solver = solver;
        _settings = settings;
        _player = new SudokuPlayer();
        
        _view.SetChangeLevelOptions(EnumConverter.ToStringArray<ChangeLevel>(new SpaceConverter()), (int)_changeLevel);
        
        _player.MainLocation = _settings.MainLocation;
        
        _settings.AddEvent(SpecificSettings.StartAngle, _ => RefreshHighlights());
        _settings.AddEvent(SpecificSettings.RotationDirection, _ => RefreshHighlights());
        _settings.AddEvent(SpecificSettings.MainLocation, v => _player.MainLocation = (PossibilitiesLocation)v.ToInt());

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

    public async void GetClue()
    {
        var sudoku = SudokuTranslator.TranslateSolvingState(_player);
        var list = await Task.Run(() => BackTracking.Fill(sudoku, _player, 1));
        if (list.Length == 0)
        {
            _view.ShowClue(new SudokuClue("The current sudoku has no solution"));
            return;
        }

        _solver.SetState(_player);
        var clue = await Task.Run(() => _solver.NextClue());
        _view.ShowClue(clue);
    }
    
    public void Paste(string s)
    {
        if(!_settings.OpenPasteDialog) Paste(s, _settings.DefaultPasteFormat);
        else _view.OpenOptionDialog("Paste", i =>
        {
            Paste(s, (SudokuStringFormat)i);
        }, EnumConverter.ToStringArray<SudokuStringFormat>(SpaceConverter.Instance));
    }
    
    private void Paste(string s, SudokuStringFormat format)
    {
        ISolvingState state = format switch
        {
            SudokuStringFormat.Line => SudokuTranslator.TranslateLineFormat(s),
            SudokuStringFormat.Grid => SudokuTranslator.TranslateGridFormat(s, _settings.SoloToGiven),
            SudokuStringFormat.Base32 => SudokuTranslator.TranslateBase32Format(s, new AlphabeticalBase32Translator()),
            _ => throw new Exception()
        };

        if (_player.Execute(new PasteAction(state, _player.MainLocation)))
        {
            RefreshNumbers();
            _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
        }
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
                var pc = _player.GetCellDataFor(row, col);
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
            ChangeLevel.BottomPossibilities => PossibilitiesLocation.Bottom,
            ChangeLevel.MiddlePossibilities => PossibilitiesLocation.Middle,
            ChangeLevel.TopPossibilities => PossibilitiesLocation.Top,
            _ => throw new Exception()
        };
    }
}

public enum ChangeLevel
{
    Solution, TopPossibilities, MiddlePossibilities, BottomPossibilities
}