using RawInput.Touchpad.Midi;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RawInput.Touchpad {
    public partial class MainWindow : Window {
        public bool TouchpadExists {
            get { return (bool)GetValue(TouchpadExistsProperty); }
            set { SetValue(TouchpadExistsProperty, value); }
        }
        public static readonly DependencyProperty TouchpadExistsProperty =
            DependencyProperty.Register("TouchpadExists", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public string TouchpadContacts {
            get { return (string)GetValue(TouchpadContactsProperty); }
            set { SetValue(TouchpadContactsProperty, value); }
        }
        public static readonly DependencyProperty TouchpadContactsProperty =
            DependencyProperty.Register("TouchpadContacts", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        public string SelectedMidiDevice {
            get { return (string)GetValue(SelectedMidiDeviceProperty); }
            set { SetValue(SelectedMidiDeviceProperty, value); }
        }
        public static readonly DependencyProperty SelectedMidiDeviceProperty =
            DependencyProperty.Register("SelectedMidiDevice", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        public string SelectedConfiguration {
            get { return (string)GetValue(SelectedConfigurationProperty); }
            set { SetValue(SelectedConfigurationProperty, value); }
        }
        public static readonly DependencyProperty SelectedConfigurationProperty =
            DependencyProperty.Register("SelectedConfiguration", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        public MainWindow() {
            InitializeComponent();
        }

        private HwndSource _targetSource;
        private readonly List<string> _log = new();

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            _targetSource = PresentationSource.FromVisual(this) as HwndSource;
            _targetSource?.AddHook(WndProc);

            TouchpadExists = TouchpadHelper.Exists();

            _log.Add($"Precision touchpad exists: {TouchpadExists}");

            if (TouchpadExists) {
                var success = TouchpadHelper.RegisterInput(_targetSource.Handle);
                Touchpad.Restart();

                _log.Add($"Precision touchpad registered: {success}");
            }

            ApplicationState.Load();
            Reload();

            ApplicationState.OnConfigChanged += Reload;
        }

        protected override void OnClosing(CancelEventArgs e) {
            ApplicationState.OnConfigChanged -= Reload;
        }

        private string MidiStateString() => $"{(Touchpad.Enabled ? "Sending to" : "Paused")}: {ApplicationState.Instance.selectedMidiDevice}";

        public void Reload() {
            SelectedMidiDevice = MidiStateString();
            SelectedConfiguration = ApplicationState.Instance.selectedConfiguration;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case TouchpadHelper.WM_INPUT:
                    var contacts = TouchpadHelper.ParseInput(lParam);
                    Touchpad.RegisterContact(contacts);
                    TouchpadContacts = string.Join(Environment.NewLine, contacts.Select(x => x.ToString()));

                    _log.Add("---");
                    _log.Add(TouchpadContacts);
                    break;
            }
            return IntPtr.Zero;
        }

        private void Act_SelectMidi(object sender, RoutedEventArgs e) {
            List<string> options = MIDI.GetMidiDevices();

            if (options.Count == 0) {
                Console.WriteLine("No MIDI devices found");
                return;
            }

            var selector = new MultiOptionSelector(options, index => {
                if (index >= 0) {
                    Console.WriteLine("Selected MIDI device: " + options[index]);
                    ApplicationState.Instance.selectedMidiDevice = options[index];
                    ApplicationState.Save();
                }
            });
            selector.ShowDialog();
        }

        private void Act_SelectConfiguration(object sender, RoutedEventArgs e) {
            List<string> options = ApplicationState.Instance.configurations.Keys.ToList();

            var selector = new MultiOptionSelector(options, index => {
                if (index >= 0) {
                    Console.WriteLine("Selected configuration: " + options[index]);
                    ApplicationState.Instance.selectedConfiguration = options[index];
                    ApplicationState.Save();
                }
            });
            selector.ShowDialog();
        }

        private void Act_EditConfiguration(object sender, RoutedEventArgs e) {
            var config = ApplicationState.Instance.GetSelectedConfig();

            string prevName = config.name;
            string json = config.ToJSON();

            config = null;

            var selector = new TextEditor(json, editedJSON => {
                try {
                    TouchpadConfig edited = Newtonsoft.Json.JsonConvert.DeserializeObject<TouchpadConfig>(editedJSON);
                    var errors = TouchpadConfigs.CheckForConfigError(edited);

                    if (errors.Count > 0) {
                        Console.WriteLine("Errors in config");
                        foreach (var err in errors) {
                            Console.WriteLine("    " + err);
                        }
                        return false;
                    }

                    if (!prevName.Equals(edited.name) && ApplicationState.Instance.configurations.ContainsKey(edited.name)) {
                        Console.WriteLine("    Config already exists: " + edited.name);
                    }

                    ApplicationState.Instance.EditConfig(prevName, edited);
                    ApplicationState.Save();

                    return true;
                } catch (Exception ex) {
                    Console.WriteLine("Syntax error editing JSON: " + ex.Message);
                    return false;
                }
            });
            selector.ShowDialog();
        }

        private void Act_DeleteConfiguration(object sender, RoutedEventArgs e) {
            var dialog = new ConfirmDialog("Are you sure you want to delete this item?", () => {
                ApplicationState.Instance.RemoveConfig(ApplicationState.Instance.selectedConfiguration);
                ApplicationState.Save();

                
            });
            dialog.ShowDialog();
        }

        private void Act_NewConfiguration(object sender, RoutedEventArgs e) {
            ApplicationState.Instance.AddConfig(TouchpadConfigs.DefaultConfig());
            ApplicationState.Save();
        }

        private void Act_ToggleMidi(object sender, RoutedEventArgs e) {
            Touchpad.Enabled = !Touchpad.Enabled;
            Touchpad.Restart();
            Reload();
        }
    }
}