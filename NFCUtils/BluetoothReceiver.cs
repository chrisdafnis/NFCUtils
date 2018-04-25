using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace com.touchstar.chrisd.nfcutils
{
    public class BluetoothReceiver : BroadcastReceiver
    {
        public static readonly string TAG = "BroadcastReceiverBluetooth";

        private MainActivity mActivity;
        //private TapAndPairActivity mActivity;

        //public BluetoothReceiver(BluetoothFragment fragment)
        //{
        //    mFragment = fragment;
        //}

        public BluetoothReceiver(MainActivity activity)
        {
            mActivity = activity;
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
                    mActivity.OnDeviceFound(device, deviceClass);
                }
            }

            if (action == BluetoothAdapter.ActionDiscoveryStarted)
            {
                mActivity.OnScanStarted();
            }

            if (action == BluetoothAdapter.ActionDiscoveryFinished)
            {
                mActivity.OnScanComplete();
            }

            if (action == BluetoothDevice.ActionPairingRequest)
            {
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                BluetoothClass deviceClass = (BluetoothClass)intent.GetParcelableExtra(BluetoothDevice.ExtraClass);

                mActivity.OnPairDevice(device, (int)Bond.None);
            }

            if (action == BluetoothDevice.ActionBondStateChanged)
            {
                int state = intent.GetIntExtra(BluetoothDevice.ExtraBondState, BluetoothDevice.Error);
                int prevState = intent.GetIntExtra(BluetoothDevice.ExtraPreviousBondState, BluetoothDevice.Error);

                if (state == (int)Bond.Bonded && prevState == (int)Bond.Bonding)
                {
                    //if (mFragment != null)
                    //{
                    //    mFragment.OnPaired(true, (Bond)state, true);
                    //}
                    //else if (mActivity != null)
                    //{
                        BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        BluetoothClass deviceClass = (BluetoothClass)intent.GetParcelableExtra(BluetoothDevice.ExtraClass);

                        mActivity.OnPairDevice(device, state);
                    //}
                    return;
                }
                if (state == (int)Bond.None && prevState == (int)Bond.Bonded)
                {
                    //if (mFragment != null)
                    //{
                    //    mFragment.OnPaired(false, (Bond)state, true);
                    //}
                    //else if (mActivity != null)
                    //{
                        BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        BluetoothClass deviceClass = (BluetoothClass)intent.GetParcelableExtra(BluetoothDevice.ExtraClass);

                        mActivity.OnPairDevice(device, state);
                    //}
                    return;
                }

                if (state == (int)Bond.None && (prevState == (int)Bond.None || prevState == (int)Bond.Bonding))
                {
                    //if (mFragment != null)
                    //{
                    //    mFragment.OnPaired(false, (Bond)state, false);
                    //}
                    //else if (mActivity != null)
                    //{
                        BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        BluetoothClass deviceClass = (BluetoothClass)intent.GetParcelableExtra(BluetoothDevice.ExtraClass);

                        mActivity.OnPairDevice(device, state);
                    //}
                    return;
                }
            }
        }
    }
}