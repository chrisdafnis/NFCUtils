using System;
using System.Collections.Generic;
using System.Threading;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Nfc;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang.Reflect;
using Zebra.Android.Discovery;

namespace com.touchstar.chrisd
{
    public class TapAndPairFragment : Fragment
    {
        private ListView _listview;

        // event handlers
        public event EventHandler DeviceListItemClicked;

        public static TapAndPairFragment NewInstance()
        {
            var tapAndPairFrag = new TapAndPairFragment { Arguments = new Bundle() };
            return tapAndPairFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }
            return inflater.Inflate(Resource.Layout.tap_and_pair_fragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            // get the controls
            _listview = view.FindViewById<ListView>(Resource.Id.lvPairedDevices);

            // assign the click events
            _listview.ItemClick += ListViewItem_Click;

            base.OnViewCreated(view, savedInstanceState);
        }

        private void ListViewItem_Click(object sender, AdapterView.ItemClickEventArgs e)
        {
            var view = (View)sender;
            if (view.Id == Resource.Id.lvPairedDevices)
            {
                DeviceListItemClicked(sender, e);
            }
        }

        public override void OnResume()
        {
            base.OnResume();
        }
    }
}