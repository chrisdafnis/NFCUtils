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
    public class MainFragment : ListFragment
    {
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            
            var intent = new Intent();
            intent.SetClass(Activity, typeof(MainMenuActivity));
            StartActivity(intent);
        }
    }
}