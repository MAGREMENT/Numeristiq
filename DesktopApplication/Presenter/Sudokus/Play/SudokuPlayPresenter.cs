using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesktopApplication.Presenter.Sudokus.Solve;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus;
using Model.Sudokus.Player;
using Model.Sudokus.Player.Actions;
using Model.Sudokus.Solver;
using Model.Utility;

namespace DesktopApplication.Presenter.Sudokus.Play;

public class SudokuPlayPresenter
{
    private readonly ISudokuPlayView _view;
    private readonly SudokuSolver _solver;
    private readonly Settings _settings;
    private readonly SudokuPlayer _player;
    private readonly SudokuHighlighterTranslator _translator;
    private readonly Disabler _disabler;

    private ISudokuPlayerCursor _cursor = new CellsCursor();
    private ChangeLevel _changeLevel = ChangeLevel.Solution;
    private bool _isClueShowing;
    private Clue<ISudokuHighlighter>? _clueBuffer;
    
    public SettingsPresenter SettingsPresenter { get; }

    public SudokuPlayPresenter(ISudokuPlayView view, SudokuSolver solver, Settings settings)
    {
        _view = view;
        _solver = solver;
        _settings = settings;
        _player = new SudokuPlayer();
        _translator = new SudokuHighlighterTranslator(_view.ClueShower, _settings);
        _disabler = new Disabler(_view);
        
        _view.SetChangeLevelOptions(EnumConverter.ToStringArray<ChangeLevel>(new SpaceConverter()), (int)_changeLevel);
        
        _player.MainLocation = _settings.MainLocation;
        
        _settings.AddEvent(SpecificSettings.StartAngle, _ => RefreshHighlights());
        _settings.AddEvent(SpecificSettings.RotationDirection, _ => RefreshHighlights());
        _settings.AddEvent(SpecificSettings.MainLocation, v => _player.MainLocation = (PossibilitiesLocation)v.ToInt());

        SettingsPresenter = new SettingsPresenter(_settings, SettingCollections.SudokuPlayPage);

        _player.Timer.TimeElapsed += _view.SetTimeElapsed;

        App.Current.ThemeInformation.ThemeChanged += () =>
        {
            RefreshHighlights();
            _view.InitializeHighlightColorBoxes();
        };
    }

