using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ButtonpadExtensions
{
    public static IEnumerable<int> IndicesOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        
        var finalList = new List<int>();

        var index = 0;

        foreach (var item in source)
        {
            if (predicate(item))
                finalList.Add(index);

            index++;
        }
        
        return finalList;
    }

    public static T[] GetColumn<T>(this IEnumerable<T> source, int startIndex)
    {
        var list = source.ToList();

        return Enumerable.Range(0, 35).Select(x => list[x * 8 + startIndex]).ToArray();
    }
    
    public static Color GetDarkerShade(this Color color) => new Color(color.r > 0 ? color.r - 0.2f : color.r, color.g > 0 ? color.g - 0.2f : color.g, color.b > 0 ? color.b - 0.2f : color.b);
}