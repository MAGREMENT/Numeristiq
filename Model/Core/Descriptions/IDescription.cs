namespace Model.Core.Descriptions;

public interface IDescription<in TDisplayer> where TDisplayer : IDescriptionDisplayer
{
    void Display(TDisplayer displayer);
}

public interface IDescriptionDisplayer
{
    void AddParagraph(string s);
}