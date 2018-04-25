using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Graphics;
using Android.Nfc;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Zebra.Sdk.Printer.Discovery;
using static Android.Widget.AdapterView;

namespace com.touchstar.chrisd.nfcutils
{
    [Activity(Label = "Tap And Pair", Name = "NFCUtils.NFCUtils.TapAndPairActivity")]
    public class TapAndPairActivity : Activity
    {
        private bool InstanceFieldsInitialized = false;
        internal Context context;
        private ListView listview;
        private TextView tvInfo;
        private BluetoothReceiver broadcastReceiver;
        private NfcAdapter nfcAdapter;
        ObservableCollection<BluetoothDevice> _deviceList = new ObservableCollection<BluetoothDevice>();
        BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        private INfcDevice nfcDevice = new NfcDevice();
        System.Collections.IList deviceList = new List<BluetoothDevice>();
        BluetoothDevice selectedDevice;
        public event EventHandler OnDevicePaired;
        public event EventHandler OnDeviceUnPaired;
        public enum ActivityCode { NFCPair = 0, NFCPairMenu };

        public TapAndPairActivity()
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            //base.OnCreate(savedInstanceState);
            //SetContentView(Resource.Layout.tap_and_pair_fragment);

            //broadcastReceiver = new BluetoothReceiver(this);
            //RegisterReceiver(broadcastReceiver, new IntentFilter(BluetoothDevice.ActionFound));
            //RegisterReceiver(broadcastReceiver, new IntentFilter(BluetoothDevice.ActionBondStateChanged));

            //listview = FindViewById<ListView>(Resource.Id.lvPairedDevices);
            //tvInfo = FindViewById<TextView>(Resource.Id.tvInfo);
            //ObservableCollection<BluetoothDevice> deviceList = GetPairedDeviceCollection();
            //listview.Adapter = new BluetoothDeviceArrayAdapter(context, Resource.Layout.row_layout, deviceList);

            //var okButton = FindViewById<Button>(Resource.Id.buttonOK);
            //okButton.Enabled = false;
            //okButton.Click += OkButton_Click;
            //OnDevicePaired += TapAndPairActivity_OnDevicePaired;
            //// Set up EventHandlers
            //listview.ItemClick += delegate (object sender, ItemClickEventArgs e)
            //{
            //    selectedDevice = deviceList[e.Position];
            //    listview.SetSelection(e.Position);
            //    (listview.Adapter as BluetoothDeviceArrayAdapter).SelectedIndex = e.Position;
            //    //View selected = listview.SelectedView;
            //    //selected.SetBackgroundColor(Color.LightBlue);
            //    // select / unselect the correct row
            //    //BluetoothDeviceArrayAdapter rowAdapter = (sender as ListView).Adapter as BluetoothDeviceArrayAdapter;
            //    //if (rowAdapter != null)
            //    //{
            //    //    rowAdapter.ChangeBackgroundColor(sender as ListView, e);
            //    //    if ((BluetoothDevice)(sender as ListView).GetItemAtPosition(e.Position) != null
            //    //        && ((sender as ListView).Adapter as BluetoothDeviceArrayAdapter).SelectedIndex > -1)
            //    //    {
            //    //        selectedDevice = (BluetoothDevice)(sender as ListView).GetItemAtPosition(e.Position);
            //    //        okButton.Enabled = true;
            //    //    }
            //    //    else
            //    //    {
            //    //        selectedDevice = null;
            //    //        okButton.Enabled = false;
            //    //    }
            //    //}
            //};
            //listview.ItemSelected += delegate (object sender, ItemSelectedEventArgs e)
            //{

            //};

            //nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
            //string operation = Intent.GetStringExtra("Operation");
            //string activity = Intent.GetStringExtra("Activity");
            //Toast.MakeText(Application.Context, string.Format("TapAndPair - {0} {1} {2} {3}", "Launch Intent Activity=", activity, ", Operation=", operation), ToastLength.Long).Show();

        }

