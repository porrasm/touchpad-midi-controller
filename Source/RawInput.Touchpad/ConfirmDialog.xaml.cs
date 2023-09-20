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
    /// Interaction logic for ConfirmDialog.xaml
    /// </summary>
    public partial class ConfirmDialog : Window {
        public bool Result { get; private set; }

        public ConfirmDialog(string message) {
            InitializeComponent();
            messageTextBlock.Text = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            Result = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Result = false;
            Close();
        }
    }
}
