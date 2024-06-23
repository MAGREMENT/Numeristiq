using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Kakuros;

namespace ConsoleApplication.Commands;

public class KakuroSolveCommand : SolveCommand
{
    public KakuroSolveCommand() : base("Kakuro")
    {
    }

    protected override ISolver GetSolverAndSetPuzzle(ArgumentInterpreter interpreter, string puzzle)
    {
        var solver = interpreter.Instantiator.InstantiateKakuroSolver();
        solver.SetKakuro(KakuroTranslator.TranslateSumFormat(puzzle));

        return solver;
    }

    protected override string PuzzleAsString(ISolver solver)
    {
        return ((KakuroSolver)solver).Kakuro.ToString()!;
    }
}