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

        private readonly int _secondsToCount;
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
                    return new CountdownTimerEpi(config);
                });
        }

        public CountdownTimerEpi(DeviceConfig config) 
            : base(config.Key)
        {
            var timerName = string.Format("{0}-timer", config.Key);
            _countdownTimer = new SecondsCountdownTimer(timerName);

            var props = JsonConvert.DeserializeObject<CountdowmTimerPropsConfig>(config.Properties.ToString());
            _countdownTimer.SecondsToCount = props.CountdownTime;
            _secondsToCount = props.CountdownTime;

            if (_secondsToCount == 0) throw new Exception("The countdown timer must be > 0");

            _warningTime = props.WarningTime;
            _extendTime = props.ExtendTime;

            TimerWarningFb = new BoolFeedbackPulse(500);
            TimerExpiredFb = new BoolFeedbackPulse(500);
        }

        public override bool CustomActivate()
        {
            _countdownTimer.HasStarted += (sender, args) =>
                {
                    var timer = sender as SecondsCountdownTimer;
                    Debug.Console(2, timer, "Countdown started and will expire at {0}", timer.FinishTime);
                };

            _countdownTimer.HasFinished += (sender, args) =>
                {
                    TimerExpiredFb.Feedback.FireUpdate();
                    _countdownTimer.SecondsToCount = _secondsToCount;
                };

            _countdownTimer.WasCancelled += (sender, args) =>
                {
                    _countdownTimer.SecondsToCount = _secondsToCount;
                };

            _countdownTimer.TimeRemainingFeedback.OutputChange += (sender, args) =>
                {
                    var timer = sender as SecondsCountdownTimer;
                    var timeRemaining = Convert.ToInt32(_countdownTimer.TimeRemainingFeedback.ValueFunc.Invoke());
                    Debug.Console(2, timer, "Time remaining:{0}", timeRemaining);

                    if (_warningTime == null) return;
                    if (timeRemaining == (int)_warningTime)
                    {
                        TimerWarningFb.Feedback.FireUpdate();
                    }
                };

            return base.CustomActivate();
        }

        public Action TimerStart { get { return _countdownTimer.Start; } }
        public Action TimerCancel { get { return _countdownTimer.Cancel; } }
        public Action TimerFinish { get { return _countdownTimer.Finish; } }

        public void Extend()
        {
            if (_extendTime == null) return;
            _countdownTimer.SecondsToCount = (int)_extendTime;
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

            trilist.SetSigTrueAction(joinMap.TimerStart, timer.TimerStart);
            trilist.SetSigTrueAction(joinMap.TimerCancel, timer.TimerCancel);
            trilist.SetSigTrueAction(joinMap.TimerFinish, timer.TimerFinish);
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

