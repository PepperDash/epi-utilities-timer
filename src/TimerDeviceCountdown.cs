using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Plugins
{
	public class CountdownTimer : EssentialsBridgeableDevice
	{
		private readonly SecondsCountdownTimer _countdownTimer;
		private int _secondsToCount;
		private readonly int? _warningTime;
		private readonly int? _extendTime;        

		/// <summary>
		/// Timer objects
		/// </summary>
		public SecondsCountdownTimer Timer { get { return _countdownTimer; } }

		/// <summary>
		/// Get/Set seconds to count
		/// </summary>
		public int SecondsToCount
		{
			get { return _secondsToCount; }
			set
			{
				_secondsToCount = value;
				_countdownTimer.SecondsToCount = value;
			}
		}		

		public BoolFeedback TimerRunningFb { get { return _countdownTimer.IsRunningFeedback; } }
		public BoolFeedbackPulse TimerExpiredFb { get; private set; }
		public BoolFeedbackPulse TimerWarningFb { get; private set; }
		public IntFeedback TimerPercentageFb { get { return _countdownTimer.PercentFeedback; } }
		public StringFeedback TimerValueFb { get { return _countdownTimer.TimeRemainingFeedback; } }		

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="propertiesConfig"></param>
        public CountdownTimer(string key, string name, CountdownTimerPropertiesConfig propertiesConfig)
			: base(key, name)
		{
			if (propertiesConfig == null) return;

			_secondsToCount = propertiesConfig.CountdownTime;
			_warningTime = propertiesConfig.WarningTime;
			_extendTime = propertiesConfig.ExtendTime;

			_countdownTimer = new SecondsCountdownTimer(string.Format("{0}-timer", Key))
			{
				SecondsToCount = propertiesConfig.CountdownTime				
			};

			TimerWarningFb = new BoolFeedbackPulse(500);
			TimerExpiredFb = new BoolFeedbackPulse(500);

			CrestronEnvironment.ProgramStatusEventHandler += eventType =>
			{
				if (eventType != eProgramStatusEventType.Stopping) return;

				_countdownTimer.Cancel();
			};
		}

		/// <summary>
		/// device custom activate
		/// </summary>
		/// <returns></returns>
		protected override bool CustomActivate()
		{			
			this.LogVerbose("Activated a new timer with a {SecondsToCount} second countdown", _countdownTimer.SecondsToCount);

            _countdownTimer.HasStarted += (sender, args) =>
			{
				var timer = sender as SecondsCountdownTimer;
				if (timer != null)
					this.LogDebug("Countdown started and will expire at {FinishTime}", timer.FinishTime.ToShortTimeString());
			};

			_countdownTimer.HasFinished += (sender, args) =>
			{
				var timer = sender as SecondsCountdownTimer;
				TimerExpiredFb.Start();
				
				this.LogDebug("Countdown has completed");
				_countdownTimer.SecondsToCount = SecondsToCount;
			};

			_countdownTimer.WasCancelled += (sender, args) =>
			{
				var timer = sender as SecondsCountdownTimer;				
				this.LogDebug("Countdown cancelled");

                _countdownTimer.SecondsToCount = SecondsToCount;
			};

			_countdownTimer.TimeRemainingFeedback.OutputChange += (sender, args) =>
			{
				if (_warningTime == null)
					return;

				var timeRemainingString = _countdownTimer.FinishTime.Subtract(DateTime.Now).ToString();
				var timeRemaining = _countdownTimer.FinishTime.Subtract(DateTime.Now).TotalSeconds;				
                this.LogDebug("Time remaining for warning:{TimeRemaining}|{SecondsRemaining}", timeRemainingString, timeRemaining);


                if (timeRemaining == (int)_warningTime)
					TimerWarningFb.Start();
			};

			_countdownTimer.PercentFeedback.OutputChange += (sender, args) =>
			{
				/*
				var timer = sender as Feedback;
				var timeRemaining = args.IntValue;

				if (_warningTime == null)
					return;
				 */
			};

			return base.CustomActivate();
		}

		/// <summary>
		/// Links the plugin device to the EISC bridge
		/// </summary>
		/// <param name="trilist"></param>
		/// <param name="joinStart"></param>
		/// <param name="joinMapKey"></param>
		/// <param name="bridge"></param>
		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new CountdownTimerJoinMap(joinStart);

			// this adds the join map to the colleciton of the bridge
			if (bridge != null)
			{
				bridge.AddJoinMap(Key, joinMap);
			}

			var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
			if (customJoins != null)
			{
				joinMap.SetCustomJoinData(customJoins);
			}
			
            this.LogInformation("Linking to Trilist '{TrilistId}'", trilist.ID.ToString("X"));           
            this.LogInformation("Linking to Bridge Type {BridgeType}", GetType().Name);


            TimerRunningFb.LinkInputSig(trilist.BooleanInput[joinMap.TimerCounting.JoinNumber]);
			TimerExpiredFb.Feedback.LinkInputSig(trilist.BooleanInput[joinMap.TimerExpired.JoinNumber]);
			TimerWarningFb.Feedback.LinkInputSig(trilist.BooleanInput[joinMap.TimerWarning.JoinNumber]);

			TimerPercentageFb.LinkInputSig(trilist.UShortInput[joinMap.TimerPercentage.JoinNumber]);
			TimerValueFb.LinkInputSig(trilist.StringInput[joinMap.TimerValue.JoinNumber]);

			trilist.SetSigTrueAction(joinMap.TimerStart.JoinNumber, () => Timer.Start());
			trilist.SetSigTrueAction(joinMap.TimerCancel.JoinNumber, () => Timer.Cancel());
			trilist.SetSigTrueAction(joinMap.TimerFinish.JoinNumber, () => Timer.Finish());
			trilist.SetSigTrueAction(joinMap.TimerExtend.JoinNumber, Extend); 
			trilist.SetUShortSigAction(joinMap.TimerCountdownSet.JoinNumber, value =>
			{
				SecondsToCount = value;
			});
		}


		/// <summary>
		/// Extends active timer for the configured length
		/// </summary>
		public void Extend()
		{
			if (!_countdownTimer.IsRunningFeedback.BoolValue)
				return;

			var timeToExtend = _secondsToCount;
			if (_extendTime != null)
				timeToExtend = (int)_extendTime;
		
            this.LogDebug("Countdown extended {ExtendSeconds}", timeToExtend);

            _countdownTimer.SecondsToCount = timeToExtend;
			_countdownTimer.Reset();
		}
	}
}