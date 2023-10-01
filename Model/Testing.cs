using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.SharedCellSearcher;

namespace Model;

public static class Testing
{
    public static void Main(string[] args)
    {
        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        FullSudokuBankTest("OnlineBank2.txt ");
        
        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        Console.WriteLine($"Time taken : {((double) end - start) / 1000}s");
    }

    private static void RecursionListTest()
    {
        List<int> numbers = new();
        for (int i = 1; i <= 5; i++)
        {
            numbers.Add(i);
        }

        var result = Combos(numbers, 3);
        foreach (var t in result)
        {
            string s = "-";
            foreach (var n in t)
            {
                s += n + " ";
            }

            Console.WriteLine(s);
        }
    }

    private static List<int[]> Combos(List<int> numbers, int max)
    {
        List<int[]> result = new();
        
        Combos(numbers, max, 0, 0, new RecursionList<int>(max), result);

        return result;
    }

    private static void Combos(List<int> numbers, int max, int count, int start, RecursionList<int> rList, List<int[]> result)
    {
        for (int i = start; i < numbers.Count; i++)
        {
            rList.SetCursorAndSetOrAdd(count, numbers[i]);

            if (count < max - 1) Combos(numbers, max, count + 1, i + 1, rList, result);
            else if (count == max - 1)
            {
                result.Add(rList.CopyUntilCursor());
            }
        }
    }

    private static void SharedSeenCellSearcherCompare(ISharedSeenCellSearcher one, ISharedSeenCellSearcher two)
    {
        for (int i = 0; i < 81; i++)
        {
            for (int j = i + 1; j < 81; j++)
            {
                int iRow = i / 9;
                int iCol = i % 9;
                int jRow = j / 9;
                int jCol = j % 9;

                var oneResult = one.SharedSeenCells(iRow, iCol, jRow, jCol).ToArray();
                var twoResult = two.SharedSeenCells(iRow, iCol, jRow, jCol).ToArray();

                bool ok = oneResult.Length == twoResult.Length;

                if (ok)
                {
                    foreach (var oneCoord in oneResult)
                    {
                        if (!twoResult.Contains(oneCoord))
                        {
                            ok = false;
                            break;
                        }
                    }  
                }

                if (!ok)
                {
                    Console.WriteLine($"Different result for : [{iRow + 1}, {iCol + 1}] and [{jRow + 1}, {jCol + 1}]");
                }
            }
        }

        var loops = 1000;
        
        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        for (int n = 0; n < loops; n++)
        {
            for (int i = 0; i < 81; i++)
            {
                for (int j = i + 1; j < 81; j++)
                {
                    int iRow = i / 9;
                    int iCol = i % 9;
                    int jRow = j / 9;
                    int jCol = j % 9;

                    foreach (var coord in one.SharedSeenCells(iRow, iCol, jRow, jCol))
                    {
                        int a = coord.Row;
                    }
                }
            }
        }
        
        
        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Console.WriteLine("One time : " + (end - start) + "ms");
        
        start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        for (int n = 0; n < loops; n++)
        {
            for (int i = 0; i < 81; i++)
            {
                for (int j = i + 1; j < 81; j++)
                {
                    int iRow = i / 9;
                    int iCol = i % 9;
                    int jRow = j / 9;
                    int jCol = j % 9;
                
                    foreach (var coord in two.SharedSeenCells(iRow, iCol, jRow, jCol))
                    {
                        int a = coord.Row;
                    }
                }
            }
        }

        end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Console.WriteLine("Two time : " + (end - start) + "ms");
    }

    private static Cell LineBullshit(Cell from, Cell to)
    {
        var space = 10;
        var proportion = space / Math.Sqrt(Math.Pow(to.Row - from.Row, 2) + Math.Pow(to.Col - from.Col, 2));

        return new Cell((int) (from.Row + proportion * (to.Row - from.Row)), (int) (from.Col +proportion * (to.Col - from.Col)));
    }

    private static void AlsSearch(int[] ints, List<IPossibilities> list, int start, IPossibilities current)
    {
        for (int i = start; i < ints.Length; i++)
        {
            var copy = current.Copy();
            copy.Add(ints[i]);
            list.Add(copy);
            AlsSearch(ints, list, i + 1, copy);
        }
    }

