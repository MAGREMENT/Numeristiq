using System;
using System.Collections.Generic;
using Model.Utility.Collections;

namespace Model.YourPuzzles.Syntax;

public class SyntaxInterpreter
{
    public InterpreterError? TryInterpret(IReadOnlyList<ParsedLine> parsed,
        out IReadOnlyList<INumericPuzzleRule> result)
    {
        var list = new List<INumericPuzzleRule>();
        result = Array.Empty<INumericPuzzleRule>();

        foreach (var line in parsed)
        {
            var e = line.Value;
            if (e.Value.Type != SyntaxElementType.Operator)
                return new InterpreterError(line.Line, e, InterpreterErrorType.NoOperator);

            if (e.Value.Priority != 0)
                return new InterpreterError(line.Line, e, InterpreterErrorType.NoAssignment);

            CheckForExpected(e, line.Line);
        }
        
        result = list;
        return InterpreterError.None;
    }

    private static InterpreterError? CheckForExpected(BinaryTreeNode<ISyntaxElement> btn, int line)
    {
        if (btn.Value is not ISyntaxOperator opl) return InterpreterError.None;
        if (opl.ExpectedOnLeft == SyntaxElementType.None && btn.Left is not null)
            return new InterpreterError(line, btn.Left, InterpreterErrorType.UnexpectedValue);
        if (btn.Left is null)
            return new InterpreterError(line, btn, InterpreterErrorType.MissingValue);
        if((opl.ExpectedOnLeft & btn.Left.Value.Type) == 0)
            return new InterpreterError(line, btn.Left, InterpreterErrorType.WrongType);

        var e = CheckForExpected(btn.Left, line);
        if (e != InterpreterError.None) return e;
        
        if (btn.Value is not ISyntaxOperator opr) return InterpreterError.None;
        if (opr.ExpectedOnLeft == SyntaxElementType.None && btn.Left is not null)
            return new InterpreterError(line, btn.Left, InterpreterErrorType.UnexpectedValue);
        if (btn.Left is null)
            return new InterpreterError(line, btn, InterpreterErrorType.MissingValue);
        if((opr.ExpectedOnLeft & btn.Left.Value.Type) == 0)
            return new InterpreterError(line, btn.Left, InterpreterErrorType.WrongType);

        e = CheckForExpected(btn.Left, line);
        if (e != InterpreterError.None) return e;
        
        return InterpreterError.None;
    }
}

public enum InterpreterErrorType
{
    NoOperator, NoAssignment, UnexpectedValue, WrongType, MissingValue
}

public class InterpreterError : ISyntaxError
{
    public static readonly InterpreterError? None = null;

    public InterpreterError(int line, BinaryTreeNode<ISyntaxElement> element, InterpreterErrorType error)
    {
        Line = line;
        Element = element;
        Error = error;
    }

    public int Line { get; }
    public BinaryTreeNode<ISyntaxElement> Element { get; }
    public InterpreterErrorType Error { get; }
    
    public (int, int, int) FindWhereToHighlight(string text)
    {
        throw new System.NotImplementedException();
    }

    public string GetErrorMessage()
    {
        throw new System.NotImplementedException();
    }
}