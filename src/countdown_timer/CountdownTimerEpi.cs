using System;
using System.Linq;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       				// For Basic SIMPL#Pro classes
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Core;
using PepperDash.Essentials.Bridges;
using Crestron.SimplSharp.Reflection;
using Newtonsoft.Json;

namespace epi_utilities_countdown_timer
{
    public class CountdownTimerEpi : Device, IBridge
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

        private int _secondsToCount;
        private readonly int? _warningTime;
        private readonly int? _extendTime;

        public BoolFeedback TimerRunningFb { get { return _countdownTimer.IsRunningFeedback; } }
        public BoolFeedbackPulse TimerExpiredFb { get; private set; }
        public BoolFeedbackPulse TimerWarningFb { get; private set; }

        public IntFeedback TimerPercentageFb { get { return _countdownTimer.PercentFeedback; } }

        public StringFeedback TimerValueFb { get { return _countdownTimer.TimeRemainingFeedback; } }

        public static void LoadPlugin()
        {
            DeviceFactory.AddFactoryForType("countdowntimer", config =>
                {
                    var props = JsonConvert.DeserializeObject<CountdowmTimerPropsConfig>(config.Properties.ToString());
                    var timer = new SecondsCountdownTimer(string.Format("{0}-timer", config.Key));
                    timer.SecondsToCount = props.CountdownTime;

                    return new CountdownTimerEpi(config.Key, timer, props);
                });
        }

        public CountdownTimerEpi(string key, SecondsCountdownTimer timer, CountdowmTimerPropsConfig props)
            : base(key)
        {
            _secondsToCount = props.CountdownTime;
            _countdownTimer = timer;
            _warningTime = props.WarningTime;
            _extendTime = props.ExtendTime;

            TimerWarningFb = new BoolFeedbackPulse(500);
            TimerExpiredFb = new BoolFeedbackPulse(500);

            CrestronEnvironment.ProgramStatusEventHandler += eventType =>
                {
                    if (eventType != eProgramStatusEventType.Stopping) return;

                    _countdownTimer.Cancel();
                };
        }

        public override bool CustomActivate()
        {
            Debug.Console(2, _countdownTimer, "Activated a new timer with a {0} second countdown", _countdownTimer.SecondsToCount);

            _countdownTimer.HasStarted += (sender, args) =>
                {
                    var timer = sender as SecondsCountdownTimer;
                    Debug.Console(2, timer, "Countdown started and will expire at {0}", timer.FinishTime.ToShortTimeString());
                };

            _countdownTimer.HasFinished += (sender, args) =>
                {
                    var timer = sender as SecondsCountdownTimer;
                    TimerExpiredFb.Start();

                    Debug.Console(2, timer, "Countdown has completed");
                    _countdownTimer.SecondsToCount = SecondsToCount;
                };

            _countdownTimer.WasCancelled += (sender, args) =>
                {
                    var timer = sender as SecondsCountdownTimer;
                    Debug.Console(2, timer, "Countdown cancelled");
                    _countdownTimer.SecondsToCount = SecondsToCount;
                };

            _countdownTimer.TimeRemainingFeedback.OutputChange += (sender, args) =>
                {
                    if (_warningTime == null) 
                        return;

                    var timeRemainingString = _countdownTimer.FinishTime.Subtract(DateTime.Now).ToString();
                    double timeRemaining = _countdownTimer.FinishTime.Subtract(DateTime.Now).TotalSeconds;
                    Debug.Console(2, this, "Checking Time remaining for warning:{0}|{1}", timeRemainingString, timeRemaining);

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

            Debug.Console(2, _countdownTimer, "Countdown extended {0}", timeToExtend);
            _countdownTimer.SecondsToCount = timeToExtend;
            _countdownTimer.Reset();
        }

        #region IBridge Members

        public void LinkToApi(Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            this.LinkToApiExt(trilist, joinStart, joinMapKey);
        }

        #endregion
    }

    public class CountdowmTimerPropsConfig
    {
        public int CountdownTime { get; set; }
        public int? WarningTime { get; set; }
        public int? ExtendTime { get; set; }
    }

    public static class CountdownTimerBridge
    {
        public static void LinkToApiExt(this CountdownTimerEpi timer, Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = new CountdownTimerJoinMap(joinStart);

            timer.TimerRunningFb.LinkInputSig(trilist.BooleanInput[joinMap.TimerCounting]);
            timer.TimerExpiredFb.Feedback.LinkInputSig(trilist.BooleanInput[joinMap.TimerExpired]);
            timer.TimerWarningFb.Feedback.LinkInputSig(trilist.BooleanInput[joinMap.TimerWarning]);

            timer.TimerPercentageFb.LinkInputSig(trilist.UShortInput[joinMap.TimerPercentage]);
            timer.TimerValueFb.LinkInputSig(trilist.StringInput[joinMap.TimerValue]);

            trilist.SetSigTrueAction(joinMap.TimerStart, () =>
                {
                    timer.Timer.Start();
                });

            trilist.SetSigTrueAction(joinMap.TimerCancel, () =>
                {
                    timer.Timer.Cancel();
                });

            trilist.SetSigTrueAction(joinMap.TimerFinish, () =>
                {
                    timer.Timer.Finish();
                });

            trilist.SetUShortSigAction(joinMap.TimerCountdownSet, value =>
                {
                    timer.SecondsToCount = value;
                });

            trilist.SetSigTrueAction(joinMap.TimerExtend, timer.Extend);
        }
    }

    public class CountdownTimerJoinMap : JoinMapBase
    {
        public uint TimerStart { get; private set; }
        public uint TimerCancel { get; private set; }
        public uint TimerFinish { get; private set; }
        public uint TimerExtend { get; private set; }

        public uint TimerCounting { get; private set; }
        public uint TimerExpired { get; private set; }
        public uint TimerWarning { get; private set; }

        public uint TimerPercentage { get; private set; }
        public uint TimerCountdownSet { get; private set; }

        public uint TimerValue { get; private set; }


        CountdownTimerJoinMap()
        {
            TimerStart = 1;
            TimerCancel = 2;
            TimerFinish = 3;
            TimerExtend = 4;

            TimerCounting = 1;
            TimerExpired = 2;
            TimerWarning = 3;

            TimerCountdownSet = 1;

            TimerPercentage = 1;

            TimerValue = 1;
        }

        public CountdownTimerJoinMap(uint joinStart)
            : this()
        {
            OffsetJoinNumbers(joinStart);
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinActual = joinStart - 1;

            GetType()
                .GetCType()
                .GetProperties()
                .Where(prop => prop.PropertyType == typeof(uint).GetCType())
                .ToList()
                .ForEach(x => x.SetValue(this, (uint)x.GetValue(this, null) + joinActual, null));
        }
    }
}

