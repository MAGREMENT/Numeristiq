namespace Model.Helpers.Descriptions;

public class TextImageDescriptionLine : IDescriptionLine
{
    public TextImageDescriptionLine(string text, string imagePath, TextDisposition disposition)
    {
        Text = text;
        ImagePath = imagePath;
        Disposition = disposition;
    }

    public string Text { get; }
    public string ImagePath { get; }
    public TextDisposition Disposition { get; }
}

public enum TextDisposition
{
    Left, Right
}