    public void SelectAllPossibilities(int p)
    {
        if (_cursor is AllPossibilitiesCursor aps && aps.Possibility == p)
        {
            EnforceCellCursor(out var _);
            return;
        }
        
        var buffer = new AllPossibilitiesCursor(p, new HashSet<Cell>(), _player.MainLocation);
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (_player.GetCellDataFor(row, col).PeekPossibility(p, buffer.Location))
                    buffer.Cells.Add(new Cell(row, col));
            }
        }

        _cursor = buffer;
        _view.Drawer.ClearCursor();
        RefreshCursor();
    }

    public void SelectCell(Cell cell)
    {
        EnforceCellCursor(out var set);
        var wasContained = set.Contains(cell);
        
        set.Clear();
        if(!wasContained) set.Add(cell);
        
        RefreshCursor();
    }

    public void AddCellToSelection(Cell cell)
    {
        if (EnforceCellCursor(out var set) && set.Add(cell)) RefreshCursor();
    }

    public void ActOnCurrentCells(int n)
    {
        if (_cursor is not CellsCursor set || set.Count == 0) return;

        ICellAction action = _changeLevel switch
        {
            ChangeLevel.Solution => new SolutionChangeAction(n),
            _ => new PossibilityChangeAction(n, ToLocation(_changeLevel))
        };

        if (_player.Execute(action, set)) OnCellDataChange();
    }

    public void RemoveCurrentCells()
    {
        if (_cursor is not CellsCursor set || set.Count == 0) return;

        ICellAction action = _changeLevel switch
        {
            ChangeLevel.Solution => new SolutionChangeAction(0),
            _ => new PossibilityRemovalAction(ToLocation(_changeLevel))
        };

        if (_player.Execute(action, set)) OnCellDataChange();
    }

    public void HighlightCurrentCells(HighlightColor color)
    {
        switch (_cursor)
        {
            case CellsCursor set :
                if (set.Count == 0) return;
                if (_player.Execute(new CellHighlightChangeAction(color), set))
                {
                    RefreshHighlights();
                    _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
                }
                break;
            case AllPossibilitiesCursor aps :
                if (aps.Cells.Count == 0) return;
                if (_player.Execute(new CellPossibilityHighlightChangeAction(color, _player.MainLocation,
                        aps.Possibility), aps.Cells))
                {
                    RefreshNumbers();
                    _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
                }
                break;
        }
    }

    public void ClearHighlightsFromCurrentCells()
    {
        if (_cursor is not CellsCursor set || set.Count == 0) return;

        if (_player.Execute(new HighlightClearAction(), set))
        {
            RefreshHighlights();
            _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
        }
    }

    public void SetChangeLevel(int index)
    {
        _changeLevel = (ChangeLevel)index;
        _view.FocusDrawer();
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
        RefreshHighlights();
        OnCellDataChange();
    }

    public void MoveForward()
    {
        _player.MoveForward();
        RefreshHighlights();
        OnCellDataChange();
    }
    
    public void Paste(string s)
    {
        if(!_settings.OpenPasteDialog) Paste(s, _settings.DefaultPasteFormat);
        else _view.OpenOptionDialog("Paste", i =>
        {
            Paste(s, (SudokuStringFormat)i);
        }, EnumConverter.ToStringArray<SudokuStringFormat>(SpaceConverter.Instance));
    }

    public void ComputePossibilities()
    {
        if(_player.Execute(new ComputePossibilitiesAction(_player.MainLocation))) OnCellDataChange();
    }

    public void ChangeClueState()
    {
        if (_isClueShowing) HideClue(false);
        else ShowClue();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="set"></param>
    /// <returns>True if the cursor was already a cell cursor</returns>
    private bool EnforceCellCursor(out CellsCursor set)
    {
        if (_cursor is CellsCursor s)
        {
            set = s;
            return false;
        }
        
        set = new CellsCursor();
        _cursor = set;
        RefreshNumbers();
        return true;
    }
    
    private void Paste(string s, SudokuStringFormat format)
    {
        INumericSolvingState state = format switch
        {
            SudokuStringFormat.Line => SudokuTranslator.TranslateLineFormat(s),
            SudokuStringFormat.Grid => SudokuTranslator.TranslateGridFormat(s, _settings.SoloToGiven),
            SudokuStringFormat.Base32 => SudokuTranslator.TranslateBase32Format(s, new AlphabeticalBase32Translator()),
            _ => throw new Exception()
        };

        if (_player.Execute(new PasteAction(state, _player.MainLocation))) OnCellDataChange();
    }

    private void OnCellDataChange()
    {
        RefreshNumbers();
        _view.SetHistoricAvailability(_player.CanMoveBack(), _player.CanMoveForward());
        HideClue(true);
    }
    
    private async Task<Clue<ISudokuHighlighter>?> GetClue()
    {
        var sudoku = SudokuTranslator.TranslateSolvingState(_player);
        if (sudoku.NumberCount() < 17) return new Clue<ISudokuHighlighter>("Not enough numbers in the Sudoku");

        _disabler.Disable(1);
        if (_settings.TestSolutionCount)
        {
            var count = await Task.Run(() => BackTracking.Count(sudoku, _player, 1));
            if (count == 0) return new Clue<ISudokuHighlighter>("The current sudoku has no solution"); 
        }
        
        _solver.SetState(_player);
        var result = await Task.Run(() => _solver.NextClue());
        
        _disabler.Enable(1);
        return result;
    }

    private async void ShowClue()
    {
        if (_isClueShowing) return;

        _isClueShowing = true;
        _clueBuffer ??= await GetClue();

        if (_clueBuffer is null)
        {
            _view.ShowClueText("No clue found");
        }
        else
        {
            _view.ShowClueText(_clueBuffer.Text);
            _translator.Translate(_clueBuffer);
        }
        
        _view.ShowClueState(_isClueShowing);
    }

    private void HideClue(bool removeBuffer)
    {
        if (removeBuffer) _clueBuffer = null;
        if (!_isClueShowing) return;
        
        _isClueShowing = false;
        _view.ShowClueText("");
        RefreshHighlights();
        _view.ShowClueState(_isClueShowing);
    }
    
    private void RefreshCursor()
    {
        var drawer = _view.Drawer;

        switch (_cursor)
        {
            case CellsCursor set :
                switch (set.Count)
                {
                    case 0 : 
                        drawer.ClearCursor();
                        break;
                    case 1 : drawer.PutCursorOn(set.First());
                        break;
                    default : drawer.PutCursorOn(set);
                        break;
                }
                break;
            case AllPossibilitiesCursor :
                RefreshNumbers();
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
                var hd = _player.GetHighlightsFor(new Cell(row, col));
                var cell = new Cell(row, col);
                
                drawer.SetClue(row, col, !pc.Editable);
                if(pc.IsNumber()) drawer.ShowSolution(row, col, pc.Number());
                else
                {
                    if(pc.PossibilitiesCount(PossibilitiesLocation.Top) > 0) 
                    {
                        if (_cursor.ShouldHaveOutline(cell, PossibilitiesLocation.Top, out var p))
                            drawer.ShowLinePossibilities(row, col, pc.Possibilities(PossibilitiesLocation.Top),
                                PossibilitiesLocation.Top, hd.CellPossibilitiesColorToEnumerable(PossibilitiesLocation.Top),
                                p);
                        else drawer.ShowLinePossibilities(row, col, pc.Possibilities(PossibilitiesLocation.Top),
                            PossibilitiesLocation.Top, hd.CellPossibilitiesColorToEnumerable(PossibilitiesLocation.Top));
                    }
                    
                    if(pc.PossibilitiesCount(PossibilitiesLocation.Middle) > 0)
                    {
                        if (_cursor.ShouldHaveOutline(cell, PossibilitiesLocation.Middle, out var p))
                            drawer.ShowLinePossibilities(row, col, pc.Possibilities(PossibilitiesLocation.Middle),
                                PossibilitiesLocation.Middle, hd.CellPossibilitiesColorToEnumerable(PossibilitiesLocation.Middle),
                                p);
                        else drawer.ShowLinePossibilities(row, col, pc.Possibilities(PossibilitiesLocation.Middle),
                            PossibilitiesLocation.Middle, hd.CellPossibilitiesColorToEnumerable(PossibilitiesLocation.Middle));
                    }
                    
                    if(pc.PossibilitiesCount(PossibilitiesLocation.Bottom) > 0)
                    {
                        if (_cursor.ShouldHaveOutline(cell, PossibilitiesLocation.Bottom, out var p))
                            drawer.ShowLinePossibilities(row, col, pc.Possibilities(PossibilitiesLocation.Bottom),
                                PossibilitiesLocation.Bottom, hd.CellPossibilitiesColorToEnumerable(PossibilitiesLocation.Bottom),
                                p);
                        else drawer.ShowLinePossibilities(row, col, pc.Possibilities(PossibilitiesLocation.Bottom), 
                            PossibilitiesLocation.Bottom, hd.CellPossibilitiesColorToEnumerable(PossibilitiesLocation.Bottom));
                    }
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
                _settings.StartAngle), (int)_settings.RotationDirection, entry.Value.CellColorsToArray());
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

public interface ISudokuPlayerCursor
{
    bool ShouldHaveOutline(Cell cell, PossibilitiesLocation location, out int p);
}

public class CellsCursor : HashSet<Cell>, ISudokuPlayerCursor
{
    public bool ShouldHaveOutline(Cell cell, PossibilitiesLocation location, out int p)
    {
        p = 0;
        return false;
    }
}

public class AllPossibilitiesCursor : ISudokuPlayerCursor
{
    public AllPossibilitiesCursor(int possibility, HashSet<Cell> cells, PossibilitiesLocation location)
    {
        Possibility = possibility;
        Cells = cells;
        Location = location;
    }

    public int Possibility { get; }
    public HashSet<Cell> Cells { get; }
    public PossibilitiesLocation Location { get; }

    public bool ShouldHaveOutline(Cell cell, PossibilitiesLocation location, out int p)
    {
        if (location == Location && Cells.Contains(cell))
        {
            p = Possibility;
            return true;
        }

        p = 0;
        return false;
    }
        
}