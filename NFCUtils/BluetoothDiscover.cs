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
using Zebra.Android.Comm;
using Zebra.Android.Printer;
using Zebra.Android.Printer.Internal;
using Zebra.Sdk.Comm;

namespace com.touchstar.chrisd
{
    public class BluetoothDiscover
    {
        public static void FindDevices(Context paramContext, IDiscoveryHandler paramDiscoveryHandler)
        {
            try
            {
                BluetoothDiscoverer.FindDevices(paramContext, paramDiscoveryHandler, null);
            }
            catch (ConnectionException localConnectionException)
            {
                throw new ZebraPrinterConnectionException(localConnectionException.GetBaseException().ToString());
            }
        }
    }
}