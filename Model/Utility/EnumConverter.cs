﻿using System;
using System.Linq.Expressions;

namespace Model.Utility;

public static class EnumConverter
{
    public static int ToInt<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        return StaticGenericCache<TEnum>.ToIntFunction(value);
    }

    public static TEnum ToEnum<TEnum>(int value) where TEnum : struct, Enum
    {
        return StaticGenericCache<TEnum>.ToEnumFunction(value);
    }

    public static string[] ToStringArray<TEnum>(IStringConverter converter) where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();
        var result = new string[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            result[i] = converter.Convert(values[i].ToString());
        }

        return result;
    }

    public static int[] ToIntArray<TEnum>() where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();
        var result = new int[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            result[i] = ToInt(values[i]);
        }

        return result;
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