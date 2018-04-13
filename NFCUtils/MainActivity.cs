using Android.App;
using Android.Widget;
using Android.OS;
using System;

namespace com.touchstar.chrisd.nfcutils
{
    [Activity(Label = "NFC Utilities", MainLauncher = true)]
    public class MainActivity : Activity
    {
        Button _nfcPairButton;
        Button _nfcUtilsButton;
        Button _bluetoothUtilsButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_main);

            // get the buttons
            _nfcPairButton = FindViewById<Button>(Resource.Id.nfc_pair_button);
            _nfcUtilsButton = FindViewById<Button>(Resource.Id.nfc_utils_button);
            _bluetoothUtilsButton = FindViewById<Button>(Resource.Id.bluetooth_utils_button);

            // assign the click events
            _nfcPairButton.Click += NfcPairButton_OnClick;
            _nfcUtilsButton.Click += NfcUtilsButton_OnClick;
            _bluetoothUtilsButton.Click += BluetoothUtilsButton_OnClick;

            //var fragmentTransaction = FragmentManager.BeginTransaction();
            //fragmentTransaction.Add(Android.Resource.Id.Content, menu);
            //fragmentTransaction.Commit();
        }

        private void NfcPairButton_OnClick(object sender, EventArgs e)
        {
            //var intent = new Intent();
            //intent.SetClass(((Fragment)sender).Activity, typeof(TapAndPairActivity));
            //StartActivity(intent);
            StartActivity(typeof(TapAndPairActivity));
        }

        private void NfcUtilsButton_OnClick(object sender, EventArgs e)
        {
            //var intent = new Intent();
            //intent.SetClass(((Fragment)sender).Activity, typeof(NfcUtilsActivity));
            //StartActivity(intent);
            StartActivity(typeof(NfcUtilsActivity));
        }

        private void BluetoothUtilsButton_OnClick(object sender, EventArgs e)
        {
            //var intent = new Intent();
            //intent.SetClass(((Fragment)sender).Activity, typeof(BluetoothUtilsActivity));
            //StartActivity(intent);
            StartActivity(typeof(BluetoothUtilsActivity));
        }
    }
}

