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
    [Activity(Label = "NFC Utilities")]
    public class MainMenuActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.main_menu_fragment);

            var menu = MainMenuFragment.NewInstance();
            menu.NfcPairButtonClicked += NfcPairButton_OnClick;
            menu.NfcUtilsButtonClicked += NfcUtilsButton_OnClick;
            menu.BluetoothUtilsButtonClicked += BluetoothUtilsButton_OnClick;

            var fragmentTransaction = FragmentManager.BeginTransaction();
            fragmentTransaction.Add(Android.Resource.Id.Content, menu);
            fragmentTransaction.Commit();
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