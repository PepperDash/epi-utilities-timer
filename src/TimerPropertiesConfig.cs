using Newtonsoft.Json;

namespace TimerDevice
{
	/// <summary>
	/// Configuration objects specific to device
	/// </summary>
    public class CountdownTimerPropertiesConfig
	{
		[JsonProperty("countdownTime")]
		public int CountdownTime { get; set; }

		[JsonProperty("warningTime")]
		public int? WarningTime { get; set; }

		[JsonProperty("extendTime")]
		public int? ExtendTime { get; set; }
	}

    /// <summary>
    /// Configuration objects specific to device
    /// </summary>
    public class CountupTimerPropertiesConfig
    {
        [JsonProperty("autoStopOnStartRelease")]
        public bool autoStopOnStartRelease { get; set; }
    }
}