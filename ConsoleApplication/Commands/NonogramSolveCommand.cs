using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Nonograms;

namespace ConsoleApplication.Commands;

public class NonogramSolveCommand : SolveCommand
{
    public NonogramSolveCommand() : base("Nonogram")
    {
    }

    protected override ISolver GetSolverAndSetPuzzle(ArgumentInterpreter interpreter, string puzzle)
    {
        var solver = interpreter.Instantiator.InstantiateNonogramSolver();
        solver.SetNonogram(NonogramTranslator.TranslateLineFormat(puzzle));

        return solver;
    }

    protected override string PuzzleAsString(ISolver solver)
    {
        return ((NonogramSolver)solver).Nonogram.ToString()!;
    }
}