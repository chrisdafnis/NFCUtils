using System;

namespace com.touchstar.chrisd.nfcutils
{
    public interface INfcDevice
    {
        String FriendlyName { get; set; }
        String MacAddress { get; set; }
    }

    public class NfcDevice : INfcDevice
    {
        public String FriendlyName { get; set; }
        public String MacAddress { get; set; }
    }

}