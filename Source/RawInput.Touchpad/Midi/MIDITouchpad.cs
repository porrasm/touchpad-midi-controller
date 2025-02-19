﻿using NAudio.Midi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RawInput.Touchpad.Midi {
    public class MIDITouchpad {
        private MIDIPlayer midi;

        public TouchpadConfig Config { get; set; }

        public TouchpadContactHistory ContactHistory { get; private set; } = new();
        private HashSet<int> handledContacts = new();

        public MIDITouchpad(MIDIPlayer midi, TouchpadConfig config) {
            this.midi = midi;
            Config = config;
        }

        public void TouchpadToMIDI(TouchpadContact[] contacts) {
            if (contacts == null) {
                ContactHistory.UpdatePressOrder(new TouchpadContact[0]);
                ContactHistory.UpdateContactsState(new TouchpadContact[0]);
                return;
            }

            ContactHistory.UpdatePressOrder(contacts);

            if (contacts.Length == 0) {
                ContactHistory.UpdateContactsState(contacts);
                return;
            }

            var events = GetEventsFromContacts(contacts);

            midi.SendMIDI(events);

            ContactHistory.UpdateContactsState(contacts);
        }

        public void SendDefaultValues() {
            List<ControlChangeEvent> events = new List<ControlChangeEvent>();

            foreach (var partition in Config.partitions) {
                foreach (var finger in partition.fingers) {
                    if (finger == null) {
                        continue;
                    }

                    AddDefaultValues(events, finger.xAxis, finger.yAxis, finger.xSwipe, finger.ySwipe);
                }
            }

            midi.SendMIDI(events);
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
            handledContacts.Clear();
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

                handledContacts.Add(contact.ContactId);

                var finger = partition.fingers[i];
                AddFingerEvents(events, partition, finger, contacts, contact, i);
            }

            // Default event values
            for (int i = orderedContacts.Length; i < partition.fingers.Length; i++) {
                var finger = partition.fingers[i];
                AddDefaultValues(events, finger.xAxis, finger.yAxis, finger.xSwipe, finger.ySwipe);
            }
        }

        private void AddFingerEvents(List<ControlChangeEvent> events, TouchpadPartition partition, TouchpadFinger finger, TouchpadContact[] contacts, TouchpadContact contact, int i) {
            Console.WriteLine("Handle finger events");
            // Axis events
            if (finger.xAxis != null) {
                events.Add(GetFingerAxisEvent(contact.X, partition.xMin, partition.xMax, finger.xAxis));
            }
            if (finger.yAxis != null) {
                events.Add(GetFingerAxisEvent(contact.Y, partition.yMin, partition.yMax, finger.yAxis));
            }

            // Swipe events
            var deltaExists = ContactHistory.GetContactDelta(contact, out int xDelta, out int yDelta);

            if (!deltaExists) {
                return;
            }

            if (finger.xSwipe != null) {
                events.Add(GetFingerSwipeEvent(xDelta, partition.xMin, partition.xMax, finger.xSwipe));
            }
            if (finger.ySwipe != null) {
                events.Add(GetFingerSwipeEvent(yDelta, partition.yMin, partition.yMax, finger.ySwipe));
            }

            if (finger.pairings != null) {
                foreach (var pairing in finger.pairings) {
                    AddFingerPairingEvents(events, partition, contacts, contact, i + 1, pairing);
                }
            }
        }


        private void AddFingerPairingEvents(List<ControlChangeEvent> events, TouchpadPartition partition, TouchpadContact[] contacts, TouchpadContact originalContact, int contactIndex, FingerPairing pairing) {
            if (contactIndex >= contacts.Length) {
                Console.WriteLine("Index above");
                AddFingerDefaultEvents(events, pairing.finger);
                return;
            }

            var filter = pairing.ParsePositionFilter();
            int newIndex = GetFirstMatchingContactIndex(contacts, originalContact, filter);

            if (newIndex == -1) {
                Console.WriteLine("No matching pairing finger");
                AddFingerDefaultEvents(events, pairing.finger);
                return;
            }

            Console.WriteLine("Found matching pairing finger");
            AddFingerEvents(events, partition, pairing.finger, contacts, contacts[newIndex], contactIndex + 1);
        }


        private int GetFirstMatchingContactIndex(TouchpadContact[] contacts, TouchpadContact originalContact, FingerPositionFilter filter) {
            for (int i = 0; i < contacts.Length; i++) {
                if (!handledContacts.Contains(originalContact.ContactId) || TouchMatchesFilter(originalContact, contacts[i], filter)) {
                    return i;
                }
            }

            return -1;
        }

        // Replace with Dictionary<int, Dictionary> in history
        private bool TouchMatchesFilter(TouchpadContact b, TouchpadContact a, FingerPositionFilter filter) {
            Console.WriteLine($"Check if {a.ContactId} matches {b.ContactId} with filter {filter}");


            if (filter == FingerPositionFilter.any) {
                return true;
            }

            if (filter == FingerPositionFilter.top) {
                return a.Y < b.Y;
            }

            if (filter == FingerPositionFilter.bottom) {
                return a.Y > b.Y;
            }

            if (filter == FingerPositionFilter.left) {
                return a.X < b.X;
            }

            if (filter == FingerPositionFilter.right) {
                return a.X > b.X;
            }

            if (filter == FingerPositionFilter.topLeft) {
                return a.Y < b.Y && a.X < b.X;
            }

            if (filter == FingerPositionFilter.topRight) {
                return a.Y < b.Y && a.X > b.X;
            }

            if (filter == FingerPositionFilter.bottomLeft) {
                return a.Y > b.Y && a.X < b.X;
            }

            if (filter == FingerPositionFilter.bottomRight) {
                return a.Y > b.Y && a.X > b.X;
            }

            return false;
        }

        private void AddFingerDefaultEvents(List<ControlChangeEvent> events, TouchpadFinger finger) {
            AddDefaultValues(events, finger.xAxis, finger.yAxis, finger.xSwipe, finger.ySwipe);
            if (finger.pairings != null) {
                foreach (var pairing in finger.pairings) {
                    AddFingerDefaultEvents(events, pairing.finger);
                }
            }
        }
        private void AddDefaultValues(List<ControlChangeEvent> events, params TouchAxisConfig[] configs) {
            foreach (var config in configs) {
                if (config != null && config.defaultValue >= 0) {
                    events.Add(new ControlChangeEvent(0, config.midiChannel, (MidiController)config.midiCC, config.defaultValue));
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

        private ControlChangeEvent GetFingerAxisEvent(int contactPos, int partitionMin, int partitionMax, TouchAxisConfig axisConfig) {
            int ccValue = ValueInRange(contactPos, partitionMin, partitionMax, axisConfig.minCC, axisConfig.maxCC, axisConfig.invertValue);
            return new ControlChangeEvent(0, axisConfig.midiChannel, (MidiController)axisConfig.midiCC, ccValue);
        }

        private ControlChangeEvent GetFingerSwipeEvent(int contactDelta, int partitionMin, int partitionMax, SwipeAxisConfig swipeConfig) {
            ControlChangeEvent e = new ControlChangeEvent(0, swipeConfig.midiChannel, (MidiController)swipeConfig.midiCC, 0);

            bool lastValueExists = midi.GetLastValue(e, out int lastValue);

            if (!lastValueExists) {
                e.ControllerValue = swipeConfig.defaultValue;
                return e;
            }

            double deltaAdjusted = 1.0 * contactDelta * swipeConfig.sensitivity;

            if (swipeConfig.invertValue) {
                deltaAdjusted = -deltaAdjusted;
            }

            int newValue = Math.Clamp(swipeConfig.defaultValue + (int)deltaAdjusted, swipeConfig.minCC, swipeConfig.maxCC);

            e.ControllerValue = newValue;

            return e;
        }

        private TouchpadContact[] GetOrderedContacts(TouchpadContact[] contacts, FingerOrdering ordering) {
            switch (ordering) {
                case FingerOrdering.pressOrder:
                    return contacts.OrderBy(c => ContactHistory.GetPressIndexOf(c.ContactId)).ToArray();
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
