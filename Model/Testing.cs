using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Model.Positions;
using Model.Possibilities;
using Model.Strategies.AlternatingChains;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LoopFinder;
using Model.StrategiesUtil.LoopFinder.Types;

namespace Model;

public static class Testing
{
    public static void Main(string[] args) 
    {
        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        FullSudokuBankTest("OnlineBank3.txt");
        
        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        Console.WriteLine($"Time taken : {((double) end - start) / 1000}s");
    }

    private static void CompareSharedSeenCellsAlgorithms()
    {
        int turns = 10000;
        
        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        for(int i = 0; i < turns; i++)
        {
            for (int row1 = 0; row1 < 9; row1++)
            {
                for (int col1 = 0; col1 < 9; col1++)
                {
                    Coordinate one = new Coordinate(row1, col1);

                    for (int row2 = row1; row2 < 9; row2++)
                    {
                        for (int col2 = col1 + 1; col2 < 9; col2++)
                        {
                            Coordinate two = new Coordinate(row2, col2);

                            one.SharedSeenCells(two);
                        }
                    }
                }
            }
        }
        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Console.WriteLine($"Time taken for V1 : {((double) end - start) / 1000}s");
        
        start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        for(int i = 0; i < turns; i++)
        {
            for (int row1 = 0; row1 < 9; row1++)
            {
                for (int col1 = 0; col1 < 9; col1++)
                {
                    Coordinate one = new Coordinate(row1, col1);

                    for (int row2 = row1; row2 < 9; row2++)
                    {
                        for (int col2 = col1 + 1; col2 < 9; col2++)
                        {
                            Coordinate two = new Coordinate(row2, col2);

                            one.SharedSeenCellsV2(two);
                        }
                    }
                }
            }
        }
        end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Console.WriteLine($"Time taken for V2 : {((double) end - start) / 1000}s");
    }

    private static bool AreTheSame(IEnumerable<Coordinate> one, IEnumerable<Coordinate> two)
    {
        HashSet<Coordinate> h1 = new HashSet<Coordinate>(one);
        HashSet<Coordinate> h2 = new HashSet<Coordinate>(two);

        if (h1.Count != h2.Count) return false;
        foreach (var coord in h1)
        {
            if (!h2.Contains(coord)) return false;
        }

        return true;
    }

    private static void LoopFinderTest()
    {
        LinkGraph<LoopElementInt> example = new();
        example.AddLink(new LoopElementInt(1), new LoopElementInt(2), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(1), new LoopElementInt(4), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(2), new LoopElementInt(3), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(3), new LoopElementInt(4), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(2), new LoopElementInt(5), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(5), new LoopElementInt(6), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(6), new LoopElementInt(7), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(7), new LoopElementInt(8), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(4), new LoopElementInt(8), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(8), new LoopElementInt(11), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(11), new LoopElementInt(12), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(12), new LoopElementInt(13), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(13), new LoopElementInt(10), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(10), new LoopElementInt(11), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(10), new LoopElementInt(9), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(9), new LoopElementInt(14), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(14), new LoopElementInt(7), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(14), new LoopElementInt(15), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(15), new LoopElementInt(16), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(16), new LoopElementInt(19), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(19), new LoopElementInt(14), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(16), new LoopElementInt(17), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(17), new LoopElementInt(18), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(18), new LoopElementInt(19), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(18), new LoopElementInt(20), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(19), new LoopElementInt(20), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(20), new LoopElementInt(21), LinkStrength.Strong);
        example.AddLink(new LoopElementInt(19), new LoopElementInt(21), LinkStrength.Weak);
        example.AddLink(new LoopElementInt(21), new LoopElementInt(22), LinkStrength.Strong);

        LoopFinder<LoopElementInt> finder = new LoopFinder<LoopElementInt>(example,
            new AICLoopsV4<LoopElementInt>(), (_) => false);
        finder.Run();
        Console.WriteLine(finder.GetStats());
    }

    private class LoopElementInt : ILoopElement, ILinkGraphElement
    {
        private readonly int _i;

        public LoopElementInt(int i)
        {
            _i = i;
        }
        
        public bool IsSameLoopElement(ILoopElement other)
        {
            return other is LoopElementInt n && n._i == _i;
        }

        public override bool Equals(object? obj)
        {
            return obj is LoopElementInt n && n._i == _i;
        }

        public override int GetHashCode()
        {
            return _i;
        }

        public override string ToString()
        {
            return _i.ToString();
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
        var counter = 1;
        var success = 0;
        var solverIsTrash = 0;
        Solver solver = new Solver(new Sudoku())
        {
            LogsManaged = false
        };
        try
        {
            using TextReader reader =
                new StreamReader($"C:\\Users\\Zach\\Desktop\\Perso\\SudokuSolver\\Model\\Data\\{fileNameInDataFolder}", Encoding.UTF8);

            while (reader.ReadLine() is { } line)
            {
                solver.SetSudoku(new Sudoku(line));
                solver.Solve();

                if (!solver.Sudoku.IsCorrect())
                {
                    if (solver.IsWrong())
                    {
                        Console.WriteLine(counter++ + " WRONG, Solver is trash ! => " + line);
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
            Console.WriteLine($"-{strategy.Name} : {strategy.Score}");
        }
    }

    private static void SharedSeenCellsTest()
    {
        foreach (var coord in new Coordinate(4, 2).SharedSeenCells(new Coordinate(1, 2)))
        {
            Console.WriteLine(coord);
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
        PrintPositions(one.Mash(two));
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
            Console.WriteLine("Has : " + n[0] + ", " + n[1]);
        }
    }

    private static void SudokuResolutionTest(string asString)
    {
        var sud = new Sudoku(asString);
        Console.WriteLine("Sudoku initial : ");
        Console.WriteLine(sud);

        var solver = new Solver(sud);
        int numbersAdded = 0;
        solver.NumberAdded += (_, _) => numbersAdded++;
        solver.Solve();
        Console.WriteLine("Sudoku après résolution : ");
        Console.WriteLine(solver.Sudoku);
        Console.WriteLine("Chiffres ajoutés : " + numbersAdded);
        Console.WriteLine("Est correct ? : " + sud.IsCorrect());
    }
}