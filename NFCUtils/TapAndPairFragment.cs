using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Zebra.Android.Discovery;
using static Android.Widget.AdapterView;

namespace com.touchstar.chrisd.nfcutils
{
    [Register("com.touchstar.chrisd.nfcutils.TapAndPairFragment")]
    public class TapAndPairFragment : VisibleFragment
    {
        public enum ActivityCode { NFCPair = 0, NFCPairMenu };
        public enum Bonding { None = 10, Bonding, Bonded, Pairing, Cancelled };
        public static readonly string FRAGMENT_TAG_MAIN_MENU = "MainMenuFragment";
        public static readonly string FRAGMENT_TAG_NFC_PAIR = "TapAndPairFragment";
        public static readonly string FRAGMENT_TAG_NFC_UTILS = "NfcUtilsFragment";
        public static readonly string FRAGMENT_TAG_BLUETOOTH = "BluetoothFragment";
        public static readonly string ARG_REQUEST_CODE = "request_code";

        private ListView _listview;
        private TextView _tvInfo;
        private Button _buttonOk;
        ObservableCollection<BluetoothDevice> _deviceList = new ObservableCollection<BluetoothDevice>();
        BluetoothDevice _selectedDevice;
        public event EventHandler OnDevicePaired;

        private int _requestCode;
        private MainActivity _activity;

