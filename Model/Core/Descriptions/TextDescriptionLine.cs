namespace Model.Core.Descriptions;

public class TextDescriptionLine : IDescriptionLine
{
    public TextDescriptionLine(string text)
    {
        Text = text;
    }

    public string Text { get; }
}