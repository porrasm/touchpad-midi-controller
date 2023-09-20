using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInput.Touchpad.Midi {
    public class MIDIPlayer : IDisposable {
        #region fields
        private MidiOut midi;

        private static string defaultMidi = "LoopBe Internal MIDI";

        private struct CCValueSent {
            public long ticks;
            public int value;

            public long AgeInMilliseconds => (DateTime.Now.Ticks - ticks) / TimeSpan.TicksPerMillisecond;
        }

        private const long HISTORY_CC_LIFETIME_MS = 5000000000;

        private Dictionary<int, CCValueSent> history = new();

        private static long Ticks => DateTime.Now.Ticks;
        #endregion

        public MIDIPlayer(string deviceName = null) {
            midi = MIDI.GetMidiDeviceByName(deviceName ?? defaultMidi);
        }

        public void SendMIDI(IEnumerable<ControlChangeEvent> events) {
            ClearOldHistory();

            var newEvents = GetNewEvents(events);

            foreach (ControlChangeEvent e in newEvents) {
                midi.Send(e.GetAsShortMessage());
                Console.WriteLine("Sended: " + e);
                AddToHistory(e);
            }
        }

        private void ClearOldHistory() {
            var oldKeys = history.Where(kvp => kvp.Value.AgeInMilliseconds > HISTORY_CC_LIFETIME_MS).Select(kvp => kvp.Key);
            foreach (int key in oldKeys) {
                history.Remove(key);
            }
        }

        private IEnumerable<ControlChangeEvent> GetNewEvents(IEnumerable<ControlChangeEvent> events) {
            return events.Where(e => !history.ContainsKey(CCEventToKey(e)) || history[CCEventToKey(e)].value != e.ControllerValue);
        }

        private void AddToHistory(ControlChangeEvent e) {
            int key = CCEventToKey(e);
            var val = new CCValueSent() { ticks = Ticks, value = e.ControllerValue };
            if (history.ContainsKey(key)) {
                history[key] = val;
            } else {
                history.Add(key, val);
            }
        }

        private int CCEventToKey(ControlChangeEvent e) {
            return ((int)e.Controller << 16) | e.Channel;
        }

        public bool GetLastValue(ControlChangeEvent e, out int value) {
            int key = CCEventToKey(e);
            if (history.ContainsKey(key)) {
                value = history[key].value;
                return true;
            } else {
                value = 0;
                return false;
            }
        }

        public void Dispose() {
            midi.Dispose();
        }
    }
}
