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

# Timer JoinMap

## Digitals

| Join Number | Join Span | Description                                                           | Type    | Capabilities |
| ----------- | --------- | --------------------------------------------------------------------- | ------- | ------------ |
| 401         | 1         | Timer countdown start                                                 | Digital | FromSIMPL    |
| 401         | 1         | Timer countdown active feedback                                       | Digital | ToSIMPL      |
| 402         | 1         | Timer countdown cancel                                                | Digital | FromSIMPL    |
| 402         | 1         | Timer expired feedback (pulse)                                        | Digital | ToSIMPL      |
| 403         | 1         | Timer countdown finish/end (expired feedback will pulse)              | Digital | FromSIMPL    |
| 403         | 1         | Timer warning feedback (pulse)                                        | Digital | ToSIMPL      |
| 404         | 1         | Timer extend active countdown (extends based on the configured value) | Digital | ToSIMPL      |

## Analogs

| Join Number | Join Span | Description                        | Type   | Capabilities |
| ----------- | --------- | ---------------------------------- | ------ | ------------ |
| 401         | 1         | Timer countdown length set         | Analog | FromSIMPL    |
| 401         | 1         | Timer percentage complete (0-100d) | Analog | ToSIMPL      |

## Serials

| Join Number | Join Span | Description                         | Type   | Capabilities |
| ----------- | --------- | ----------------------------------- | ------ | ------------ |
| 401         | 1         | Timer countdown remaining time text | Serial | ToSIMPL      |







