using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Sudoku;
using Model.Sudoku.Generator;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;

namespace DesktopApplication.Presenter.Sudoku.Generate;

public class SudokuGeneratePresenter
{
    private readonly ISudokuGenerateView _view;
    private readonly ISudokuPuzzleGenerator _generator;
    private readonly Settings _settings;
    private readonly SudokuSolver _solver;
    private readonly RatingTracker _rTracker = new();
    private readonly HardestStrategyTracker _hsTracker = new();

    private Model.Sudoku.Sudoku? _currentlyEvaluated;
    private readonly Queue<Model.Sudoku.Sudoku> _evaluationQueue = new();
    private readonly List<EvaluatedGeneratedPuzzle> _evaluatedList = new();

    private int _generationCount = 1;
    
    public SettingsPresenter SettingsPresenter { get; }

    public SudokuGeneratePresenter(ISudokuGenerateView view, SudokuSolver solver, Settings settings)
    {
        _view = view;
        _solver = solver;
        _settings = settings;
        _generator = new RCRSudokuPuzzleGenerator(new BackTrackingFilledSudokuGenerator());

        _solver.AddTracker(_rTracker);
        _solver.AddTracker(_hsTracker);

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
        _evaluationQueue.Enqueue(sudoku);
        _view.UpdateNotEvaluatedList(_evaluationQueue);
        if (_evaluationQueue.Count >= _settings.EvaluationTriggerCount)
        {
            for (int i = 0; i < _settings.SuccessiveEvaluationCount; i++)
            {
                var current = _evaluationQueue.Dequeue();
                _view.UpdateNotEvaluatedList(_evaluationQueue);

                EvaluatePuzzle(current);
            }
        }
    }

    private void EvaluatePuzzle(Model.Sudoku.Sudoku sudoku)
    {
        _currentlyEvaluated = sudoku;
        _view.UpdateCurrentlyEvaluated(_currentlyEvaluated);
            
        _solver.SetSudoku(_currentlyEvaluated.Copy());
        _solver.Solve();
        _evaluatedList.Add(new EvaluatedGeneratedPuzzle(SudokuTranslator.TranslateLineFormat(_currentlyEvaluated,
            SudokuLineFormatEmptyCellRepresentation.Zeros), _rTracker.Rating, _hsTracker.Hardest));
            
        _rTracker.Clear();
        _hsTracker.Clear();
        _currentlyEvaluated = null;

        _view.UpdateCurrentlyEvaluated(_currentlyEvaluated);
        _view.UpdateEvaluatedList(_evaluatedList);
    }

    private void Reset()
    {
        _currentlyEvaluated = null;
        _evaluationQueue.Clear();
        _evaluatedList.Clear();
        
        _view.UpdateCurrentlyEvaluated(_currentlyEvaluated);
        _view.UpdateNotEvaluatedList(_evaluationQueue);
        _view.UpdateEvaluatedList(_evaluatedList);
    }
}