using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInput.Touchpad.Midi {
    public class MIDITouchpad {
        private MIDIPlayer midi;

        public byte TouchpadXCC { get; set; } = 57;
        public byte TouchpadYCC { get; set; } = 61;

        public TouchpadConfig Config { get; set; }

        private List<int> pressOrder = new List<int>();

        public MIDITouchpad(MIDIPlayer midi, TouchpadConfig config) {
            this.midi = midi;
            Config = config;
        }

        public void TouchpadToMIDI(TouchpadContact[] contacts) {
            if (contacts == null) {
                UpdatePressOrder(new int[0]);
                return;
            }

            UpdatePressOrder(contacts.Select(c => c.ContactId).ToArray());

            if (contacts.Length == 0) {
                return;
            }

            var events = GetEventsFromContacts(contacts);

            midi.SendMIDI(events);
        }

        private void UpdatePressOrder(int[] currentlyPressed) {
            currentlyPressed = currentlyPressed.Distinct().ToArray();
            
            if (currentlyPressed.Length == 0) {
                pressOrder.Clear();
                return;
            }

            if (pressOrder.Count == 0) {
                pressOrder.AddRange(currentlyPressed);
                return;
            }

            int[] newPresses = currentlyPressed.Except(pressOrder).ToArray();
            int[] released = pressOrder.Except(currentlyPressed).ToArray();

            pressOrder.AddRange(newPresses);
            pressOrder.RemoveAll(p => released.Contains(p));
        }



        #region events
        private List<ControlChangeEvent> GetEventsFromContacts(TouchpadContact[] contacts) {
            List<ControlChangeEvent> events = new List<ControlChangeEvent>();

            foreach (var partition in Config.partitions) {
                AddPartitionEvents(events, partition, contacts);
            }

            return events;
        }

        private TouchpadContact[] ContactsInPartition(TouchpadPartition partition, TouchpadContact[] contacts) {
            return contacts.Where(c => c.X >= partition.xMin && c.X <= partition.xMax && c.Y >= partition.yMin && c.Y <= partition.yMax).ToArray();
        }

        private void AddPartitionEvents(List<ControlChangeEvent> events, TouchpadPartition partition, TouchpadContact[] contacts) {
            var contactsInPartition = ContactsInPartition(partition, contacts);

            if (contactsInPartition.Length == 0) {
                return;
            }

            var orderedContacts = GetOrderedContacts(contactsInPartition, partition.ParseFingerOrdering());

            for (int i = 0; i < orderedContacts.Length; i++) {
                var contact = orderedContacts[i];

                if (i >= partition.fingers.Length) {
                    return;
                }

                var finger = partition.fingers[i];

                if (finger.xAxis != null) {
                    events.Add(GetFingerEvent(contact.X, partition.xMin, partition.xMax, finger.xAxis));
                }
                if (finger.yAxis != null) {
                    events.Add(GetFingerEvent(contact.Y, partition.yMin, partition.yMax, finger.yAxis));
                }
            }
        }

        public int ValueInRange(int val, int min, int max, int newMin, int newMax, bool invert) {
            if (val > max) {
                val = max;
            }

            if (val < min) {
                val = min;
            }

            double t = 1.0 * (val - min) / (max - min);

            int inNewRange = (int)(t * (newMax - newMin) + newMin);

            if (invert) {
                inNewRange = newMax - inNewRange + newMin;
            }

            return inNewRange;
        }

        private ControlChangeEvent GetFingerEvent(int contactPos, int partitionMin, int partitionMax, TouchAxisConfig axisConfig) {
            int ccValue = ValueInRange(contactPos, partitionMin, partitionMax, axisConfig.minCC, axisConfig.maxCC, axisConfig.invertValue);
            return new ControlChangeEvent(0, axisConfig.midiChannel, (MidiController)axisConfig.midiCC, ccValue);
        }

        private TouchpadContact[] GetOrderedContacts(TouchpadContact[] contacts, FingerOrdering ordering) {
            switch (ordering) {
                case FingerOrdering.pressOrder:
                    return contacts.OrderBy(c => pressOrder.IndexOf(c.ContactId)).ToArray();
                case FingerOrdering.leftToRight:
                    return contacts.OrderBy(c => c.X).ToArray();
                case FingerOrdering.rightToLeft:
                    return contacts.OrderByDescending(c => c.X).ToArray();
                case FingerOrdering.topToBottom:
                    return contacts.OrderByDescending(c => c.Y).ToArray();
                case FingerOrdering.bottomToTop:
                    return contacts.OrderBy(c => c.Y).ToArray();
                default:
                    return contacts;
                    
            }
        }
        #endregion
    }
}
