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
using static Android.Widget.AdapterView;

namespace com.touchstar.chrisd.nfcutils
{
    [Register("com.touchstar.chrisd.nfcutils.BluetoothFragment")]
    public class BluetoothFragment : VisibleFragment
    {
        public enum ActivityCode { NFCPair = 0, NFCPairMenu, NFCUtils, Bluetooth };
        public static readonly string FRAGMENT_TAG_MAIN_MENU = "MainMenuFragment";
        public static readonly string FRAGMENT_TAG_NFC_PAIR = "TapAndPairFragment";
        public static readonly string FRAGMENT_TAG_NFC_UTILS = "NfcUtilsFragment";
        public static readonly string FRAGMENT_TAG_BLUETOOTH = "BluetoothFragment";

        IOnButtonClicked mButtonClicked;
        public interface IOnButtonClicked
        {
            void BluetoothFragmentOnOKButtonClicked(int requestCode, int result, BluetoothDevice bluetoothDevice);
        }
        private View parentView;

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
        ArrayAdapter<String> mArrayAdapter;
        Button mButtonScan;
        bool mScanStarted = false;
        bool mSuccess = false;
        BluetoothAction mBluetoothAction;
        ICollection<BluetoothDevice> mBluetoothDevices;
        ListView mDeviceFoundList;
        BluetoothDevice mBluetoothDevice;
        Bluetooth mBluetooth;
        int mRequestCode;
        //BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;


        OnButtonClicked mButtonOkClicked;
        private static Activity mActivity;

        public interface OnButtonClicked
        {
            void BluetoothFragmentOnOKButtonClicked(Intent result, int requestCode);
        }

        public ICollection<BluetoothDevice> BluetoothDevices
        {
            get { return mBluetoothDevices; }
            set { mBluetoothDevices = value; }
        }

        public static BluetoothFragment NewInstance(Bundle args)
        {
            BluetoothFragment fragment = new BluetoothFragment
            {
                Arguments = args
            };
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
            parentView = inflater.Inflate(Resource.Layout.bluetooth_utils_fragment, container, false);
            mButtonScan = parentView.FindViewById<Button>(Resource.Id.search_button);
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
            mArrayAdapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleListItem1);
            mBluetoothDevices = new List<BluetoothDevice>();
            mBluetooth.Adapter = BluetoothAdapter.DefaultAdapter;
            mBluetoothDevices.Clear();

            mDeviceFoundList = parentView.FindViewById<ListView>(Resource.Id.device_list);
            mDeviceFoundList.Adapter = mArrayAdapter;
            mDeviceFoundList.ItemClick += delegate (object sender, AdapterView.ItemClickEventArgs e)
            {
                int selected = e.Position;
                if (selected != -1)
                {
                    mBluetoothDevice = mBluetoothDevices.ElementAt(selected);
                    //mBluetoothAction = BluetoothAction.PairDevice;

                    //if (mBluetooth.IsDevicePaired())
                    //{
                    //    Toast.MakeText(parentView.Context, mBluetoothDevice.Name + " " + GetString(Resource.String.MainActivity_AlreadyPaired), ToastLength.Long).Show();
                    //}
                    //else
                    //{
                    //    mBluetooth.Device = mBluetoothDevice;
                    //    mBluetooth.Adapter.CancelDiscovery();
                    //    mBluetooth.Device.CreateBond();
                    //}
                }
            };

            Button buttonOK = parentView.FindViewById<Button>(Resource.Id.buttonOK);
            buttonOK.Click += delegate
            {
                var returnIntent = new Intent();
                returnIntent.PutExtra("BluetoothDeviceName", mBluetoothDevice.Name);
                returnIntent.PutExtra("BluetoothDeviceAddress", mBluetoothDevice.Address);

                mButtonOkClicked.BluetoothFragmentOnOKButtonClicked(returnIntent, (int)ActivityCode.Bluetooth);
                return;
            };
            mSuccess = false;

            switch (mBluetoothAction)
            {
                case BluetoothAction.UnpairDevice:
                    {
                        mBluetooth.Device = mBluetoothDevice;
                        mBluetooth.UnpairDevice();
                    }
                    break;
                case BluetoothAction.GetListOfAvailableDevices:
                    {
                        mScanStarted = true;
                        mBluetooth.Adapter.CancelDiscovery();
                        mBluetooth.Adapter.StartDiscovery();
                    }
                    break;
                case BluetoothAction.PairDevice:
                    {
                        mBluetooth.Device = mBluetoothDevice;
                        if (mBluetooth.IsDevicePaired())
                        {
                            break;
                        }

                        mBluetoothDevices.Clear();
                        mBluetoothDevices.Add(mBluetoothDevice);
                        mArrayAdapter.Add(String.Format("{0} # {1}", mBluetoothDevice.Name, mBluetoothDevice.Address));
                        mArrayAdapter.NotifyDataSetChanged();
                        mBluetooth.Adapter.CancelDiscovery();
                        mBluetooth.Device.CreateBond();
                    }
                    break;
            }
            return parentView;
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
            mBluetoothReceiver = new BluetoothReceiver(this/*.Activity as MainActivity*/);

            IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            filter.AddAction(BluetoothDevice.ActionPairingRequest);
            filter.AddAction(BluetoothDevice.ActionBondStateChanged);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryStarted);
            mActivity.RegisterReceiver(mBluetoothReceiver, filter);
        }

        public override void OnStop()
        {
            base.OnStop();
            mActivity.UnregisterReceiver(mBluetoothReceiver);

        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            mActivity = context as Activity;
            try
            {
                mButtonOkClicked = (OnButtonClicked)context;
            }
            catch (NotImplementedException e)
            {
                throw new Exception(e.Message + " - Must implement OnButtonClicked");
            }
        }

        public override void OnDetach()
        {
            base.OnDetach();

            //mButtonClicked = null;
        }

        /// <summary>
        /// This method is called when an NFC tag is discovered by the application.
        /// </summary>
        /// <param name="intent"></param>
        //public void OnNewIntent(Intent intent)
        //{
        //}

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
                mArrayAdapter.Add(String.Format("{0} # {1}", bluetoothDevice.Name, bluetoothDevice.Address));
                mArrayAdapter.NotifyDataSetChanged();
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
            Activity.RunOnUiThread(() =>
            {
                if (!success)
                {
                    mSuccess = false;
                    if (mBluetoothAction == BluetoothAction.PairDevice)
                    {
                        Toast.MakeText(parentView.Context, GetString(Resource.String.MainActivity_PairFailed) + " " + mBluetoothDevice.Name, ToastLength.Long).Show();
                    }
                    else
                    {
                        Toast.MakeText(parentView.Context, GetString(Resource.String.MainActivity_PairFailed) + " " + mBluetoothDevice.Name, ToastLength.Long).Show();
                    }

                    return;
                }

                mSuccess = true;

                if (paired)
                {
                    Toast.MakeText(parentView.Context, GetString(Resource.String.MainActivity_PairSuccessful) + " " + mBluetoothDevice.Name, ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(parentView.Context, GetString(Resource.String.MainActivity_PairFailed) + " " + mBluetoothDevice.Name, ToastLength.Long).Show();
                }
            });
        }
        #endregion

        //public void BluetoothDiscoveryFinished()
        //{
        //    Activity.RunOnUiThread(() =>
        //    {
        //        ProgressBar pbSearching = parentView.FindViewById<ProgressBar>(Resource.Id.pbSearching);
        //        pbSearching.Visibility = ViewStates.Gone;
        //    });
        //}

        //public void BluetoothDiscoveryStarted()
        //{
        //    mBluetoothDevices.Clear();

        //    Activity.RunOnUiThread(() =>
        //    {
        //        ProgressBar pbSearching = parentView.FindViewById<ProgressBar>(Resource.Id.pbSearching);
        //        pbSearching.Visibility = ViewStates.Visible;
        //    });
        //}

        private void BluetoothDeviceFound(BluetoothDevice device)
        {
            if (device != null)
            {
                if (!mBluetoothDevices.Contains<BluetoothDevice>(device))
                    mBluetoothDevices.Add(device);
            }
        }

        public override void OnPause()
        {
            base.OnPause();
        }

        //private void SearchButton_OnClick(object sender, EventArgs e)
        //{
        //    mBluetoothAction = BluetoothAction.GetListOfAvailableDevices;
        //    mScanStarted = true;

        //    // cancel any requests which may be in progress
        //    mBluetooth.Adapter.CancelDiscovery();
        //    mBluetooth.Adapter.StartDiscovery();
        //}

        void DeviceList_ItemClick(object sender, EventArgs e)
        {
            ItemClickEventArgs args = e as ItemClickEventArgs;
            BluetoothDevice device = mBluetoothDevices.ElementAt<BluetoothDevice>(args.Position);
            if (device != null)
            {
                try
                {
                    var retVal = device.CreateBond();
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
            Activity.RunOnUiThread(() =>
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

        //public Bundle Arguments { get; private set; }

        public void FoundDevice(object device)
        {
            if (device is BluetoothDevice bluetoothDevice)
            {
                BluetoothDeviceFound(bluetoothDevice);
            }
        }
    }
}