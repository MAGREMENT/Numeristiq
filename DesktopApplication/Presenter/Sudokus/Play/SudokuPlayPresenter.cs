using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesktopApplication.Presenter.Sudokus.Solve;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Repositories;
using Model.Sudokus;
using Model.Sudokus.Player;
using Model.Sudokus.Player.Actions;
using Model.Sudokus.Solver;
using Model.Utility;
using Model.Utility.Collections;
using Repository;

namespace DesktopApplication.Presenter.Sudokus.Play;

public class SudokuPlayPresenter
{
    private readonly ISudokuPlayView _view;
    private readonly SudokuSolver _solver;
    private readonly Settings _settings;
    private readonly SudokuPlayer _player;
    private readonly SudokuBackTracker _backTracker;
    private readonly SudokuHighlighterTranslator _translator;
    private readonly Disabler _disabler;
    private readonly ISudokuBankRepository _repository;

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
        _backTracker = new SudokuBackTracker(new Sudoku(), _player)
        {
            StopAt = 1
        };
        _translator = new SudokuHighlighterTranslator(_view.ClueShower, _settings);
        _disabler = new Disabler(_view);
        _repository = new MySqlSudokuBankRepository();
        
        _player.MainLocation = _settings.MainLocation;
        _view.Drawer.StartAngle = View.Utility.MathUtility.ToRadians(_settings.StartAngle);
        _view.Drawer.RotationFactor = (int)_settings.RotationDirection;

        _settings.MainLocationSetting.ValueChanged += v => _player.MainLocation = (PossibilitiesLocation)v.ToInt();
        _settings.StartAngleSetting.ValueChanged += v =>
        {
            _view.Drawer.StartAngle = View.Utility.MathUtility.ToRadians(v.ToDouble());
            _view.Drawer.Refresh();
        };
        _settings.RotationDirectionSetting.ValueChanged += v =>
        {
            _view.Drawer.RotationFactor = v.ToInt();
            _view.Drawer.Refresh();
        };

        SettingsPresenter = new SettingsPresenter(_settings, SettingCollections.SudokuPlayPage);

        _player.Timer.TimeElapsed += _view.SetTimeElapsed;

        App.Current.ThemeInformation.ThemeChanged += () =>
        {
            _view.Drawer.Refresh();
            _view.InitializeHighlightColorBoxes();
        };
    }

    public bool SelectAllPossibilities(int p)
    {
        if (_cursor is AllPossibilitiesCursor aps && aps.Possibility == p)
        {
            EnforceCellCursor(out _);
            return false;
        }

        var cells = _cursor is CellsCursor cc && cc.Count != 0 ? cc : null;
        _cursor = new AllPossibilitiesCursor(p, cells, _player.MainLocation);
        _view.Drawer.ClearCursor();
        RefreshCursor();

        return true;
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
                if (_player.Execute(new CellPossibilityHighlightChangeAction(color, _player.MainLocation,
                        aps.Possibility), aps.EnumerateCells(_player)))
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

    public void SetChangeLevel(ChangeLevel level)
    {
        _changeLevel = level;
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

    public void LoadFromBank(Difficulty difficulty)
    {
        try
        {
            var s = _repository.FindRandom(difficulty);
            if (s is null) return;

            if (_player.Execute(new PasteAction(s, _player.MainLocation))) OnCellDataChange();
        }
        catch
        {
            //ignored
        }
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
            return true;
        }
        
        set = new CellsCursor();
        _cursor = set;
        RefreshNumbers();
        return false;
    }
    
    private void Paste(string s, SudokuStringFormat format)
    {
        var state = format switch
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
        if (sudoku.GetSolutionCount() < 17) return new Clue<ISudokuHighlighter>("Not enough numbers in the Sudoku");

        _disabler.Disable(1);
        if (_settings.TestSolutionCount)
        {
            var count = await Task.Run(() =>
            {
                _backTracker.Set(sudoku);
                return _backTracker.Count();
            });
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
            _translator.Translate(_clueBuffer, false);
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
                
                _view.UnselectPossibilityCursor();
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
            drawer.FillCell(entry.Key.Row, entry.Key.Column, entry.Value.CellColorsToArray());
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

public class CellsCursor : ContainingHashSet<Cell>, ISudokuPlayerCursor
{
    public bool ShouldHaveOutline(Cell cell, PossibilitiesLocation location, out int p)
    {
        p = 0;
        return false;
    }
}

public class AllPossibilitiesCursor : ISudokuPlayerCursor
{
    private readonly PossibilitiesLocation _location;
    private readonly HashSet<Cell>? _cells;
    
    public int Possibility { get; }
    
    public AllPossibilitiesCursor(int possibility, HashSet<Cell>? cells, PossibilitiesLocation location)
    {
        Possibility = possibility;
        _cells = cells;
        _location = location;
    }

    public IEnumerable<Cell> EnumerateCells(SudokuPlayer player)
    {
        if (_cells is null)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (player.GetCellDataFor(row, col).PeekPossibility(Possibility, player.MainLocation))
                        yield return new Cell(row, col);
                }
            }
        }
        else
        {
            foreach (var cell in _cells)
            {
                if (player.GetCellDataFor(cell).PeekPossibility(Possibility, player.MainLocation))
                    yield return cell;
            }
        }
    }

    public bool ShouldHaveOutline(Cell cell, PossibilitiesLocation location, out int p)
    {
        if (location == _location && (_cells is null || _cells.Contains(cell)))
        {
            p = Possibility;
            return true;
        }

        p = 0;
        return false;
    }
        
}