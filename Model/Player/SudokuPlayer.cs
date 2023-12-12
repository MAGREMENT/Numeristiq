namespace Model.Player;

public class SudokuPlayer : IPlayer
{
    private readonly PlayerCell[,] _cells = new PlayerCell[9, 9];
    
    public event OnChange? Changed;
}