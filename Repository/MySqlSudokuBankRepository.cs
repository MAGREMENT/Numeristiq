using System.Text;
using Model.Core;
using Model.Repositories;
using Model.Sudokus;
using MySql.Data.MySqlClient;

namespace Repository;

public class MySqlSudokuBankRepository : ISudokuBankRepository
{
    private const string DropQuery = "DROP TABLE Numeristiq_Sudoku;";

    private const string CreateQuery = "CREATE TABLE Numeristiq_Sudoku(\n" +
                                       "    id varchar(81) PRIMARY KEY,\n" +
                                       "    difficulty int\n" +
                                       ");";

    private const string FindRandomQuery = "SELECT id\n" +
                                           "From Numeristiq_Sudoku\n" +
                                           "WHERE difficulty = @difficulty\n" +
                                           "ORDER BY Rand()\n" +
                                           "LIMIT 1;";

    private const string InsertQuery = "INSERT IGNORE INTO Numeristiq_Sudoku(id, difficulty)" +
                                       "\nVALUES (@id, @difficulty);";

    private const string DeleteQuery = "DELETE FROM Numeristiq_Sudoku;";
    
    public void Initialize()
    {
        try
        {
            using var conn = MySqlConnectionManager.Open();
            try
            {
                var cmdDrop = new MySqlCommand(DropQuery, conn);
                cmdDrop.ExecuteNonQuery();
            }
            catch (Exception)
            {
                //ignored
            }
            
            var cmdCreate = new MySqlCommand(CreateQuery, conn);
            cmdCreate.ExecuteNonQuery();
        }
        catch (Exception)
        {
            throw new SudokuBankException("Couldn't initialize the database");
        }
    }

    public Sudoku? FindRandom(Difficulty difficulty)
    {
        try
        {
            using var conn = MySqlConnectionManager.Open();
            var cmd = new MySqlCommand(FindRandomQuery, conn);
            cmd.Parameters.AddWithValue("@difficulty", (int)difficulty);
            
            var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return reader[0] is not string s ? null : SudokuTranslator.TranslateLineFormat(s);
        }
        catch (Exception)
        {
            throw new SudokuBankException("Couldn't find a random sudoku");
        }
    }

    public void Clear()
    {
        try
        {
            using var conn = MySqlConnectionManager.Open();
            var cmd = new MySqlCommand(DeleteQuery, conn);
            cmd.ExecuteNonQuery();
        }
        catch (Exception)
        {
            throw new SudokuBankException("Couldn't clear the database");
        }
    }

    public void Add(Sudoku sudoku, Difficulty difficulty)
    {
        try
        {
            using var conn = MySqlConnectionManager.Open();
            var cmd = new MySqlCommand(InsertQuery, conn);
            cmd.Parameters.AddWithValue("@id",
                SudokuTranslator.TranslateLineFormat(sudoku, SudokuLineFormatEmptyCellRepresentation.Points));
            cmd.Parameters.AddWithValue("@difficulty", (int)difficulty);
            cmd.ExecuteNonQuery();
        }
        catch (Exception)
        {
            throw new SudokuBankException("Couldn't add the sudoku");
        }
    }

    public int Add(IEnumerable<(Sudoku, Difficulty)> entries)
    {
        try
        {
            var builder = new StringBuilder();
            foreach (var (sudoku, difficulty) in entries)
            {
                var query = InsertQuery.Replace("@id", $"'{SudokuTranslator.TranslateLineFormat(sudoku, SudokuLineFormatEmptyCellRepresentation.Points)}'")
                    .Replace("@difficulty",
                        ((int)difficulty).ToString());
                builder.Append(query);
                builder.Append('\n');
            }
            
            using var conn = MySqlConnectionManager.Open();
            var cmd = new MySqlCommand(builder.ToString(), conn);
            return cmd.ExecuteNonQuery();
        }
        catch (Exception)
        {
            throw new SudokuBankException("Couldn't add the Sudoku's");
        }
    }
}

public class SudokuBankException : Exception
{
    public SudokuBankException(string description) : base(description){}
}