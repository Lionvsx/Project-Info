using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_Info
{
    public static class Functions
    {
        public static Image ReadImage(string path)
        {
            
        }

        public static double ConvertToInt(IEnumerable<byte> data)
        {
            double result = 0;
            var enumerable = data.ToList();
            for (int i = 0; i < enumerable.Count; i++)
            {
                result = result * Math.Pow(256, i);
            }
            return result;
        }
    }
}