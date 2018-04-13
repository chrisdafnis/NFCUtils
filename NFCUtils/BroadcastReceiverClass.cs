using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Widget;
using com.touchstar.chrisd.nfcutils;

namespace com.touchstar.chrisd
{
    class BroadcastReceiverClass : BroadcastReceiver
    {
        private readonly Activity outerInstance;

        public BroadcastReceiverClass(Activity outerInstance)
        {
            this.outerInstance = outerInstance;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;
            switch (intent.Action)
            {
                case BluetoothDevice.ActionFound:
                    {
                        // Get device
                        BluetoothDevice newDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        if (outerInstance.GetType() == typeof(TapAndPairActivity))
                            (outerInstance as TapAndPairActivity).FoundDevice(newDevice);
                        else if (outerInstance.GetType() == typeof(BluetoothUtilsActivity))
                            (outerInstance as BluetoothUtilsActivity).FoundDevice(newDevice);
                    }
                    break;
                case BluetoothDevice.ActionBondStateChanged:
                    {
                        int state = intent.GetIntExtra(BluetoothDevice.ExtraBondState, BluetoothDevice.Error);
                        int prevState = intent.GetIntExtra(BluetoothDevice.ExtraPreviousBondState, BluetoothDevice.Error);
                        BluetoothDevice bondDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                        if (state == (int)Bond.Bonded && prevState == (int)Bond.Bonding)
                        {
                            Toast.MakeText(context, "Paired " + bondDevice.Name, ToastLength.Long).Show();
                        }
                        else if (state == (int)Bond.None && prevState == (int)Bond.Bonded)
                        {
                            Toast.MakeText(context, "Unpaired " + bondDevice.Name, ToastLength.Long).Show();
                        }
                    }
                    break;
                case BluetoothDevice.ActionPairingRequest:
                    {
                        BluetoothDevice pairDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        Toast.MakeText(context, "Pairing Request with " + pairDevice.Name, ToastLength.Long).Show();
                    }
                    break;
                case BluetoothAdapter.ActionDiscoveryStarted:
                    {
                        if (outerInstance.GetType() == typeof(BluetoothUtilsActivity))
                            (outerInstance as BluetoothUtilsActivity).BluetoothDiscoveryStarted();
                    }
                    break;
                case BluetoothAdapter.ActionDiscoveryFinished:
                    {
                        if (outerInstance.GetType() == typeof(BluetoothUtilsActivity))
                            (outerInstance as BluetoothUtilsActivity).BluetoothDiscoveryFinished();
                    }
                    break;
            }

            if (outerInstance.GetType() == typeof(TapAndPairActivity))
                (outerInstance as TapAndPairActivity).RefreshList();
            else if (outerInstance.GetType() == typeof(BluetoothUtilsActivity))
                (outerInstance as BluetoothUtilsActivity).RefreshList();
        }
    }
}