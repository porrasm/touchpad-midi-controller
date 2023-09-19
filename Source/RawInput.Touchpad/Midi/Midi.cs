using System;
using NAudio.Midi;

namespace RawInput.Touchpad.Midi {
    public static class MIDI {
        public static MidiOut GetMidiDeviceByName(string name) {

            MidiOut Out = null;

            for (int i = 0; i < MidiOut.NumberOfDevices; i++) {
                string productName = MidiOut.DeviceInfo(i).ProductName;
                Console.WriteLine(productName);
                if (productName.Equals(name)) {
                    if (Out != null) {
                        Out.Dispose();
                        Out = null;
                    }
                    Out = new MidiOut(i);
                    Console.WriteLine("Midi device found");
                    return Out;
                }
            }

            Console.WriteLine("Midi device not found");
            return null;
        }
    }
}