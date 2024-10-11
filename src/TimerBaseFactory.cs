using System;
using System.Linq;
using TimerDevice;
using PepperDash.Essentials.Core;

namespace Timer.Factories
{

    public abstract class TimerBaseDeviceFactory<T> : EssentialsPluginDeviceFactory<T> where T : EssentialsDevice
    {
        public const string MinumumEssentialsVersion = "1.16.0";
    }
}