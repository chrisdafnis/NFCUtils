using Android.App;
using Android.Bluetooth;
using Android.Content;
using static com.touchstar.chrisd.nfcutils.TapAndPairFragment;

namespace com.touchstar.chrisd.nfcutils
{
    public class BluetoothReceiver : BroadcastReceiver
    {
        public static readonly string TAG = "BroadcastReceiverBluetooth";

        private MainActivity _activity;
        private VisibleFragment _fragment;

        public BluetoothReceiver(VisibleFragment fragment)
        {
            _fragment = fragment;
        }

        public BluetoothReceiver(MainActivity activity)
        {
            _activity = activity;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;

            switch (action)
            {
                case BluetoothDevice.ActionFound:
                    {
                        BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        BluetoothClass deviceClass = (BluetoothClass)intent.GetParcelableExtra(BluetoothDevice.ExtraClass);

                        // only interested in devices not already paired
                        if (device.BondState == Bond.None)
                        {
                            if (_fragment != null)
                            {
                                _fragment.OnDeviceFound(device, deviceClass);
                            }
                            else if (_activity != null)
                            {
                                _activity.OnDeviceFound(device, deviceClass);
                            }
                        }
                    }
                    break;

                case BluetoothAdapter.ActionDiscoveryStarted:
                    {
                        if (_fragment != null)
                        {
                            _fragment.OnScanStarted();
                        }
                        else if (_activity != null)
                        {
                            _activity.OnScanStarted();
                        }
                    }
                    break;

                case BluetoothAdapter.ActionDiscoveryFinished:
                    {
                        if (_fragment != null)
                        {
                            _fragment.OnScanComplete();
                        }
                        else if (_activity != null)
                        {
                            _activity.OnScanComplete();
                        }
                    }
                    break;

                case BluetoothAdapter.ActionRequestDiscoverable:
                    {
                        //if (mFragment != null)
                        //    mFragment.OnScanComplete();
                        //((BluetoothUtilsActivity)mActivity).OnScanComplete();
                    }
                    break;

                case BluetoothDevice.ActionPairingRequest:
                    {
                        BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        BluetoothClass deviceClass = (BluetoothClass)intent.GetParcelableExtra(BluetoothDevice.ExtraClass);

                        //_fragment.OnPairDevice(device, (int)Bond.None);
                    }
                    break;

                case BluetoothDevice.ActionBondStateChanged:
                    {
                        Bonding state = (Bonding)intent.GetIntExtra(BluetoothDevice.ExtraBondState, BluetoothDevice.Error);
                        Bonding prevState = (Bonding)intent.GetIntExtra(BluetoothDevice.ExtraPreviousBondState, BluetoothDevice.Error);
                        BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                        // device has successfully paired
                        if (state == Bonding.Bonded && prevState == Bonding.Bonding)
                        {
                            if (_fragment != null)
                            {
                                _fragment.OnPairDevice(device, (int)state);
                            }
                            else if (_activity != null)
                            {
                                _activity.OnPairDevice(device, (int)state);
                            }
                            return;
                        }

                        // device has successfully unpaired
                        if (state == Bonding.None && prevState == Bonding.Bonded)
                        {
                            if (_fragment != null)
                            {
                                _fragment.OnPairDevice(device, (int)state);
                            }
                            else if (_activity != null)
                            {
                                _activity.OnPairDevice(device, (int)state);
                            }
                            return;
                        }

                        // device pairing was cancelled
                        if (state == Bonding.None && prevState == Bonding.Bonding)
                        {
                            if (_fragment != null)
                            {
                                _fragment.OnPairDevice(device, (int)Bonding.Cancelled);
                            }
                            else if (_activity != null)
                            {
                                _activity.OnPairDevice(device, (int)Bonding.Cancelled);
                            }
                            return;
                        }

                        if (state == Bonding.None && prevState == Bonding.None)
                        {
                            if (_fragment != null)
                            {
                                _fragment.OnPairDevice(device, (int)state);
                            }
                            else if (_activity != null)
                            {
                                _activity.OnPairDevice(device, (int)state);
                            }
                            return;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}