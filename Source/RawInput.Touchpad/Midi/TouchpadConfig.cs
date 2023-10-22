using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInput.Touchpad.Midi {
    interface IErrorValidatable {
        void Validate(List<string> errors);
    }

    public class TouchpadConfig : IErrorValidatable {
        public string name;
        public TouchpadPartition[] partitions;

        public TouchpadConfig() {

        }

        public TouchpadConfig(string name, TouchpadPartition[] partitions) {
            this.name = name;
            this.partitions = partitions;
        }

        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        public void Validate(List<string> errors) {
            if (name == null || name.Length == 0) {
                errors.Add("Config name cannot be empty");
            }
            if (partitions == null || partitions.Length == 0) {
                errors.Add("Config must have at least one partition");
            } else {
                foreach (TouchpadPartition partition in partitions) {
                    if (partition == null) {
                        errors.Add("Config partition cannot be null");
                    } else {
                        partition.Validate(errors);
                    }
                }
            }
        }
    }

    public class TouchpadPartition : IErrorValidatable {
        public int xMin, xMax, yMin, yMax;
        // pressOrder, leftToRight, rightToLeft, topToBottom, bottomToTop
        public string fingerOrdering = "pressOrder";
        public TouchpadFinger[] fingers;

        public FingerOrdering ParseFingerOrdering() {
            return Enum.Parse<FingerOrdering>(fingerOrdering);
        }

        public TouchpadPartition() {

        }

        public void Validate(List<string> errors) {
            try {
                ParseFingerOrdering();
            } catch (Exception e) {
                errors.Add("Partition finger ordering must be one of: " + string.Join(", ", Enum.GetNames(typeof(FingerOrdering))));
            }
            if (xMin > xMax) {
                errors.Add("Partition xMin must be less than or equal to xMax");
            }
            if (yMin > yMax) {
                errors.Add("Partition yMin must be less than or equal to yMax");
            }
            if (fingers == null || fingers.Length == 0) {
                errors.Add("Partition must have at least one finger");
            } else {
                foreach (var finger in fingers) {
                    if (finger == null) {
                        errors.Add("Partition finger cannot be null");
                    } else {
                        finger.Validate(errors);
                    }
                }
            }
        }
    }

    public class TouchpadFinger : IErrorValidatable {
        // optional
        public TouchAxisConfig? xAxis, yAxis;

        // optional
        public SwipeAxisConfig? xSwipe, ySwipe;

        // optional
        public FingerPairing[]? pairings;

        public TouchpadFinger() {

        }

        public void Validate(List<string> errors) {
            xAxis?.Validate(errors);
            yAxis?.Validate(errors);
            xSwipe?.Validate(errors);
            ySwipe?.Validate(errors);

            if (pairings != null) {
                foreach (var finger in pairings) {
                    if (finger == null) {
                        errors.Add("Recursive finger cannot be null");
                    } else {
                        finger.Validate(errors);
                    }
                }
            }
        }
    }

    public class FingerPairing : IErrorValidatable {
        public string positionFilter;
        public TouchpadFinger finger;

        public FingerPairing() {

        }

        public FingerPositionFilter ParsePositionFilter() {
            return Enum.Parse<FingerPositionFilter>(positionFilter);
        }

        public void Validate(List<string> errors) {
            try {
                ParsePositionFilter();
            } catch (Exception e) {
                errors.Add("Finger pairing positionFilter must be one of: " + string.Join(", ", Enum.GetNames(typeof(FingerPositionFilter))));
            }
            if (finger == null) {
                errors.Add("Finger pairing must have a finger");
            } else {
                finger.Validate(errors);
            }
        }
    }

    public class TouchAxisConfig : IErrorValidatable {
        public int midiCC;
        public int midiChannel;
        public int minCC, maxCC;
        public bool invertValue;
        public int defaultValue = -1;

        public TouchAxisConfig() {

        }

        public virtual void Validate(List<string> errors) {
            if (midiCC < 0 || midiCC > 127) {
                errors.Add("Axis midi CC must be between 0 and 127");
            }
            if (midiChannel < 1 || midiChannel > 16) {
                errors.Add("Axis midi channel must be between 1 and 16");
            }
            if (minCC < 0 || minCC > 127) {
                errors.Add("Axis min CC must be between 0 and 127");
            }
            if (maxCC < 0 || maxCC > 127) {
                errors.Add("Axis max CC must be between 0 and 127");
            }
            if (minCC > maxCC) {
                errors.Add("Axis min CC must be less than or equal to max CC");
            }
            if (defaultValue < -1 || defaultValue > 127) {
                errors.Add("Axis default value must be between 0 and 127 or -1 when disabled");
            }
        }
    }

    public class SwipeAxisConfig : TouchAxisConfig, IErrorValidatable {
        public double sensitivity = 1.0;

        public SwipeAxisConfig() {

        }

        public override void Validate(List<string> errors) {
            base.Validate(errors);
            if (sensitivity == 0) {
                errors.Add("Swipe axis sensitivity cannot be 0");
            }
            if (defaultValue == -1) {
                errors.Add("Swipe axis must have a default in the range from 0 to 127");
            }
        }
    }

    public enum FingerOrdering {
        pressOrder,
        leftToRight,
        rightToLeft,
        topToBottom,
        bottomToTop
    }

    public enum FingerPositionFilter {
        any,
        left,
        right,
        top,
        bottom,
        topLeft,
        topRight,
        bottomLeft,
        bottomRight
    }
}

/*

type TouchpadConfig = {
  name: string
  partitions: TouchpadPartition[]
}

type TouchpadPartition = {
  xMin: number
  xMax: number
  yMin: number
  yMax: number
  fingerOrdering: "pressOrder" | "leftToRight" | "rightToLeft" | "topToBottom" | "bottomToTop"
  fingers: TouchpadFinger[]
}

type TouchpadFinger = {
  xAxis?: TouchAxisConfig
  yAxis?: TouchAxisConfig
  swipeX?: SwipeAxisConfig
  swipeY?: SwipeAxisConfig
  recursiveFingers?: FingerPairing[]
}

type FingerPairing = {
  positionFilter: "any" | "left" | "right" | "top" | "bottom" | "topLeft" | "topRight" | "bottomLeft" | "bottomRight"
  finger: TouchpadFinger
}

type TouchAxisConfig = {
  midiCC: number
  midiChannel: number
  minCC: number
  maxCC: number
  invertValue: boolean
  defaultValue?: number
}

type SwipeAxisConfig = TouchAxisConfig & {
  sensitivity: number
}

*/