        OnButtonClicked _buttonOkClicked;
        public interface OnButtonClicked
        {
            void TapAndPairFragmentOnOKButtonClicked(Intent result, int requestCode);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestCode"></param>
        /// <returns></returns>
        public static TapAndPairFragment NewInstance(int requestCode)
        {
            Bundle args = new Bundle();
            args.PutInt(ARG_REQUEST_CODE, requestCode);
            TapAndPairFragment fragment = new TapAndPairFragment
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

            _requestCode = Arguments.GetInt(ARG_REQUEST_CODE);
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
            View parentView = inflater.Inflate(Resource.Layout.tap_and_pair_fragment, container, false);
            _buttonOk = parentView.FindViewById<Button>(Resource.Id.buttonOK);
            _buttonOk.Click += delegate
            {
                var returnIntent = new Intent();
                returnIntent.PutExtra("NFCDeviceName", _selectedDevice.Name);
                returnIntent.PutExtra("NFCDeviceAddress", _selectedDevice.Address);

                _buttonOkClicked.TapAndPairFragmentOnOKButtonClicked(returnIntent, _requestCode);
                return;
            };
            _buttonOk.Enabled = false;

            _listview = parentView.FindViewById<ListView>(Resource.Id.lvPairedDevices);
            _tvInfo = parentView.FindViewById<TextView>(Resource.Id.tvInfo);
            _deviceList = GetPairedDeviceCollection();
            _listview.Adapter = new BluetoothDeviceArrayAdapter(Activity, Resource.Layout.row_layout, _deviceList);
            
            OnDevicePaired += TapAndPair_OnDevicePaired;
            // Set up EventHandlers
            _listview.ItemClick += delegate (object sender, ItemClickEventArgs e)
            {
                _deviceList = GetPairedDeviceCollection();
                _selectedDevice = _deviceList[e.Position];
                _listview.SetSelection(e.Position);
                (_listview.Adapter as BluetoothDeviceArrayAdapter).SetSelectedIndex(e.Position);

                if (_selectedDevice != null)
                    _buttonOk.Enabled = true;
                else
                    _buttonOk.Enabled = false;
            };
            return parentView;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            _activity = context as MainActivity;
            _activity.EnableReadMode();
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
        public override void OnResume()
        {
            base.OnResume();
            _activity.EnableReadMode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TapAndPair_OnDevicePaired(object sender, EventArgs e)
        {
            _selectedDevice = sender as BluetoothDevice;
            var returnIntent = new Intent();
            if (_selectedDevice != null)
            {
                returnIntent.PutExtra("NFCDeviceName", _selectedDevice.Name);
                returnIntent.PutExtra("NFCDeviceAddress", _selectedDevice.Address);
                returnIntent.PutExtra("NFCDevice", _selectedDevice);
            }
            else
            {
                returnIntent.PutExtra("NFCDevice", "No selected device");
                returnIntent.PutExtra("NFCDevice", _selectedDevice);
            }
            OnPairDevice(_selectedDevice, (int)Bonding.Pairing);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="intent"></param>
        public void OnNewIntent(Intent intent)
        {
            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
            var rawMsgs = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);

            if (tag == null)
            {
                return;
            }

            if (NfcAdapter.ExtraTag.Contains("nfc"))
            {
                ProcessNfcScan(intent);
            }
        }
        /// <summary>
        /// Accepts data from an NFC touch and performs a search for any printers with matching information.
        /// </summary>
        /// <param name="intent"> intent that contains NFC payload data </param>
        private void ProcessNfcScan(Intent intent)
        {
            IParcelable[] scannedTags = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
            if (scannedTags != null && scannedTags.Length > 0)
            {
                try
                {
                    NdefMessage msg = (NdefMessage)scannedTags[0];
                    byte[] payloadBytes = msg.GetRecords()[0].GetPayload();
                    String payload = String.Empty;
                    foreach (byte b in payloadBytes)
                    {
                        payload += Convert.ToChar(b);
                    }
                    string friendlyName = GetDeviceFriendlyName(payload);
                    string macAddress = GetDeviceMacAddress(payloadBytes);
                    if (IsDevicePaired(friendlyName))
                    {
                        ShowAlreadyPaired(friendlyName);
                    }
                    else
                    {
                        StartSearching(friendlyName);
                        Activity.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (_activity.Bluetooth.Adapter == null)
                                    throw new Exception("No Bluetooth adapter found.");

                                if (!_activity.Bluetooth.Adapter.IsEnabled)
                                    throw new Exception("Bluetooth adapter is not enabled.");

                                PairDevice(friendlyName, macAddress);
                            }
                            catch (DiscoveryException e)
                            {
                                Console.WriteLine(e.ToString());
                                Console.Write(e.StackTrace);
                            }
                            finally
                            {
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                }

                intent.RemoveExtra(NfcAdapter.ExtraNdefMessages);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nfcDevice"></param>
        private void PairDevice(string friendlyName, string macAddress)
        {
            BluetoothDevice device = _activity.Bluetooth.Adapter.GetRemoteDevice(macAddress);

            if (device != null)
            {
                if (device.CreateBond())
                {
                    OnDevicePaired(device, EventArgs.Empty);
                }
            }
            else
            {
                Toast.MakeText(Activity, string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairFailed), friendlyName), ToastLength.Long).Show();
            }
        }

        public override void OnPairDevice(object device, int state)
        {
            BluetoothDevice btDevice = device as BluetoothDevice;
            if ((Bonding)state == Bonding.None)
            {
                _deviceList.Remove(btDevice);
            }
            else if ((Bonding)state == Bonding.Bonded)
            {
                _deviceList.Add(btDevice);
            }
            StopSearching(btDevice.Name, state);
            RefreshList();
        }
        /// <summary>
        /// Updates UI elements to show that the app has began searching for the printer
        /// </summary>
        /// <param name="deviceName"> name of the printer that is being searched for </param>
        private void StartSearching(string deviceName)
        {
            Activity.RunOnUiThread(() =>
            {
                if (!(deviceName is null))
                {
                    _tvInfo.Text = string.Format("{0} {1}", GetString(Resource.String.MainActivity_AttemptToPair), deviceName);
                }
                _tvInfo.Visibility = ViewStates.Visible;

                ProgressBar pbSearching = Activity.FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Visible;
            });
        }
        /// <summary>
        /// Updates UI elements to show that the app has stopped searching for the printer
        /// </summary>
        /// <param name="deviceName"> name of the printer that was being searched for </param>
        private void StopSearching(string deviceName, int state)
        {
            Activity.RunOnUiThread(() =>
            {
                if (!(deviceName is null) && IsDevicePaired(deviceName))
                {
                    Toast.MakeText(Activity, string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairSuccessful), deviceName), ToastLength.Long).Show();
                }
                else
                {
                    switch (state)
                    {
                        case (int)Bonding.None:
                            Toast.MakeText(Activity, string.Format("{0} {1}", GetString(Resource.String.MainActivity_UnpairSuccessful), deviceName), ToastLength.Long).Show();
                            break;
                        case (int)Bonding.Bonding:
                            break;
                        case (int)Bonding.Bonded:
                            Toast.MakeText(Activity, string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairSuccessful), deviceName), ToastLength.Long).Show();
                            break;
                        case (int)Bonding.Pairing:
                            break;
                        case (int)Bonding.Cancelled:
                            Toast.MakeText(Activity, string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairCancelled), deviceName), ToastLength.Long).Show();
                            break;
                        default:
                            Toast.MakeText(Activity, string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairFailed), deviceName), ToastLength.Long).Show();
                            break;
                    }
                }