        private void @delegate(object sender, ItemSelectedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void TapAndPairActivity_OnDevicePaired(object sender, EventArgs e)
        {
            selectedDevice = sender as BluetoothDevice;
            var returnIntent = new Intent();
            if (selectedDevice != null)
            {
                returnIntent.PutExtra("NFCDeviceName", selectedDevice.Name);
                returnIntent.PutExtra("NFCDeviceAddress", selectedDevice.Address);
                returnIntent.PutExtra("NFCDevice", selectedDevice);
            }
            else
            {
                returnIntent.PutExtra("NFCDevice", "No selected device");
                returnIntent.PutExtra("NFCDevice", selectedDevice);
            }

            SetResult(Result.Ok, returnIntent);
            Finish();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.tap_and_pair_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_refresh:
                    RefreshList();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Attempt EnableReadMode so we intercept NFC read messages
            EnableReadMode();
        }

        private void EnableReadMode()
        {
            // Create an intent filter for when an NFC tag is discovered.  When
            // the NFC tag is discovered, Android will u
            var tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
            var filters = new[] { tagDetected };

            // When an NFC tag is detected, Android will use the PendingIntent to come back to this activity.
            // The OnNewIntent method will invoked by Android.
            var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);

            if (nfcAdapter == null)
            {
                var alert = new AlertDialog.Builder(this).Create();
                alert.SetMessage("NFC is not supported on this device.");
                alert.SetTitle("NFC Unavailable");
                alert.SetButton("OK", delegate
                {
                    tvInfo.Text = "NFC is not supported on this device.";
                    tvInfo.Visibility = ViewStates.Visible;
                });
                alert.Show();
            }
            else
                nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
        }

        protected override void OnDestroy()
        {
            if (broadcastReceiver != null)
            {
                UnregisterReceiver(broadcastReceiver);
            }

            base.OnDestroy();
        }

