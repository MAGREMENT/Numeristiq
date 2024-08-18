using System.Text;

namespace Repository.Files.Types;

public class ThemeNativeType : IFileType<List<ThemeDto>>
{
    public string Extension => ".nqth";
    public void Write(Stream stream, List<ThemeDto> DAO)
    {
        foreach (var t in DAO)
        {
            var nameBytes = Encoding.UTF8.GetBytes(t.Name);
            stream.Write(BitConverter.GetBytes(nameBytes.Length));
            stream.Write(nameBytes);
            foreach (var color in t.AllColors())
            {
                stream.Write(BitConverter.GetBytes(color));
            }

            stream.Write(BitConverter.GetBytes(-1));
        }
    }

    public List<ThemeDto> Read(Stream stream)
    {
        var intSize = sizeof(int);
        var intBuffer = new byte[intSize];
        var n = stream.Read(intBuffer, 0, intSize);
        List<ThemeDto> list = new();
        while (n == intSize)
        {
            var nameSize = BitConverter.ToInt32(intBuffer);
            var nameBuffer = new byte[nameSize];
            n = stream.Read(nameBuffer, 0, nameSize);
            if (n != nameSize) break;

            var name = Encoding.UTF8.GetString(nameBuffer);
            var colors = GetColors(stream, intSize, intBuffer);
            if(colors.Count != ThemeDto.ColorCount) break;

            list.Add(new ThemeDto(name, colors));
            n = stream.Read(intBuffer, 0, intSize);
        }

        return list;
    }

    private static List<int> GetColors(Stream stream, int intSize, byte[] intBuffer)
    {
        List<int> result = new();
        var n = stream.Read(intBuffer, 0, intSize);
        while (n == intSize)
        {
            var color = BitConverter.ToInt32(intBuffer);
            if (color == -1) return result;

            result.Add(color);
            n = stream.Read(intBuffer, 0, intSize);
        }

        return result;
    }
}