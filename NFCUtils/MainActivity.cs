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
        public enum ActivityCode { NFCPair = 0, NFCPairMenu, NFCUtils, Bluetooth };
        public static readonly string FRAGMENT_TAG_MAIN_MENU = "MainMenuFragment";
        public static readonly string FRAGMENT_TAG_NFC_PAIR = "TapAndPairFragment";
        public static readonly string FRAGMENT_TAG_NFC_UTILS = "NfcUtilsFragment";
        public static readonly string FRAGMENT_TAG_BLUETOOTH = "BluetoothFragment";
        public static readonly string TAG = "MainActivity";
        public const int MY_PERMISSION_REQUEST_CONSTANT = 0;
        private NfcAdapter _nfcAdapter;
        private BluetoothReceiver _bluetoothReceiver;
        private Bluetooth _bluetooth;
        private String _nfcTag;

        public NfcAdapter NfcAdapter
        {
            get { return _nfcAdapter; }
        }

        public BluetoothReceiver BluetoothReceiver
        {
            get { return _bluetoothReceiver; }
        }

        public Bluetooth Bluetooth
        {
            get { return _bluetooth; }
            set { _bluetooth = value; }
        }

        public String NfcTag
        {
            get { return _nfcTag; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_main);
            Bundle args = new Bundle();
            MainMenuFragment mainMenuFrag = MainMenuFragment.NewInstance(args);

            FragmentTransaction transaction = FragmentManager.BeginTransaction();
            transaction.Add(Resource.Id.main_menu_container, mainMenuFrag).Commit();
            _nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
            _bluetooth = new Bluetooth
            {
                Adapter = BluetoothAdapter.DefaultAdapter
            };

            string operation = Intent.GetStringExtra("Operation");
            string activity = Intent.GetStringExtra("Activity");
            //if (activity == null)
            //    activity = "NFCUtilities";
            //if (operation == null)
            //    operation = "Application Run";
            //Toast.MakeText(Application.Context, string.Format("MainActivity {0} {1} {2} {3}", "Launch Intent Activity=", activity, ", Operation=", operation), ToastLength.Long).Show();

            LauchActivity(activity);
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            string[] permissions = new string[]
                { Android.Manifest.Permission.AccessFineLocation,
                  Android.Manifest.Permission.Bluetooth,
                  Android.Manifest.Permission.BluetoothAdmin,
                  Android.Manifest.Permission.Nfc };
            RequestPermissions(permissions, MY_PERMISSION_REQUEST_CONSTANT);
        }

        protected override void OnResume()
        {
            base.OnResume();

            _bluetoothReceiver = new BluetoothReceiver(this);

            IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            filter.AddAction(BluetoothDevice.ActionPairingRequest);
            filter.AddAction(BluetoothDevice.ActionBondStateChanged);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryStarted);
            filter.AddAction(BluetoothAdapter.ActionConnectionStateChanged);

            RegisterReceiver(_bluetoothReceiver, filter);
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void OnDestroy()
        {
            UnregisterReceiver(_bluetoothReceiver);
            base.OnDestroy();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="permissions"></param>
        /// <param name="grantResults"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="launchIntentAction"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="requestCode"></param>
        public void TapAndPairFragmentOnOKButtonClicked(Intent result, int requestCode)
        {
            OnActivityResult(requestCode, 0, result);
            OnBackPressed();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="requestCode"></param>
        public void BluetoothFragmentOnOKButtonClicked(Intent result, int requestCode)
        {
            OnActivityResult(requestCode, 0, result);
            OnBackPressed();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            switch ((ActivityCode)requestCode)
            {
                case ActivityCode.NFCPair:
                    {
                        SetResult(Result.Ok, data);
                        Finish();
                    }
                    break;
                case ActivityCode.Bluetooth:
                    {
                        _nfcTag = String.Format("{0}&s={1}", data.GetStringExtra("BluetoothDeviceAddress"), data.GetStringExtra("BluetoothDeviceName"));
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void OnBackPressed()
        {
            int backstackCount = FragmentManager.BackStackEntryCount;

            if (backstackCount == 0)
                base.OnBackPressed();
            else
                FragmentManager.PopBackStackImmediate();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="intent"></param>
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
            }
        }
        /// <summary>
        /// 
        /// </summary>
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

            if (_nfcAdapter == null)
            {
                var alert = new Android.App.AlertDialog.Builder(this).Create();
                alert.SetMessage("NFC is not supported on this device.");
                alert.SetTitle("NFC Unavailable");
                alert.Show();
            }
            else
                _nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
        }
        /// <summary>
        /// 
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

            if (NfcAdapter == null)
            {
                var alert = new Android.App.AlertDialog.Builder(this).Create();
                alert.SetMessage("NFC is not supported on this device.");
                alert.SetTitle("NFC Unavailable");
                alert.Show();
            }
            else
                NfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
        }
        public void OnScanStarted()
        {
            TapAndPairFragment tapAndPairFrag = FragmentManager.FindFragmentByTag<TapAndPairFragment>(FRAGMENT_TAG_NFC_PAIR);
            NfcUtilsFragment nfcUtilsFrag = FragmentManager.FindFragmentByTag<NfcUtilsFragment>(FRAGMENT_TAG_NFC_UTILS);
            BluetoothFragment bluetoothFrag = FragmentManager.FindFragmentByTag<BluetoothFragment>(FRAGMENT_TAG_BLUETOOTH);

            if (tapAndPairFrag != null && nfcUtilsFrag == null && bluetoothFrag == null)
            {
                tapAndPairFrag.OnScanStarted();
            }
            else if (tapAndPairFrag == null && nfcUtilsFrag != null && bluetoothFrag == null)
            {
            }
            else if (tapAndPairFrag == null && nfcUtilsFrag == null && bluetoothFrag != null)
            {
                bluetoothFrag.OnScanStarted();
            }
        }

        public void OnScanComplete()
        {
            TapAndPairFragment tapAndPairFrag = FragmentManager.FindFragmentByTag<TapAndPairFragment>(FRAGMENT_TAG_NFC_PAIR);
            NfcUtilsFragment nfcUtilsFrag = FragmentManager.FindFragmentByTag<NfcUtilsFragment>(FRAGMENT_TAG_NFC_UTILS);
            BluetoothFragment bluetoothFrag = FragmentManager.FindFragmentByTag<BluetoothFragment>(FRAGMENT_TAG_BLUETOOTH);

            if (tapAndPairFrag != null && nfcUtilsFrag == null && bluetoothFrag == null)
            {
                tapAndPairFrag.OnScanComplete();
            }
            else if (tapAndPairFrag == null && nfcUtilsFrag != null && bluetoothFrag == null)
            {
            }
            else if (tapAndPairFrag == null && nfcUtilsFrag == null && bluetoothFrag != null)
            {
                bluetoothFrag.OnScanComplete();
            }
        }

        public void OnDeviceFound(object bluetoothDevice, object bluetoothClass)
        {
            TapAndPairFragment tapAndPairFrag = FragmentManager.FindFragmentByTag<TapAndPairFragment>(FRAGMENT_TAG_NFC_PAIR);
            NfcUtilsFragment nfcUtilsFrag = FragmentManager.FindFragmentByTag<NfcUtilsFragment>(FRAGMENT_TAG_NFC_UTILS);
            BluetoothFragment bluetoothFrag = FragmentManager.FindFragmentByTag<BluetoothFragment>(FRAGMENT_TAG_BLUETOOTH);

            if (tapAndPairFrag != null && nfcUtilsFrag == null && bluetoothFrag == null)
            {
                tapAndPairFrag.OnDeviceFound(bluetoothDevice, bluetoothClass);
            }
            else if (tapAndPairFrag == null && nfcUtilsFrag != null && bluetoothFrag == null)
            {
            }
            else if (tapAndPairFrag == null && nfcUtilsFrag == null && bluetoothFrag != null)
            {
                bluetoothFrag.OnDeviceFound(bluetoothDevice, bluetoothClass);
            }
        }

        public void OnPairDevice(object device, int state)
        {
            TapAndPairFragment tapAndPairFrag = FragmentManager.FindFragmentByTag<TapAndPairFragment>(FRAGMENT_TAG_NFC_PAIR);
            NfcUtilsFragment nfcUtilsFrag = FragmentManager.FindFragmentByTag<NfcUtilsFragment>(FRAGMENT_TAG_NFC_UTILS);
            BluetoothFragment bluetoothFrag = FragmentManager.FindFragmentByTag<BluetoothFragment>(FRAGMENT_TAG_BLUETOOTH);

            if (tapAndPairFrag != null && nfcUtilsFrag == null && bluetoothFrag == null)
            {
                tapAndPairFrag.OnPairDevice(device, state);
            }
            else if (tapAndPairFrag == null && nfcUtilsFrag != null && bluetoothFrag == null)
            {
            }
            else if (tapAndPairFrag == null && nfcUtilsFrag == null && bluetoothFrag != null)
            {
            }
        }
    }
}

