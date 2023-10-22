# Touchpad Midi Controller

An application which transforms touchpad input to MIDI input. This is currently a work in progress. Windows only.

## Features

- assign different fingers to different CC values (e.g. left most finger is CC 1 and right most CC2 or first finger pressed is CC100 and second finger pressed is CC101). Should work for as many fingers as the touchpad supports. Apple Magic Trackpad supports 5+
- assign different partitions for the touchpad (e.g. left side is CC1 and right side is CC2).
- XY slider support
- Assign a MIDI CC and a MIDI channel. You can assign the output range of the CC values as well (e.g. 0-127 or 50-100)

For example you could partition the touchpad into 4 different XY sliders such that the top left, top right, bottom left and bottom right areas are different XY sliders, totaling 8 different CC channels. Of course each area can have the multiple finger support as well, so with 4 partitions and 3 fingers, each using the X and Y axes, you could controls 24 different CC values with this app (not useful probably).

Alternatively you could setup e.g. 12 different Y sliders next to each other.

One use case I found was that I assigned my first finger Y axis to dynamics and X axis to vibrato. Then I assigned the second pressed finger Y axis to flutter, giving me control of dynamic, vibrato and flutter all with 1 hand using 2 fingers

The app is buggy right now and lacks the feature to edit the configuration without touching the code. Once the bugs are fixed this should be ready. 

## Config type

The configuration possibilities are illustrated here using Typescript interfaces for clarity.

```Typescript
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
}

type SwipeAxisConfig = TouchAxisConfig & {
  sensitivity: number
}
```

## Requirements

- .net 5.0
- A windows precision touchpad (e.g. Apple Magic Trackpad with a custom Windows driver: https://github.com/imbushuo/mac-precision-touchpad)
- A virtual MIDI controller, e.g. LoopBe1 midi or similar
