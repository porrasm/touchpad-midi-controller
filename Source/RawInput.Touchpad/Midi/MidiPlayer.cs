using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInput.Touchpad.Midi {
    public class MIDIPlayer {
        #region fields
        private MidiOut midi;

        private static string defaultMidi = "LoopBe Internal MIDI";

        private int channel = 1;
        #endregion

        public MIDIPlayer(string deviceName = null) {
            midi = MIDI.GetMidiDeviceByName(deviceName ?? defaultMidi);
        }

        public void SendMIDI(byte cc, int value) {
            midi.Send(new ControlChangeEvent(0, channel, (MidiController)cc, value).GetAsShortMessage());
        }

        public void SendMIDI(ControlChangeEvent e) {
            midi.Send(e.GetAsShortMessage());
        }

        public void SendMIDI(IEnumerable<ControlChangeEvent> events) {
            foreach (ControlChangeEvent e in events) {
                midi.Send(e.GetAsShortMessage());
            }
        }
    }
}
