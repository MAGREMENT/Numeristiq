using System.Collections;
using System.Collections.Generic;
using Model.StrategiesUtil.LoopFinder.Types;

namespace Model.StrategiesUtil.LoopFinder;

public class LoopFinder<T> : IEnumerable<T> where T : ILoopElement
{
    public HashSet<Loop<T>> Loops { get; }= new();

    public int RepeatLoopAdd { get; private set; }
    public int GetLinksCall { get; private set; }

    public Graph<T> Graph { get; }
    private readonly ILoopType<T> _type;
    private readonly LoopHandler _handler;

    public delegate bool LoopHandler(Loop<T> loop);

    public LoopFinder(Graph<T> graph, ILoopType<T> type, LoopHandler handler)
    {
        Graph = graph;
        _type = type;
        _handler = handler;
    }

    public void Run()
    {
        Loops.Clear();
        RepeatLoopAdd = 0;
        GetLinksCall = 0;
        
        _type.Apply(this);
    }

    public bool AddLoop(Loop<T> loop)
    {
        if (Loops.Contains(loop)) RepeatLoopAdd++;
        else
        {
            Loops.Add(loop);
            return _handler(loop);
        }

        return false;
    }

    public HashSet<T> GetLinks(T from, LinkStrength strength)
    {
        GetLinksCall++;
        return Graph.GetLinks(from, strength);
    }

    public string GetStats()
    {
        string result = "Loops found : " + Loops.Count;
        result += "\nRepeat count : " + RepeatLoopAdd;
        result += "\nGetLinks call : " + GetLinksCall;
        result += "\nAll loops : \n\n";
        int oddCount = 0;
        int evenCount = 0;
        foreach (var loop in Loops)
        {
            result += loop + "\n";
            if (loop.Count % 2 == 0) evenCount++;
            else oddCount++;
        }

        result += "\nOdd count : " + oddCount;
        result += "\nEven count : " + evenCount;

        return result;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Graph.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}