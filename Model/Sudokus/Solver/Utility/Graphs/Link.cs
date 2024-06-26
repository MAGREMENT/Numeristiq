﻿namespace Model.Sudokus.Solver.Utility.Graphs;

public class Link<T>
{
    public Link(T from, T to)
    {
        From = from;
        To = to;
    }

    public T From { get; }
    public T To { get; }
}