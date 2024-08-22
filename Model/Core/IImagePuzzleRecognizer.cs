namespace Model.Core;

public interface IImagePuzzleRecognizer<out T>
{
    T? Recognize(string path);
}