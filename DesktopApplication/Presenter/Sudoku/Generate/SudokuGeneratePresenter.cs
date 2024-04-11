using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Sudoku.Generator;
using Model.Sudoku.Solver;

namespace DesktopApplication.Presenter.Sudoku.Generate;

public class SudokuGeneratePresenter
{
    private readonly ISudokuGenerateView _view;
    private readonly ISudokuPuzzleGenerator _generator;
    private readonly Settings _settings;
    private readonly SudokuEvaluator _evaluator;
    
    private readonly List<GeneratedSudokuPuzzle> _evaluatedList = new();

    private int _generationCount = 1;
    private int _currentId = 1;
    
    public SettingsPresenter SettingsPresenter { get; }

    public SudokuGeneratePresenter(ISudokuGenerateView view, SudokuSolver solver, Settings settings)
    {
        _view = view;
        _evaluator = new SudokuEvaluator(solver);
        _settings = settings;
        _generator = new RCRSudokuPuzzleGenerator(new BackTrackingFilledSudokuGenerator());

        SettingsPresenter = new SettingsPresenter(_settings, SettingCollections.SudokuGeneratePage);
    }

    public async void Generate()
    {
        Reset();

        _view.AllowGeneration(false);
        await Task.Run(GeneratePuzzles);
        _view.AllowGeneration(true);
    }

    public void SetGenerationCount(int value) => _generationCount = value;

    private void GeneratePuzzles()
    {
        
    }

    private void Reset()
    {
        _currentId = 1;
        _evaluatedList.Clear();
       
        _view.UpdateEvaluatedList(_evaluatedList);
    }
}