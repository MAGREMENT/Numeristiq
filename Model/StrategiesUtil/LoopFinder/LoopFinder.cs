using System.Collections;
using System.Collections.Generic;
using Model.StrategiesUtil.LoopFinder.Types;

namespace Model.StrategiesUtil.LoopFinder;

public class LoopFinder<T> : IEnumerable<T> where T : ILoopElement, ILinkGraphElement
{
    public HashSet<Loop<T>> Loops { get; }= new();

    public int RepeatLoopAdd { get; private set; }
    public int GetLinksCall { get; private set; }

    public LinkGraph<T> Graph { get; }
    private readonly ILoopType<T> _type;

    public LoopFinder(LinkGraph<T> graph, ILoopType<T> type)
    {
        Graph = graph;
        _type = type;
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
            return true;
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