        protected override void OnNewIntent(Intent intent)
        {
            Intent = intent;
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
            base.OnNewIntent(intent);
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
                NdefMessage msg = (NdefMessage)scannedTags[0];
                byte[] payloadBytes = msg.GetRecords()[0].GetPayload();
                String payload = String.Empty;
                foreach (byte b in payloadBytes)
                {
                    payload += Convert.ToChar(b);
                }
                NfcDevice nfcDevice = new NfcDevice
                {
                    FriendlyName = GetDeviceFriendlyName(payload),
                    MacAddress = GetDeviceMacAddress(payloadBytes),
                    PIN = GetDevicePIN(payload).Value
                };
                if (IsDevicePaired(nfcDevice.FriendlyName))
                {
                    ShowAlreadyPaired(nfcDevice.FriendlyName);
                }
                else
                {
                    StartSearching(nfcDevice.FriendlyName);
                    RunOnUiThread(() =>
                    {
                        try
                        {
                            if (bluetoothAdapter == null)
                                throw new Exception("No Bluetooth adapter found.");

                            if (!bluetoothAdapter.IsEnabled)
                                throw new Exception("Bluetooth adapter is not enabled.");

                            PairDevice(nfcDevice);
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

                intent.RemoveExtra(NfcAdapter.ExtraNdefMessages);
            }
        }

        private void PairDevice(NfcDevice nfcDevice)
        {
            BluetoothDevice device = bluetoothAdapter.GetRemoteDevice(nfcDevice.MacAddress);

            if (device != null)
            {
                if (device.CreateBond())
                {
                    OnDevicePaired(device, EventArgs.Empty);
                }
                else
                {
                    Toast.MakeText(context, string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairFailed), nfcDevice.FriendlyName), ToastLength.Long).Show();
                }
            }
            else
            {
                Toast.MakeText(context, string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairFailed), nfcDevice.FriendlyName), ToastLength.Long).Show();
            }
        }

        public void OnPairDevice(BluetoothDevice device, int state)
        {
            StopSearching(device.Name, state);
            RefreshList();
        }

        /// <summary>
        /// Updates UI elements to show that the app has began searching for the printer
        /// </summary>
        /// <param name="deviceName"> name of the printer that is being searched for </param>
        private void StartSearching(string deviceName)
        {
            RunOnUiThread(() =>
            {
                if (!(deviceName is null))
                {
                    tvInfo.Text = string.Format("{0} {1}", GetString(Resource.String.MainActivity_AttemptToPair), deviceName);
                }
                tvInfo.Visibility = ViewStates.Visible;

                ProgressBar pbSearching = FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Visible;
            });
        }

        /// <summary>
        /// Updates UI elements to show that the app has stopped searching for the printer
        /// </summary>
        /// <param name="deviceName"> name of the printer that was being searched for </param>
        private void StopSearching(string deviceName, int state)
        {
            RunOnUiThread(() =>
            {
                if (!(deviceName is null) && IsDevicePaired(deviceName))
                {
                    Toast.MakeText(context, string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairSuccessful), deviceName), ToastLength.Long).Show();
                }
                else
                {
                    switch (state)
                    {
                        case (int)Bond.None:
                            Toast.MakeText(context, string.Format("{0} {1}", GetString(Resource.String.MainActivity_UnpairSuccessful), deviceName), ToastLength.Long).Show();
                            break;
                        case (int)Bond.Bonding:
                            break;
                        case (int)Bond.Bonded:
                            Toast.MakeText(context, string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairSuccessful), deviceName), ToastLength.Long).Show();
                            break;
                        default:
                            Toast.MakeText(context, string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairFailed), deviceName), ToastLength.Long).Show();
                            break;
                    }
                }

                tvInfo.Text = GetString(Resource.String.MainActivity_TapToPair);
                tvInfo.Visibility = ViewStates.Visible;

                ProgressBar pbSearching = FindViewById<ProgressBar>(Resource.Id.pbSearching);
                pbSearching.Visibility = ViewStates.Gone;
            });
        }

        /// <summary>
        /// Shows a message informing the user that the device is already paired
        /// </summary>
        /// <param name="deviceName"> name of the printer that was being searched for </param>
        private void ShowAlreadyPaired(string deviceName)
        {
            RunOnUiThread(() =>
            {
                if (!(deviceName is null) && IsDevicePaired(deviceName))
                {
                    Toast.MakeText(ApplicationContext, string.Format("{0} {1}", deviceName, GetString(Resource.String.MainActivity_AlreadyPaired)), ToastLength.Long).Show();
                }
            });
        }

        /// <summary>
        /// Refreshed the list of paired bluetooth devices
        /// </summary>
        public void RefreshList()
        {
            RunOnUiThread(() =>
            {
                ObservableCollection<BluetoothDevice> deviceList = GetPairedDeviceCollection();
                listview.Adapter = new BluetoothDeviceArrayAdapter(context, Resource.Layout.row_layout, deviceList);
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
        /// Returns a list of all the printers currently paired to the Android device via bluetooth.
        /// </summary>
        /// <returns> a list of all the printers currently paired to the Android device via bluetooth. </returns>
        private ICollection<BluetoothDevice> PairedDevices
        {
            get
            {
                BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                ICollection<BluetoothDevice> pairedDevices = mBluetoothAdapter.BondedDevices;
                List<BluetoothDevice> pairedDevicesList = new List<BluetoothDevice>();
                foreach (BluetoothDevice device in pairedDevices)
                {
                    _deviceList.Add(device);
                }
                return _deviceList;
            }
        }

        private BluetoothDevice[] GetPairedDevices()
        {
            ICollection<BluetoothDevice> pairedDevices = bluetoothAdapter.BondedDevices;
            BluetoothDevice[] deviceArray = new BluetoothDevice[pairedDevices.Count];
            int i = 0;
            foreach (BluetoothDevice device in pairedDevices)
            {
                deviceArray[i] = device;
                i++;
            }
            return deviceArray;
        }

        private ObservableCollection<BluetoothDevice> GetPairedDeviceCollection()
        {
            ICollection<BluetoothDevice> pairedDevices = bluetoothAdapter.BondedDevices;
            ObservableCollection<BluetoothDevice> deviceList = new ObservableCollection<BluetoothDevice>();
            foreach (BluetoothDevice device in pairedDevices)
            {
                deviceList.Add(device);
            }
            return deviceList;
        }

        private void TextViewInfo_TextChanged(object sender, EventArgs e)
        {
            TextChangedEventArgs args = e as TextChangedEventArgs;
            TextView view = (TextView)sender;
            if (view.Id == Resource.Id.tvInfo)
            {
                if (args.Text.ToString() != view.Text)
                    view.Text = args.Text.ToString();
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var returnIntent = new Intent();
            if (selectedDevice != null)
            {
                returnIntent.PutExtra("NFCDeviceName", selectedDevice.Name);
                returnIntent.PutExtra("NFCDeviceAddress", selectedDevice.Address);
                returnIntent.PutExtra("NFCDevice", selectedDevice);
            }
            else
            {
                returnIntent.PutExtra("NFCDevice", "No selected device");
                returnIntent.PutExtra("NFCDevice", selectedDevice);
            }

            SetResult(Result.Ok, returnIntent);
            Finish();
        }
    }
}