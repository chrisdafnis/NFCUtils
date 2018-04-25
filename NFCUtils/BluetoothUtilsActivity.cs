﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang.Reflect;
using static Android.Widget.AdapterView;

namespace com.touchstar.chrisd.nfcutils
{
    [Activity(Label = "Bluetooth Utilities")]
    public class BluetoothUtilsActivity : SingleFragmentActivity, BluetoothFragment.IOnButtonClicked
    {
        internal Context context;
        ArrayAdapter<String> mBluetoothAdapter;
        ICollection<BluetoothDevice> mBluetoothDevices;
        BluetoothDevice mBluetoothDevice;
        Bluetooth mBluetooth;
        Button mButtonOk;
        BluetoothAction mBluetoothAction;
        bool mSuccess;
        bool mScanStarted;
        private bool InstanceFieldsInitialized = false;
        int mRequestCode;

        public static readonly string ARG_REQUEST_BLUETOOTH_ACTION = "bluetooth_action";
        public static readonly string ARG_REQUEST_CODE = "request_code";
        public static readonly string ARG_REQUEST_BLUETOOTH_ADDRESS = "bluetooth_address";
        public static readonly string ARG_REQUEST_BLUETOOTH_DEVICE = "bluetooth_device";

        public enum BluetoothAction
        {
            GetDeafaultAdapter,
            CheckIfBluetoothIsEnabled,
            IsDevicePaired,
            GetListOfPairedDevices,
            GetListOfAvailableDevices,
            PairDevice,
            UnpairDevice,
            End
        }

        public enum BluetoothError
        {
            UnknownDevice,
            InvalidBluetoothAddress
        }


        public BluetoothUtilsActivity()
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }

        }

        private void InitializeInstanceFields()
        {
            context = this;
        }

        protected override Fragment CreateFragment()
        {
            return BluetoothFragment.NewInstance(0);

        }
        
        #region Called from broadcastreceiver
        public void OnDeviceFound(BluetoothDevice bluetoothDevice, BluetoothClass bluetoothClass)
        {
            if (bluetoothDevice == null)
                return;

            bool found = false;
            // in case of rescan check the one found isn't already in our list
            foreach (BluetoothDevice device in mBluetoothDevices)
            {
                if (device.Address == bluetoothDevice.Address)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                mBluetoothDevices.Add(bluetoothDevice);
                mBluetoothAdapter.Add(String.Format("{0} # {1}", bluetoothDevice.Name, bluetoothDevice.Address));
                mBluetoothAdapter.NotifyDataSetChanged();
            }
        }
        public void OnScanStarted()
        {
            mScanStarted = true;
            RunOnUiThread(() =>
            {
                ProgressBar pbSearching = FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Visible;
            });
        }
        public void OnScanComplete()
        {
            mScanStarted = false;
            RunOnUiThread(() =>
            {
                ProgressBar pbSearching = FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Gone;
            });
        }

        public void OnPaired(bool paired, Bond state, bool success)
        {
            if (!success)
            {
                mSuccess = false;
                if (mBluetoothAction == BluetoothAction.PairDevice)
                {
                }
                else
                {
                }

                return;
            }

            mSuccess = true;

            if (paired)
            {
            }
            else
            {
            }
        }
        #endregion

        public void BluetoothDiscoveryFinished()
        {
            RunOnUiThread(() =>
            {
                ProgressBar pbSearching = FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Gone;
            });
        }

        public void BluetoothDiscoveryStarted()
        {
            mBluetoothDevices.Clear();

            RunOnUiThread(() =>
            {
                ProgressBar pbSearching = FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Visible;
            });
        }

        private void BluetoothDeviceFound(BluetoothDevice device)
        {
            if (device != null)
            {
                if (!mBluetoothDevices.Contains<BluetoothDevice>(device))
                    mBluetoothDevices.Add(device);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        private void SearchButton_OnClick(object sender, EventArgs e)
        {
            mBluetoothAction = BluetoothAction.GetListOfAvailableDevices;
            mScanStarted = true;

            // cancel any requests which may be in progress
            mBluetooth.Adapter.CancelDiscovery();
            mBluetooth.Adapter.StartDiscovery();
        }

        void DeviceList_ItemClick(object sender, EventArgs e)
        {
            ItemClickEventArgs args = e as ItemClickEventArgs;
            BluetoothDevice device = mBluetoothDevices.ElementAt<BluetoothDevice>(args.Position);
            if (device != null)
            {
                try
                {
                    Method m = device.Class.GetMethod("createBond", null);
                    var retVal = m.Invoke(device, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.Write(ex.StackTrace);
                }

            }
        }

        /// <summary>
		/// Refreshed the list of paired bluetooth devices
		/// </summary>
		public void RefreshList()
        {
            RunOnUiThread(() =>
            {
                //_arrayAdapter.Clear();
                //_arrayAdapter.AddAll(PairedDevices);
                //_arrayAdapter.NotifyDataSetChanged();
            });
        }

        /// <summary>
        /// Returns a list of all the printers currently paired to the Android device via bluetooth.
        /// </summary>
        /// <returns> a list of all the printers currently paired to the Android device via bluetooth. </returns>
        private List<BluetoothDevice> PairedDevices
        {
            get
            {
                BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                ICollection<BluetoothDevice> pairedDevices = mBluetoothAdapter.BondedDevices;
                List<BluetoothDevice> pairedDevicesList = new List<BluetoothDevice>();
                foreach (BluetoothDevice device in pairedDevices)
                {
                    pairedDevicesList.Add(device);
                }
                return pairedDevicesList;
            }
        }

        public Bundle Arguments { get; private set; }

        public void FoundDevice(object device)
        {
            if (device is BluetoothDevice bluetoothDevice)
            {
                BluetoothDeviceFound(bluetoothDevice);
            }
        }

        public void BluetoothFragmentOnOKButtonClicked(int requestCode, int result, BluetoothDevice device)
        {
            if (result == (int)Result.Ok && device != null)
            {

            }
        }
    }
}