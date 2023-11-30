using Newtonsoft.Json;

namespace CountdownTimerEpi
{
	public class TimerPropertiesConfig
	{
		[JsonProperty("countdownTime")]
		public int CountdownTime { get; set; }

		[JsonProperty("warningTime")]
		public int? WarningTime { get; set; }

		[JsonProperty("extendTime")]
		public int? ExtendTime { get; set; }
	}
}