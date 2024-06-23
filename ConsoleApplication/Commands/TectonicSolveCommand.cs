using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Tectonics;
using Model.Tectonics.Solver;

namespace ConsoleApplication.Commands;

public class TectonicSolveCommand : SolveCommand
{
    public TectonicSolveCommand() : base("Tectonic")
    {
    }

    protected override ISolver GetSolverAndSetPuzzle(ArgumentInterpreter interpreter, string puzzle)
    {
        var solver = interpreter.Instantiator.InstantiateTectonicSolver();
        solver.SetTectonic(TectonicTranslator.TranslateRdFormat(puzzle));

        return solver;
    }

    protected override string PuzzleAsString(ISolver solver)
    {
        return ((TectonicSolver)solver).Tectonic.ToString()!;
    }
}