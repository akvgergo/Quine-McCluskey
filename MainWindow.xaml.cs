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

        Q_McC Current;

        public MainWindow()
        {
            InitializeComponent();
            Random rnd = new Random();
            for (int i = 0; i < 128; i++)
            {
                InputBox.Children.Add(new TextBox() {
                    MinWidth = 20,
                }); ;
            }
        }

        private void RandomB_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            foreach (var textbox in InputBox.Children)
            {
                var tb = textbox as TextBox;
                if (random.NextDouble() > .0)
                {
                    tb.Text = random.Next(0, 200).ToString();
                } else
                {
                    tb.Text = "0";
                }
                
            }
        }

        private void StartB_Click(object sender, RoutedEventArgs e)
        {
            List<long> list = new List<long>();
            foreach (var textbox in InputBox.Children)
            {
                var tb = textbox as TextBox;
                list.Add(long.Parse(tb.Text));
            }

            Current = new Q_McC(list.ToArray());

            ColumnView.Children.Clear();

            Progress<Q_McC_ProgressReport> progress = new Progress<Q_McC_ProgressReport>();
            progress.ProgressChanged += (o, eArgs) => taskLbl.Content = eArgs.ToString();
            Current.CreateTable(progress);

            foreach (var column in Current.Columns)
            {
                Border brd = new Border() { BorderThickness = new Thickness(3), BorderBrush = Brushes.Gray };
                Label lbl = new Label() { Content = column.ToString() };
                brd.Child = lbl;
                ColumnView.Children.Add(brd);
            }
        }

        private void Wreck_Click(object sender, RoutedEventArgs e)
        {
            List<long> vs = new List<long>();
            Random rnd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                vs.Add(rnd.Next(0, 2000));
            }

            Current = new Q_McC(Enumerable.Range(7, 1500).Select(n => (long)n).ToArray());

            ColumnView.Children.Clear();

            Progress<Q_McC_ProgressReport> progress = new Progress<Q_McC_ProgressReport>();
            progress.ProgressChanged += (o, eArgs) => taskLbl.Content = eArgs.ToString();
            Task.Run(() => Current.CreateTable(progress));
            progress.ProgressChanged += (t, r) =>
            {
                foreach (var column in Current.Columns)
                {
                    Border brd = new Border() { BorderThickness = new Thickness(3), BorderBrush = Brushes.Gray };
                    Label lbl = new Label() { Content = column.ToString() };
                    brd.Child = lbl;
                    ColumnView.Children.Add(brd);
                }
            };
        }
    }
}
