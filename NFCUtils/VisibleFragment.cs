using Android.App;
using Android.Bluetooth;
using Android.OS;
using Android.Views;

namespace com.touchstar.chrisd.nfcutils
{
    public class VisibleFragment : Fragment
    {
        private static readonly string TAG = "VisibleFragment";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedInstanceState"></param>
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
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
           return base.OnCreateView(inflater, container, savedInstanceState);
        }
        /// <summary>
        /// 
        /// </summary>
        public override void OnStart()
        {
            base.OnStart();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void OnStop()
        {
            base.OnStop();
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void OnScanStarted()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void OnScanComplete()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public virtual void OnDeviceFound(object a, object b)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="state"></param>
        public virtual void OnPairDevice(object device, int state)
        {
        }
    }
}