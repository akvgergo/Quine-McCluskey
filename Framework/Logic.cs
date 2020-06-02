using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace quine_McCluskey.Framework
{
    /// <summary>
    /// Gyakori műveleteknek gyűjtőosztáj
    /// </summary>
    public static class Ops
    {

        /// <summary>
        /// Hamming súly számítás
        /// </summary>
        public static int PopCnt(long N)
        {
            int count = 0;
            while (N != 0)
            {
                N = (N - 1) & N; //kivonással valamint ÉS múvelettel sorban eltávolítjuk a legkisebb helyiértékű biteket...
                count++; //...közbe pedig megszámoljuk azokat
            }
            return count;
        }

        /// <summary>
        /// Amikor már csak kettes hatványai alapján vonunk össze, praktikus az értéket egy változóban tárolni, minden összevonási szám egy új bit.
        /// Összeadni egyszerű, csak egy VAGY művelet, szétszedni nehezebb, erre van ez a metódus.
        /// </summary>
        public static List<long> BreakupImplicants(long N) {
            List<long> values = new List<long>(32);
            while (N != 0)
            {
                values.Add((N - 1) & N ^ N); //black magic
                N = (N - 1) & N;
            }
            return values;
        }

        /// <summary>
        /// Ezt csak nem akarom újra meg újra leírni, előreláthatólag gyakran fog kelleni...
        /// </summary>
        public static bool isPow2(long N)
        {
            return ((N - 1) & N) == 0;
        }

        /// <summary>
        /// Oszlop számozáshoz convertálás római számokra.
        /// </summary>
        /// <remarks>
        /// Puszta lustaságból StackOverflow-ról másolva...
        /// </remarks>
        public static string ToRoman(int N)
        {
            if ((N < 0) || (N > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (N < 1) return string.Empty;
            if (N >= 1000) return "M" + ToRoman(N - 1000);
            if (N >= 900) return "CM" + ToRoman(N - 900);
            if (N >= 500) return "D" + ToRoman(N - 500);
            if (N >= 400) return "CD" + ToRoman(N - 400);
            if (N >= 100) return "C" + ToRoman(N - 100);
            if (N >= 90) return "XC" + ToRoman(N - 90);
            if (N >= 50) return "L" + ToRoman(N - 50);
            if (N >= 40) return "XL" + ToRoman(N - 40);
            if (N >= 10) return "X" + ToRoman(N - 10);
            if (N >= 9) return "IX" + ToRoman(N - 9);
            if (N >= 5) return "V" + ToRoman(N - 5);
            if (N >= 4) return "IV" + ToRoman(N - 4);
            if (N >= 1) return "I" + ToRoman(N - 1);
            return null;
        }
    }
}
