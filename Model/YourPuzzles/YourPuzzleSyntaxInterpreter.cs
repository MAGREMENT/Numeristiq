

namespace Model.YourPuzzles;

/*public class YourPuzzleSyntaxInterpreter : ISyntaxInterpreter<ISyntaxElement, INumericPuzzleRule, YourPuzzleInterpreterErrorType>
{
    public InterpreterError<ISyntaxElement, YourPuzzleInterpreterErrorType>? TryInterpret(IReadOnlyList<ParsedLine<ISyntaxElement>> parsed, out IReadOnlyList<INumericPuzzleRule> result)
    {
        var list = new List<INumericPuzzleRule>();
        result = Array.Empty<INumericPuzzleRule>();

        foreach (var line in parsed)
        {
            var e = line.Value;
            if (e.Value.Type != SyntaxElementType.Operator)
                return new YourPuzzleInterpreterError(line.Line, e, YourPuzzleInterpreterErrorType.NoOperator);

            if (e.Value.Priority != 0)
                return new YourPuzzleInterpreterError(line.Line, e, YourPuzzleInterpreterErrorType.NoAssignment);

            CheckForExpected(e, line.Line);
        }
        
        result = list;
        return YourPuzzleInterpreterError.None;
    }
    
    private static InterpreterError<ISyntaxElement, YourPuzzleInterpreterErrorType>? CheckForExpected(
        BinaryTreeNode<ISyntaxElement> btn, int line)
    {
        if (btn.Value is not SyntaxOperator opl) return YourPuzzleInterpreterError.None;
        if (opl.ExpectedOnLeft == SyntaxElementType.None && btn.Left is not null)
            return new YourPuzzleInterpreterError(line, btn.Left, YourPuzzleInterpreterErrorType.UnexpectedValue);
        if (btn.Left is null)
            return new YourPuzzleInterpreterError(line, btn, YourPuzzleInterpreterErrorType.MissingValue);
        if((opl.ExpectedOnLeft & btn.Left.Value.Type) == 0)
            return new YourPuzzleInterpreterError(line, btn.Left, YourPuzzleInterpreterErrorType.WrongType);

        var e = CheckForExpected(btn.Left, line);
        if (e != YourPuzzleInterpreterError.None) return e;
        
        if (btn.Value is not SyntaxOperator opr) return YourPuzzleInterpreterError.None;
        if (opr.ExpectedOnLeft == SyntaxElementType.None && btn.Left is not null)
            return new YourPuzzleInterpreterError(line, btn.Left, YourPuzzleInterpreterErrorType.UnexpectedValue);
        if (btn.Left is null)
            return new YourPuzzleInterpreterError(line, btn, YourPuzzleInterpreterErrorType.MissingValue);
        if((opr.ExpectedOnLeft & btn.Left.Value.Type) == 0)
            return new YourPuzzleInterpreterError(line, btn.Left, YourPuzzleInterpreterErrorType.WrongType);

        e = CheckForExpected(btn.Left, line);
        if (e != YourPuzzleInterpreterError.None) return e;
        
        return YourPuzzleInterpreterError.None;
    }
}

public enum YourPuzzleInterpreterErrorType
{
    NoOperator, NoAssignment, UnexpectedValue, WrongType, MissingValue
}

public class YourPuzzleInterpreterError : InterpreterError<ISyntaxElement, YourPuzzleInterpreterErrorType>
{
    public YourPuzzleInterpreterError(int line, BinaryTreeNode<ISyntaxElement> element, 
        YourPuzzleInterpreterErrorType error) 
        : base(line, element, error)
    {
    }

    public override string GetErrorMessage()
    {
        throw new NotImplementedException();
    }
}*/