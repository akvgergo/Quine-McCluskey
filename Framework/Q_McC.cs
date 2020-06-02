using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace quine_McCluskey.Framework
{

    /// <summary>
    /// Elvégzi a teljes számítási folyamatot. Inicializálhatjuk egy 64bites int tömbbel amiből létrehozza az első oszlopot.
    /// May or may not crash on input counts above 25. Kevés az angol betű a full potenciálhoz ¯\_(ツ)_/¯
    /// </summary>
    public class Q_McC
    {
        /// <summary>
        /// Az eredetileg megkapott sorszámok, rendezve
        /// </summary>
        public long[] Minterms { get; }

        /// <summary>
        /// Az összes oszlop
        /// </summary>
        public List<TableSegment> Columns { get; }
        
        /// <summary>
        /// Az összes megtalált prímimplikáns, a táblák létrehozásakor kap értékeket
        /// </summary>
        public List<PIC> PrimeImplicates { get; }

        /// <summary>
        /// Az egyszerűsítéshez szükséges prím implikánsok
        /// </summary>
        public List<PIC> NecessaryPIs { get; protected set; }

        /// <summary>
        /// Létrehozza a legelső oszlopot, az elemeket szortírozva és csoportozva.
        /// </summary>
        /// <param name="minterms"></param>
        public Q_McC(long[] minterms)
        {
            Minterms = minterms.OrderBy(N => N).Distinct().ToArray(); //szortírozunk nagyságrendbe

            var weightGroups = Minterms
                .Select(N => new PIC(new[] { N }, 0, 0)) //a számokból létrehozzuk az alap adatstruktúrát az oszlopok soraihoz 
                .GroupBy(minterm => Ops.PopCnt(minterm.Minterms[0])) //csoportosítjuk Hamming súly alapján, ez stabil művelet tehát a sorbarendezés marad
                .Select(pic => new WeightGroup(pic.ToArray())) //Létrehozzuk a súlycsoportokat
                .ToArray(); //majd tömböt csinálunk belőle

            Columns = new List<TableSegment>(weightGroups.Length); // az oszlopok száma (gyakran) megegyezik a hamming súly csoportok számával
            PrimeImplicates = new List<PIC>();

            // a legelső oszlop. A számítási művelet időt vehet igénybe, ezért az külön metódus.
            Columns.Add(new TableSegment(weightGroups));
        }

        /// <summary>
        /// Itt történik a csoda. Ajánlott Async-ba futtatni, mivel időbe kerülhet
        /// </summary>
        public async void CreateTable(Progress<Q_McC_ProgressReport> progReport)
        {
            //Egy oszlop befelyezése, valamint prím implikáns felfedezése olyan infó,
            //amit jó tudni ha esetleg sokáig futna a program.
            IProgress<Q_McC_ProgressReport> rep = progReport;
            Q_McC_ProgressReport repInfo = new Q_McC_ProgressReport(1, 0);

            int colIndex = 1;
            while (Columns[colIndex - 1].Count(wg => wg.Rows.Length > 1) > 1) //Addig futunk amí ki nem fogyunk összehasonlítható oszlopokból.
            {
                WeightGroup[] nextCol = new WeightGroup[Columns[colIndex - 1].Groups.Length - 1];
                var prevCol = Columns[colIndex - 1];
                for (int j = 0; j < prevCol.Groups.Length - 1; j++)
                {
                    var highSeg = prevCol.Groups[j];
                    var lowSeg = prevCol.Groups[j + 1];
                    HashSet<PIC> nextColSeg = new HashSet<PIC>(); //elkerüljük az ismétlést
                    
                    foreach (var highrow in highSeg)
                    {
                        foreach (var lowRow in lowSeg)
                        {
                            //Lásd: structures.cs
                            //ez mindössze az alap algoritmus, a műveletek többsége ott történik
                            if (PIC.CanJoin(highrow, lowRow))
                            {
                                nextColSeg.Add(highrow + lowRow);
                            }
                        }
                    }
                    nextCol[j] = new WeightGroup(nextColSeg.ToArray());
                }
                //Az elemek melyeket nem sikerült úly oszlopba belerakni számon tartjuk
                PrimeImplicates.AddRange(prevCol.SelectMany(wg => wg).Where(pic => !pic.Eliminated));
                Columns.Add(new TableSegment(nextCol));
                repInfo.ColumnCount++;
                repInfo.PrimeImplicantCount = PrimeImplicates.Count;
                rep.Report(repInfo);
                colIndex++;
            }

            PrimeImplicates.AddRange(Columns.Last().SelectMany(wg => wg));
            repInfo.PrimeImplicantCount = PrimeImplicates.Count;
            rep.Report(repInfo);
        }

        /// <summary>
        /// Megkeresi a legegyszerűbb alakot
        /// </summary>
        public List<PIC> GetMinimumImplicants()
        {
            List<PIC> list = new List<PIC>();

            PrimeImplicates.Reverse();
            HashSet<long> mins = Minterms.ToHashSet();
            int count = 0;
            while (mins.Count != 0)
            {
                mins.ExceptWith(PrimeImplicates[count].Minterms);
                list.Add(PrimeImplicates[count++]);
            }
            NecessaryPIs = list;

            return list;
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
