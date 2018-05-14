using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
namespace RouteNavigation
{
    public static class extensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;

            while (n > 1)
            {
                int k = (rng.Next(0, n) % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        public static List<RouteCalculator> SortByDistanceAsc(this List<RouteCalculator> list)
        {
            list.Sort((x, y) => x.metadata.routesLengthMiles.CompareTo(y.metadata.routesLengthMiles));
            return list;
        }

        public static List<RouteCalculator> SortByDistanceDesc(this List<RouteCalculator> list)
        {
            list.Sort((x, y) => y.metadata.routesLengthMiles.CompareTo(x.metadata.routesLengthMiles));
            return list;
        }

    }



}