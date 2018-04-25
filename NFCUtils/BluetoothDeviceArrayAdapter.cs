using System;
using System.Collections;
using Android.Bluetooth;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using static Android.Widget.AdapterView;

namespace com.touchstar.chrisd.nfcutils
{
    public class BluetoothDeviceArrayAdapter : ArrayAdapter
    {
        private Context mContext;
        private IList deviceList;
        private ViewGroup parentViewGroup;
        private View rowView;
        public int SelectedIndex { get; set; }

        public BluetoothDeviceArrayAdapter(Context context, int layout, IList objects) : base(context, layout, objects)
        {
            mContext = context;
            deviceList = objects;
            SelectedIndex = -1;
        }

        public override int Count => base.Count;

        public int GetSelectedIndex() => SelectedIndex;

        public void SetSelectedIndex(int index)
        {
            UnhighlightAllRows();
            if (index != SelectedIndex)
            {
                SelectedIndex = index;
                HighlightCurrentRow();
            }
            //else
            //{
            //    UnhighlightCurrentRow(rowView);
            //}
            
        }

        public override int GetItemViewType(int position) => base.GetItemViewType(position);

        public override Java.Lang.Object GetItem(int position) => base.GetItem(position);

        public override long GetItemId(int position) => base.GetItemId(position);

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LayoutInflater inflater = (LayoutInflater)mContext.GetSystemService(Context.LayoutInflaterService);
            rowView = inflater.Inflate(Resource.Layout.row_layout, parent, false);
            parentViewGroup = parent;
            TextView tvDeviceName = rowView.FindViewById<TextView>(Resource.Id.tvDeviceName);
            TextView tvDeviceAddress = rowView.FindViewById<TextView>(Resource.Id.tvDeviceAddress);

            try
            {
                tvDeviceName.Text = ((BluetoothDevice)deviceList[position]).Name;
                tvDeviceAddress.Text = ((BluetoothDevice)deviceList[position]).Address;
            }
            catch
            {

            }

            //if (SelectedIndex != -1 && position == SelectedIndex)
            //{
            //    HighlightCurrentRow(rowView);
            //}
            //else
            //{
            //    UnhighlightCurrentRow(rowView);
            //}
            return rowView;
        }

        private void UnhighlightAllRows()
        {
            for (int i = 0; i < parentViewGroup.ChildCount; i++)
            {
                UnhighlightCurrentRow(parentViewGroup.GetChildAt(i));
            }
        }

        private void UnhighlightCurrentRow(View rowView)
        {
            if (rowView.GetType() == typeof(Android.Widget.RelativeLayout))
            {
                ViewGroup vg = (ViewGroup)rowView;
                for (int i = 0; i < vg.ChildCount; i++)
                {
                    View child = vg.GetChildAt(i);
                    child.SetBackgroundColor(Color.WhiteSmoke);
                }
            }
            rowView.SetBackgroundColor(Color.WhiteSmoke);
        }

        private void HighlightCurrentRow()
        {
            View rowView = parentViewGroup.GetChildAt(SelectedIndex);
            if (rowView.GetType() == typeof(Android.Widget.RelativeLayout))
            {
                ViewGroup vg = (ViewGroup)rowView;
                for (int i = 0; i < vg.ChildCount; i++)
                {
                    View child = vg.GetChildAt(i);
                    child.SetBackgroundColor(Color.LightBlue);
                }
            }
            rowView.SetBackgroundColor(Color.LightBlue);
        }

        //public override View GetView(int position, View convertView, ViewGroup parent)
        //{
        //    View listItem = convertView;
        //    if (listItem == null)
        //        listItem = LayoutInflater.From(mContext).Inflate(Resource.Layout.row_layout, parent, false) as ListView;

        //    parentView = parent;
        //    try
        //    {
        //        BluetoothDevice device = deviceList[position] as BluetoothDevice;

        //        TextView tvDeviceName = listItem.FindViewById<TextView>(Resource.Id.tvDeviceName);
        //        tvDeviceName.Text = device.Name;
        //        TextView tvDeviceAddress = listItem.FindViewById<TextView>(Resource.Id.tvDeviceAddress);
        //        tvDeviceAddress.Text = device.Address;
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    //if (listItem != null)
        //    //{
        //    //    if (parentView.GetChildAt(SelectedIndex) == listItem)
        //    //    {
        //    //        //parentView.GetChildAt(SelectedIndex).SetBackgroundColor(Color.LightBlue);
        //    //        listItem.SetBackgroundColor(Color.LightBlue);
        //    //    }
        //    //    else
        //    //    {
        //    //        listItem.SetBackgroundColor(Color.WhiteSmoke);
        //    //    }
        //    //}

        //    //    for (int i = 0; i < parentView.ChildCount; i++)
        //    //    {
        //    //        View child = parentView.GetChildAt(i);
        //    //        if (child == listItem)
        //    //        {
        //    //            SetSelectedIndex(i);
        //    //        }

        //    //        //if (SelectedIndex != -1 && position == SelectedIndex)
        //    //        //{
        //    //        //    child.SetBackgroundColor(Color.LightBlue);
        //    //        //}
        //    //        //else
        //    //        //{
        //    //        //    child.SetBackgroundColor(Color.WhiteSmoke);
        //    //        //}
        //    //    }
        //    //    //listItem.Click -= ChangeBackgroundColor;
        //    //    //listItem.Click += ChangeBackgroundColor;
        //    //}

        //    return listItem;
        //}

        public void ChangeBackgroundColor(object sender, ItemClickEventArgs e)
        {
            // deselect and reset background color on all items
            //for (int i = 0; i < parentView.ChildCount; i++)
            //{
            //    View child = parentView.GetChildAt(i);
            //    child.SetBackgroundColor(Color.WhiteSmoke);
            //}
            ListView devices = sender as ListView;
            View child = devices.GetChildAt(e.Position);
            //        
            // select / unselect the correct row
            if (child.Selected == false)
            {
                child.SetBackgroundColor(Color.LightBlue);
                child.Selected = true;
                devices.GetChildAt(e.Position).SetBackgroundColor(Color.LightBlue);
                devices.GetChildAt(e.Position).Selected = true;
                SetSelectedIndex(e.Position);
            }
            else
            {
                child.SetBackgroundColor(Color.WhiteSmoke);
                child.Selected = false;
                devices.GetChildAt(e.Position).SetBackgroundColor(Color.WhiteSmoke);
                devices.GetChildAt(e.Position).Selected = false;
                SetSelectedIndex(-1);
            }
        }
    }
}