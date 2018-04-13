using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace com.touchstar.chrisd.nfcutils
{
    class ViewHolder : Java.Lang.Object
    {
        public TextView Text { get; set; }
        public bool Selected { get; set; }
        public bool Printed { get; set; }
        public object Device { get; set; }
    }

    class ListCustomArrayAdapter : ArrayAdapter
    {
        IList objectList;
        int layoutResourceId, selectedIndex;
        Dictionary<int, bool> printedItems;
        int[] colors = new int[] { Color.LightBlue, Color.White };

        public ListCustomArrayAdapter(Context context, int layout, IList objects) : base(context, layout, objects)
        {
            objectList = objects;
            layoutResourceId = layout;
            printedItems = new Dictionary<int, bool>();
        }

        public override int Count => base.Count;

        public int GetSelectedIndex() => selectedIndex;

        public void SetSelectedIndex(int index)
        {
            selectedIndex = index;
            NotifyDataSetChanged();
        }

        public void SetPrintedIndex(int index)
        {
            try
            {
                if (printedItems.ContainsKey(index))
                {
                    printedItems.Remove(index);
                }

                printedItems.Add(index, true);
                NotifyDataSetChanged();
            }
            catch (Exception ex)
            {
            }
        }

        public override int GetItemViewType(int position) => base.GetItemViewType(position);

        public override Java.Lang.Object GetItem(int position) => base.GetItem(position);

        public override long GetItemId(int position) => base.GetItemId(position);

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            convertView = ((LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService)).Inflate(Resource.Layout.list_row, null);
            var inflater = Application.Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;

            var colorPos = position % colors.Length;
            var color = new Color(colors[colorPos]);
            convertView.SetBackgroundColor(color);

            var text = convertView.FindViewById<TextView>(Resource.Id.Name);
            text.Text = ((BluetoothDevice)objectList[position]).Name + ", " + ((BluetoothDevice)objectList[position]).Address;
            convertView.Tag = ((BluetoothDevice)objectList[position]);
            return convertView;
        }

        public void HighlightCurrentRow(View rowView)
        {
            rowView.SetBackgroundColor(Color.DarkGray);
            var textView = (TextView)rowView.FindViewById(Resource.Id.Name);
            if (textView != null)
                textView.SetTextColor(Color.Yellow);
        }

        public void UnhighlightCurrentRow(View rowView)
        {
            rowView.SetBackgroundColor(Color.Transparent);
            var textView = (TextView)rowView.FindViewById(Resource.Id.Name);
            if (textView != null)
                textView.SetTextColor(Color.Black);
        }

        public void HighlightPrintedRow(View rowView, int position)
        {
            if (printedItems.ContainsKey(position))
            {
                rowView.SetBackgroundColor(Color.ParseColor("#0A64A2"));
                var textView = (TextView)rowView.FindViewById(Resource.Id.Name);
                if (textView != null)
                    textView.SetTextColor(Color.White);
            }
        }
    }

    public class ListAlternateRowAdapter : ArrayAdapter
    {
        int[] colors = new int[] { Color.LightBlue, Color.White };
        IList objectList;

        public ListAlternateRowAdapter(Context context, int layout, System.Collections.IList objects) : base(context, layout, objects)
        {
            objectList = objects;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder holder = null;
            convertView = ((LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService)).Inflate(Resource.Layout.list_row, null);
            holder = new ViewHolder();
            var inflater = Application.Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;

            var colorPos = position % colors.Length;
            var color = new Color(colors[colorPos]);
            convertView.SetBackgroundColor(color);

            var textView = convertView.FindViewById<TextView>(Resource.Id.Name);
            textView.Text = ((BluetoothDevice)objectList[position]).Name + ", " + ((BluetoothDevice)objectList[position]).Address;
            convertView.Tag = ((BluetoothDevice)objectList[position]);
            if (textView != null)
                textView.SetTextColor(Color.Black);

            return convertView;
        }
    }

    class CheckListCustomArrayAdapter : ArrayAdapter
    {
        IList objectList;
        int layoutResourceId, selectedIndex;
        Dictionary<int, bool> printedItems;
        int[] colors = new int[] { Color.LightBlue, Color.White };
        bool[] checkBoxState;

        public CheckListCustomArrayAdapter(Context context, int layout, IList objects) : base(context, layout, objects)
        {
            objectList = objects;
            layoutResourceId = layout;
            printedItems = new Dictionary<int, bool>();
            checkBoxState = new bool[objectList.Count];
        }

        public override int Count => base.Count;

        public int GetSelectedIndex() => selectedIndex;

        public void SetSelectedIndex(int index)
        {
            selectedIndex = index;
            NotifyDataSetChanged();
        }

        public bool GetChecked(View view, int index)
        {
            var simpleCheckedTextView = view as CheckedTextView;
            var checkedValue = simpleCheckedTextView.Checked;
            return checkBoxState[index];
        }

        public void SetPrinted(View view, int position)
        {
        }

        public void SetChecked(int index, bool check)
        {
            checkBoxState[index] = check;
        }

        internal void SetRowPrinted(CheckedTextView view, int pos)
        {
            view.SetBackgroundColor(Color.ParseColor("#0A64A2"));
            view.SetTextColor(Color.White);
            view.Invalidate();
        }

        public override int GetItemViewType(int position) => base.GetItemViewType(position);

        public override Java.Lang.Object GetItem(int position) => base.GetItem(position);

        public override long GetItemId(int position) => base.GetItemId(position);

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView as CheckedTextView;
            if (view == null)
            {
                view = ((LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService)).Inflate(global::Android.Resource.Layout.SimpleListItemChecked, null) as CheckedTextView;
                view.Click += delegate
                {
                    OnViewClick(view, position);
                };
            }

            view.SetTextColor(Color.Black);

            var inflater = Application.Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;

            var colorPos = position % colors.Length;
            var color = new Color(colors[colorPos]);
            view.SetBackgroundColor(color);

            return view;
        }

        void OnViewClick(object sender, int position)
        {
            var view = sender as CheckedTextView;
            view.Checked = !view.Checked;

            for (int i = 0; i < objectList.Count; i++)
            {
            }
        }
    }
    
    class CheckListAlternateRowAdapter : ArrayAdapter
    {
        int[] colors = new int[] { Color.LightBlue, Color.White };
        IList objectList;

        public CheckListAlternateRowAdapter(Context context, int layout, System.Collections.IList objects) : base(context, layout, objects)
        {
            objectList = objects;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder holder = null;
            convertView = ((LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService)).Inflate(Android.Resource.Layout.SimpleListItemChecked, null);
            holder = new ViewHolder();
            var inflater = Application.Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;

            var colorPos = position % colors.Length;
            var color = new Color(colors[colorPos]);
            convertView.SetBackgroundColor(color);

            var checkTextView = convertView as CheckedTextView;
            checkTextView.Text = ((BluetoothDevice)objectList[position]).Name + ", " + ((BluetoothDevice)objectList[position]).Address;
            convertView.Tag = ((BluetoothDevice)objectList[position]);
            checkTextView.Click += (sender, args) =>
            {
                var pos = ((View)sender).Tag;
            };

            if (checkTextView != null)
                checkTextView.SetTextColor(Color.Black);

            return convertView;
        }
    }
}