# PepperDash Countdown Timer Plugin

> This plugin is a Simpl Windows wrapper of the existing Essentials Countdown timer.

## Join Map

### Digitals

| Join | Type (RW) | Write Description | Read Description |
| ---- | --------- | ----------------- | ---------------- |
| 1    | RW        | Timer Start       | Timer Counting   |
| 2    | RW        | Timer Cancel      | Timer Expired (Pulse)   |
| 3    | RW        | Timer Finish      | Timer Warning (Pulse)   |
| 4    | W         | Timer Extend      | -                |

### Analogs

| Join | Type (RW) | Write Description | Read Description |
| ---- | --------- | ----------------- | ---------------- |
| 1    | R         | -                 | Timer Percentage |

### Serials

| Join | Type (RW) | Write Description | Read Description |
| ---- | --------- | ----------------- | ---------------- |
| 1    | R         | -                 | Timer Value      |

## Join Details

1. Timer Start - Pulse to begin the countdown
1. Timer Cancel - Pulse to cancel the countdown.  Timer expired will not pulse.
1. Timer Finish - Pulse to finish the countdown.  Timer expired will pulse.
1. Timer Extend - Pulse to extend the timer the number of seconds defined in the config file.
1. Timer Counting - Held high while the countdown is in progress.
1. Timer Expired - Pulsed when the countdown is completed or finished.
1. Timer Warning - Pulsed when the warning time defined in the config file is reached.
1. Timer Percentage - Analog value of percentage remaining
1. Timer Value - Serial value of seconds remaining

## Config Example

```JSON
{
    "key": "MainCountdown",
    "uid": 74,
    "name": "MainCountdown",
    "type": "countdowntimer",
    "group": "timers",
    "properties": {
        "countdownTime": 60,
        "warningTime": 30,
        "extendTime": 60
    }
},
```

__Properties:__

1. Countdown Time - The total number of seconds of the countdown.
1. Warning Time - The number of seconds remaining in the countdown that will trigger the Timer Warning join to pulse.
1. Extend Timer - The number of seconds the timer will be reset to when the Extend Timer join is pressed.
