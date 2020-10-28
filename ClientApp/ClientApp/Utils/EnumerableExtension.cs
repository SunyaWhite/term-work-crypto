using System;
using System.Collections.Generic;
using System.Text;

namespace ClientApp.Utils
{
    public static class EnumerableExtension
    {
        public static string Display(this IEnumerable<byte> array)
        {
            var sb = new StringBuilder();

            foreach (var elem in array)
            {
                sb.Append($"{elem} ");
            }
            
            return sb.ToString();
        }

        public static byte[] InsertCorruption(this byte[] array)
        {
            var rand = new Random();
            var pos = rand.Next(0, array.Length);
            var newValue = (byte) rand.Next(0, 255);
            array[pos] = newValue;
            
            return array;
        }
    }
}