using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInput.Touchpad.Midi {
    public class TouchpadConfig {
        public string name;
        public TouchpadPartition[] partitions;

        public TouchpadConfig() {
            
        }

        public TouchpadConfig(string name, TouchpadPartition[] partitions) {
            this.name = name;
            this.partitions = partitions;
        }
    }

    public class TouchpadPartition {
        public int xMin, xMax, yMin, yMax;
        // pressOrder, leftToRight, rightToLeft, topToBottom, bottomToTop
        public string fingerOrdering = "pressOrder";
        public TouchpadFinger[] fingers;

        public FingerOrdering ParseFingerOrdering() {
            switch (fingerOrdering) {
                case "pressOrder":
                    return FingerOrdering.pressOrder;
                case "leftToRight":
                    return FingerOrdering.leftToRight;
                case "rightToLeft":
                    return FingerOrdering.rightToLeft;
                case "topToBottom":
                    return FingerOrdering.topToBottom;
                case "bottomToTop":
                    return FingerOrdering.bottomToTop;
                default:
                    throw new Exception("Invalid finger ordering");
            }
        }

        public TouchpadPartition() {
            
        }

        public TouchpadPartition(int xMin, int xMax, int yMin, int yMax, FingerOrdering fingerOrdering, TouchpadFinger[] fingers) {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
            this.fingerOrdering = fingerOrdering.ToString();
            this.fingers = fingers;
        }
    }

    public class TouchpadFinger {
        // optional
        public TouchAxisConfig? xAxis, yAxis;

        public TouchpadFinger() {
            
        }

        public TouchpadFinger(TouchAxisConfig? xAxis, TouchAxisConfig? yAxis) {
            this.xAxis = xAxis;
            this.yAxis = yAxis;
        }
    }

    public class TouchAxisConfig {
        public int midiCC;
        public int midiChannel;
        public int minCC, maxCC;
        public bool invertValue;

        public TouchAxisConfig() {
            
        }

        public TouchAxisConfig(int midiCC, int midiChannel, int minCC, int maxCC, bool invertValue) {
            this.midiCC = midiCC;
            this.midiChannel = midiChannel;
            this.minCC = minCC;
            this.maxCC = maxCC;
            this.invertValue = invertValue;
        }
    }

    public enum FingerOrdering {
        pressOrder,
        leftToRight,
        rightToLeft,
        topToBottom,
        bottomToTop
    }
}

/*

interface TouchpadConfig {
  name: string
  partitions: TouchpadPartition[]
}

interface TouchpadPartition {
  xMin: number
  xMax: number
  yMin: number
  yMax: number
  fingerOrdering: "pressOrder" | "leftToRight" | "rightToLeft" | "topToBottom" | "bottomToTop"
  fingers: TouchpadFinger[]
}

interface TouchpadFinger {
  xAxis?: TouchAxisConfig
  yAxis?: TouchAxisConfig
}

interface TouchAxisConfig {
  midiCC: number
  midiChannel: number
  minCC: number
  maxCC: number
  invertValue: boolean
}

*/