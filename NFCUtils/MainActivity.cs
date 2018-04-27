using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using Android.Support.V7.App;
using Android.Bluetooth;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Android.Nfc;

namespace com.touchstar.chrisd.nfcutils
{
    [Activity(Label = "NFC Utilities", Name = "NFCUtils.NFCUtils.MainActivity", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, TapAndPairFragment.OnButtonClicked, BluetoothFragment.OnButtonClicked
    {
        //Button _nfcPairButton;
        //Button _nfcUtilsButton;
        //Button _bluetoothUtilsButton;
        public enum ActivityCode { NFCPair = 0, NFCPairMenu, NFCUtils, Bluetooth };
        public static readonly string FRAGMENT_TAG_MAIN_MENU = "MainMenuFragment";
        public static readonly string FRAGMENT_TAG_NFC_PAIR = "TapAndPairFragment";
        public static readonly string FRAGMENT_TAG_NFC_UTILS = "NfcUtilsFragment";
        public static readonly string FRAGMENT_TAG_BLUETOOTH = "BluetoothFragment";
        public static readonly string TAG = "MainActivity";
        public const int MY_PERMISSION_REQUEST_CONSTANT = 0;

        private BluetoothReceiver mBluetoothReceiver;
        BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        private NfcAdapter nfcAdapter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_main);
            Bundle args = new Bundle();
            MainMenuFragment mainMenuFrag = MainMenuFragment.NewInstance(args);

            FragmentTransaction transaction = FragmentManager.BeginTransaction();
            transaction.Add(Resource.Id.main_menu_container, mainMenuFrag).Commit();

            //mBluetoothReceiver = new BluetoothReceiver(this);
            //IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
            //filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            //filter.AddAction(BluetoothDevice.ActionPairingRequest);
            //filter.AddAction(BluetoothDevice.ActionBondStateChanged);
            //filter.AddAction(BluetoothAdapter.ActionDiscoveryStarted);
            //RegisterReceiver(mBluetoothReceiver, filter);
            //RegisterReceiver(mBluetoothReceiver, new IntentFilter(BluetoothDevice.ActionFound));
            //RegisterReceiver(mBluetoothReceiver, new IntentFilter(BluetoothDevice.ActionBondStateChanged));
            //SetContentView(Resource.Layout.activity_main);
            nfcAdapter = NfcAdapter.GetDefaultAdapter(this);

            //base.OnCreate(bundle);
            // get the buttons
            //_nfcPairButton = FindViewById<Button>(Resource.Id.nfc_pair_button);
            //_nfcUtilsButton = FindViewById<Button>(Resource.Id.nfc_utils_button);
            //_bluetoothUtilsButton = FindViewById<Button>(Resource.Id.bluetooth_utils_button);

            //// assign the click events
            //_nfcPairButton.Click += NfcPairButton_OnClick;
            //_nfcUtilsButton.Click += NfcUtilsButton_OnClick;
            //_bluetoothUtilsButton.Click += BluetoothUtilsButton_OnClick;

            string operation = Intent.GetStringExtra("Operation");
            string activity = Intent.GetStringExtra("Activity");
            if (activity == null)
                activity = "NFCUtilities";
            if (operation == null)
                operation = "Application Run";
            Toast.MakeText(Application.Context, string.Format("MainActivity {0} {1} {2} {3}", "Launch Intent Activity=", activity, ", Operation=", operation), ToastLength.Long).Show();

            LauchActivity(activity);
        }

        protected override void OnStart()
        {
            base.OnStart();
            //mBluetoothReceiver = new BluetoothReceiver(this);

            //IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
            //filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            //filter.AddAction(BluetoothDevice.ActionPairingRequest);
            //filter.AddAction(BluetoothDevice.ActionBondStateChanged);
            //filter.AddAction(BluetoothAdapter.ActionDiscoveryStarted);
            //RegisterReceiver(mBluetoothReceiver, filter);
            string[] permissions = new string[] { Android.Manifest.Permission.AccessFineLocation };
            RequestPermissions(permissions, MY_PERMISSION_REQUEST_CONSTANT);
        }

        public void OnRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
        {
            switch (requestCode)
            {
                case MY_PERMISSION_REQUEST_CONSTANT:
                {
                    // If request is cancelled, the result arrays are empty.
                    if (grantResults.Length > 0 && grantResults[0] == (int)Android.Content.PM.Permission.Granted)
                    {
                        //permission granted!
                    }
                    return;
                }
            }
        }

        private void LauchActivity(string launchIntentAction)
        {
            switch (launchIntentAction)
            {
                case "NFCPair":
                {
                    TapAndPairFragment tapAndPairFrag = TapAndPairFragment.NewInstance((int)ActivityCode.NFCPair);
                    FragmentManager.BeginTransaction()
                        .Replace(Resource.Id.main_menu_container, tapAndPairFrag, FRAGMENT_TAG_NFC_PAIR)
                        .Commit();
                }
                break;
                default:
                    break;
            }
        }

        public void TapAndPairFragmentOnOKButtonClicked(Intent result, int requestCode)
        {
            OnActivityResult(requestCode, 0, result);
            OnBackPressed();
        }

        public void BluetoothFragmentOnOKButtonClicked(Intent result, int requestCode)
        {
            OnActivityResult(requestCode, 0, result);
            OnBackPressed();
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            //Fragment fragment = FragmentManager.FindFragmentByTag(FRAGMENT_TAG_NFC_PAIR);
            //if (fragment != null)
            //{
            //    fragment.OnActivityResult(requestCode, resultCode, intent);
            //}
            switch ((ActivityCode)requestCode)
            {
                case ActivityCode.NFCPair:
                {
                    //Java.Lang.Object nfcBundle = data.GetParcelableExtra("NFCDevice");
                    SetResult(Result.Ok, data);
                    Finish();
                }
                break;
                case ActivityCode.Bluetooth:
                    {
                        string name = data.GetStringExtra("BluetoothDeviceName");
                        string mac = data.GetStringExtra("BluetoothDeviceAddress");
                    }
                    break;
                default:
                    break;
            }
        }

        public override void OnBackPressed()
        {
            int backstackCount = FragmentManager.BackStackEntryCount;
            
            if (backstackCount == 0)
                base.OnBackPressed();
            else
                FragmentManager.PopBackStackImmediate();
        }

        protected override void OnNewIntent(Intent intent)
        {
            TapAndPairFragment tapAndPairFrag = FragmentManager.FindFragmentByTag<TapAndPairFragment>(FRAGMENT_TAG_NFC_PAIR);
            NfcUtilsFragment nfcUtilsFrag = FragmentManager.FindFragmentByTag<NfcUtilsFragment>(FRAGMENT_TAG_NFC_UTILS);
            BluetoothFragment bluetoothFrag = FragmentManager.FindFragmentByTag<BluetoothFragment>(FRAGMENT_TAG_BLUETOOTH);

            if (tapAndPairFrag != null && nfcUtilsFrag == null && bluetoothFrag == null)
            {
                tapAndPairFrag.OnNewIntent(intent);
            }
            else if (tapAndPairFrag == null && nfcUtilsFrag != null && bluetoothFrag == null)
            {
                nfcUtilsFrag.OnNewIntent(intent);
            }
            else if (tapAndPairFrag == null && nfcUtilsFrag == null && bluetoothFrag != null)
            {
                //bluetoothFrag.OnNewIntent(intent);
            }
        }

        public void OnDeviceFound(BluetoothDevice bluetoothDevice, BluetoothClass bluetoothClass)
        {
            BluetoothFragment bluetoothFrag = FragmentManager.FindFragmentByTag<BluetoothFragment>(FRAGMENT_TAG_BLUETOOTH);
            if (bluetoothFrag != null)
            {
                bluetoothFrag.OnDeviceFound(bluetoothDevice, bluetoothClass);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Attempt EnableReadMode so we intercept NFC read messages
            EnableReadMode();
        }

        public void EnableReadMode()
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
                var alert = new Android.App.AlertDialog.Builder(this).Create();
                alert.SetMessage("NFC is not supported on this device.");
                alert.SetTitle("NFC Unavailable");
                //alert.SetButton("OK", delegate
                //{
                //    tvInfo.Text = "NFC is not supported on this device.";
                //    tvInfo.Visibility = ViewStates.Visible;
                //});
                alert.Show();
            }
            else
                nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
        }

