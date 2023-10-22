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
  defaultValue?: number
}

type SwipeAxisConfig = TouchAxisConfig & {
  sensitivity: number
}
```

Each config can have multiple partitions. 5 partitions next to each other would correspond to a MIDI controller with 5 sliders next to each other.

- `TouchpadPartition`: This type defines the properties of a partition, which is an independent area in which touch events are handled. It has properties such as `xMin`, `xMax`, `yMin`, `yMax`, `fingerOrdering`, and `fingers`. The `fingers` list can be filled to add support for multiple fingers. 

- `TouchpadFinger`: This type defines the properties of a finger, which is a touch point on the touchpad. It has optional properties such as `xAxis`, `yAxis`, `swipeX` and `swipeY` that define what type of MIDI events will be sent by this finger. The `recursiveFingers` property allows you to add a events for the 2nd finger that will be pressed down. This is an alternative for defining multiple `fingers` in `TouchpadPartition`.

- `FingerPairing`: This type defines the properties of a finger pairing, which is a pairing of two fingers that are used together. It has properties such as `positionFilter` and `finger`. The position filter defines when the finger is activated (e.g. if you place 1 finger on the touchpad and another one above the 1st finger then the `top` position filter is active). Currently this requires that the finger stays e.g. above the 2nd finger at all times but this is a bug that will be fixed at some point.

- `TouchAxisConfig`: This type defines the properties of a touch axis, which is a configuration for a touch axis such as the X or Y axis. It has properties such as `midiCC`, `midiChannel`, `minCC`, `maxCC`, `invertValue`, and `defaultValue`. It is basically a MIDI slider that can be moved up and down/left and right. It's values correspond to some MIDI CC and can be clamped. The `defaultValue` is sent (if defined) when the finger is lifted.

- `SwipeAxisConfig`: This type extends `TouchAxisConfig` and adds a `sensitivity` property, which is a configuration for a swipe axis such as the X or Y axis. It is like `TouchAxisConfig`, but it's not a slider. It is like the modwheel on some synths that resets to some position after not being touched. The `defaultValue` is sent (if defined) when the finger is lifted.

### Examples

#### Typical Roland Modwheel + pitch bend: 

![image](https://github.com/porrasm/touchpad-midi-controller/assets/31691452/90275862-64fd-4352-9936-000cae976391)

```JSON
{
  "name": "Typical Roland Modwheel + pitch bend",
  "partitions": [
    {
      "xMin": 0,
      "xMax": 7612,
      "yMin": 0,
      "yMax": 5065,
      "fingerOrdering": "pressOrder",
      "fingers": [
        {
          "xSwipe": {
            "midiCC": 2,
            "midiChannel": 1,
            "minCC": 0,
            "maxCC": 127,
            "invertValue": false,
            "defaultValue": 64
          },
          "ySwipe": {
            "midiCC": 1,
            "midiChannel": 1,
            "minCC": 0,
            "maxCC": 127,
            "invertValue": true,
            "defaultValue": 0
          },
        }
      ]
    }
  ]
}
```


#### 5 Y Sliders

```JSON
{
  "name": "5 Y sliders",
  "partitions": [
    {
      "xMin": 0,
      "xMax": 1500,
      "yMin": 0,
      "yMax": 5065,
      "fingerOrdering": "pressOrder",
      "fingers": [
        {
          "yAxis": {
            "midiCC": 1,
            "midiChannel": 1,
            "minCC": 0,
            "maxCC": 127,
            "invertValue": true,
            "defaultValue": -1
          },
          "xSwipe": null,
          "ySwipe": null
        }
      ]
    },
    {
      "xMin": 1500,
      "xMax": 3000,
      "yMin": 0,
      "yMax": 5065,
      "fingerOrdering": "pressOrder",
      "fingers": [
        {
          "yAxis": {
            "midiCC": 2,
            "midiChannel": 1,
            "minCC": 0,
            "maxCC": 127,
            "invertValue": true,
            "defaultValue": -1
          },
          "xSwipe": null,
          "ySwipe": null
        }
      ]
    },
    {
      "xMin": 3000,
      "xMax": 4500,
      "yMin": 0,
      "yMax": 5065,
      "fingerOrdering": "pressOrder",
      "fingers": [
        {
          "yAxis": {
            "midiCC": 3,
            "midiChannel": 1,
            "minCC": 0,
            "maxCC": 127,
            "invertValue": true,
            "defaultValue": -1
          },
          "xSwipe": null,
          "ySwipe": null
        }
      ]
    },
    {
      "xMin": 4500,
      "xMax": 6000,
      "yMin": 0,
      "yMax": 5065,
      "fingerOrdering": "pressOrder",
      "fingers": [
        {
          "yAxis": {
            "midiCC": 4,
            "midiChannel": 1,
            "minCC": 0,
            "maxCC": 127,
            "invertValue": true,
            "defaultValue": -1
          },
          "xSwipe": null,
          "ySwipe": null
        }
      ]
    },
    {
      "xMin": 6000,
      "xMax": 7612,
      "yMin": 0,
      "yMax": 5065,
      "fingerOrdering": "pressOrder",
      "fingers": [
        {
          "yAxis": {
            "midiCC": 5,
            "midiChannel": 1,
            "minCC": 0,
            "maxCC": 127,
            "invertValue": true,
            "defaultValue": -1
          },
          "xSwipe": null,
          "ySwipe": null
        }
      ]
    }
  ]
}
```

#### XY Dynamics & Vibrato w/ aux

```JSON
{
  "name": "XY Dynamics & Vibrato w/ aux",
  "partitions": [
    {
      "xMin": 0,
      "xMax": 7612,
      "yMin": 0,
      "yMax": 5065,
      "fingerOrdering": "pressOrder",
      "fingers": [
        {
          "xAxis": {
            "midiCC": 57,
            "midiChannel": 1,
            "minCC": 0,
            "maxCC": 127,
            "invertValue": false,
            "defaultValue": -1
          },
          "yAxis": {
            "midiCC": 61,
            "midiChannel": 1,
            "minCC": 0,
            "maxCC": 127,
            "invertValue": true,
            "defaultValue": -1
          },
          "xSwipe": null,
          "ySwipe": null,
          "pairings": [
            {
              "positionFilter": "topLeft",
              "finger": {
                "xAxis": null,
                "yAxis": null,
                "xSwipe": null,
                "ySwipe": {
                  "sensitivity": 0.03,
                  "midiCC": 49,
                  "midiChannel": 1,
                  "minCC": 0,
                  "maxCC": 127,
                  "invertValue": true,
                  "defaultValue": 0
                },
                "pairings": null
              }
            },
            {
              "positionFilter": "topRight",
              "finger": {
                "xAxis": null,
                "yAxis": null,
                "xSwipe": null,
                "ySwipe": {
                  "sensitivity": 0.03,
                  "midiCC": 3,
                  "midiChannel": 1,
                  "minCC": 0,
                  "maxCC": 127,
                  "invertValue": true,
                  "defaultValue": 0
                },
                "pairings": null
              }
            },
            {
              "positionFilter": "bottom",
              "finger": {
                "xAxis": null,
                "yAxis": null,
                "xSwipe": {
                  "sensitivity": 0.03,
                  "midiCC": 60,
                  "midiChannel": 1,
                  "minCC": 0,
                  "maxCC": 127,
                  "invertValue": false,
                  "defaultValue": 63
                },
                "ySwipe": null,
                "pairings": null
              }
            }
          ]
        }
      ]
    }
  ]
}
```

In this config the first finger functions as an XY slider, assigned to dynamics and vibrato. If 2nd finger is pressed on the top left corner then the Y axis of the 2nd finger functions as a swipe axis (assigned to woodwind growl). If 2nd finger is pressed on the top right corner then the Y axis of the 2nd finger functions as a swipe axis (assigned to woodwind flutter). If 2nd finger is pressed on the bottom then the X axis of the 2nd finger functions as a swipe axis (assigned to pitch bend).

## Requirements

- .net 5.0
- A windows precision touchpad (e.g. Apple Magic Trackpad with a custom Windows driver: https://github.com/imbushuo/mac-precision-touchpad)
- A virtual MIDI controller, e.g. LoopBe1 midi or similar
