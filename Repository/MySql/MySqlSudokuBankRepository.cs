using System.Text;
using Model.Core;
using Model.Repositories;
using Model.Sudokus;
using MySql.Data.MySqlClient;

namespace Repository.MySql;

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

    private const string InsertQuery = "INSERT IGNORE INTO Numeristiq_Sudoku(id, difficulty)\n" +
                                       "VALUES (@id, @difficulty);";

    private const string DeleteQuery = "DELETE FROM Numeristiq_Sudoku;";
    
    public void Initialize()
    {
        try
        {
            using var conn = Open();
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
        catch (Exception e)
        {
            throw new MySqlRepositoryException(e);
        }
    }

    public Sudoku? FindRandom(Difficulty difficulty)
    {
        try
        {
            using var conn = Open();
            var cmd = new MySqlCommand(FindRandomQuery, conn);
            cmd.Parameters.AddWithValue("@difficulty", (int)difficulty);
            
            var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return reader[0] is not string s ? null : SudokuTranslator.TranslateLineFormat(s);
        }
        catch (Exception e)
        {
            throw new MySqlRepositoryException(e);
        }
    }

    public void Clear()
    {
        try
        {
            using var conn = Open();
            var cmd = new MySqlCommand(DeleteQuery, conn);
            cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            throw new MySqlRepositoryException(e);
        }
    }

    public void Add(Sudoku sudoku, Difficulty difficulty)
    {
        try
        {
            using var conn = Open();
            var cmd = new MySqlCommand(InsertQuery, conn);
            cmd.Parameters.AddWithValue("@id",
                SudokuTranslator.TranslateLineFormat(sudoku, SudokuLineFormatEmptyCellRepresentation.Points));
            cmd.Parameters.AddWithValue("@difficulty", (int)difficulty);
            cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            throw new MySqlRepositoryException(e);
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

            using var conn = Open();
            var cmd = new MySqlCommand(builder.ToString(), conn);
            return cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            throw new MySqlRepositoryException(e);
        }
    }

    public bool IsAvailable()
    {
        try
        {
            using var c = Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static MySqlConnection Open()
    {
        const string s = $"server={MySqlCredentials.Host};user={MySqlCredentials.User};database={MySqlCredentials.Database};port={MySqlCredentials.Port};password={MySqlCredentials.Password}";
        var conn = new MySqlConnection(s);
        conn.Open();
        return conn;
    }
}

public class MySqlRepositoryException : Exception
{
    public MySqlRepositoryException(Exception e) : base(Handle(e)){}

    private static string Handle(Exception e)
    {
        if (e is ArgumentException) return "Incorrect connection string";

        if (e is MySqlException)
        {
            switch (e.HResult)
            {
                //http://dev.mysql.com/doc/refman/5.0/en/error-messages-server.html
                case 1042: // Unable to connect to any of the specified MySQL hosts (Check Server,Port)
                    return "Unable to connect to the hose";
                case 0: // Access denied (Check DB name,username,password)
                    return "Access denied";
            }
        }

        return "An error has occured";
    }
}