using System.Collections.Generic;
using Model.Utility;

namespace Model.Core.Generators;

public interface IFilledPuzzleGenerator<out T>
{
   T Generate();
   T Generate(out List<Cell> removableCells);
}