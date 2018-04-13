using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Lang.Reflect;
using Java.Util;

namespace com.touchstar.chrisd.nfcutils
{
    public class Bluetooth
    {
        private static string TAG = "Bluetooth";

        private static UUID mUUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

        BluetoothAdapter mAdapter;

        BluetoothDevice mDevice;

        BluetoothSocket mSocket;

        Stream mInputStream;

        Stream mOutputStream;

        bool mCreateSocketUsingReflection;

        public Bluetooth() : this(false)
        {
            
        }
        public Bluetooth(bool createSocketUsingReflection)
        {
            mCreateSocketUsingReflection = createSocketUsingReflection;
            Log.Debug(TAG, "Constructor");
            Adapter = BluetoothAdapter.DefaultAdapter;
            TurnBluetoothOn();
        }

        public void TurnBluetoothOn()
        {
            if (!Adapter.IsEnabled)
            {
                Log.Debug(TAG, "Enabling Bluetooth");

                Adapter.Enable();
            }
        }

        public ParcelUuid[] GetAdapterUUID()
        {
            return Device.GetUuids();
        }

        public bool OpenSocket()
        {

            //ParcelUuid[] Uid = GetAdapterUUID();
            //UUID uidTemp = Uid[0].Uuid;
            //uidTemp = Uid[1].Uuid;

            Log.Debug(TAG, "Opening Socket");

            if (!mCreateSocketUsingReflection)
            {
                Socket = Device.CreateInsecureRfcommSocketToServiceRecord(mUUID);
            }
            else
            {
                // this is not the recommended way to do this but for the LPT printer dongle it is the only function which works
                Method m = Device.Class.GetMethod("createRfcommSocket", new Java.Lang.Class[] { Integer.Type });
                Socket = (BluetoothSocket)m.Invoke(Device, 1);
            }
            if (Socket == null)
            {
                Log.Debug(TAG, "Opening Socket - Failed");
                return false;
            }

            Log.Debug(TAG, "Opening Socket - Success");
            return true;
        }
        public bool CloseSocket()
        {
            try
            {
                if (mInputStream != null)
                {
                    Log.Debug(TAG, "Closing Input Streamt");
                    mInputStream.Close();
                    //mInputStream.Dispose();
                    mInputStream = null;
                }
            }
            catch { }

            try
            {
                if (mOutputStream != null)
                {
                    Log.Debug(TAG, "Closing Output Stream");
                    mOutputStream.Close();
                    //mOutputStream.Dispose();
                    mOutputStream = null;
                }
            }
            catch { }


            try
            {
                if (Socket != null)
                {
                    Log.Debug(TAG, "Closing Socket");
                    Socket.Close();
                    Socket = null;
                }

            }
            catch { }

            return true;
        }
        public bool Connect()
        {
            try
            {
                //Note: You should always call cancelDiscovery() to ensure that the device is not performing device discovery before you call connect().
                //If discovery is in progress, then the connection attempt is significantly slowed, and it's more likely to fail.
                //if(Adapter.IsDiscovering)   
                    Adapter.CancelDiscovery();


                //// http://stackoverflow.com/questions/20078457/android-bluetoothsocket-write-fails-on-4-2-2
                //try
                //{
                //    BluetoothSocket bs = Device.CreateRfcommSocketToServiceRecord(mUUID);
                //    Field f = bs.Class.GetDeclaredField("mFdHandle");
                //    f.Accessible = true;
                //    f.Set(bs, 0x8000);
                //    bs.Close();
                //    Thread.Sleep(2000);
                //}
                //catch (System.Exception e)
                //{
                //    Log.Debug(TAG, "Reset Failed", e);
                //}







                Log.Debug(TAG, "Connecting");
                Socket.Connect();
                Log.Debug(TAG, "Connected");
                return true;
            }
            catch (System.Exception e1)
            {
                try
                {
                    Log.Debug(TAG, e1.Message);
                    Log.Debug(TAG, "Connected Failed");

                    // try fall back option which sometimes works when above fails
                    Method m = Device.Class.GetMethod("createRfcommSocket", new Java.Lang.Class[] { Integer.Type });
                    Socket = (BluetoothSocket)m.Invoke(Device, 1);
                    Log.Debug(TAG, "Connecting 2");
                    Socket.Connect();
                    Log.Debug(TAG, "Connected 2");
                    return true;
                }
                catch (System.Exception e2)
                {
                    Log.Debug(TAG, e2.Message);
                    Log.Debug(TAG, "Connected 2 Failed");
                    CloseSocket();
                }

                return false;
            }
        }

