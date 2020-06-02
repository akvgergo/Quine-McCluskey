using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace quine_McCluskey.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Q_McC Current;
        int NumBoxCount { get; set; } = 1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void HandleBoxInputs(object obj, TextChangedEventArgs eventArgs)
        {
            var numBox = obj as NumberBox;
            if (!long.TryParse(numBox.Text, out _) && numBox.Text != "") return;

            var boxes = InputBox.Children.Cast<NumberBox>();

            if (boxes.Select(nb => nb.Text).Distinct().Count() != NumBoxCount)
            {
                if (boxes.Last().Text == "")
                {
                    InputBox.Children.Remove(boxes.Last());
                    NumBoxCount--;
                }
                return;
            }

            int emptyCount = 0;
            foreach (var nb in boxes.Reverse())
            {
                if (nb.Text == "") emptyCount++;
                if (emptyCount > 1)
                {
                    break;
                }
            }

            if (boxes.Last().Text != "" && emptyCount == 0)
            {
                var newInput = new NumberBox()
                {
                    Margin = FirstInput.Margin,
                };
                newInput.TextChanged += (o, e) => HandleBoxInputs(o, e);
                InputBox.Children.Add(newInput);
                NumBoxCount++;
            }
        }

        List<long> GetInputs()
        {
            return InputBox.Children.Cast<NumberBox>().Select(nb => nb.Value).Distinct().ToList();
        }

        void SetInputs(IEnumerable<long> inputs)
        {
            int index = 0;
            foreach (var val in inputs.OrderBy(N => N).Distinct())
            {
                ((NumberBox)InputBox.Children[index++]).Text = val.ToString();
            }
        }

        private void RandomB_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            long[] values = new long[random.Next(30, 100)];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = random.Next(0, 500);
            }
            SetInputs(values);
        }

        private void StartB_Click(object sender, RoutedEventArgs e)
        {
            Current = new Q_McC(GetInputs().ToArray());

            TableView.Children.Clear();
            ImplicantsView.Children.Clear();

            Progress<Q_McC_ProgressReport> progress = new Progress<Q_McC_ProgressReport>();
            progress.ProgressChanged += (obj, eArgs) => TaskLbl.Content = eArgs.ToString();

            Current.CreateTable(progress);

            for (int i = 0; i < Current.Columns.Count; i++)
            {
                Border brd = new Border() { BorderThickness = new Thickness(3), BorderBrush = Brushes.Gray };
                Label lbl = new Label() { Content = Current.Columns[i].ToString() };
                brd.Child = lbl;
                TableView.Children.Add(brd);
            }

            foreach (var pic in Current.PrimeImplicates)
            {
                ImplicantsView.Children.Add(new TextBlock() { Text = pic.ToString() });
            }
            ImplicantsView.Children.Add(new TextBlock() { Text = "-------------------------------" });

            Current.GetMinimumImplicants();

            foreach (var pic in Current.NecessaryPIs)
            {
                ImplicantsView.Children.Add(new TextBlock() { Text = pic.ToString() });
            }
        }

        private void WreckB_Click(object sender, RoutedEventArgs e)
        {
            List<long> vs = new List<long>();
            Random rnd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                vs.Add(rnd.Next(0, 2000));
            }
            Current = new Q_McC(Enumerable.Range(7, 1500).Select(n => (long)n).ToArray());

            TableView.Children.Clear();
            Progress<Q_McC_ProgressReport> progress = new Progress<Q_McC_ProgressReport>();
            progress.ProgressChanged += (obj, eArgs) => TaskLbl.Content = eArgs.ToString();
            progress.ProgressChanged += (t, r) =>
            {
                TableView.Children.Clear();
                for (int i = 0; i < Current.Columns.Count; i++)
                {
                    Border brd = new Border() { BorderThickness = new Thickness(3), BorderBrush = Brushes.Gray };
                    Label lbl = new Label() { Content = Current.Columns[i].ToString() };
                    brd.Child = lbl;
                    TableView.Children.Add(brd);
                }
            };
            Application.Current.Dispatcher.Invoke(() => Current.CreateTable(progress), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
