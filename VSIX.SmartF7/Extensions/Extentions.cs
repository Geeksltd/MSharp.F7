namespace System
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Extensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> list) => list == null || list.Count() == 0;
    }
}