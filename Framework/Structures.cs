using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents.DocumentStructures;

namespace quine_McCluskey.Framework
{

    /// <summary>
    /// Prime-Implicant Candidate. Minden oszlop egy sorát tárolja.
    /// </summary>
    public class PIC
    {
        /// <summary>
        /// Az összes eddig összevont szám.
        /// </summary>
        public long[] Minterms { get; set; }

        /// <summary>
        /// Az első oszlop után itt tároljuk a különbségi értékeket.
        /// </summary>
        public long JoinDiffs { get; set; }
        
        /// <summary>
        /// Mely oszlopba van a sor. Számon kell tartani hogy a sorok mejelölhessék magukat összevontként.
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Igaz, ha ez a sor már nem lehet prím implikáns.
        /// </summary>
        public bool Eliminated { get; set; }

        public PIC(long[] minterms, long joindiffs, int columnIndex)
        {
            Minterms = minterms;
            JoinDiffs = joindiffs;
            ColumnIndex = columnIndex;
        }

        /// <summary>
        /// Ezzel masszívan leegyszerűsíthető a folyamat. Az összevonás mindössze két sor összeadása, valamint ha nem volt összeadás, megállapíthatjuk
        /// hogy a sor egy prím implikáns.
        /// </summary>
        public static PIC operator +(PIC higher, PIC lower) {
            higher.Eliminated = lower.Eliminated = true;
            return new PIC(higher.Minterms.Concat(lower.Minterms).ToArray(), higher.JoinDiffs | Math.Abs(higher.Minterms[0] - lower.Minterms[0]), higher.ColumnIndex + 1);
        }

        /// <summary>
        /// Megállapítja hogy két sor összevonható-e.
        /// </summary>
        public static bool CanJoin(PIC higher, PIC lower)
        {
            //Nem így specifikált, de praktikus és gyors előszűrés.
            if (higher.JoinDiffs != lower.JoinDiffs) return false;
            //legegyszerűbb és leggyorsabb hogy megállapítsuk, a felső szám(-összevonás) nagyobb-e. Elvileg mindíg rendezett a sor,
            //így nem kell végig menni
            if (higher.Minterms[0] > lower.Minterms[0]) return false;
            //Ellenőrizzük az összes mintermszámot. ha mindegyik különbsége egyenlő, a sorok összevonhatók.
            var diffSum = higher.Minterms.Zip(lower.Minterms, (l, r) => l - r).Distinct().Select(n => Math.Abs(n));
            return diffSum.Count() == 1 && Ops.isPow2(diffSum.First());
        }

        //formázás ugyanaz mint a munkaf-ben
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Join(", ", Minterms));
            if (JoinDiffs != 0)
                sb.Append(string.Format(" ({0})", string.Join(", ", Ops.BreakupImplicants(JoinDiffs))));
            if (Eliminated)
                sb.Append("✓");
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            var other = obj as PIC;
            if (obj == null) return false;
            if (other.Minterms.Length != Minterms.Length) return false;
            return JoinDiffs == other.JoinDiffs && other.Minterms.Except(Minterms).Count() == 0;
        }

        // VS egész jó hash funkciókat generál, ez nem az én kreálmányom hanem autógenerált
        public override int GetHashCode()
        {
            int hashCode = -1872184839;
            hashCode ^= (int)JoinDiffs * (int)Minterms.Last();
            return hashCode;
        }

        public static bool operator ==(PIC left, PIC right)
        {
            return EqualityComparer<PIC>.Default.Equals(left, right);
        }

        public static bool operator !=(PIC left, PIC right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Egy egységbe tárolja egy oszlop szegmenseit.
    /// </summary>
    public class WeightGroup : IEnumerable<PIC>
    {

        public PIC[] Rows { get; set; }

        public WeightGroup(PIC[] rows)
        {
            Rows = rows;
        }

        public IEnumerator<PIC> GetEnumerator()
        {
            for (int i = 0; i < Rows.Length; i++)
            {
                yield return Rows[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Rows)
            {
                sb.Append(item);
                sb.Append('\n');
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// A táblázat-rendszer egy oszlopa.
    /// </summary>
    public class TableSegment : IEnumerable<WeightGroup>
    {
        public WeightGroup[] Groups { get; set; }

        public TableSegment(WeightGroup[] groups)
        {
            Groups = groups;
        }

        public IEnumerator<WeightGroup> GetEnumerator()
        {
            for (int i = 0; i < Groups.Length; i++)
            {
                yield return Groups[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var group in Groups)
            {
                sb.Append(group);
                sb.Append("-------\n");
            }
            return sb.ToString();
        }
    }
}
