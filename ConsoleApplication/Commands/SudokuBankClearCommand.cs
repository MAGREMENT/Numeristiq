namespace ConsoleApplication.Commands;

public class SudokuBankClearCommand : Command
{
    public SudokuBankClearCommand() : base("Clear"){}

    public override string Description => "Clears the Sudoku bank";
    public override CommandResult? Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var repo = interpreter.Instantiator.InstantiateBankRepository();
        try
        {
            repo.Clear();
            Console.WriteLine("Operation was done successfully");
            return CommandResult.Succes;
        }
        catch(Exception e)
        {
            return CommandResult.Fail(e.Message);
        }
    }
}