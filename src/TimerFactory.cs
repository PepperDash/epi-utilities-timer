using System.Collections.Generic;
using System.Linq;
using TimerDevice;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace Timer.Factories
{
	public class CountdownTimerFactory : TimerBaseDeviceFactory<CountdownTimer>
	{
		/// <summary>
		/// Plugin device factory constructor
		/// </summary>
		public CountdownTimerFactory()
		{
			MinimumEssentialsFrameworkVersion = "2.0.0";
            TypeNames = new List<string> { "countdownTimer" };
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
            var propertiesConfig = dc.Properties.ToObject<CountdownTimerPropertiesConfig>();
            if (propertiesConfig != null) return new CountdownTimer(dc.Key, dc.Name, propertiesConfig);
			
			Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
			return null;
		}
	}

	public class CountupTimerFactory : TimerBaseDeviceFactory<CountupTimer>
	{
		/// <summary>
		/// Plugin device factory constructor
		/// </summary>
		public CountupTimerFactory()
		{
			MinimumEssentialsFrameworkVersion = "2.0.0";
            TypeNames = new List<string> { "countupTimer" };
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
            var propertiesConfig = dc.Properties.ToObject<CountupTimerPropertiesConfig>();
            if (propertiesConfig != null) return new CountupTimer(dc.Key, dc.Name, propertiesConfig);
			
			Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
			return null;
		}
	}
}