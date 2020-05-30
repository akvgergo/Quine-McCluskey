using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quine_McCluskey.Framework
{

    /// <summary>
    /// Elvégzi a teljes számítási folyamatot. Inicializálhatjuk egy 64bites int tömbbel amiből létrehozza az első oszlopot.
    /// May or may not crash on input counts above 25. Kevés az angol betű a full potenciálhoz ¯\_(ツ)_/¯
    /// </summary>
    public class Q_McC
    {

        public TableSegment[] Columns { get; }

        /// <summary>
        /// Létrehozza a legelső oszlopot, az elemeket szortírozva és csoportozva.
        /// </summary>
        /// <param name="minterms"></param>
        public Q_McC(long[] minterms)
        {
            var weightGroups = minterms.OrderBy(N => N) //szortírozunk nagyságrendbe
                .Select(N => new PIC(new[] { N }, 0, 0)) //a számokból létrehozzuk az alap adatstruktúrát az oszlopok soraihoz 
                .GroupBy(minterm => Ops.PopCnt(minterm.Minterms[0])) //csoportosítjuk Hamming súly alapján, ez stabil művelet tehát a sorbarendezés marad
                .Select(pic => new WeightGroup(pic.ToArray())) //Létrehozzuk a súlycsoportokat
                .ToArray(); //majd tömböt csinálunk belőle

            Columns = new TableSegment[weightGroups.Length]; // az oszlopok száma megegyezik a hamming súly csoportok számával

            // a legelső oszlop. A számítási művelet időt vehet igénybe, ezért az külön metódus.
            Columns[0] = new TableSegment(weightGroups);
        }

        /// <summary>
        /// Itt történik a csoda. Ajánlott Async-ba futtatni, mivel időbe kerülhet
        /// </summary>
        public void CreateTable(Progress<Q_McC_ProgressReport> progReport)
        {
            //Egy oszlop befelyezése, valamint prím implikáns felfedezése olyan infó,
            //amit jó tudni ha esetleg sokáig futna a program
            IProgress<Q_McC_ProgressReport> rep = progReport;
            Q_McC_ProgressReport repInfo = new Q_McC_ProgressReport(1, 0);

            WeightGroup[] nextCol = new WeightGroup[Columns[0].Groups.Length - 1];

            //A második oszlopot tekinthetjük különleges estnek, mivel ott jön létre először a joinDiffs mező a sorokban.
            //Azután bitműveletekkel minden pofon egyszerű.
            for (int i = 0; i < Columns[0].Groups.Length - 1; i++)
            {
                //ezzel a nagyobb súly feltétel megvan, pusztán az algoritmus természetéből
                var highSeg = Columns[0].Groups[i];
                var lowSeg = Columns[0].Groups[i + 1];
                List<PIC> nextColSeg = new List<PIC>();
                foreach (var highrow in highSeg)
                {
                    foreach (var lowRow in lowSeg)
                    {
                        //Nem érdemes folytatni ha a felső szám nagyobb...
                        if (highrow.Minterms[0] > lowRow.Minterms[0]) continue;
                        //ha kettő hatványa, nyertünk
                        if (Ops.isPow2(lowRow.Minterms[0] - highrow.Minterms[0]))
                        {
                            var prep = highrow + lowRow;
                            prep.JoinDiffs = lowRow.Minterms[0] - highrow.Minterms[0];
                            nextColSeg.Add(prep);
                        }
                    }
                }
                nextCol[i] = new WeightGroup(nextColSeg.ToArray());
            }

            //A második sor kész, mehet a menet.
            Columns[1] = new TableSegment(nextCol);
            repInfo.ColumnCount++;
            repInfo.PrimeImplicantCount = Columns[0].SelectMany(wg => wg).Count(pic => !pic.Eliminated);
            rep.Report(repInfo);

            for (int i = 2; i < Columns.Length; i++)
            {
                //Ha korán sikerült a teljes redukció, ne fussunk üresjáratban.
                if (Columns[i - 1].Count(wg => wg.Rows.Length > 0) < 2)
                    return;

                WeightGroup[] nextSeg = new WeightGroup[Columns[i - 1].Groups.Length - 1];
                var prevCol = Columns[i - 1];
                for (int j = 0; j < prevCol.Groups.Length - 1; j++)
                {

                    var highSeg = prevCol.Groups[0];
                    var lowSeg = prevCol.Groups[1];
                    List<PIC> nextColSeg = new List<PIC>();
                    foreach (var highrow in highSeg)
                    {
                        foreach (var lowRow in lowSeg)
                        {
                            if (highrow.JoinDiffs == lowRow.JoinDiffs)
                            {
                                nextColSeg.Add(highrow + lowRow);
                            }
                        }
                    }
                    nextSeg[j] = new WeightGroup(nextColSeg.ToArray());
                }
                Columns[i] = new TableSegment(nextSeg);

                repInfo.ColumnCount++;
                repInfo.PrimeImplicantCount += Columns[0].SelectMany(wg => wg).Count(pic => !pic.Eliminated);
                rep.Report(repInfo);

            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var column in Columns)
            {
                sb.Append(column?.ToString());
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// A táblázat kitöltéséhez folyamatinfó
    /// </summary>
    public class Q_McC_ProgressReport
    {
        public int ColumnCount { get; set; } = 1;
        public int PrimeImplicantCount { get; set; } = 0;

        public Q_McC_ProgressReport(int Ccount, int PICount)
        {
            ColumnCount = Ccount;
            PrimeImplicantCount = PICount;
        }

        public override string ToString()
        {
            return string.Format("Columns Created: {0}\nPrime implicants found:{1}", ColumnCount, PrimeImplicantCount);
        }
    }
}
