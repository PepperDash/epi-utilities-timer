using System;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Core;
using PepperDash.Essentials.Core.Devices;
using Newtonsoft.Json;

namespace epi_utilities_countdown_timer
{
    public class CountdownTimerEpi : ReconfigurableDevice, IBridgeAdvanced
    {
        private readonly SecondsCountdownTimer _countdownTimer;

        public SecondsCountdownTimer Timer { get { return _countdownTimer; } }
        public int SecondsToCount 
        {
            get { return _secondsToCount; }
            set
            {
                _secondsToCount = value;
                _countdownTimer.SecondsToCount = value;
            }
        }

        private string Key;

        private DeviceConfig _Dc;

        private int _secondsToCount;
        private readonly int? _warningTime;
        private readonly int? _extendTime;

        public BoolFeedback TimerRunningFb { get { return _countdownTimer.IsRunningFeedback; } }
        public BoolFeedbackPulse TimerExpiredFb { get; private set; }
        public BoolFeedbackPulse TimerWarningFb { get; private set; }

        public IntFeedback TimerPercentageFb { get { return _countdownTimer.PercentFeedback; } }
        public IntFeedback TimerSetFeedback { get; private set; }

        public StringFeedback TimerValueFb { get { return _countdownTimer.TimeRemainingFeedback; } }

        public static void LoadPlugin()
        {
            DeviceFactory.AddFactoryForType("countdowntimer", config =>
                {
                    var props = JsonConvert.DeserializeObject<CountdowmTimerPropsConfig>(config.Properties.ToString());
                    var timer = new SecondsCountdownTimer(string.Format("{0}-timer", config.Key));
                    timer.SecondsToCount = props.CountdownTime;

                    return 
                        new CountdownTimerEpi(config, timer, props);
                });
        }

        public CountdownTimerEpi(DeviceConfig config, SecondsCountdownTimer timer, CountdowmTimerPropsConfig props)
            : base(config)
        {
            _Dc = config;
            Key = config.Key;
            _secondsToCount = props.CountdownTime;
            _countdownTimer = timer;
            _warningTime = props.WarningTime;
            _extendTime = props.ExtendTime;

            TimerWarningFb = new BoolFeedbackPulse(500);
            TimerExpiredFb = new BoolFeedbackPulse(500);

            CrestronEnvironment.ProgramStatusEventHandler += eventType =>
                {
                    if (eventType != eProgramStatusEventType.Stopping) 
                        return;

                    _countdownTimer.Cancel();
                };
        }

        public override bool CustomActivate()
        {
            TimerSetFeedback = new IntFeedback(() => _secondsToCount);
            Debug.Console(2, _countdownTimer, "Activated a new timer with a {0} second countdown", _countdownTimer.SecondsToCount);

            _countdownTimer.HasStarted += (sender, args) =>
                {
                    var timer = sender as SecondsCountdownTimer;
                    Debug.Console(1, timer, "Countdown started and will expire at {0}", timer.FinishTime.ToShortTimeString());
                };

            _countdownTimer.HasFinished += (sender, args) =>
                {
                    var timer = sender as SecondsCountdownTimer;
                    TimerExpiredFb.Start();

                    Debug.Console(1, timer, "Countdown has completed");
                    _countdownTimer.SecondsToCount = SecondsToCount;
                };

            _countdownTimer.WasCancelled += (sender, args) =>
                {
                    var timer = sender as SecondsCountdownTimer;
                    Debug.Console(1, timer, "Countdown cancelled");
                    _countdownTimer.SecondsToCount = SecondsToCount;
                };

            _countdownTimer.TimeRemainingFeedback.OutputChange += (sender, args) =>
                {
                    if (_warningTime == null) 
                        return;

                    var timeRemainingString = _countdownTimer.FinishTime.Subtract(DateTime.Now).ToString();
                    double timeRemaining = _countdownTimer.FinishTime.Subtract(DateTime.Now).TotalSeconds;
                    Debug.Console(1, this, "Time remaining for warning:{0}|{1}", timeRemainingString, timeRemaining);

                    if (timeRemaining == (int)_warningTime)
                        TimerWarningFb.Start();
                };

            _countdownTimer.PercentFeedback.OutputChange += (sender, args) =>
            {
                var timer = sender as Feedback;
                var timeRemaining = args.IntValue;

                if (_warningTime == null) 
                    return;
            };

            return base.CustomActivate();
        }

