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
using Android.Views;
using Android.Widget;

namespace com.touchstar.chrisd
{
    class BluetoothPrinter
    {
        public BluetoothPrinter(BluetoothDevice device) : this(device,false)
        {
        }
        public BluetoothPrinter(BluetoothDevice device,bool createSocketUsingReflection)
        {
            base.Device = device;
            base.CreateSocketUsingReflection = createSocketUsingReflection;
        }
        /// <summary>
        /// Prints the file to the currently connected bluetooth printer
        /// If its via the parallel port on a blaster the TxtBuffer + OverflowBuffer on the blaster needs to be larger than the
        /// file being sent
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool PrintFile(string filename)
        {
            if (!File.Exists(filename))
                return false;

            if (!IsDevicePaired())
                return false;

            if (!OpenSocket())
                return false;

            if (!Connect())
            {
                CloseSocket();
                return false;
            }

            if (!GetStreams())
            {
                CloseSocket();
                return false;
            }

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

            if(fs == null)
            {
                CloseSocket();
                return false;
            }

            long length = fs.Length;



            // write it in one go
            //byte[] buffer = new byte[fs.Length];
            //int bytesRead = fs.Read(buffer, 0, buffer.Length);
            //Write(buffer, 0, bytesRead);

           
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            int i = 0;
            for (;;)
            {
                bytesRead = fs.Read(buffer, 0, 1024);
                if (bytesRead == 0)
                    break;

                Write(buffer, 0, bytesRead);

                i++;
                if(i % 15 == 0)
                    System.Threading.Thread.Sleep(5000);

                //if(i != 0)
                //{
                //    if(i % 3 == 0)
                //    {
                //        System.Threading.Thread.Sleep(5000);
                //        CloseSocket();

                //        System.Threading.Thread.Sleep(2000);
                //        if (!OpenSocket())
                //            return false;

                //        if (!Connect())
                //        {
                //            CloseSocket();
                //            return false;
                //        }

                //        if (!GetStreams())
                //        {
                //            CloseSocket();
                //            return false;
                //        }

                //    }
                //}


                //i++;

            }

            //FlushOutputStream();

            fs.Close();


            // required otherwise closing the socket gets done before data is fully written
            // rough timings at 19200 gave a throughput of 3k per second
            double delay = length / 1000;
            if (delay < 5)
                delay = 5;
            System.Threading.Thread.Sleep((int)delay * 1000);

            CloseSocket();            

            return true;
        }




    }
}