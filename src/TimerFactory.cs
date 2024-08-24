using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace CountdownTimerEpi
{
	public class TimerFactory : EssentialsPluginDeviceFactory<TimerDevice>
	{
		/// <summary>
		/// Plugin device factgory constructor
		/// </summary>
		public TimerFactory()
		{
			MinimumEssentialsFrameworkVersion = "1.10.3";
            TypeNames = new List<string> { "countdownTimer", "countupTimer" };
		}

		/// <summary>
		/// Builds and returns and instance of the timer device
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>
		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);

			// get the plugin device properties configuration object & check for null 
			var propertiesConfig = dc.Properties.ToObject<TimerPropertiesConfig>();
            if (propertiesConfig != null) new CountupTimer(dc.Key, dc.Name, propertiesConfig);
            if (propertiesConfig != null) return new TimerDevice(dc.Key, dc.Name, propertiesConfig);
			
			Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
			return null;
		}
	}
}