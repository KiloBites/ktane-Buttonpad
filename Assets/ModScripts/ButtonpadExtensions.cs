using System;
using System.Collections.Generic;

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
}