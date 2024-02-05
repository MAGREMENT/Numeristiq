namespace Model.Utility;

public static class MathUtility
{
    public static int MaxIndex(int[] array)
    {
        if (array.Length == 0) return -1;

        var index = 0;
        var max = array[index];

        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] > max)
            {
                index = i;
                max = array[i];
            }
        }

        return index;
    }
}