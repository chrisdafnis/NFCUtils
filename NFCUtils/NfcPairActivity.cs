using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace com.touchstar.chrisd
{
    [Activity(Label = "NFC Pair")]
    public class NfcPairActivity : Activity
    {
        private NfcAdapter _nfcAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.nfc_pair_fragment);

            var nfcPair = NfcPairFragment.NewInstance();
            nfcPair.PairButtonClicked += PairButton_OnClick;
            _nfcAdapter = NfcAdapter.GetDefaultAdapter(this);

            var fragmentTransaction = FragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Android.Resource.Id.Content, nfcPair);
            fragmentTransaction.Commit();
        }

        private void PairButton_OnClick(object sender, EventArgs e)
        {
            var view = (View)sender;
            if (view.Id == Resource.Id.pair_button)
            {
                DisplayMessage("Touch and hold the tag against the device to read.");
                EnableReadMode();
            }
        }
        private void DisplayMessage(string message)
        {
            //_messageTextView.Text = message;
            //Log.Info(this.Application.PackageName, message);
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

            if (_nfcAdapter == null)
            {
                var alert = new AlertDialog.Builder(this).Create();
                alert.SetMessage("NFC is not supported on this device.");
                alert.SetTitle("NFC Unavailable");
                alert.SetButton("OK", delegate
                {
                    //_readTagButton.Enabled = false;
                    DisplayMessage("NFC is not supported on this device.");
                });
                alert.Show();
            }
            else
                _nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
        }

        protected override void OnNewIntent(Intent intent)
        {
            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
            var rawMsgs = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);

            if (tag == null)
            {
                return;
            }

            if (NfcAdapter.ExtraTag.Contains("nfc"))
            {
                HandleNFC(intent, true);
            }
        }

        private static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        protected void HandleNFC(Intent intent, Boolean inForeground)
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
            }
            else
            {
                DisplayMessage("No message");
            }
        }
    }
}