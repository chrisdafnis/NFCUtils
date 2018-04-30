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
    [Register("com.touchstar.chrisd.nfcutils.MainMenuFragment")]
    internal class MainMenuFragment : VisibleFragment
    {
        Button _nfcPairButton;
        Button _nfcUtilsButton;
        Button _bluetoothUtilsButton;

        private static MainActivity _activity;

        public enum ActivityCode { NFCPair = 0, NFCPairMenu, NFCUtils, Bluetooth };
        public static readonly string FRAGMENT_TAG_MAIN_MENU = "MainMenuFragment";
        public static readonly string FRAGMENT_TAG_NFC_PAIR = "TapAndPairFragment";
        public static readonly string FRAGMENT_TAG_NFC_UTILS = "NfcUtilsFragment";
        public static readonly string FRAGMENT_TAG_BLUETOOTH = "BluetoothFragment";


        public static MainMenuFragment NewInstance(Bundle args)
        {
            //args.Put
            var mainMenuFrag = new MainMenuFragment { Arguments = args };
            return mainMenuFrag;
        }

       public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle args)
        {
            //byte[] activtyBytes = args.GetByteArray("MainActivity");

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            _activity = context as MainActivity;
        }
        private void NfcPairButton_OnClick(object sender, EventArgs e)
        {
            TapAndPairFragment tapAndPairFrag = (TapAndPairFragment)FragmentManager.FindFragmentByTag(FRAGMENT_TAG_NFC_PAIR);
            if (tapAndPairFrag == null)
                tapAndPairFrag = TapAndPairFragment.NewInstance((int)ActivityCode.NFCPairMenu);

            FragmentManager.BeginTransaction()
                .Replace(Resource.Id.main_menu_container, tapAndPairFrag, FRAGMENT_TAG_NFC_PAIR)
                .AddToBackStack(FRAGMENT_TAG_NFC_PAIR)
                .Commit();
        }

        private void NfcUtilsButton_OnClick(object sender, EventArgs e)
        {
            NfcUtilsFragment nfcUtilsPairFrag = (NfcUtilsFragment)FragmentManager.FindFragmentByTag(FRAGMENT_TAG_NFC_UTILS);
            if(nfcUtilsPairFrag == null)
                nfcUtilsPairFrag = NfcUtilsFragment.NewInstance(_activity.NfcTag);

            FragmentManager.BeginTransaction()
               .Replace(Resource.Id.main_menu_container, nfcUtilsPairFrag, FRAGMENT_TAG_NFC_UTILS)
               .AddToBackStack(FRAGMENT_TAG_NFC_UTILS)
               .Commit();
        }

        private void BluetoothUtilsButton_OnClick(object sender, EventArgs e)
        {
            BluetoothFragment bluetoothPairFrag = (BluetoothFragment)FragmentManager.FindFragmentByTag(FRAGMENT_TAG_BLUETOOTH);
            Bundle args = new Bundle();
            //args.PutByteArray(this.ToString(), this.ToArray<byte>());

            if (bluetoothPairFrag == null)
                bluetoothPairFrag = BluetoothFragment.NewInstance(args);
            FragmentManager.BeginTransaction()
                .Replace(Resource.Id.main_menu_container, bluetoothPairFrag, FRAGMENT_TAG_BLUETOOTH)
                .AddToBackStack(FRAGMENT_TAG_BLUETOOTH)
                .Commit();
            //var intent = new Intent(Activity, typeof(BluetoothUtilsActivity));
            //StartActivity(intent);
        }
    }
}