using System;
using System.Linq.Expressions;

namespace Model.Utility;

public class EnumConverter
{
    public static int ToInt<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        return StaticGenericCache<TEnum>.ToIntFunction(value);
    }

    public static TEnum ToEnum<TEnum>(int value) where TEnum : struct, Enum
    {
        return StaticGenericCache<TEnum>.ToEnumFunction(value);
    }
    
    private static class StaticGenericCache<T>
        where T : struct, Enum
    {
        public static readonly Func<T, int> ToIntFunction = GenerateToIntFunction<T>();

        public static readonly Func<int, T> ToEnumFunction = GenerateToEnumFunction<T>();
    }

    private static Func<TEnum, int> GenerateToIntFunction<TEnum>()
        where TEnum : struct, Enum
    {
        var inputParameter = Expression.Parameter(typeof(TEnum));
        return Expression.Lambda<Func<TEnum, int>>(Expression.Convert(inputParameter, typeof(int)), inputParameter).Compile();
    }

    private static Func<int, TEnum> GenerateToEnumFunction<TEnum>()
    {
        var inputParameter = Expression.Parameter(typeof(int));
        return Expression.Lambda<Func<int, TEnum>>(Expression.Convert(inputParameter, typeof(TEnum)), inputParameter).Compile();
    }
}