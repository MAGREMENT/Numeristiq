namespace Model.Core.Graphs;

public class Link<T> where T : notnull
{
    public Link(T from, T to)
    {
        From = from;
        To = to;
    }

    public T From { get; }
    public T To { get; }

    public override int GetHashCode()
    {
        return From.GetHashCode() ^ To.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is Link<T> l &&
               ((l.From.Equals(From) && l.To.Equals(To)) || (l.From.Equals(To) && l.To.Equals(From)));
    }
}