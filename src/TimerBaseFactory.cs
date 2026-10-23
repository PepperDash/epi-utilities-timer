using System;
using System.Linq;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugins
{

    public abstract class TimerBaseDeviceFactory<T> : EssentialsPluginDeviceFactory<T> where T : EssentialsDevice
    {
        public const string MinumumEssentialsVersion = "3.0.0";
    }
}