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
        private Action onConfirm;

        public ConfirmDialog(string message, Action onConfirm) {
            InitializeComponent();
            messageTextBlock.Text = message;
            this.onConfirm = onConfirm;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            onConfirm?.Invoke();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
