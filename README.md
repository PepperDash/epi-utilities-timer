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

- public void Factory_Source_Sets_MinimumEssentialsFrameworkVersion_To_Numeric_Core_Of_Essentials_Package(string factoryName)
- public void Factory_Source_Sets_TypeNames(string factoryName)
- public void Factory_Source_Registers_TypeName(string factoryName, string typeName)
- public void No_Duplicate_TypeNames_Across_Factories()
- public void Every_Discovered_Factory_Is_Covered_By_These_Tests()
- public void JoinMap_Derives_From_JoinMapBaseAdvanced_With_JoinStart_Constructor(string joinMapName)
- public void JoinMap_Fields_Are_Public_Instance_With_Matching_JoinName(string joinMapName)
- public void JoinMap_JoinNames_Are_Unique(string joinMapName)
- public void JoinMap_Source_Matches_Compiled_Fields(string joinMapName)
- public void JoinMap_Joins_Have_Valid_Number_Span_And_Type(string joinMapName)
- public void JoinMap_Join_Numbers_Do_Not_Overlap_Per_Type_And_Direction(string joinMapName)
- public void JoinMap_Matches_Documented_Join_Number_And_Type(string joinMapName, string joinName, uint number, string type)
- public void JoinMap_Has_Expected_Join_Count(string joinMapName, int count)
- public void Config_Class_Exists_With_Public_Parameterless_Constructor(string className)
- public void Config_Property_Has_JsonPropertyAttribute(string className, string jsonName, string clrTypeName)
- public void ConfigurationFile_Timer_Types_Resolve_To_Registered_Factory_TypeNames()
- public void Readme_Examples_Use_Registered_Factory_TypeNames()
- public void Countdown_Readme_Example_Deserializes()
- public void Countup_Readme_Example_Deserializes()
- public void Countdown_ConfigurationFile_Example_Deserializes()
- public void Countup_ConfigurationFile_Example_Deserializes()
- public void Sample_Property_Keys_All_Map_To_Config_Properties(string deviceType, Type configType)
- public void Countdown_Optional_Times_Default_To_Null_When_Omitted()
- public void Countup_Defaults_AutoStop_To_False_When_Omitted()
- public void Assembly_Loads_Successfully()
- public void Assembly_Name_Matches_Expected()
- public void Plugin_Csproj_Pins_Essentials_v3_Release_Candidate()
- public void Essentials_Dependencies_Resolve_From_v3_Release_Candidate()
- public void Factory_Base_Type_Resolves_From_Essentials_v3()
- public void Factory_Count_Matches_Expected()
- public void Factory_Exists_ByName(string factoryName)
- public void All_Factories_Have_Public_Parameterless_Constructor()
- public void Factory_Builds_Expected_Device_Type(string factoryName, string deviceName)
- public void Device_Type_Is_Public_EssentialsDevice(string deviceName)
- public void Device_Type_Is_Bridgeable(string deviceName)
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
