![PepperDash Essentials Plugin](./images/essentials-plugin-blue.png)

# PepperDash Utilites Timer Plugin

> This plugin is a Simpl Windows wrapper of the existing Essentials Countdown timer.

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







<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 2.4.4
<!-- END Minimum Essentials Framework Versions -->
<!-- START Config Example -->
### Config Example

```json
{
    "key": "GeneratedKey",
    "uid": 1,
    "name": "GeneratedName",
    "type": "CountdownTimerProperties",
    "group": "Group",
    "properties": {
        "countdownTime": 0,
        "warningTime": 0,
        "extendTime": 0
    }
}
```
<!-- END Config Example -->
<!-- START Supported Types -->

<!-- END Supported Types -->
<!-- START Join Maps -->

<!-- END Join Maps -->
<!-- START Interfaces Implemented -->

<!-- END Interfaces Implemented -->
<!-- START Base Classes -->
### Base Classes

- JoinMapBaseAdvanced
- TimerBaseDeviceFactory<CountdownTimer>
- EssentialsBridgeableDevice
<!-- END Base Classes -->
<!-- START Public Methods -->
### Public Methods

- public void Extend()
- public void TriggerCountupTimerChange(string message)
- public void Start()
- public void Stop()
- public void Reset()
- public void CallTimerIncrement(object notUsed)
<!-- END Public Methods -->
<!-- START Bool Feedbacks -->
### Bool Feedbacks

- TimerRunningFb
- CountupTimerRunningFb
<!-- END Bool Feedbacks -->
<!-- START Int Feedbacks -->
### Int Feedbacks

- TimerPercentageFb
<!-- END Int Feedbacks -->
<!-- START String Feedbacks -->
### String Feedbacks

- TimerValueFb
- CountupTimerValueFb
<!-- END String Feedbacks -->
