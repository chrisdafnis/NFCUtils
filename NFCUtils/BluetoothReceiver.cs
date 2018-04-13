using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace com.touchstar.chrisd.nfcutils
{
    public class BluetoothReceiver : BroadcastReceiver
    {
        public static readonly string TAG = "BroadcastReceiverBluetooth";

        private BluetoothFragment mFragment;

        public BluetoothReceiver(BluetoothFragment fragment)
        {
            mFragment = fragment;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;

            if (action == BluetoothDevice.ActionFound)
            {
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                BluetoothClass deviceClass = (BluetoothClass)intent.GetParcelableExtra(BluetoothDevice.ExtraClass);

                // only interested in devices not already paired
                if (device.BondState == Bond.None)
                {
                    mFragment.OnDeviceFound(device, deviceClass);
                }

            }

            if (action == BluetoothAdapter.ActionDiscoveryStarted)
            {
                mFragment.OnScanStarted();

            }


            if (action == BluetoothAdapter.ActionDiscoveryFinished)
            {
                mFragment.OnScanComplete();


            }

            if (action == BluetoothDevice.ActionBondStateChanged)
            {
                int state = intent.GetIntExtra(BluetoothDevice.ExtraBondState, BluetoothDevice.Error);
                int prevState = intent.GetIntExtra(BluetoothDevice.ExtraPreviousBondState, BluetoothDevice.Error);

                if (state == (int)Bond.Bonded && prevState == (int)Bond.Bonding)
                {
                    mFragment.OnPaired(true, (Bond)state, true);
                    return;
                }
                if (state == (int)Bond.None && prevState == (int)Bond.Bonded)
                {
                    mFragment.OnPaired(false, (Bond)state, true);
                    return;
                }



                if (state == (int)Bond.None && (prevState == (int)Bond.None || prevState == (int)Bond.Bonding))
                {
                    mFragment.OnPaired(false, (Bond)state, false);
                    return;
                }

            }


        }
    }
}