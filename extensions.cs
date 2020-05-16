using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Data;
using NLog;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RouteNavigation
{
    public static class Extensions
    {
        private static object listLocker = new object();
        public static IList<T> Shuffle<T>(this IList<T> list, Random rng)
        {
            lock (listLocker)
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
        }

        public static List<Location> DeepClone(this List<Location> list)
        {
            List<Location> locations = list.ConvertAll(l => l.DeepClone(l));
            return locations;
        }

        public static List<RouteCalculator> SortByDistanceAsc(this List<RouteCalculator> list)
        {
            lock (listLocker)
            {
                list.Sort((x, y) => x.metadata.routesLengthMiles.CompareTo(y.metadata.routesLengthMiles));
                return list;
            }
        }

        public static List<RouteCalculator> SortByDistanceDesc(this List<RouteCalculator> list)
        {
            lock (listLocker)
            {
                list.Sort((x, y) => y.metadata.routesLengthMiles.CompareTo(x.metadata.routesLengthMiles));
                return list;
            }
        }


        public static List<RouteCalculator> SortByFitnessAsc(this List<RouteCalculator> list)
        {
            lock (listLocker)
            {
                list.Sort((x, y) => x.metadata.fitnessScore.CompareTo(y.metadata.fitnessScore));
                return list;
            }
        }

        public static List<RouteCalculator> SortByFitnessDesc(this List<RouteCalculator> list)
        {
            lock (listLocker)
            {
                list.Sort((x, y) => y.metadata.fitnessScore.CompareTo(x.metadata.fitnessScore));
                return list;
            }
        }


        public static void RoundDataTable(DataTable dataTable, int roundInt = 2)
        {
            foreach (DataColumn dc in dataTable.Columns)
            {
                if (dc.DataType.Equals(typeof(double)))
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        if (dr[dc] != DBNull.Value)
                            dr[dc] = Math.Round(Convert.ToDouble(dr[dc]), roundInt);
                    }
                }
            }
        }
    }
}