using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public class BluetoothFragment : VisibleFragment
    {
        OnButtonClicked mButtonClicked;
        public interface OnButtonClicked
        {
            void BluetoothFragmentOnButtonClicked(int requestCode, int result, BluetoothDevice bluetoothDevice);
        }

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

        BluetoothReceiver mBluetoothReceiver;
        ArrayAdapter<String> mBluetoothAdapter;
        Button mButtonScan;
        bool mScanStarted;
        bool mSuccess;
        BluetoothAction mBluetoothAction;
        ICollection<BluetoothDevice> mBluetoothDevices;
        ListView mDeviceFoundList;
        BluetoothDevice mBluetoothDevice;
        Bluetooth mBluetooth;
        int mRequestCode;

        public static BluetoothFragment NewInstance(int requestCode)
        {

            Bundle args = new Bundle();
            args.PutInt(ARG_REQUEST_CODE, requestCode);
            BluetoothFragment fragment = new BluetoothFragment();
            fragment.Arguments = args;
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            mBluetoothAction = (BluetoothAction)Arguments.GetInt(ARG_REQUEST_BLUETOOTH_ACTION);
            mBluetoothDevice = (BluetoothDevice)Arguments.GetParcelable(ARG_REQUEST_BLUETOOTH_DEVICE);
            mRequestCode = Arguments.GetInt(ARG_REQUEST_CODE);

            if (mBluetooth == null)
            {
                mBluetooth = new Bluetooth();
            }

            mScanStarted = false;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.bluetooth_utils_fragment, container, false);
            mButtonScan = (Button)v.FindViewById(Resource.Id.search_button);
            mButtonScan.Text = "Search";
            mButtonScan.Click += delegate
            {
                // reset so spinner selectionused
                mBluetoothAction = BluetoothAction.GetListOfAvailableDevices;
                mScanStarted = true;

                // cancel any requests which may be in progress
                mBluetooth.Adapter.CancelDiscovery();

                mBluetooth.Adapter.StartDiscovery();

            };
            mBluetoothAdapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleListItem1);
            mBluetoothDevices = new List<BluetoothDevice>();

            mBluetoothDevices.Clear();

            mDeviceFoundList = (ListView)v.FindViewById(Resource.Id.device_list);
            mDeviceFoundList.Adapter = mBluetoothAdapter;
            mDeviceFoundList.ItemClick += delegate (object sender, AdapterView.ItemClickEventArgs e)
            {
                int selected = e.Position;
                if (selected != -1)
                {
                    mBluetoothDevice = mBluetoothDevices.ElementAt(selected);
                    mBluetoothAction = BluetoothAction.PairDevice;

                    if (mBluetooth.IsDevicePaired())
                    {
                        Toast.MakeText(Context, mBluetoothDevice.Name + " " + Resource.String.MainActivity_AlreadyPaired, ToastLength.Long).Show();
                    }
                    else
                    {
                        mBluetooth.Device = mBluetoothDevice;
                        mBluetooth.Adapter.CancelDiscovery();
                        mBluetooth.Device.CreateBond();
                    }
                }
            };


            mSuccess = false;

            switch (mBluetoothAction)
            {
                case BluetoothAction.UnpairDevice:
                    mBluetooth.Device = mBluetoothDevice;
                    mBluetooth.UnpairDevice();
                    break;
                case BluetoothAction.GetListOfAvailableDevices:
                    mScanStarted = true;
                    mBluetooth.Adapter.CancelDiscovery();
                    mBluetooth.Adapter.StartDiscovery();
                    break;
                case BluetoothAction.PairDevice:
                    mBluetooth.Device = mBluetoothDevice;
                    if (mBluetooth.IsDevicePaired())
                    {
                        break;
                    }

                    mBluetoothDevices.Clear();
                    mBluetoothDevices.Add(mBluetoothDevice);
                    mBluetoothAdapter.Add(String.Format("{0} # {1}", mBluetoothDevice.Name, mBluetoothDevice.Address));
                    mBluetoothAdapter.NotifyDataSetChanged();
                    mBluetooth.Adapter.CancelDiscovery();
                    mBluetooth.Device.CreateBond();

                    break;
            }
            return v;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);



        }

        public override void OnResume()
        {
            base.OnResume();
        }
        public override void OnStart()
        {
            base.OnStart();
            mBluetoothReceiver = new BluetoothReceiver(this);

            IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            filter.AddAction(BluetoothDevice.ActionBondStateChanged);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryStarted);
            Activity.RegisterReceiver(mBluetoothReceiver, filter);
        }

        public override void OnStop()
        {
            base.OnStop();
            Activity.UnregisterReceiver(mBluetoothReceiver);

        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);

            try
            {
                mButtonClicked = (OnButtonClicked)context;
            }
            catch (NotImplementedException e)
            {
                throw new Exception(e.Message + " - Must implement OnRecordSelected");
            }

        }

        public override void OnDetach()
        {
            base.OnDetach();

            mButtonClicked = null;
        }
        private void SendResult(int requestCode, int result, BluetoothDevice bluetoothDevice)
        {
            // try and work out if fragment called from an activity or another fragment
            if (TargetFragment == null)
            {
                mButtonClicked.BluetoothFragmentOnButtonClicked(requestCode, result, bluetoothDevice);
                return;
            }

            TargetFragment.OnActivityResult(TargetRequestCode, result, null);


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
            
            Activity.RunOnUiThread(() =>
            {
                ProgressBar pbSearching = Activity.FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Visible;
            });
        }
        public void OnScanComplete()
        {
            mScanStarted = false;

            Activity.RunOnUiThread(() =>
            {
                ProgressBar pbSearching = Activity.FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Invisible;
            });
        }

        public void OnPaired(bool paired, Bond state, bool success)
        {
            if (!success)
            {
                mSuccess = false;
                if (mBluetoothAction == BluetoothAction.PairDevice)
                {
                    Toast.MakeText(Context, Resource.String.MainActivity_PairFailed + " " + mBluetoothDevice.Name, ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(Context, Resource.String.MainActivity_PairFailed + " " + mBluetoothDevice.Name, ToastLength.Long).Show();
                }

                return;
            }

            mSuccess = true;

            if (paired)
            {
                Toast.MakeText(Context, Resource.String.MainActivity_PairSuccessful + " " + mBluetoothDevice.Name, ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(Context, Resource.String.MainActivity_PairFailed + " " + mBluetoothDevice.Name, ToastLength.Long).Show();
            }
        }
        #endregion
    }
}