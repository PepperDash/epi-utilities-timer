![PepperDash Essentials Plugin Logo](/images/essentials-plugin-blue.png)

![PepperDash Essentials](https://img.shields.io/badge/PepperDash%20Essentials-≥%20v3.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Crestron](https://img.shields.io/badge/Crestron-4--Series-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

# PepperDash Utilities Timer Plugin

> This plugin is a SIMPL Windows wrapper of the existing Essentials Countdown timer, plus a simple count-up timer.

## Requirements

- **PepperDash Essentials v3.0.0 or later** (built against `PepperDashEssentials` `3.0.0-rc.19`, targeting .NET 8).
- Crestron **4-Series** processor.

> Plugin releases **v3.0.0 and later require Essentials v3**. For Essentials v2.x, use plugin release v2.0.0.

<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 3.0.0
<!-- END Minimum Essentials Framework Versions -->

<!-- START Supported Types -->
### Supported Types

- countdownTimer
- countupTimer
<!-- END Supported Types -->

# Countdown Timer Config Example

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

# Countup Timer Config Example

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
|  1          | 1         | Timer countup start                                                   | Digital | FromSIMPL    |
|  1          | 1         | Timer countup active feedback                                         | Digital | ToSIMPL      |
|  2          | 1         | Timer countup stop                                                    | Digital | FromSIMPL    |

## Serials

| Join Number | Join Span | Description                         | Type   | Capabilities |
| ----------- | --------- | ----------------------------------- | ------ | ------------ |
|  1          | 1         | Timer countup elapsed time text     | Serial | ToSIMPL      |







