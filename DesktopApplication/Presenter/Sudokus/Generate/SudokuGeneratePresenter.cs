using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Sudokus;
using Model.Sudokus.Generator;
using Model.Sudokus.Solver;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter.Sudokus.Generate;

public class SudokuGeneratePresenter : IManageCriteriaCallback
{
    private readonly ISudokuGenerateView _view;
    private readonly RDRSudokuPuzzleGenerator _generator;
    private readonly SudokuEvaluator _evaluator;
    private readonly Settings _setting;
    
    private readonly List<GeneratedSudokuPuzzle> _evaluatedList = new();

    private int _generationCount = 1;
    private bool _running;
    private int _currentId = 1;
    
    public SettingsPresenter SettingsPresenter { get; }

    public SudokuGeneratePresenter(ISudokuGenerateView view, SudokuSolver solver, Settings settings)
    {
        _view = view;
        _setting = settings;
        _evaluator = new SudokuEvaluator(solver);
        _generator = new RDRSudokuPuzzleGenerator(new BackTrackingFilledSudokuGenerator());

        SettingsPresenter = new SettingsPresenter(settings, SettingCollections.SudokuGeneratePage);
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

    public void SetKeepUniqueness(bool value) => _generator.KeepUniqueness = value;

    public void SetRandomFilled() => _generator.FilledGenerator = new BackTrackingFilledSudokuGenerator();

    public void SetSeedFilled(string s, SudokuStringFormat format)
    {
        var sudoku = format switch
        {
            SudokuStringFormat.Base32 => SudokuTranslator.TranslateSolvingState(
                SudokuTranslator.TranslateBase32Format(s, new AlphabeticalBase32Translator())),
            SudokuStringFormat.Line => SudokuTranslator.TranslateLineFormat(s),
            SudokuStringFormat.Grid => SudokuTranslator.TranslateSolvingState(
                SudokuTranslator.TranslateGridFormat(s, _setting.SoloToGiven)),
            _ => throw new ArgumentOutOfRangeException()
        };

        _generator.FilledGenerator = new ConstantFilledSudokuGenerator(sudoku);
    }
    
    public ManageCriteriaPresenterBuilder ManageCriteria() => new(_evaluator.GetCriteriasCopy(), this);

    public void ShowSeed()
    {
        if (_generator.FilledGenerator is ConstantFilledSudokuGenerator cfsg) _view.ShowSudoku(cfsg.Sudoku);
    }

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
            else
            {
                _view.ShowTransition(TransitionPlace.ToBin);
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

    public void SetCriterias(UniqueList<EvaluationCriteria> criteriaList)
    {
        _evaluator.SetCriterias(criteriaList);
        _view.SetCriteriaList(criteriaList);
    }
}

public class ManageCriteriaPresenterBuilder
{
    private readonly UniqueList<EvaluationCriteria> _criteriaList;
    private readonly IManageCriteriaCallback _callback;


    public ManageCriteriaPresenterBuilder(UniqueList<EvaluationCriteria> criteriaList, IManageCriteriaCallback callback)
    {
        _criteriaList = criteriaList;
        _callback = callback;
    }

    public ManageCriteriaPresenter Build(IManageCriteriaView view) => new(view, _criteriaList, _callback);
}

public interface IManageCriteriaCallback
{
    public void SetCriterias(UniqueList<EvaluationCriteria> criteriaList);
}