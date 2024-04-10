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

    private GeneratedSudokuPuzzle? _currentlyEvaluated;
    private readonly Queue<GeneratedSudokuPuzzle> _evaluationQueue = new();
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
        DumpNotEvaluatedQueueOneByOne();
        _view.AllowGeneration(true);
    }

    public void SetGenerationCount(int value) => _generationCount = value;

    private void DumpNotEvaluatedQueueOneByOne()
    {
        while (_evaluationQueue.Count > 0)
        {
            _evaluationQueue.Dequeue();
            _view.UpdateNotEvaluatedList(_evaluationQueue);
        }
    }

    private void GeneratePuzzles()
    {
        while (_evaluatedList.Count < _generationCount)
        {
            _generator.Generate(OnNewPuzzleGenerated);
        }
    }

    private void OnNewPuzzleGenerated(Model.Sudoku.Sudoku sudoku)
    {
        _evaluationQueue.Enqueue(new GeneratedSudokuPuzzle(_currentId++, sudoku));
        _view.UpdateNotEvaluatedList(_evaluationQueue);
        if (_evaluationQueue.Count >= _settings.EvaluationTriggerCount)
        {
            for (int i = 0; i < _settings.SuccessiveEvaluationCount && _evaluatedList.Count < _generationCount; i++)
            {
                var current = _evaluationQueue.Dequeue();
                _view.UpdateNotEvaluatedList(_evaluationQueue);

                EvaluatePuzzle(current);
            }
        }
    }

    private void EvaluatePuzzle(GeneratedSudokuPuzzle sudoku)
    {
        _currentlyEvaluated = sudoku;
        _view.UpdateCurrentlyEvaluated(_currentlyEvaluated);

        var result = _evaluator.Evaluate(_currentlyEvaluated);
        
        if(result is not null) _evaluatedList.Add(_currentlyEvaluated);
        _currentlyEvaluated = null;

        _view.UpdateCurrentlyEvaluated(_currentlyEvaluated);
        _view.UpdateEvaluatedList(_evaluatedList);
    }

    private void Reset()
    {
        _currentId = 1;
        _currentlyEvaluated = null;
        _evaluationQueue.Clear();
        _evaluatedList.Clear();
        
        _view.UpdateCurrentlyEvaluated(_currentlyEvaluated);
        _view.UpdateNotEvaluatedList(_evaluationQueue);
        _view.UpdateEvaluatedList(_evaluatedList);
    }
}