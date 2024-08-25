using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace TimerDevice
{
    public class CountupTimer : EssentialsBridgeableDevice
    {
        private CTimer _countupCTimer;
        private TimeSpan _countupTimerTimeSpan;
        private bool _autoStopOnStartReleaseBool;
        public BoolFeedback CountupTimerRunningFb { get; private set; }
        public StringFeedback CountupTimerValueFb { get; private set; }
        public event EventHandler<CountupTimerEventArgs> CountupTimerChanged;
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        public CountupTimer(string key, string name, CountupTimerPropertiesConfig propertiesConfig)
            : base(key, name)
        {
            if (propertiesConfig == null) return;

            _autoStopOnStartReleaseBool = propertiesConfig.autoStopOnStartRelease;
            CountupTimerRunningFb = new BoolFeedback(() => this.IsRunning);
            CountupTimerValueFb = new StringFeedback(() => _countupTimerTimeSpan.ToString());

            CrestronEnvironment.ProgramStatusEventHandler += eventType =>
            {
                if (eventType != eProgramStatusEventType.Stopping)
                {
                    this.Stop();
                    _countupCTimer = null;
                }
            };
        }

        /// <summary>
        /// Update the method signature to use the custom EventArgs class:
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCountupTimerChange(CountupTimerEventArgs e)
        {
            CountupTimerChanged += (sender, args) => { };
        }

        /// <summary>
        /// Method to trigger custom event
        /// </summary>
        /// <param name="message"></param>
        public void TriggerCountupTimerChange(string message)
        {
            OnCountupTimerChange(new CountupTimerEventArgs(message));
        }
        
        /// <summary>
        /// Start countupTimer
        /// </summary>
        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            CountupTimerRunningFb.FireUpdate();

            Debug.Console(1, this, "CountupTimer.Start() requested...");

            if(_countupTimerTimeSpan == null)
                _countupTimerTimeSpan = new TimeSpan();

            if (_countupCTimer == null)
            {
                Debug.Console(1, this, "Creating CountupTimer");
                _countupCTimer = new CTimer(CallTimerIncrement, null, 1000, 1000);
            }
            else
            {
                Debug.Console(1, this, "Resetting CountupTimer");
                _countupCTimer.Reset(1000, 1000);
            }
        }

        /// <summary>
        /// Stop countupTimer
        /// </summary>
        public void Stop()
        {
            Debug.Console(1, this, "CountupTimer.Stop() requested...");

            if (_countupCTimer != null)
            {
                _countupCTimer.Stop();
                IsRunning = false;
                CountupTimerRunningFb.FireUpdate();
                _countupTimerTimeSpan = new TimeSpan();
                _countupCTimer = null;
            }
            else
            { Debug.Console(1, this, "Stop() called while _countupCTimer null."); }
        }

        /// <summary>
        /// Restart the existing timer where it left off
        /// </summary>
        public void Reset()
        {
            _countupCTimer.Reset(1000, 1000);
            this.IsRunning = true;
        }

        /// <summary>
        /// Callback method for countupTimer
        /// </summary>
        /// <param name="notUsed">Not used</param>
        public void CallTimerIncrement(object notUsed)
        {
            _countupTimerTimeSpan = _countupTimerTimeSpan.Add(TimeSpan.FromSeconds(1));
            var elapsedTime = GetElapsedTime();
            TriggerCountupTimerChange(elapsedTime.ToString());
            CountupTimerValueFb.FireUpdate();
        }

        /// <summary>
        /// Method that returns the countupTimer in a specific format
        /// </summary>
        /// <returns>String in HH:mm:ss format</returns>
        private string GetElapsedTime()
        {
            return string.Format("{0:00}:{1:00}:{2:00}", (int)_countupTimerTimeSpan.TotalHours, _countupTimerTimeSpan.Minutes, _countupTimerTimeSpan.Seconds);
        }

        /// <summary>
        /// Method to trigger Start or Stop countupTimer
        /// </summary>
        /// <param name="state">Pass</param>
        private void StartStopCountupTimer(bool state)
        {
            if (state)
            { 
                Start();
            }
            else if (!state)
            {
                if (_autoStopOnStartReleaseBool)
                    this.Stop();
            }
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
            var joinMap = new CountupTimerJoinMap(joinStart);

            // this adds the join map to the collection of the bridge
            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            Debug.Console(0, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, this, "Linking to Bridge Type {0}", GetType().Name);

            CountupTimerRunningFb.LinkInputSig(trilist.BooleanInput[joinMap.CountupTimerCounting.JoinNumber]);
            CountupTimerValueFb.LinkInputSig(trilist.StringInput[joinMap.CountupTimerValue.JoinNumber]);

            trilist.SetBoolSigAction(joinMap.CountupTimerStart.JoinNumber, StartStopCountupTimer);
            trilist.SetSigTrueAction(joinMap.CountupTimerStop.JoinNumber, () => Stop());
        }
    }

    public class CountupTimerEventArgs : EventArgs
    {
        public string Message { get; set; }

        public CountupTimerEventArgs(string message)
        {
            Message = message;
        }
    }
}