    private static void CompareIPossibilitiesImplementation(IPossibilities one, IPossibilities two)
    {
        Console.WriteLine(one.Count);
        Console.WriteLine(two.Count);
        Console.WriteLine(one);
        Console.WriteLine(two);

        one.Remove(1);
        one.Remove(9);
        one.Remove(5);
        
        two.Remove(1);
        two.Remove(9);
        two.Remove(5);

        Console.WriteLine(one.Count);
        Console.WriteLine(two.Count);
        Console.WriteLine(one);
        Console.WriteLine(two);
        
        Console.WriteLine(one.Peek(2));
        Console.WriteLine(one.Peek(5));
        Console.WriteLine(one.Remove(1));
        Console.WriteLine(one.Remove(3));
        Console.WriteLine(two.Peek(2));
        Console.WriteLine(two.Peek(5));
        Console.WriteLine(two.Remove(1));
        Console.WriteLine(two.Remove(3));
        
        Console.WriteLine(one.Count);
        Console.WriteLine(two.Count);
        Console.WriteLine(one);
        Console.WriteLine(two);
    }

    private static void FullSudokuBankTest(string fileNameInDataFolder)
    {
        List<string> fuckUps = new();
        var counter = 1;
        var success = 0;
        var solverIsTrash = 0;
        Solver.Solver solver = new Solver.Solver(new Sudoku())
        {
            LogsManaged = false,
            StatisticsTracked = true
        };
        try
        {
            using TextReader reader =
                new StreamReader(PathsInfo.PathToData() + $@"/SudokuBanks/{fileNameInDataFolder}", Encoding.UTF8);

            while (reader.ReadLine() is { } line)
            {
                solver.SetSudoku(SudokuTranslator.Translate(line));
                solver.Solve();

                if (!solver.Sudoku.IsCorrect())
                {
                    if (solver.IsWrong())
                    {
                        Console.WriteLine(counter++ + " WRONG, Solver is trash ! => " + line);
                        fuckUps.Add(line);
                        solverIsTrash++;
                    }
                    else Console.WriteLine(counter++ + " WRONG, did not find solution ! => " + line);
                }
                else
                {
                    Console.WriteLine(counter++ + " OK!");
                    success++;
                }
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("Reader problem : " + e.Message);
        }
        
        Console.WriteLine("\nResult-------------------------------");
        Console.WriteLine($"Completion rate = {success} / {counter - 1}");
        Console.WriteLine($"Solver fuck ups : {solverIsTrash}");
        Console.WriteLine();
        Console.WriteLine("Strategy usage : ");
        foreach (var strategy in solver.Strategies)
        {
            Console.WriteLine($"-{StringUtil.FillWithSpace(strategy.Name, 40)} => Usage : {strategy.Tracker.Usage} |" +
                              $" Score : {strategy.Tracker.Score} | Time used : {strategy.Tracker.TimeUsed}");
        }

        Console.WriteLine("\nFuck ups : ");
        foreach (var f in fuckUps)
        {
            Console.WriteLine("-" + f);
        }
    }

    private static void PositionsTest()
    {
        LinePositions one = new()
        {
            0,
            8,
            5
        };

        LinePositions two = new()
        {
            2,
            3,
            4,
            5
        };

        MiniGridPositions three = new(1, 2);
        three.Add(0, 0); //3, 6
        three.Add(2, 2); //5, 8
        three.Add(1, 1); //4, 7
        three.Add(0, 2); //3, 8

        PrintPositions(one);
        PrintPositions(two);
        PrintPositions(one.Or(two));
        PrintPositions(three);
    }

    private static void PrintPositions(LinePositions pos)
    {
        Console.WriteLine("Count : " + pos.Count);
        foreach (var n in pos)
        {
            Console.WriteLine("Has : " + n);
        }
    }

    private static void PrintPositions(MiniGridPositions pos)
    {
        Console.WriteLine("Count : " + pos.Count);
        foreach (var n in pos)
        {
            Console.WriteLine("Has : " + n.Row + ", " + n.Col);
        }
    }

    private static void SudokuResolutionTest(string asString)
    {
        var sud = SudokuTranslator.Translate(asString);
        Console.WriteLine("Initial sudoku : ");
        Console.WriteLine(sud);

        var solver = new Solver.Solver(sud);
        int numbersAdded = 0;
        solver.NumberAdded += (_, _) => numbersAdded++;
        solver.Solve();
        Console.WriteLine("After solving : ");
        Console.WriteLine(solver.Sudoku);
        Console.WriteLine("Numbers added : " + numbersAdded);
        Console.WriteLine("Is correct ? : " + sud.IsCorrect());
    }
}