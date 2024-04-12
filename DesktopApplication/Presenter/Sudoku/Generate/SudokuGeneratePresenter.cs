using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Sudoku.Generator;
using Model.Sudoku.Solver;

namespace DesktopApplication.Presenter.Sudoku.Generate;

public class SudokuGeneratePresenter
{
    private readonly ISudokuGenerateView _view;
    private readonly RDRSudokuPuzzleGenerator _generator;
    private readonly Settings _settings;
    private readonly SudokuEvaluator _evaluator;
    
    private readonly List<GeneratedSudokuPuzzle> _evaluatedList = new();

    private int _generationCount = 1;
    private bool _running;
    private int _currentId = 1;
    
    public SettingsPresenter SettingsPresenter { get; }

    public SudokuGeneratePresenter(ISudokuGenerateView view, SudokuSolver solver, Settings settings)
    {
        _view = view;
        _evaluator = new SudokuEvaluator(solver);
        _settings = settings;
        _generator = new RDRSudokuPuzzleGenerator(new BackTrackingFilledSudokuGenerator());

        SettingsPresenter = new SettingsPresenter(_settings, SettingCollections.SudokuGeneratePage);
    }

    public async void Generate()
    {
        Reset();

        _view.AllowGeneration(false);
        await Task.Run(GeneratePuzzles);
        _view.AllowGeneration(true);
    }

    public void Stop()
    {
        _running = false;
        _view.AllowCancel(false);
    }

    public void SetGenerationCount(int value) => _generationCount = value;

    public void SetKeepSymmetry(bool value) => _generator.KeepSymmetry = value;

    public ManageCriteriaPresenterBuilder ManageCriteria() => new(_evaluator);

    private void GeneratePuzzles()
    {
        _running = true;
        _view.AllowCancel(true);

        while (_running && _evaluatedList.Count < _generationCount)
        {
            _view.ActivateFilledSudokuGenerator(true);
            var generated = new GeneratedSudokuPuzzle(_currentId++, _generator.Generate(OnFilledSudokuGenerated));
            
            _view.ActivateRandomDigitRemover(false);
            _view.ShowTransition(TransitionPlace.ToEvaluator);
            _view.ActivatePuzzleEvaluator(true);
            
            var evaluated = _evaluator.Evaluate(generated);
            _view.ActivatePuzzleEvaluator(false);

            if (evaluated is not null)
            {
                _evaluatedList.Add(evaluated);
                _view.ShowTransition(TransitionPlace.ToFinalList);
                _view.UpdateEvaluatedList(_evaluatedList);
            }
        }

        _running = false;
        _view.AllowCancel(false);
    }

    private void OnFilledSudokuGenerated()
    {
        _view.ActivateFilledSudokuGenerator(false);
        _view.ShowTransition(TransitionPlace.ToRDR);
        _view.ActivateRandomDigitRemover(true);
    }

    private void Reset()
    {
        _currentId = 1;
        _evaluatedList.Clear();
       
        _view.UpdateEvaluatedList(_evaluatedList);
    }
}

public class ManageCriteriaPresenterBuilder
{
    private readonly SudokuEvaluator _evaluator;

    public ManageCriteriaPresenterBuilder(SudokuEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    public ManageCriteriaPresenter Build(IManageCriteriaView view) => new ManageCriteriaPresenter(view, _evaluator);
}