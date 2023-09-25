namespace Model;

public static class PathsInfo //TODO make relative paths
{
    private const string HardCodedPath = @"C:/Users/Zach/Desktop/Perso/SudokuSolver";
        
    public static string PathToData()
    {
        return HardCodedPath + "/Model/Data";
    }
}