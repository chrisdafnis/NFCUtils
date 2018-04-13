using System;
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;

namespace com.touchstar.chrisd.nfcutils
{
    public interface IDiscoveryHandler : LinkOS.Plugin.Abstractions.IDiscoveryHandler
    {
        void FoundDevice(IDiscoveredDevice discoveredDevice);
        //void DiscoveryFinished();
        //void DiscoveryError(String paramString);
    }
    public delegate void FoundDeviceHandler();

    public interface IDiscoveryEventHandler : IDiscoveryHandler
    {
        event DiscoveryErrorHandler OnDiscoveryError;
        event DiscoveryFinishedHandler OnDiscoveryFinished;
        event FoundDeviceHandler OnFoundDevice;
    }

    public class DiscoveryEventHandler : IDiscoveryEventHandler
    {
        IDiscoveryEventHandler discoveryEventHandler;

        public DiscoveryEventHandler()
        {
            var handler = DiscoveryHandlerFactory.Current.GetInstance();
            //discoveryEventHandler.
            //discoveryEventHandler.OnFoundDevice += DiscoveryEventHandler_OnFoundDevice;
        }

        public void FoundDevice(IDiscoveredDevice discoveredDevice)
        {
            //discoveryEventHandler.FoundDevice(discoveredDevice);
        }

        public void FoundPrinter(IDiscoveredPrinter discoveredPrinter)
        {
            
        }

        private void DiscoveryEventHandler_OnFoundDevice(LinkOS.Plugin.Abstractions.IDiscoveryHandler handler, IDiscoveredPrinter discoveredPrinter)
        {
            throw new NotImplementedException();
        }

        public void DiscoveryFinished()
        {

        }

        public void DiscoveryError(String paramString)
        {

        }

        //
        // Summary:
        //     Current DiscoveryHandler to use
        public static IDiscoveryHandler Current { get; }

        //
        // Summary:
        //     This event is invoked when there is an error during discovery.
        [Obsolete("Use IDiscoveryEventHandler instance events", true)]
        public event LinkOS.Plugin.Abstractions.DiscoveryErrorHandler OnDiscoveryError;
        //
        // Summary:
        //     This event is invoked when discovery is finished.
        [Obsolete("Use IDiscoveryEventHandler instance events", true)]
        public event LinkOS.Plugin.Abstractions.DiscoveryFinishedHandler OnDiscoveryFinished;
        //
        // Summary:
        //     This event is invoked when a printer has been discovered.
        //[Obsolete("Use IDiscoveryEventHandler instance events", true)]
        //public static event FoundPrinterHandler OnFoundDevice;
        [Obsolete("Use IDiscoveryEventHandler instance events", true)]
        public event FoundDeviceHandler OnFoundDevice;
        //
        // Summary:
        //     This method is invoked when there is an error during discovery. The discovery
        //     will be cancelled when this method is invoked. discoveryFinished() will not be
        //     called if this method is invoked.
        //
        // Parameters:
        //   sender:
        //     the specific IDiscoveryHandler initiating this event
        //
        //   message:
        //     the error message
        [Obsolete("Use IDiscoveryEventHandler instance events", true)]
        public delegate void DiscoveryErrorHandler(object sender, string message);
        //
        // Summary:
        //     This method is invoked when discovery is finished.
        //
        // Parameters:
        //   sender:
        //     the specific IDiscoveryHandler initiating this event
        [Obsolete("Use IDiscoveryEventHandler instance events", true)]
        public delegate void DiscoveryFinishedHandler(object sender);
        //
        // Summary:
        //     This method is invoked when a printer has been discovered. This method will be
        //     invoked for each printer that is found.
        //
        // Parameters:
        //   sender:
        //     the specific IDiscoveryHandler initiating this event
        //
        //   discoveredPrinter:
        //     a discovered printer.
        [Obsolete("Use IDiscoveryEventHandler instance events", true)]
        public delegate void FoundPrinterHandler(object sender, IDiscoveredDevice discoveredPrinter);
    }
}