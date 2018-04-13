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
    public class NfcUtilsFragment : Fragment
    {
        public event EventHandler ReadTagButtonClicked;
        public event EventHandler WriteTagButtonClicked;

        Button _readTagButton;
        Button _writeTagButton;
        TextView _messageTextView;
        TextView _writeTextView;

        public static NfcUtilsFragment NewInstance()
        {
            var nfcUtilsFrag = new NfcUtilsFragment { Arguments = new Bundle() };
            return nfcUtilsFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }
            return inflater.Inflate(Resource.Layout.nfc_utils_fragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            // get the buttons
            _readTagButton = view.FindViewById<Button>(Resource.Id.read_tag_button);
            _writeTagButton = view.FindViewById<Button>(Resource.Id.write_tag_button);
            _messageTextView = view.FindViewById<TextView>(Resource.Id.text_view);
            _writeTextView = view.FindViewById<TextView>(Resource.Id.text_view);
            // assign the click events
            _readTagButton.Click += ReadTagButton_OnClick;
            _writeTagButton.Click += WriteTagButton_OnClick;

            base.OnViewCreated(view, savedInstanceState);
        }

        private void ReadTagButton_OnClick(object sender, EventArgs e)
        {
            ReadTagButtonClicked(sender, e);
        }

        private void WriteTagButton_OnClick(object sender, EventArgs e)
        {
            WriteTagButtonClicked(sender, e);
        }
    }
}