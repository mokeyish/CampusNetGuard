using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusNetGuard
{
    public static class Utils
    {
        public static bool ComparisonHashAssign<T>(ref T a, T b)
        {
            if (b == null) return false;
            if (a == null||a.GetHashCode() != b.GetHashCode())
            {
                a = b;
                return true;
            }
            return false;

        }
        public static bool ComparisonAssign<T>(ref T a, T b)
        {
            if (b == null) return false;
            if (a == null)
            {
                a = b;
                return true;
            }
            bool isChanged = !a.Equals(b);
            a = b;
            return isChanged;
        }
        public static bool IsEqualByteArray(byte[] a,byte[] b)
        {
            if (a == b) return true;
            if (a == null || b == null || a.Length != b.Length) return false;
            bool r = false;
            for (int i = 0; i < a.Length; i++)
            {
                if (!(r= a[i] == b[i])) return false;
            }
            return r;
        }
        public static byte[] CutByteArray(byte[] src,int offset,int count)
        {
            byte[] r = new byte[count];
            for (int i = 0; i < count; i++) r[i] = src[offset + i];
            return r;
        }

        public static bool CompareByteArray(byte[] aArray, byte[] bArray)
        {
            return CompareByteArray(aArray, 0, bArray, 0,bArray.Length);
        }
        public static bool CompareByteArray(byte[] aArray, byte[] bArray, int bOffset, int compareLength)
        {
            return CompareByteArray(aArray, 0, bArray, bOffset, compareLength);
        }

        public static bool CompareByteArray(byte[] aArray, byte[] bArray, int bOffset)
        {
            return CompareByteArray(aArray, 0, bArray, bOffset, aArray.Length < bArray.Length ? aArray.Length : bArray.Length);
        }
        public static bool CompareByteArray(byte[] aArray, int aOffset, byte[] bArray, int bOffset, int compareLength)
        {
            bool r = false;
            for (int i = 0; i < compareLength; i++) if (!(r = aArray[aOffset + i] == bArray[bOffset + i])) break;
            return r;
        }
    }
}
