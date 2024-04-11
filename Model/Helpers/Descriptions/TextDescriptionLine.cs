namespace Model.Helpers.Descriptions;

public class TextDescriptionLine : IDescriptionLine
{
    public TextDescriptionLine(string text)
    {
        Text = text;
    }

    public string Text { get; }
}