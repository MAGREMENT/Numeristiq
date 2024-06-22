using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Core.Highlighting;
using Model.Kakuros;

namespace ConsoleApplication.Commands;

public class KakuroSolveCommand : SolveCommand<Strategy<IKakuroSolverData>, IUpdatableNumericSolvingState, INumericSolvingStateHighlighter>
{
    public KakuroSolveCommand() : base("Kakuro")
    {
    }

    protected override NumericStrategySolver<Strategy<IKakuroSolverData>, IUpdatableNumericSolvingState, INumericSolvingStateHighlighter> GetSolverAndSetPuzzle(ArgumentInterpreter interpreter, string puzzle)
    {
        var solver = interpreter.Instantiator.InstantiateKakuroSolver();
        solver.SetKakuro(KakuroTranslator.TranslateSumFormat(puzzle));

        return solver;
    }

    protected override string PuzzleAsString(NumericStrategySolver<Strategy<IKakuroSolverData>, IUpdatableNumericSolvingState, INumericSolvingStateHighlighter> solver)
    {
        return ((KakuroSolver)solver).Kakuro.ToString()!;
    }
}