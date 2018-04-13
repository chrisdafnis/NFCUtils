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
    public class DiscoveredDeviceBluetooth : DiscoveredDevice
    {
        String FriendlyName;
        public DiscoveredDeviceBluetooth(String paramString1, String paramString2) : base (paramString1)
        {
            this.FriendlyName = paramString2;
        }
    }
}