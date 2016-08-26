using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JohnnyCache.Helpers
{
    public static class StringHelper
    {
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
