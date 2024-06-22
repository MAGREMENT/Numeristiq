using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Core.Highlighting;
using Model.Tectonics;
using Model.Tectonics.Solver;

namespace ConsoleApplication.Commands;

public class TectonicSolveCommand : SolveCommand<Strategy<ITectonicSolverData>, IUpdatableTectonicSolvingState, ITectonicHighlighter>
{
    public TectonicSolveCommand() : base("Tectonic")
    {
    }

    protected override NumericStrategySolver<Strategy<ITectonicSolverData>, IUpdatableTectonicSolvingState, ITectonicHighlighter> GetSolverAndSetPuzzle(ArgumentInterpreter interpreter, string puzzle)
    {
        var solver = interpreter.Instantiator.InstantiateTectonicSolver();
        solver.SetTectonic(TectonicTranslator.TranslateRdFormat(puzzle));

        return solver;
    }

    protected override string PuzzleAsString(NumericStrategySolver<Strategy<ITectonicSolverData>, IUpdatableTectonicSolvingState, ITectonicHighlighter> solver)
    {
        return ((TectonicSolver)solver).Tectonic.ToString()!;
    }
}