        public bool GetStreams()
        {
            try
            {
                mInputStream = Socket.InputStream;
            }
            catch
            {
                mInputStream = null;
            }

            try
            {
                mOutputStream = Socket.OutputStream;
            }
            catch
            {
                mOutputStream = null;
            }

            if (mInputStream == null && mOutputStream == null)
                return false;

            return true;

        }



        public bool Write(byte buffer)
        {
            byte[] bytes = new byte[1];
            return Write(bytes, 0, bytes.Length);
        }
        public bool Write(string buffer)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(buffer);
            return Write(bytes, 0, bytes.Length);
        }
        public bool Write(byte[] buffer,int offset,int length)
        {
            try
            {
                if(mOutputStream == null)
                {
                    Log.Debug(TAG,"Write Failed - Output stream is null");
                    return false;

                }
                Log.Debug(TAG,System.String.Format("Writing {0} Bytes From Offset {1}",length, offset));

                mOutputStream.Write(buffer, offset, length);
                return true;
            }
            catch (System.Exception e)
            {
                Log.Debug(TAG,e.Message);
                return false;
            }
        }

        public int Read (byte[] buffer,int offset,int length)
        {
            try
            {
                if (mInputStream == null)
                {
                    Log.Debug(TAG, "Read Failed - Input stream is null");
                    return 0;

                }

                Log.Debug(TAG, System.String.Format("Reading {0} Bytes From Offset {1}", length, offset));
                int bytesRead = mInputStream.Read(buffer, offset, length);
                Log.Debug(TAG, System.String.Format("Read {0} Bytes",bytesRead));
                if(bytesRead > 0)
                {
                    string temp = System.Text.Encoding.ASCII.GetString(buffer, offset, bytesRead);
                    Log.Debug(TAG, temp);
                }
                return bytesRead;
            }
            catch
            {
                return 0;
            }
        }

        public void FlushOutputStream()
        {
            try
            {
                mOutputStream.Flush();
            }catch
            {

            }
        }

        public void FlushInputStream()
        {
            try
            {
                mInputStream.Flush();
            }
            catch
            {

            }
        }

        public bool IsDataAvailable()
        {
            if(mInputStream == null)
            {
                Log.Debug(TAG, "Is Data Available Failed - Input stream is null");
                return false;
            }
            bool dataAvailable = mInputStream.IsDataAvailable();
            Log.Debug(TAG, System.String.Format("Data Available {0}",dataAvailable.ToString()));

            return dataAvailable;
        }


        public bool IsDevicePaired()
        {
            return IsDevicePaired(mDevice);
        }

        public bool IsDevicePaired(BluetoothDevice bluetoothDevice)
        {
            if (bluetoothDevice == null)
                return false;

            if (Adapter == null)
            {
                return false;
            }

            if (!Adapter.IsEnabled)
            {
                return false;
            }

            ICollection<BluetoothDevice> devices;

            devices = new List<BluetoothDevice>();

            devices.Clear();


            bool paired = false;
            devices = Adapter.BondedDevices;
            foreach (BluetoothDevice device in devices)
            {
                if (device.Address == bluetoothDevice.Address)
                {
                    paired = true;
                    break;
                }
            }

            return paired;
        }

        public void PairDevice()
        {
            if (Device == null)
            {
                throw new System.Exception("Device is null");
            }

            Device.CreateBond();
        }

        public void UnpairDevice(BluetoothDevice device)
        {
            Java.Lang.Reflect.Method mi = Device.Class.GetMethod("removeBond", null);
            mi.Invoke(device, null);

        }
        public void UnpairDevice()
        {
            if(Device == null)
            {
                throw new System.Exception("Device is null");
            }
            UnpairDevice(Device);
        }

        public BluetoothDevice GetDeviceFromMacAddress(string address)
        {
            try
            {
                return Adapter.GetRemoteDevice(address);
            }
            catch
            {
                return null;
            }
           
        }



        public BluetoothAdapter Adapter
        {
            get
            {
                return mAdapter;
            }
            set
            {
                mAdapter = value;
            }
        }

        public BluetoothDevice Device
        {
            get
            {
                return mDevice;
            }
            set
            {
                mDevice = value;
            }
        }

        public BluetoothSocket Socket
        {
            get
            {
                return mSocket;
            }
            set
            {
                mSocket = value;
            }
        }

        public Stream InputStream
        {
            get
            {
                return mInputStream;
            }
            set
            {
                mInputStream = value;
            }
        }
        public Stream OutputStream
        {
            get
            {
                return mOutputStream;
            }
            set
            {
                mOutputStream = value;
            }
        }

        public bool CreateSocketUsingReflection
        {
            get
            {
                return mCreateSocketUsingReflection;
            }
            set
            {
                mCreateSocketUsingReflection = value;
            }
        }

    }
}