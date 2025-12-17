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

namespace VeterinarniOrdinace.Views
{
    /// <summary>
    /// Interakční logika pro OwnersView.xaml
    /// </summary>
    public partial class OwnersView : UserControl
    {
        public OwnersView()
        {
            InitializeComponent();
        }
        private void OnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void OnlyNumbersPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string))!;
                if (!text.All(char.IsDigit))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }
        private void Phone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb) return;

            tb.TextChanged -= Phone_TextChanged;

            var digits = new string(tb.Text.Where(char.IsDigit).ToArray());

            var formatted = string.Join(" ",
                Enumerable.Range(0, (digits.Length + 2) / 3)
                          .Select(i => digits.Substring(i * 3,
                              Math.Min(3, digits.Length - i * 3)))
            );

            tb.Text = formatted;
            tb.CaretIndex = tb.Text.Length;

            tb.TextChanged += Phone_TextChanged;
        }
    }
}
