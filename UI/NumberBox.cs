using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace quine_McCluskey.UI
{
    /// <summary>
    /// Az egyszerűsítendő bemeneti értékeknek szövegdoboz. Legyszerűsíti a dolgomat bemenet ellenőrzésnél.
    /// </summary>
    [ToolboxItem("Custom")]
    public class NumberBox : TextBox
    {
        /// <summary>
        /// A jelenleg tárolt érték.
        /// </summary>
        public long Value {
            get; set;
        }

        public NumberBox() : base() {
            MinWidth = 25;
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            long val;
            if (e.Text.All(d => char.IsDigit(d)) && long.TryParse(Text + e.Text, out val) && val >= 0)
            {
                Value = val;
            } else
            {
                e.Handled = true;
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            long val;
            if (!long.TryParse(Text, out val) || val < 0)
            {
                foreach (var change in e.Changes)
                {
                    Text = Text.Remove(change.Offset, change.AddedLength);
                }
                e.Handled = true;
            } else
            {
                Value = val;
            }
        }
    }
}