        public void Extend()
        {
            if (!_countdownTimer.IsRunningFeedback.BoolValue)
                return;

            int timeToExtend = _secondsToCount;
            if (_extendTime != null)
                timeToExtend = (int)_extendTime;

            Debug.Console(1, _countdownTimer, "Countdown extended {0}", timeToExtend);
            _countdownTimer.SecondsToCount = timeToExtend;
            _countdownTimer.Reset();
        }
        public void SetSecondsToCount(ushort value)
        {
            Timer.SecondsToCount = value;
            _Dc.Properties["countdownTime"] = value;
            CustomSetConfig(_Dc);
            TimerSetFeedback.FireUpdate();

        }

        public void LinkToApi(Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, uint joinstart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new CountdownTimerJoinMapAdvanced(joinstart);

            if(bridge != null) bridge.AddJoinMap(String.Format(this.Key), joinMap);

            TimerRunningFb.LinkInputSig(trilist.BooleanInput[joinMap.TimerCounting.JoinNumber]);
            TimerExpiredFb.Feedback.LinkInputSig(trilist.BooleanInput[joinMap.TimerExpired.JoinNumber]);
            TimerWarningFb.Feedback.LinkInputSig(trilist.BooleanInput[joinMap.TimerWarning.JoinNumber]);

            TimerPercentageFb.LinkInputSig(trilist.UShortInput[joinMap.TimerPercentage.JoinNumber]);
            TimerValueFb.LinkInputSig(trilist.StringInput[joinMap.TimerValue.JoinNumber]);

            TimerSetFeedback.LinkInputSig(trilist.UShortInput[joinMap.TimerCountdownSetFb.JoinNumber]);

            trilist.SetSigTrueAction(joinMap.TimerStart.JoinNumber, () =>
                {
                    Timer.Start();
                });

            trilist.SetSigTrueAction(joinMap.TimerCancel.JoinNumber, () =>
                {
                    Timer.Cancel();
                });

            trilist.SetSigTrueAction(joinMap.TimerFinish.JoinNumber, () =>
                {
                    Timer.Finish();
                });

            trilist.SetUShortSigAction(joinMap.TimerCountdownSet.JoinNumber, value =>
                {
                    SecondsToCount = value;
                });

            trilist.SetSigTrueAction(joinMap.TimerExtend.JoinNumber, Extend);
        }


    }

    

    public class CountdowmTimerPropsConfig
    {
        public int CountdownTime { get; set; }
        public int? WarningTime { get; set; }
        public int? ExtendTime { get; set; }
    }


    public class CountdownTimerJoinMapAdvanced : JoinMapBaseAdvanced
    {
        [JoinName("TimerStart")]
        public JoinDataComplete TimerStart =
            new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Start Timer",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("TimerCancel")]
        public JoinDataComplete TimerCancel =
            new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Cancel Timer",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("TimerFinish")]
        public JoinDataComplete TimerFinish =
            new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Finish Timer",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("TimerExtend")]
        public JoinDataComplete TimerExtend =
            new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Start Timer",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("TimerCounting")]
        public JoinDataComplete TimerCounting =
            new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Timer Is Counting",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("TimerExpired")]
        public JoinDataComplete TimerExpired =
            new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Timer Is Expired",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("TimerWarning")]
        public JoinDataComplete TimerWarning =
            new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Timer is in Warning Mode",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("TimerCountdownSet")]
        public JoinDataComplete TimerCountdownSet =
            new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Set Timer Value",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Analog
            });

        [JoinName("TimerCountdownSetFb")]
        public JoinDataComplete TimerCountdownSetFb =
            new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Get Timer Value",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });


        [JoinName("TimerPercentage")]
        public JoinDataComplete TimerPercentage =
            new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Timer Percentage Remaining",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

        [JoinName("TimerValue")]
        public JoinDataComplete TimerValue =
            new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Timer Text Remaining",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        public CountdownTimerJoinMapAdvanced(uint joinStart)
            : base(joinStart, typeof(CountdownTimerJoinMapAdvanced))
        {
        }


    }
}

