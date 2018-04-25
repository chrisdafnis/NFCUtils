using Android.App;
using Android.OS;
using Android.Views;

namespace com.touchstar.chrisd.nfcutils
{
    public class VisibleFragment : Fragment
    {
        private static readonly string TAG = "VisibleFragment";

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override void OnStart()
        {
            base.OnStart();

        }

        public override void OnStop()
        {
            base.OnStop();
        }
    }
}