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
    /// Interaction logic for MultiOptionSelector.xaml
    /// </summary>
    public partial class MultiOptionSelector : Window {
        private readonly List<string> _options;
        private readonly Action<int> _callback;

        public MultiOptionSelector(List<string> options, Action<int> callback) {
            InitializeComponent();
            _options = options;
            _callback = callback;
            optionsList.ItemsSource = _options;
        }

        private void optionsList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _callback(optionsList.SelectedIndex);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            _callback(-1);
            Close();
        }
    }
}
