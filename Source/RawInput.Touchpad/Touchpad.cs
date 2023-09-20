using RawInput.Touchpad.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInput.Touchpad {
    internal static class Touchpad {
        public static Action<TouchpadContact[]> OnContact { get; set; }

        private static MIDIPlayer midi;
        private static MIDITouchpad touchpad;

        private static int runIndex = 0;

        static Touchpad() {
            ApplicationState.OnConfigChanged += StartListening;
        }

        public static void RegisterContact(TouchpadContact[] contacts) {
            OnContact?.Invoke(contacts);
        }

        public static void StartListening() {
            if (ApplicationState.Instance == null) {
                return;
            }

            StopListening();
            midi = new MIDIPlayer();

            var config = ApplicationState.Instance.GetSelectedConfig();
            var configErrors = TouchpadConfigs.CheckForConfigError(config);

            if (configErrors.Count > 0) {
                Console.WriteLine("Errors in selected config, defaulting to default config.");
                foreach (var error in configErrors) {
                    Console.WriteLine("    " + error);
                }

                config = TouchpadConfigs.DefaultConfig();
            }

            touchpad = new MIDITouchpad(midi, config);

            OnContact += touchpad.TouchpadToMIDI;

            int startIndex = ++runIndex;
            Task.Run(() => RunLoop(startIndex));
        }

        public static void StopListening() {
            if (midi != null) {
                midi.Dispose();
                midi = null;
            }

            OnContact = null;

            runIndex++;
        }

        private static async Task RunLoop(int currentIndex) {
            while (currentIndex == runIndex) {
                if (touchpad.ContactHistory.ContactCount == 0) {
                    touchpad.SendDefaultValues();
                }
                await Task.Delay(100);
            }
        }
    }
}
