using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInput.Touchpad.Midi {
    public class TouchpadContactHistory {
        private List<int> pressOrder = new List<int>();
        private Dictionary<int, TouchpadContactState> contactsState = new Dictionary<int, TouchpadContactState>();
        private int[] newPresses, released;
        public int ContactCount => pressOrder.Count;

        
        private class TouchpadContactState {
            public TouchpadContact initialContact;
            public TouchpadContact currentContact;

            public TouchpadContactState(TouchpadContact initialContact) {
                this.initialContact = initialContact;
                this.currentContact = initialContact;
            }

            public void Update(TouchpadContact contact) {
                currentContact = contact;
            }
        }

        public void UpdatePressOrder(TouchpadContact[] contacts) {
            int[] currentlyPressed = contacts.Select(c => c.ContactId).ToArray();

            currentlyPressed = currentlyPressed.Distinct().ToArray();

            if (currentlyPressed.Length == 0) {
                newPresses = new int[0];
                released = pressOrder.ToArray();
                pressOrder.Clear();
                return;
            }

            if (pressOrder.Count == 0) {
                newPresses = currentlyPressed;
                released = new int[0];
                pressOrder.AddRange(currentlyPressed);
                return;
            }

            newPresses = currentlyPressed.Except(pressOrder).ToArray();
            int[] releasedFingers = pressOrder.Except(currentlyPressed).ToArray();
            released = releasedFingers;

            pressOrder.AddRange(newPresses);
            pressOrder.RemoveAll(p => releasedFingers.Contains(p));
        }

        public void UpdateContactsState(TouchpadContact[] contacts) {
            TouchpadContact[] existingActive = contacts.Where(c => !newPresses.Contains(c.ContactId) && !released.Contains(c.ContactId)).ToArray();

            foreach (int id in newPresses) {
                contactsState.Add(id, new TouchpadContactState(contacts.First(c => c.ContactId == id)));
            }
            foreach (var contact in existingActive) {
                contactsState[contact.ContactId].Update(contact);
            }
            foreach (int id in released) {
                contactsState.Remove(id);
            }
        }
        
        public int GetPressIndexOf(int contactId) => pressOrder.IndexOf(contactId);

        public bool GetContactDelta(TouchpadContact contact, out int xDelta, out int yDelta) {
            if (!contactsState.ContainsKey(contact.ContactId)) {
                xDelta = 0;
                yDelta = 0;
                return false;
            }

            var prev = contactsState[contact.ContactId].initialContact;

            xDelta = contact.X - prev.X;
            yDelta = contact.Y - prev.Y;

            return true;
        }
    }
}
