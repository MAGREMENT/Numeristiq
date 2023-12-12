namespace Model;

public interface IPlayer
{
    public event OnChange? Changed;
}

public delegate void OnChange();