                _tvInfo.Text = GetString(Resource.String.MainActivity_TapToPair);
                _tvInfo.Visibility = ViewStates.Visible;

                ProgressBar pbSearching = Activity.FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Gone;
            });
        }
        /// <summary>
        /// Shows a message informing the user that the device is already paired
        /// </summary>
        /// <param name="deviceName"> name of the printer that was being searched for </param>
        private void ShowAlreadyPaired(string deviceName)
        {
            Activity.RunOnUiThread(() =>
            {
                if (!(deviceName is null) && IsDevicePaired(deviceName))
                {
                    Toast.MakeText(Activity, string.Format("{0} {1}", deviceName, GetString(Resource.String.MainActivity_AlreadyPaired)), ToastLength.Long).Show();
                }
            });
        }
        /// <summary>
        /// Refreshed the list of paired bluetooth devices
        /// </summary>
        public void RefreshList()
        {
            Activity.RunOnUiThread(() =>
            {
                ObservableCollection<BluetoothDevice> deviceList = GetPairedDeviceCollection();
                _listview.Adapter = new BluetoothDeviceArrayAdapter(Activity, Resource.Layout.row_layout, deviceList);
            });
        }
        /// <summary>
        /// Parses out the printer's Friendly Name from the NFC payload
        /// </summary>
        /// <param name="payload"> NFC payload string </param>
        /// <returns> printer's Friendly Name </returns>
        private string GetDeviceFriendlyName(string payload)
        {
            string parameterFriendlyName = "s=";
            string[] payloadItems = payload.Split('&');
            for (int i = 0; i < payloadItems.Length; i++)
            {
                //Friendly Name
                if (payloadItems[i].StartsWith(parameterFriendlyName, StringComparison.Ordinal))
                {
                    return payloadItems[i].Substring(parameterFriendlyName.Length);
                }
            }
            return "";
        }
        private int? GetDevicePIN(string payload)
        {
            string parameterPin = "p=";
            string[] payloadItems = payload.Split('&');
            for (int i = 0; i < payloadItems.Length; i++)
            {
                //PIN
                if (payloadItems[i].StartsWith(parameterPin, StringComparison.Ordinal))
                {
                    return Convert.ToInt32(payloadItems[i].Substring(parameterPin.Length));
                }
            }
            return null;
        }
        private string GetDeviceMacAddress(byte[] payloadBytes)
        {
            string parameterFriendlyName = "s=";
            // Get the Language Code
            int languageCodeLength = payloadBytes[0] & 0063;

            string payload = new String(System.Text.UTF8Encoding.ASCII.GetChars(payloadBytes), languageCodeLength + 1, payloadBytes.Length - languageCodeLength - 1);
            string[] payloadItems = payload.Split('&');
            for (int i = 0; i < payloadItems.Length; i++)
            {
                //Mac Address
                if (!payloadItems[i].StartsWith(parameterFriendlyName, StringComparison.Ordinal))
                {
                    return payloadItems[i];
                }
            }
            return "";
        }
        /// <summary>
        /// Checks to see if the given printer is currently paired to the Android device via bluetooth.
        /// </summary>
        /// <param name="deviceName"> </param>
        /// <returns> true if the printer is paired </returns>
        private bool IsDevicePaired(string deviceName)
        {
            BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            ICollection<BluetoothDevice> pairedDevices = mBluetoothAdapter.BondedDevices;

            foreach (BluetoothDevice device in pairedDevices)
            {
                if (device.Name.Equals(deviceName))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<BluetoothDevice> GetPairedDeviceCollection()
        {
            ICollection<BluetoothDevice> pairedDevices = _activity.Bluetooth.Adapter.BondedDevices;
            ObservableCollection<BluetoothDevice> deviceList = new ObservableCollection<BluetoothDevice>();
            foreach (BluetoothDevice device in pairedDevices)
            {
                deviceList.Add(device);
            }
            return deviceList;
        }
    }
}