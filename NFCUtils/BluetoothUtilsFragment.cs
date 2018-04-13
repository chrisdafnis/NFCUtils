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
    public class BluetoothUtilsFragment : Fragment
    {
        public event EventHandler SearchButtonClicked;
        public event EventHandler DeviceListItemClicked;

        Button _searchButton;
        ListView _deviceListView;

        public static BluetoothUtilsFragment NewInstance()
        {
            var bluetoothUtilsFrag = new BluetoothUtilsFragment { Arguments = new Bundle() };
            return bluetoothUtilsFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }
            return inflater.Inflate(Resource.Layout.bluetooth_utils_fragment, container, false); ;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            // get the buttons
            _searchButton = view.FindViewById<Button>(Resource.Id.search_button);
            _deviceListView = view.FindViewById<ListView>(Resource.Id.device_list);
            // assign the click events
            _searchButton.Click += SearchButton_OnClick;
            _deviceListView.ItemClick += DeviceList_ItemClick;


            base.OnViewCreated(view, savedInstanceState);
        }

        private void SearchButton_OnClick(object sender, EventArgs e)
        {
            SearchButtonClicked(sender, e);
        }

        private void DeviceList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            DeviceListItemClicked(sender, e);
        }

        public void SetDeviceListAdapter(BluetoothDeviceArrayAdapter adapter)
        {
            _deviceListView.Adapter = adapter;
        }
    }
}