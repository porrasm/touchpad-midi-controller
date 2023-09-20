using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInput.Touchpad.Midi {
    public static class TouchpadConfigs {

        public const int YRange = 5065;
        public const int XRange = 7612;

        public static TouchpadConfig DefaultConfig() {
            TouchpadFinger[] fingers = new TouchpadFinger[] {
                new TouchpadFinger() {
                  xAxis = new TouchAxisConfig() {
                    midiCC = 11,
                    midiChannel = 1,
                    minCC = 0,
                    maxCC = 127,
                    invertValue = false,
                    defaultValue = -1
                  },
                  yAxis = new TouchAxisConfig() {
                    midiCC = 1,
                    midiChannel = 1,
                    minCC = 0,
                    maxCC = 127,
                    invertValue = true,
                    defaultValue = -1
                  }
                }
                };

            TouchpadPartition[] partitions = new TouchpadPartition[] {
                new TouchpadPartition() {
                    fingers = fingers,
                    xMin = 0,
                    xMax = XRange,
                    yMin = 0,
                    yMax = YRange,
                }
            };

            return new TouchpadConfig() {
                name = "Default (Dynamics & Expression)",
                partitions = partitions
            };
        }

        public static TouchpadConfig DynamicsVibratoPitch() {
            TouchpadFinger[] fingers = new TouchpadFinger[] {
                new TouchpadFinger() {
                  xAxis = new TouchAxisConfig() {
                    midiCC = 57,
                    midiChannel = 1,
                    minCC = 0,
                    maxCC = 127,
                    invertValue = false,
                    defaultValue = -1
                  },
                  yAxis = new TouchAxisConfig() {
                    midiCC = 61,
                    midiChannel = 1,
                    minCC = 0,
                    maxCC = 127,
                    invertValue = true,
                    defaultValue = -1
                  }
                },
                new TouchpadFinger {
                    //xSwipe = new SwipeAxisConfig(100, 1, 0, 127, false, 63, 0, 0.03) {
                    xSwipe = new SwipeAxisConfig() {
                        midiCC = 100,
                        midiChannel = 1,
                        minCC = 0,
                        maxCC = 127,
                        invertValue = false,
                        defaultValue = 63,
                        sensitivity = 0.03
                    }
                },
                };

            TouchpadPartition[] partitions = new TouchpadPartition[] {
                new TouchpadPartition() {
                    xMin = 0,
                    xMax = XRange,
                    yMin = 0,
                    yMax = YRange,
                    fingerOrdering = FingerOrdering.pressOrder.ToString(),
                    fingers = fingers
                }
            };

            return new TouchpadConfig($"DynamicsVibratoPitch", partitions);
        }

        public static List<string> CheckForConfigError(TouchpadConfig config) {
            List<string> errors = new List<string>();
            config.Validate(errors);
            return errors;
        }
    }
}
