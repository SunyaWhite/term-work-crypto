using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace ClientApp.Utils
{
    public static class EnumerableExtension
    {
        public static string Display(this byte[] array)
        { 
            return Convert.ToBase64String(array);
        }

        public static byte[] InsertCorruption(this byte[] array, int limit)
        {
            var rand = new Random();
            var pos = rand.Next(0, limit);
            var newValue = (byte) rand.Next(0, 255);
            array[pos] = newValue;
            
            return array;
        }
        
        public static string InsertCorruption(this string text)
        {
            // Share some memory
            var bytesCount = Encoding.Default.GetByteCount(text);
            var array = ArrayPool<byte>.Shared.Rent(bytesCount);
            Encoding.Default.GetBytes(text, array);
            // Insert corruption and getting corruptedText
            array.InsertCorruption(bytesCount);
            var corruptedText = Encoding.Default.GetString(array, 0, bytesCount);
            // Return of shared memory
            ArrayPool<byte>.Shared.Return(array);
            return corruptedText;
        }
    }
}