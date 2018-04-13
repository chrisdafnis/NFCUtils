using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace com.touchstar.chrisd.nfcutils
{
    internal class MainMenuFragment : Fragment
    {
        public event EventHandler NfcPairButtonClicked;
        public event EventHandler NfcUtilsButtonClicked;
        public event EventHandler BluetoothUtilsButtonClicked;

        Button _nfcPairButton;
        Button _nfcUtilsButton;
        Button _bluetoothUtilsButton;

        public static MainMenuFragment NewInstance()
        {
            var mainMenuFrag = new MainMenuFragment { Arguments = new Bundle() };
            return mainMenuFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }
            return inflater.Inflate(Resource.Layout.main_menu_fragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            // get the buttons
            _nfcPairButton = view.FindViewById<Button>(Resource.Id.nfc_pair_button);
            _nfcUtilsButton = view.FindViewById<Button>(Resource.Id.nfc_utils_button);
            _bluetoothUtilsButton = view.FindViewById<Button>(Resource.Id.bluetooth_utils_button);

            // assign the click events
            _nfcPairButton.Click += NfcPairButton_OnClick;
            _nfcUtilsButton.Click += NfcUtilsButton_OnClick;
            _bluetoothUtilsButton.Click += BluetoothUtilsButton_OnClick;

            base.OnViewCreated(view, savedInstanceState);
        }

        private void NfcPairButton_OnClick(object sender, EventArgs e)
        {
            NfcPairButtonClicked(this, e);
        }

        private void NfcUtilsButton_OnClick(object sender, EventArgs e)
        {
            NfcUtilsButtonClicked(this, e);
        }

        private void BluetoothUtilsButton_OnClick(object sender, EventArgs e)
        {
            BluetoothUtilsButtonClicked(this, e);
        }
    }
}