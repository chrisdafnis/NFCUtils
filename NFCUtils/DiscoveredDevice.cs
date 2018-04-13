using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Zebra.Android.Discovery;

namespace com.touchstar.chrisd
{
    public class DiscoveredDevice : IDiscoveredDevice
    {
        public String address;
        public DiscoveredDevice(String paramString)
        {
            this.address = paramString;
        }

        public override String ToString()
        {
            return this.address;
        }
    }

    public interface IDiscoveredDevice
    {
    }
}