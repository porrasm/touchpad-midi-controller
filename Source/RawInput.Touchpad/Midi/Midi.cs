using System;
using System.Collections.Generic;
using NAudio.Midi;

namespace RawInput.Touchpad.Midi {
    public static class MIDI {
        public static MidiOut GetMidiDeviceByName(string name) {

            MidiOut Out = null;

            for (int i = 0; i < MidiOut.NumberOfDevices; i++) {
                string productName = MidiOut.DeviceInfo(i).ProductName;
                if (productName.Equals(name)) {
                    if (Out != null) {
                        Out.Dispose();
                        Out = null;
                    }
                    Out = new MidiOut(i);
                    return Out;
                }
            }

            Console.WriteLine("Midi device not found: " + name);
            return null;
        }

        public static List<string> GetMidiDevices() {
            List<string> devices = new();

            for (int i = 0; i < MidiOut.NumberOfDevices; i++) {
                string productName = MidiOut.DeviceInfo(i).ProductName;
                devices.Add(productName);
            }

            return devices;
        }
    }
}