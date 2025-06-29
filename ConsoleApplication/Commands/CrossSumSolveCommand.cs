using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.CrossSums;
using Model.CrossSums.Solver;

namespace ConsoleApplication.Commands;

public class CrossSumSolveCommand : SolveCommand
{
    public CrossSumSolveCommand() : base("CrossSum")
    {
    }

    protected override ISolver GetSolverAndSetPuzzle(ArgumentInterpreter interpreter, string puzzle)
    {
        var solver = interpreter.Instantiator.InstantiateCrossSumSolver();
        solver.SetCrossSum(CrossSumTranslator.Translate(puzzle));

        return solver;
    }

    protected override string PuzzleAsString(ISolver solver)
    {
        return CrossSumTranslator.Translate(((CrossSumSolver)solver).CrossSum);
    }
}