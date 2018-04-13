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

namespace com.touchstar.chrisd
{
    public class NfcPairFragment : Fragment
    {
        public event EventHandler PairButtonClicked;

        Button _pairButton;
        TextView _pairDeviceTextView;

        public static NfcPairFragment NewInstance()
        {
            var nfcPairFrag = new NfcPairFragment { Arguments = new Bundle() };
            return nfcPairFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }
            return inflater.Inflate(Resource.Layout.nfc_pair_fragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            // get the buttons
            _pairButton = view.FindViewById<Button>(Resource.Id.pair_button);
            // assign the click events
            _pairButton.Click += PairButton_OnClick;

            // get the text view
            _pairDeviceTextView = view.FindViewById<TextView>(Resource.Id.pair_device_text_view);

            base.OnViewCreated(view, savedInstanceState);
        }

        private void PairButton_OnClick(object sender, EventArgs e)
        {
            PairButtonClicked(sender, e);
        }
    }
}