using DesktopApplication.Presenter.Sudokus.Solve;
using Model.Core;
using Model.Core.Descriptions;
using Model.Core.Highlighting;
using Model.Repositories;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Descriptions;
using Repository.Files;

namespace DesktopApplication.Presenter.Sudokus.Manage;

public class SudokuManagePresenter
{
    private readonly ISudokuManageView _view;
    private readonly StrategyManager<SudokuStrategy> _manager;
    private readonly IStrategyRepository<SudokuStrategy> _repo;
    private readonly DescriptionParser<ISudokuDescriptionDisplayer> _parser;
    private readonly Settings _settings;
    
    private ShownInfo _shownInfo = ShownInfo.Settings;
    private string? _currentlyDisplayed;

    public SudokuManagePresenter(ISudokuManageView view, StrategyManager<SudokuStrategy> manager, 
        IStrategyRepository<SudokuStrategy> repo, Settings settings)
    {
        _view = view;
        _manager = manager;
        _repo = repo;
        _settings = settings;

        _parser = new SudokuDescriptionParser(@"Descriptions\Sudoku",
            !PresenterFactory.IsForProduction, false);
    }
    
    public void Initialize()
    {
        _view.SetStrategyList(_manager.Strategies);

        foreach (var result in SudokuStrategyPool.EnumerateStrategies())
        {
            _view.AddSearchResult(result);
        }
    }

    public void OnSearch(string s)
    {
        _view.ClearSearchResults();

        foreach (var result in SudokuStrategyPool.EnumerateStrategies(s))
        {
            _view.AddSearchResult(result);
        }
    }

    public void ChangeDisplay(ShownInfo info)
    {
        _shownInfo = info;
        if (_currentlyDisplayed is null) return;

        DisplayFromName(_currentlyDisplayed);
    }

    public void OnSearchResultSelection(string s)
    {
        _view.SetSelectedStrategyName(s);
        _currentlyDisplayed = s;

        DisplayFromName(s);
    }
    
    public void OnActiveStrategySelection(int index)
    {
        if (index < 0 || index >= _manager.Strategies.Count) return;

        var s = _manager.Strategies[index];
        _view.SetSelectedStrategyName(s.Name);
        _currentlyDisplayed = s.Name;
        
        if(_shownInfo == ShownInfo.Settings) _view.SetManageableSettings(new StrategySettingsPresenter(s, _repo));
        else _view.SetStrategyDescription(_parser.Get(s.Name));
    }

    public void AddStrategy(string s, int position)
    {
        var strategy = SudokuStrategyPool.CreateFrom(s);
        if (strategy is null) return;
        
        _manager.AddStrategy(strategy, position);
        _repo.SetStrategies(_manager.Strategies);
        _view.SetStrategyList(_manager.Strategies);
    }

    public void InterchangeStrategies(int posFrom, int posTo)
    {
        _manager.InterchangeStrategies(posFrom, posTo);
        _repo.SetStrategies(_manager.Strategies);
        _view.SetStrategyList(_manager.Strategies);
    }

    public void RemoveStrategy(int index)
    {
        _manager.RemoveStrategy(index);
        _repo.SetStrategies(_manager.Strategies);
        _view.SetStrategyList(_manager.Strategies);
        _view.ClearSelectedStrategy();
    }

    public void OnShow()
    {
        _currentlyDisplayed = null;
        _view.ClearSelectedStrategy();
    }
    
    
    public void UploadPreset()
    {
        using var stream = _view.GetUploadPresetStream();
        if (stream is null) return;

        var repo = new FileSudokuPresetRepository(stream);
        repo.SetStrategies(_manager.Strategies);
    }

    public void DownloadPreset()
    {
        using var stream = _view.GetDownloadPresetStream();
        if (stream is null) return;

        var repo = new FileSudokuPresetRepository(stream);
        
        _manager.ClearStrategies();
        _manager.AddStrategies(repo.GetStrategies());
        _view.SetStrategyList(_manager.Strategies);
    }

    private void DisplayFromName(string s)
    {
        if (_shownInfo == ShownInfo.Settings)
        {
            var strategy = _manager.FindStrategy(s);
            if(strategy is null) _view.SetNotFoundSettings();
            else _view.SetManageableSettings(new StrategySettingsPresenter(strategy, _repo));
        }
        else _view.SetStrategyDescription(_parser.Get(s));
    }

    public void Highlight(ISudokuSolverDrawer drawer, IHighlightable<ISudokuHighlighter> highlight)
    {
        var h = new SudokuHighlighterTranslator(drawer, _settings);
        h.Translate(highlight, true);
    }
}

public enum ShownInfo
{
    Settings, Documentation
}