using RawInput.Touchpad.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInput.Touchpad {
    internal static class Touchpad {
        public static Action<TouchpadContact[]> OnContact { get; set; }

        public const int YRange = 5065;
        public const int XRange = 7612;

        private static MIDIPlayer midi;
        private static MIDITouchpad touchpad;

        public static void RegisterContact(TouchpadContact[] contacts) {
            OnContact?.Invoke(contacts);
        }

        public static void StartListening() {
            midi = new MIDIPlayer();
            touchpad = new MIDITouchpad(midi, TwoYSliders(57, 61));

            OnContact += touchpad.TouchpadToMIDI;
        }


        public static TouchpadConfig XYSliders(int xcc, int ycc) {

            TouchpadFinger[] fingers = new TouchpadFinger[] {
                new TouchpadFinger(
                    new TouchAxisConfig(xcc, 1, 0, 127, false),
                    new TouchAxisConfig(ycc, 1, 0, 127, true)
                    ),
                new TouchpadFinger(
                    new TouchAxisConfig(16, 1, 0, 127, false),
                    new TouchAxisConfig(17, 1, 0, 127, true)
                    ),
                };

            TouchpadPartition[] partitions = new TouchpadPartition[] {
                new TouchpadPartition(
                    0,
                    XRange,
                    0,
                    YRange,
                    FingerOrdering.pressOrder,
                fingers
                )
            };


            return new TouchpadConfig($"XY sliders ({xcc}, {ycc})", partitions);
        }

        public static TouchpadConfig TwoYSliders(int cc1, int cc2) {

            TouchpadFinger[] leftFinger = new TouchpadFinger[] {
                    new TouchpadFinger(
                            null,
                            new TouchAxisConfig(cc1, 1, 0, 127, true)
                        )
                };

            TouchpadFinger[] rightFinger = new TouchpadFinger[] {
                    new TouchpadFinger(
                            null,
                            new TouchAxisConfig(cc2, 1, 0, 127, true)
                        )
                };

            TouchpadPartition[] partitions = new TouchpadPartition[] {
                new TouchpadPartition(
                    0,
                    XRange / 2,
                    0,
                    YRange,
                    FingerOrdering.pressOrder,
                    leftFinger
                ),
                new TouchpadPartition(
                    XRange / 2,
                    XRange,
                    0,
                    YRange,
                    FingerOrdering.pressOrder,
                    rightFinger
                )
            };

            return new TouchpadConfig($"Two Y sliders ({cc1}, {cc2})", partitions);
        }
    }
}
