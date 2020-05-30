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
    }
}
