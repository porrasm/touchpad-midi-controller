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
using System.Windows.Shapes;

namespace RawInput.Touchpad {
    /// <summary>
    /// Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditor : Window {
        private readonly Func<string, bool> _onConfirm;

        public TextEditor(string text, Func<string, bool> onConfirm) {
            InitializeComponent();
            textBox.Text = text;
            _onConfirm = onConfirm;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            var valid = _onConfirm(textBox.Text);
            if (valid) {
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
