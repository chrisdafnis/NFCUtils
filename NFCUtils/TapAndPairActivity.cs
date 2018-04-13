using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Nfc;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang.Reflect;
using Zebra.Sdk.Printer.Discovery;
using static Android.Widget.AdapterView;

namespace com.touchstar.chrisd.nfcutils
{
    [Activity(Label = "Tap And Pair")]
    public class TapAndPairActivity : Activity
    {
        private bool InstanceFieldsInitialized = false;
        internal Context context;
        private ListView listview;
        private TextView tvInfo;
        private BluetoothDeviceArrayAdapter adapter;
        private BluetoothReceiver broadcastReceiver;
        private AlertDialog successDialog;
        private NfcAdapter nfcAdapter;
        ObservableCollection<BluetoothDevice> _deviceList = new ObservableCollection<BluetoothDevice>();
        BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        private INfcDevice nfcDevice = new NfcDevice();

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
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.tap_and_pair_fragment);

            listview = FindViewById<ListView>(Resource.Id.lvPairedDevices);
            tvInfo = FindViewById<TextView>(Resource.Id.tvInfo);
            adapter = new BluetoothDeviceArrayAdapter(context, PairedDevices);
            listview.Adapter = (IListAdapter)adapter;

            // Set up EventHandlers
            listview.ItemClick += ListView_ItemClick;

            nfcAdapter = NfcAdapter.GetDefaultAdapter(this);

            //broadcastReceiver = new BluetoothReceiver(this);
            //RegisterReceiver(broadcastReceiver, new IntentFilter(BluetoothDevice.ActionFound));
            //RegisterReceiver(broadcastReceiver, new IntentFilter(BluetoothDevice.ActionBondStateChanged));
            //RegisterReceiver(broadcastReceiver, new IntentFilter(BluetoothDevice.ActionPairingRequest));
            //RegisterReceiver(broadcastReceiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
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

        public void FoundDevice(object device)
        {
            if (device is BluetoothDevice bluetoothDevice)
            {
                if (bluetoothDevice.Address != null)
                {
                    if ((bluetoothDevice.Name == nfcDevice.FriendlyName) && (bluetoothDevice.Address == nfcDevice.MacAddress))
                    {
                        (new Thread(() =>
                        {
                            try
                            {
                                Method m = bluetoothDevice.Class.GetMethod("createBond", null);
                                var retVal = m.Invoke(bluetoothDevice, null);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                                Console.Write(e.StackTrace);
                            }
                            finally
                            {
                                StopSearching(bluetoothDevice.Name);
                                RefreshList();
                            }
                        })).Start();
                    }
                }
            }
        }

        public void DiscoveryError(string s)
        {
            //Ignore network errors
            if (!s.Contains("ENETUNREACH"))
            {
                StopSearching(null);
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
                NdefMessage msg = (NdefMessage)scannedTags[0];
                byte[] payloadBytes = msg.GetRecords()[0].GetPayload();
                String payload = String.Empty;
                foreach (byte b in payloadBytes)
                {
                    payload += Convert.ToChar(b);
                }
                string deviceName = GetDeviceFriendlyName(payload);
                string macAddress = GetDeviceMacAddress(payloadBytes);
                if (IsDevicePaired(deviceName))
                {
                    ShowAlreadyPaired(deviceName);
                }
                else
                {
                    StartSearching(deviceName);
                    (new Thread(() =>
                    {
                        try
                        {
                            if (bluetoothAdapter == null)
                                throw new Exception("No Bluetooth adapter found.");

                            if (!bluetoothAdapter.IsEnabled)
                                throw new Exception("Bluetooth adapter is not enabled.");

                            nfcDevice.FriendlyName = deviceName;
                            nfcDevice.MacAddress = macAddress;
                            bluetoothAdapter.StartDiscovery();
                        }
                        catch (DiscoveryException e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.Write(e.StackTrace);
                        }
                        finally
                        {
                        }
                    })).Start();
                }

                intent.RemoveExtra(NfcAdapter.ExtraNdefMessages);
            }
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
        private void StopSearching(string deviceName)
        {
            RunOnUiThread(() =>
            {
                if (!(deviceName is null) && IsDevicePaired(deviceName))
                {
                    if (successDialog == null || !successDialog.IsShowing)
                    {
                        AlertDialog.Builder builder = new AlertDialog.Builder(context);
                        builder.SetTitle("Pairing Successful");
                        builder.SetMessage(string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairSuccessful), deviceName));
                        builder.SetPositiveButton(Android.Resource.String.Yes, (c, ev) =>
                        {
                            Toast.MakeText(context, Resource.String.MainActivity_PairComplete, ToastLength.Long).Show();
                        });
                        successDialog = builder.Create();
                        successDialog.Show();
                    }
                }
                else
                {
                    Toast.MakeText(context, string.Format("{0} {1}", GetString(Resource.String.MainActivity_PairFailed), deviceName), ToastLength.Long).Show();
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
                adapter.Clear();
                adapter.AddAll(PairedDevices);
                adapter.NotifyDataSetChanged();
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

        private void ListView_ItemClick(object sender, EventArgs e)//AdapterView.ItemClickEventArgs e)
        {
            ItemClickEventArgs args = e as ItemClickEventArgs;

            BluetoothDevice item = (BluetoothDevice)(sender as ListView).GetItemAtPosition(args.Position);
            if (item != null && item.Address != null)// && IsBluetoothPrinter(item))
            {
                new AlertDialog.Builder((sender as ListView).Context)
                        .SetTitle("Unpair?")
                        .SetMessage(Resource.String.MainActivity_UnpairDevice)
                        .SetPositiveButton(Android.Resource.String.Yes, (c, ev) =>
                        {
                            try
                            {
                                Method m = item.Class.GetMethod("removeBond", null);
                                m.Invoke(item, null);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                Console.Write(ex.StackTrace);
                            }

                            // Toast.MakeText(context, "Clicked 'Yes'", ToastLength.Long).Show();
                        })
                        .SetNegativeButton(Android.Resource.String.No, (c, ev) =>
                        {
                            // Do nothing
                            // Toast.MakeText(context, "Clicked 'No'", ToastLength.Long).Show();
                        })
                        .Show();
            }
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
    }
}