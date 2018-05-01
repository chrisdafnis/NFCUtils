using System;
using System.Collections.ObjectModel;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

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
        public static readonly string ARG_REQUEST_BLUETOOTH_ACTION = "bluetooth_action";
        public static readonly string ARG_REQUEST_CODE = "request_code";
        public static readonly string ARG_REQUEST_BLUETOOTH_ADDRESS = "bluetooth_address";
        public static readonly string ARG_REQUEST_BLUETOOTH_DEVICE = "bluetooth_device";

        IOnButtonClicked mButtonClicked;
        public interface IOnButtonClicked
        {
            void BluetoothFragmentOnOKButtonClicked(int requestCode, int result, BluetoothDevice bluetoothDevice);
        }
        private View _parentView;

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

        ObservableCollection<BluetoothDevice> _deviceList = new ObservableCollection<BluetoothDevice>();

        Button _buttonScan;
        bool _scanStarted = false;
        bool _success = false;
        BluetoothAction _bluetoothAction;
        ListView _deviceFoundList;
        BluetoothDevice _bluetoothDevice;
        int _requestCode;

        OnButtonClicked _buttonOkClicked;
        private static MainActivity _activity;

        public interface OnButtonClicked
        {
            void BluetoothFragmentOnOKButtonClicked(Intent result, int requestCode);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static BluetoothFragment NewInstance(Bundle args)
        {
            BluetoothFragment fragment = new BluetoothFragment
            {
                Arguments = args
            };
            return fragment;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedInstanceState"></param>
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _bluetoothAction = (BluetoothAction)Arguments.GetInt(ARG_REQUEST_BLUETOOTH_ACTION);
            _bluetoothDevice = (BluetoothDevice)Arguments.GetParcelable(ARG_REQUEST_BLUETOOTH_DEVICE);
            _requestCode = Arguments.GetInt(ARG_REQUEST_CODE);

            if (_activity.Bluetooth == null)
            {
                _activity.Bluetooth = new Bluetooth();
            }

            _scanStarted = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inflater"></param>
        /// <param name="container"></param>
        /// <param name="savedInstanceState"></param>
        /// <returns></returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _parentView = inflater.Inflate(Resource.Layout.bluetooth_utils_fragment, container, false);
            _buttonScan = _parentView.FindViewById<Button>(Resource.Id.search_button);
            _buttonScan.Text = "Search";
            _buttonScan.Click += delegate
            {
                // reset so spinner selectionused
                _bluetoothAction = BluetoothAction.GetListOfAvailableDevices;
                _scanStarted = true;

                // cancel any requests which may be in progress
                _activity.Bluetooth.Adapter.CancelDiscovery();
                _activity.Bluetooth.Adapter.StartDiscovery();
            };

            _deviceFoundList = _parentView.FindViewById<ListView>(Resource.Id.device_list);
            _deviceFoundList.Adapter = new BluetoothDeviceArrayAdapter(_parentView.Context, Resource.Layout.row_layout, _deviceList);
            _deviceFoundList.ItemClick += delegate (object sender, AdapterView.ItemClickEventArgs e)
            {
                _bluetoothDevice = _deviceList[e.Position];
                _deviceFoundList.SetSelection(e.Position);
                (_deviceFoundList.Adapter as BluetoothDeviceArrayAdapter).SetSelectedIndex(e.Position);
                Toast.MakeText(_parentView.Context, "Copied " + _bluetoothDevice.Name + " to clipboard.", ToastLength.Long).Show();
            };

            Button buttonOK = _parentView.FindViewById<Button>(Resource.Id.buttonOK);
            buttonOK.Click += delegate
            {
                var returnIntent = new Intent();
                if (_bluetoothDevice != null)
                {
                    returnIntent.PutExtra("BluetoothDeviceName", _bluetoothDevice.Name);
                    returnIntent.PutExtra("BluetoothDeviceAddress", _bluetoothDevice.Address);
                }

                _buttonOkClicked.BluetoothFragmentOnOKButtonClicked(returnIntent, (int)ActivityCode.Bluetooth);
                return;
            };
            _success = false;

            switch (_bluetoothAction)
            {
                case BluetoothAction.UnpairDevice:
                    {
                        _activity.Bluetooth.Device = _bluetoothDevice;
                        _activity.Bluetooth.UnpairDevice();
                    }
                    break;
                case BluetoothAction.GetListOfAvailableDevices:
                    {
                        _scanStarted = true;
                        _activity.Bluetooth.Adapter.CancelDiscovery();
                        _activity.Bluetooth.Adapter.StartDiscovery();
                    }
                    break;
                case BluetoothAction.PairDevice:
                    {
                        _activity.Bluetooth.Device = _bluetoothDevice;
                        if (_activity.Bluetooth.IsDevicePaired())
                        {
                            break;
                        }

                        _deviceList.Clear();
                        _deviceList.Add(_bluetoothDevice);
                        _activity.Bluetooth.Adapter.CancelDiscovery();
                        _activity.Bluetooth.Device.CreateBond();
                    }
                    break;
            }
            return _parentView;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            _activity = context as MainActivity;
            try
            {
                _buttonOkClicked = (OnButtonClicked)context;
            }
            catch (NotImplementedException e)
            {
                throw new Exception(e.Message + " - Must implement OnButtonClicked");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bluetoothDevice"></param>
        /// <param name="bluetoothClass"></param>
        #region Called from broadcastreceiver
        public new void OnDeviceFound(object bluetoothDevice, object bluetoothClass)
        { 
            if (bluetoothDevice == null)
                return;

            bool found = false;
            // in case of rescan check the one found isn't already in our list
            foreach (BluetoothDevice device in _deviceList)
            {
                if (device.Address == ((BluetoothDevice)bluetoothDevice).Address)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                _deviceList.Add((BluetoothDevice)bluetoothDevice);
                RefreshList();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public new void OnScanStarted()
        {
            _scanStarted = true;
            
            Activity.RunOnUiThread(() =>
            {
                ProgressBar pbSearching = Activity.FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Visible;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        public new void OnScanComplete()
        {
            _scanStarted = false;

            Activity.RunOnUiThread(() =>
            {
                ProgressBar pbSearching = Activity.FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Invisible;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="paired"></param>
        /// <param name="state"></param>
        /// <param name="success"></param>
        public void OnPaired(bool paired, object state, bool success)
        {
            Activity.RunOnUiThread(() =>
            {
                if (!success)
                {
                    _success = false;
                    if (_bluetoothAction == BluetoothAction.PairDevice)
                    {
                        Toast.MakeText(_parentView.Context, GetString(Resource.String.MainActivity_PairFailed) + " " + _bluetoothDevice.Name, ToastLength.Long).Show();
                    }
                    else
                    {
                        Toast.MakeText(_parentView.Context, GetString(Resource.String.MainActivity_PairFailed) + " " + _bluetoothDevice.Name, ToastLength.Long).Show();
                    }

                    return;
                }

                _success = true;

                if (paired)
                {
                    Toast.MakeText(_parentView.Context, GetString(Resource.String.MainActivity_PairSuccessful) + " " + _bluetoothDevice.Name, ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(_parentView.Context, GetString(Resource.String.MainActivity_PairFailed) + " " + _bluetoothDevice.Name, ToastLength.Long).Show();
                }
            });
        }
        #endregion
        /// <summary>
		/// Refreshed the list of paired bluetooth devices
		/// </summary>
		public void RefreshList()
        {
            Activity.RunOnUiThread(() =>
            {
                _deviceFoundList.Adapter = new BluetoothDeviceArrayAdapter(_parentView.Context, Resource.Layout.row_layout, _deviceList);
            });
        }
    }
}