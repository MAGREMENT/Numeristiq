using System.Collections.Generic;

namespace Model.Utility.Collections;

public class BinaryTreeNode<T>
{
    public T Value { get; set; }
    public BinaryTreeNode<T>? Left { get; set; } = null;
    public BinaryTreeNode<T>? Right { get; set; } = null;

    public void SetLeft(T value)
    {
        Left = new BinaryTreeNode<T>(value);
    }

    public void SetRight(T value)
    {
        Right = new BinaryTreeNode<T>(value);
    }
    
    public IEnumerable<T> EnumerateLeftToRight()
    {
        var result = new List<T>();
        AddLeftToRight(result, this);
        return result;
    }

    private static void AddLeftToRight(List<T> list, BinaryTreeNode<T> node)
    {
        while (true)
        {
            if (node.Left is not null) AddLeftToRight(list, node.Left);
            list.Add(node.Value);
            if (node.Right is not null)
            {
                node = node.Right;
                continue;
            }

            break;
        }
    }

    public BinaryTreeNode(T value)
    {
        Value = value;
    }
}