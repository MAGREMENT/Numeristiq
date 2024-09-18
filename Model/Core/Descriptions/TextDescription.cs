namespace Model.Core.Descriptions;

public class TextDescription<TDisplayer> : IDescription<TDisplayer> where TDisplayer : IDescriptionDisplayer
{
    private readonly string _paragraph;
    
    public TextDescription(string text)
    {
        _paragraph = text;
    }

    public void Display(TDisplayer displayer)
    {
        displayer.AddParagraph(_paragraph);
    }
}