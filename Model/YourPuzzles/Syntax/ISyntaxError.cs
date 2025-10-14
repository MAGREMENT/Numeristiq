namespace Model.YourPuzzles.Syntax;

public interface ISyntaxError
{
    public (int, int, int) FindWhereToHighlight(string text);
    public string GetErrorMessage();
}