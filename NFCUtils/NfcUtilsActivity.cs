using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.Util;

namespace com.touchstar.chrisd.nfcutils
{
    [Activity(Label = "NFC Utilities")]
    public class NfcUtilsActivity : Activity
    {
        private bool _inReadMode;
        private bool _inWriteMode;
        private TextView _messageTextView;
        private TextView _writeTextView;
        Button _readTagButton;
        Button _writeTagButton;
        private NfcAdapter _nfcAdapter;
        protected string _tagUid;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.nfc_utils_fragment);

            // get the buttons
            _readTagButton = FindViewById<Button>(Resource.Id.read_tag_button);
            _writeTagButton = FindViewById<Button>(Resource.Id.write_tag_button);
            // assign the click events
            _readTagButton.Click += ReadTagButton_OnClick;
            _writeTagButton.Click += WriteTagButton_OnClick;
            _messageTextView = FindViewById<TextView>(Resource.Id.text_view);
            _writeTextView = FindViewById<TextView>(Resource.Id.write_text);

            _nfcAdapter = NfcAdapter.GetDefaultAdapter(this);

            //var fragmentTransaction = FragmentManager.BeginTransaction();
            //fragmentTransaction.Replace(Android.Resource.Id.Content, nfcUtils);
            //fragmentTransaction.Commit();
        }

        #region NFC

        #region Read specific functions
        /// <summary>
        /// Identify to Android that this activity wants to be notified when 
        /// an NFC tag is discovered. 
        /// </summary>
        private void EnableReadMode()
        {
            _inReadMode = true;

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
        private void ReadTagButtonOnClick(object sender, EventArgs eventArgs)
        {
            var view = (View)sender;
            if (view.Id == Resource.Id.read_tag_button)
            {
                DisplayMessage("Touch and hold the tag against the phone to read.");
                EnableReadMode();
            }
        }
        #endregion // Read specific functions

        #region Write Specific functions
        /// <summary>
        /// Identify to Android that this activity wants to be notified when 
        /// an NFC tag is discovered. 
        /// </summary>
        private void EnableWriteMode()
        {
            _inWriteMode = true;

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
                    DisplayMessage("NFC is not supported on this device.");
                });
                alert.Show();
            }
            else
                _nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="ndefMessage"></param>
        /// <returns></returns>
        private bool TryAndFormatTagWithMessage(Tag tag, NdefMessage ndefMessage)
        {
            var format = NdefFormatable.Get(tag);
            if (format == null)
            {
                DisplayMessage("Tag does not appear to support NDEF format.");
            }
            else
            {
                try
                {
                    format.Connect();
                    format.Format(ndefMessage);
                    DisplayMessage("Tag successfully written.");
                    return true;
                }
                catch (Java.IO.IOException ioex)
                {
                    var msg = "There was an error trying to format the tag.";
                    DisplayMessage(msg);
                    Log.Error(this.Application.PackageName, ioex.Message, msg);
                }
            }
            // clear the input text
            _writeTextView.Text = String.Empty;
            return false;
        }

        private void WriteTagButtonOnClick(object sender, EventArgs eventArgs)
        {
            var view = (View)sender;
            if (view.Id == Resource.Id.write_tag_button)
            {
                DisplayMessage("Touch and hold the tag against the phone to write.");
                EnableWriteMode();
            }
        }

        /// <summary>
        /// This method will try and write the specified message to the provided tag. 
        /// </summary>
        /// <param name="tag">The NFC tag that was detected.</param>
        /// <param name="ndefMessage">An NDEF message to write.</param>
        /// <returns>true if the tag was written to.</returns>
        private bool TryAndWriteToTag(Tag tag, NdefMessage ndefMessage)
        {

            // This object is used to get information about the NFC tag as 
            // well as perform operations on it.
            var ndef = Ndef.Get(tag);
            if (ndef != null)
            {
                ndef.Connect();

                // Once written to, a tag can be marked as read-only - check for this.
                if (!ndef.IsWritable)
                {
                    DisplayMessage("Tag is read-only.");
                }

                // NFC tags can only store a small amount of data, this depends on the type of tag its.
                var size = ndefMessage.ToByteArray().Length;
                if (ndef.MaxSize < size)
                {
                    DisplayMessage("Tag doesn't have enough space.");
                }

                ndef.WriteNdefMessage(ndefMessage);
                DisplayMessage("Succesfully wrote tag.");
                return true;
            }

            return false;
        }
        #endregion // Write Specific functions

        #endregion // NFC

        /// <summary>
        /// This method is called when an NFC tag is discovered by the application.
        /// </summary>
        /// <param name="intent"></param>
        protected override void OnNewIntent(Intent intent)
        {
            if (_inReadMode)
            {
                _inReadMode = false;
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

                //added the below lines, can delete if they don't work.
                //NdefRecord payload = ((NdefMessage)rawMsgs[0]).GetRecords()[0];
                //byte[] currentPayloadBytes = payload.GetPayload();

                //String currentPayloadString = System.Text.Encoding.UTF8.GetString(currentPayloadBytes, 0, currentPayloadBytes.Length);
                var tagId = tag.GetId();
                _tagUid = ByteArrayToString(tagId);
                Log.Info(this.Application.PackageName, "Card UID is " + _tagUid);

                //DisplayMessage("Card UID is " + _tagUid + newLine + "Payload is " + "'" + currentPayloadString + "'");
            }
            else if (_inWriteMode)
            {
                _inWriteMode = false;
                var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;

                if (tag == null)
                {
                    return;
                }

                // These next few lines will create a payload (consisting of a string)
                // and a mimetype. NFC record are arrays of bytes. 
                var payload = Encoding.ASCII.GetBytes(_writeTextView.Text);
                var nfcRecord = new NdefRecord(NdefRecord.TnfWellKnown, new byte[0], new byte[0], payload);
                var ndefMessage = new NdefMessage(new[] { nfcRecord });

                if (!TryAndWriteToTag(tag, ndefMessage))
                {
                    // Maybe the write couldn't happen because the tag wasn't formatted?
                    TryAndFormatTagWithMessage(tag, ndefMessage);
                }
            }
        }

        protected void HandleNFC(Intent intent, Boolean inForeground)
        {
            NdefMessage[] msgs = null;
            IParcelable[] rawMsgs = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);

            if (rawMsgs != null)
            {
                msgs = new NdefMessage[rawMsgs.Length];
                for (int i = 0; i < rawMsgs.Length; i++)
                {
                    msgs[i] = (NdefMessage)rawMsgs[i];
                }

                if (msgs != null)
                {
                    for (int i = 0; i < msgs.Length; i++)
                    {
                        NdefRecord[] records = msgs[i].GetRecords();
                        if (records != null)
                        {
                            for (int j = 0; j < records.Length; j++)
                            {
                                Log.Info("MainActivity", "Record found");
                                if (NdefRecord.TnfWellKnown == records[j].Tnf)
                                {
                                    // If we're a URI type, parse it.
                                    String uri = ParseUriRecord(records[j]);
                                    DisplayMessage(uri);
                                }
                                else if (NdefRecord.TnfMimeMedia == records[j].Tnf)
                                {
                                    // bluetooth mime type

                                    //String uri = ParseUriRecord(records[j]);
                                    //String mime = records[j].ToMimeType();
                                    //NdefRecord mimeRec = NdefRecord.CreateMime(mime, records[j].GetPayload());
                                    //char[] payload = Encoding.ASCII.GetChars(records[j].GetPayload());
                                    //String str = records[j].ToString();
                                    //DisplayMessage(uri + mime + str);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                DisplayMessage("No message");
            }
        }

        private string ParseUriRecord(NdefRecord ndefRecord)
        {
            byte[] payload = ndefRecord.GetPayload();

            // Get the Language Code
            int languageCodeLength = payload[0] & 0063;

            return new String(System.Text.UTF8Encoding.ASCII.GetChars(payload), languageCodeLength + 1, payload.Length - languageCodeLength - 1);
        }

        //Convert the byte array of the NfcCard Uid to string
        private static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        protected override void OnPause()
        {
            base.OnPause();
            // App is paused, so no need to keep an eye out for NFC tags.
            if (_nfcAdapter != null)
                _nfcAdapter.DisableForegroundDispatch(this);
        }

        private void DisplayMessage(string message)
        {
            _messageTextView.Text = message;
            Log.Info(this.Application.PackageName, message);
        }

        private void ReadTagButton_OnClick(object sender, EventArgs e)
        {
            var view = (View)sender;
            if (view.Id == Resource.Id.read_tag_button)
            {
                DisplayMessage("Touch and hold the tag against the device to read.");
                EnableReadMode();
            }
        }

        private void WriteTagButton_OnClick(object sender, EventArgs e)
        {
            var view = (View)sender;
            if (view.Id == Resource.Id.write_tag_button)
            {
                DisplayMessage("Touch and hold the tag against the device to write.");
                EnableWriteMode();
            }
        }

    }
}