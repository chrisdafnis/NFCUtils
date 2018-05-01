using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace com.touchstar.chrisd.nfcutils
{
    [Register("com.touchstar.chrisd.nfcutils.NfcUtilsFragment")]
    public class NfcUtilsFragment : VisibleFragment
    {
        public event EventHandler ReadTagButtonClicked;
        public event EventHandler WriteTagButtonClicked;

        Button _readTagButton;
        Button _writeTagButton;
        TextView _messageTextView;
        TextView _writeTextView;
        private bool _inReadMode;
        private bool _inWriteMode;
        protected string _tagUid;
        private View _view;
        private static MainActivity _activity;
        private static String _nfcTag;

        public static readonly string FRAGMENT_TAG_MAIN_MENU = "MainMenuFragment";
        public static readonly string FRAGMENT_TAG_NFC_PAIR = "TapAndPairFragment";
        public static readonly string FRAGMENT_TAG_NFC_UTILS = "NfcUtilsFragment";
        public static readonly string FRAGMENT_TAG_BLUETOOTH = "BluetoothFragment";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static NfcUtilsFragment NewInstance(string nfcTag)
        {
            var nfcUtilsFrag = new NfcUtilsFragment { Arguments = new Bundle() };
            _nfcTag = nfcTag;
            return nfcUtilsFrag;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inflater"></param>
        /// <param name="container"></param>
        /// <param name="savedInstanceState"></param>
        /// <returns></returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }
            return inflater.Inflate(Resource.Layout.nfc_utils_fragment, container, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="savedInstanceState"></param>
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            _view = view;
            // get the buttons
            _readTagButton = view.FindViewById<Button>(Resource.Id.read_tag_button);
            _writeTagButton = view.FindViewById<Button>(Resource.Id.write_tag_button);
            _messageTextView = view.FindViewById<TextView>(Resource.Id.text_view);
            _writeTextView = view.FindViewById<TextView>(Resource.Id.write_text);
            // assign the click events
            _readTagButton.Click += ReadTagButton_OnClick;
            _writeTagButton.Click += WriteTagButton_OnClick;

            if (_nfcTag != null)
            {
                _writeTextView.Text = _nfcTag;
            }

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void DisplayMessage(string message)
        {
            _messageTextView.Text = message;
        }

        #region NFC
        #region Read specific functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ReadTagButton_OnClick(object sender, EventArgs eventArgs)
        {
            var view = (View)sender;
            if (view.Id == Resource.Id.read_tag_button)
            {
                DisplayMessage("Touch and hold the tag against the phone to read.");
                _writeTextView.Text = String.Empty;
                _activity.EnableReadMode();
                _inReadMode = true;
            }
        }
        #endregion // Read specific functions
        #region Write Specific functions
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
                    Log.Error(this.GetType().ToString(), ioex.Message, msg);
                }
            }
            // clear the input text
            _writeTextView.Text = String.Empty;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void WriteTagButton_OnClick(object sender, EventArgs eventArgs)
        {
            var view = (View)sender;
            if (view.Id == Resource.Id.write_tag_button)
            {
                DisplayMessage("Touch and hold the tag against the phone to write.");
                _activity.EnableWriteMode();
                _inWriteMode = true;
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
            bool returnVal = false;
            var ndef = Ndef.Get(tag);
            if (ndef != null)
            {
                bool connected = ndef.IsConnected;
                try
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
                    ndef.Close();
                    DisplayMessage("Succesfully wrote tag.");
                    returnVal = true;
                }
                catch
                {
                    DisplayMessage("Write failed. Was the Tag removed too soon?");
                }
             }
            return returnVal;
        }
        #endregion // Write Specific functions
        #endregion // NFC
        /// <summary>
        /// This method is called when an NFC tag is discovered by the application.
        /// </summary>
        /// <param name="intent"></param>
        public void OnNewIntent(Intent intent)
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

                var tagId = tag.GetId();
                _tagUid = ByteArrayToString(tagId);
                Log.Info(this.GetType().ToString(), "Card UID is " + _tagUid);
            }
            else if (_inWriteMode)
            {
                _inWriteMode = false;

                var obj = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
                CultureInfo info = CultureInfo.CurrentCulture;
                var payload = _writeTextView.Text;
                var nfcRecord = NdefRecord.CreateTextRecord(info.TwoLetterISOLanguageName, payload);
                var ndefMessage = new NdefMessage(new[] { nfcRecord });

                if (!TryAndWriteToTag(obj, ndefMessage))
                {
                    // Maybe the write couldn't happen because the tag wasn't formatted?
                    TryAndFormatTagWithMessage(obj, ndefMessage);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="intent"></param>
        /// <param name="inForeground"></param>
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
                                    try
                                    {
                                        String message = ParseNdefRecord(records[j]);
                                        DisplayMessage(message);
                                    }
                                    catch
                                    {
                                        DisplayMessage("Error reading Tag.");
                                    }
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ndefRecord"></param>
        /// <returns></returns>
        private string ParseNdefRecord(NdefRecord ndefRecord)
        {
            byte[] payload = ndefRecord.GetPayload();

            // Get the Language Code
            int languageCodeLength = payload[0] & 0063;
            char[] payloadChars = UTF8Encoding.ASCII.GetChars(payload);
            var payloadValue = new String(payloadChars, languageCodeLength + 1, payload.Length - languageCodeLength - 1);

            return payloadValue;
        }
        /// <summary>
        /// Convert the byte array of the NfcCard Uid to string
        /// </summary>
        /// <param name="ba"></param>
        /// <returns></returns>
        private static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}