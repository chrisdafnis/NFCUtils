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
using Android.Bluetooth;
using Zebra.Sdk.Comm;
using static Zebra.Sdk.Printer.Discovery.BluetoothDiscoverer;
using Zebra.Sdk.Printer.Discovery;
using Zebra.Sdk.Comm.Internal;

namespace com.touchstar.chrisd
{
    public class BluetoothDiscoverer
    {
        private Context mContext;
        private IDiscoveryHandler mDiscoveryHandler;
        BroadcastReceiverClass btReceiver;
        BroadcastReceiverClass btMonitor;
        private IDeviceFilter deviceFilter;

        public BluetoothDiscoverer(Context paramContext, IDiscoveryHandler paramDiscoveryHandler, IDeviceFilter paramDeviceFilter)
        {
            this.mContext = paramContext;
            this.deviceFilter = paramDeviceFilter;
            this.mDiscoveryHandler = paramDiscoveryHandler;
        }

        public static void FindDevices(Context paramContext, IDiscoveryHandler paramDiscoveryHandler, IDeviceFilter paramDeviceFilter)
        {
            BluetoothAdapter localBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (localBluetoothAdapter == null)
            {
                paramDiscoveryHandler.DiscoveryError("No bluetooth radio found");
            }
            else if (!localBluetoothAdapter.IsEnabled)
            {
                paramDiscoveryHandler.DiscoveryError("Bluetooth radio is currently disabled");
            }
            else
            {
                if (localBluetoothAdapter.IsDiscovering)
                {
                    localBluetoothAdapter.CancelDiscovery();
                }
                new BluetoothDiscoverer(paramContext.ApplicationContext, paramDiscoveryHandler, paramDeviceFilter).DoBluetoothDiscovery();
            }
        }


        //public static void findPrinters(Context paramContext, DiscoveryHandler paramDiscoveryHandler)
        //{
        //    BluetoothDiscoverer local1 = new BluetoothDiscoverer();
        //    FindDevices(paramContext, paramDiscoveryHandler, local1);
        //}

        public static void FindServices(Context paramContext, String paramString, IServiceDiscoveryHandler paramServiceDiscoveryHandler)
        {
            BtServiceDiscoverer localBtServiceDiscoverer = new BtServiceDiscoverer(BluetoothHelper.FormatMacAddress(paramString), paramServiceDiscoveryHandler);
            localBtServiceDiscoverer.DoDiscovery(paramContext);
        }

        private void UnregisterTopLevelReceivers(Context paramContext)
        {
            if (this.btReceiver != null)
            {
                paramContext.UnregisterReceiver(this.btReceiver);
            }
            if (this.btMonitor != null)
            {
                paramContext.UnregisterReceiver(this.btMonitor);
            }
        }

        private void DoBluetoothDiscovery()
        {
            this.btReceiver = new BroadcastReceiverClass((Activity)mContext);
            this.btMonitor = new BroadcastReceiverClass((Activity)mContext);
            IntentFilter localIntentFilter1 = new IntentFilter("android.bluetooth.device.action.FOUND");
            IntentFilter localIntentFilter2 = new IntentFilter("android.bluetooth.adapter.action.DISCOVERY_FINISHED");
            IntentFilter localIntentFilter3 = new IntentFilter("android.bluetooth.adapter.action.STATE_CHANGED");
            this.mContext.RegisterReceiver(this.btReceiver, localIntentFilter1);
            this.mContext.RegisterReceiver(this.btReceiver, localIntentFilter2);
            this.mContext.RegisterReceiver(this.btMonitor, localIntentFilter3);
            BluetoothAdapter.DefaultAdapter.StartDiscovery();
        }
    }
}