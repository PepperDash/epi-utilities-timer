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
        private CTimer _countupTimer;
        private DateTime _countupStartTime;
        private TimeSpan _countupTimerTime;
        public bool _autoStopOnStartRelease;
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

            _autoStopOnStartRelease = propertiesConfig.autoStopOnStartRelease;
            CountupTimerRunningFb = new BoolFeedback(() => this.IsRunning);
            CountupTimerValueFb = new StringFeedback(() => _countupTimerTime.ToString());

            CrestronEnvironment.ProgramStatusEventHandler += eventType =>
            {
                if (eventType != eProgramStatusEventType.Stopping)
                {
                    this.Stop();
                    _countupTimer = null;
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

        public void Start()
        {
            Debug.Console(1, this, "CountupTimer.Start() requested...");
            if(_countupStartTime == null)
                _countupStartTime = new DateTime();

            this._countupStartTime = DateTime.Now;
            if(_countupTimerTime == null)
                _countupTimerTime = new TimeSpan();

            if (_countupTimer == null)
            {
                Debug.Console(1, this, "Creating CountupTimer");
                _countupTimer = new CTimer(CallTimerIncrement, null, 1000, 1000);
                this.IsRunning = true;
            }
            else
            {
                Debug.Console(1, this, "Resetting CountupTimer");
                _countupTimer.Reset(1000, 1000);
                this.IsRunning = true;
            }
        }

        public void AutoStop()
        {
            if(_autoStopOnStartRelease)
                this.Stop();
        }


        public void Stop()
        {
            Debug.Console(1, this, "CountupTimer.Stop() requested...");
            if (this._countupStartTime != null)
            {
                if (_countupTimer != null)
                {
                    _countupTimer.Stop();
                    this.IsRunning = false;
                    var usageString = string.Format("{0}", DateTime.Now.ToString("HH:mm:ss"));

                    if (CountupTimerChanged != null)
                    {
                        TriggerCountupTimerChange(usageString);
                        Debug.Console(1, this, "CountupTimer stopped at: {0}\n", usageString);
                    }
                    else
                    {
                        Debug.Console(1, this, "CountupTimerChanged Null\n");
                    }
                }
                else
                { Debug.Console(1, this, "countupTimer null"); }
            }
            else
            { Debug.Console(1, this, "CountupStartTime null."); }
        }

        public void Reset()
        {
            _countupTimer.Reset(1000, 1000);
            this.IsRunning = true;
        }

        public void CallTimerIncrement(object notUsed)
        {
            Debug.Console(1, this, "CountupTimer CTimer Increment");
            _countupTimerTime = _countupTimerTime.Add(TimeSpan.FromSeconds(1));
            string elapsedTime = (string.Format("{0:00}:{1:00}:{2:00}", _countupTimerTime.TotalHours, _countupTimerTime.Minutes, _countupTimerTime.Seconds));
            TriggerCountupTimerChange(elapsedTime);
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

            trilist.SetSigTrueAction(joinMap.CountupTimerStart.JoinNumber, () => Start());
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