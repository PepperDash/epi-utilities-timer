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
        public BoolFeedback CountUpTimerRunningFb { get; private set; }
        public StringFeedback CountUpTimerValueFb { get; private set; }
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

            CountUpTimerRunningFb = new BoolFeedback(() => this.IsRunning);
            CountUpTimerValueFb = new StringFeedback(() => _countupTimerTime.ToString());

            CrestronEnvironment.ProgramStatusEventHandler += eventType =>
            {
                if (eventType != eProgramStatusEventType.Stopping) return;

                _countupTimer.Stop();
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
            this._countupStartTime = DateTime.Now;
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
                this.IsRunning = false;
            }
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
                        Debug.Console(1, this, "CountupTimer message: {0}\n", usageString);
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
            // create a string variable of the _countupTimerTime
            var message = string.Format("{0}", _countupTimerTime.ToString());
            TriggerCountupTimerChange(message);
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

            CountUpTimerRunningFb.LinkInputSig(trilist.BooleanInput[joinMap.CountUpTimerCounting.JoinNumber]);
            CountUpTimerValueFb.LinkInputSig(trilist.StringInput[joinMap.CountUpTimerValue.JoinNumber]);

            trilist.SetSigTrueAction(joinMap.CountUpTimerStart.JoinNumber, () => Start());
            trilist.SetSigFalseAction(joinMap.CountUpTimerStart.JoinNumber, () => Stop());
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