        /// <summary>
        /// Identify to Android that this activity wants to be notified when 
        /// an NFC tag is discovered. 
        /// </summary>
        public void EnableWriteMode()
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
                var alert = new Android.App.AlertDialog.Builder(this).Create();
                alert.SetMessage("NFC is not supported on this device.");
                alert.SetTitle("NFC Unavailable");
                //alert.SetButton("OK", delegate
                //{
                //    DisplayMessage("NFC is not supported on this device.");
                //});
                alert.Show();
            }
            else
                nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
        }

        protected override void OnDestroy()
        {
            //if (mBluetoothReceiver != null)
            //{
            //    UnregisterReceiver(mBluetoothReceiver);
            //}

            base.OnDestroy();
        }

        public void OnScanStarted()
        {
            BluetoothFragment bluetoothFrag = FragmentManager.FindFragmentByTag<BluetoothFragment>(FRAGMENT_TAG_BLUETOOTH);
            if (bluetoothFrag != null)
            {
                bluetoothFrag.OnScanStarted();
            }
        }

        public void OnScanComplete()
        {
            BluetoothFragment bluetoothFrag = FragmentManager.FindFragmentByTag<BluetoothFragment>(FRAGMENT_TAG_BLUETOOTH);
            if (bluetoothFrag != null)
            {
                bluetoothFrag.OnScanComplete();
            }
        }

        //public void OnDeviceFound(BluetoothDevice bluetoothDevice, BluetoothClass bluetoothClass)
        //{
        //    if (bluetoothDevice == null)
        //        return;

        //    bool found = false;
        //    // in case of rescan check the one found isn't already in our list
        //    foreach (BluetoothDevice device in mBluetoothDevices)
        //    {
        //        if (device.Address == bluetoothDevice.Address)
        //        {
        //            found = true;
        //            break;
        //        }
        //    }

        //    if (!found)
        //    {
        //        mBluetoothDevices.Add(bluetoothDevice);
        //        mBluetoothAdapter.Add(String.Format("{0} # {1}", bluetoothDevice.Name, bluetoothDevice.Address));
        //        mBluetoothAdapter.NotifyDataSetChanged();
        //    }
        //}

        public void OnPairDevice(BluetoothDevice device, int state)
        {
            TapAndPairFragment tapAndPairFrag = FragmentManager.FindFragmentByTag<TapAndPairFragment>(FRAGMENT_TAG_NFC_PAIR);
            if (tapAndPairFrag != null)
            {
                tapAndPairFrag.OnPairDevice(device, state);
            }
        }
    }
}

