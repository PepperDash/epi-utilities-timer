![PepperDash Essentials Plugin](./images/essentials-plugin-blue.png)

# PepperDash Utilites Timer Plugin

> This plugin is a Simpl Windows wrapper of the existing Essentials Countdown timer.

## Config Example

```json
{
    "key": "shutdown-timer",
    "name": "Shutdown Timer",
    "type": "countdowntimer",
    "group": "timers",
    "properties": {
        "countdownTime": 30,
        "warningTime": 10,
        "extendTime": 5
    }
}
```

**Time values must be entered in 'seconds'**

# Countdown Timer JoinMap

## Digitals

| Join Number | Join Span | Description                                                           | Type    | Capabilities |
| ----------- | --------- | --------------------------------------------------------------------- | ------- | ------------ |
|   1         | 1         | Timer countdown start                                                 | Digital | FromSIMPL    |
|   1         | 1         | Timer countdown active feedback                                       | Digital | ToSIMPL      |
|   2         | 1         | Timer countdown cancel                                                | Digital | FromSIMPL    |
|   2         | 1         | Timer expired feedback (pulse)                                        | Digital | ToSIMPL      |
|   3         | 1         | Timer countdown finish/end (expired feedback will pulse)              | Digital | FromSIMPL    |
|   3         | 1         | Timer warning feedback (pulse)                                        | Digital | ToSIMPL      |
|   4         | 1         | Timer extend active countdown (extends based on the configured value) | Digital | ToSIMPL      |

## Analogs

| Join Number | Join Span | Description                        | Type   | Capabilities |
| ----------- | --------- | ---------------------------------- | ------ | ------------ |
|   1         | 1         | Timer countdown length set         | Analog | FromSIMPL    |
|   1         | 1         | Timer percentage complete (0-100d) | Analog | ToSIMPL      |

## Serials

| Join Number | Join Span | Description                         | Type   | Capabilities |
| ----------- | --------- | ----------------------------------- | ------ | ------------ |
|   1         | 1         | Timer countdown remaining time text | Serial | ToSIMPL      |

## Config Example

```json
{
    "key": "countup-timer",
    "name": "Count Up Timer",
    "type": "countuptimer",
    "group": "timers",
    "properties": {
        "autoStopOnStartRelease": true
    }
}
```

**Time values must be entered in 'seconds'**

# Countup Timer JoinMap

## Digitals

| Join Number | Join Span | Description                                                           | Type    | Capabilities |
| ----------- | --------- | --------------------------------------------------------------------- | ------- | ------------ |
|  11         | 1         | Timer countup start                                                   | Digital | FromSIMPL    |
|  11         | 1         | Timer countup active feedback                                         | Digital | ToSIMPL      |
|  12         | 1         | Timer countup stop                                                    | Digital | FromSIMPL    |

## Serials

| Join Number | Join Span | Description                         | Type   | Capabilities |
| ----------- | --------- | ----------------------------------- | ------ | ------------ |
|  11         | 1         | Timer countup elapsed time text     | Serial | ToSIMPL      |







