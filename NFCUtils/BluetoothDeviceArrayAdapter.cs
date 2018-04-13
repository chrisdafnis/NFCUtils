using System.Collections.Generic;
using Android.Bluetooth;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace com.touchstar.chrisd.nfcutils
{
    public class BluetoothDeviceArrayAdapter : ArrayAdapter<BluetoothDevice>
    {
        private readonly Context context;
        private readonly List<BluetoothDevice> values;

        public BluetoothDeviceArrayAdapter(Context context, List<BluetoothDevice> values) : base(context, Resource.Layout.row_layout, values)
        {
            this.context = context;
            this.values = values;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            View rowView = inflater.Inflate(Resource.Layout.row_layout, parent, false);

            try
            {
                TextView tvDeviceName = rowView.FindViewById<TextView>(Resource.Id.tvDeviceName);
                tvDeviceName.Text = values[position].Name;
                TextView tvDeviceAddress = rowView.FindViewById<TextView>(Resource.Id.tvDeviceAddress);
                tvDeviceAddress.Text = values[position].Address;
            }
            catch
            {

            }
            return rowView;
        }
    }
}