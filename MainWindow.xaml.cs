using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using quine_McCluskey.Framework;

namespace quine_McCluskey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Q_McC qmcc = new Q_McC(new[] { 1l, 8l, 3l, 5l, 17l, 12l, 23l, 24l, 7l, 19l, 11l, 21l, 28l });
            Progress<Q_McC_ProgressReport> progress = new Progress<Q_McC_ProgressReport>();
            progress.ProgressChanged += (o, e) => { Console.WriteLine(e); };
            qmcc.CreateTable(progress);
            Console.WriteLine(qmcc);
        }
    }
}
