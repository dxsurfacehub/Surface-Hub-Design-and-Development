using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Networking.Proximity;

namespace NfcConnect
{
    public class NFC
    {
        private ProximityDevice proximityDevice;
        private LogWindow logWindow;
        private long publishingId = 0;

        public NFC(LogWindow wnd)
        {
            logWindow = wnd;
        }

        public async Task Init()
        {
            proximityDevice = await GetNfcDevice();

            if (proximityDevice != null)
            {
                Log("Successfully retrieved Device!");
                Log($"Device ID: {proximityDevice.DeviceId} ");
                Log($"Bits Per Second: {proximityDevice.BitsPerSecond} ");
                Log($"Max Message Bytes: {proximityDevice.MaxMessageBytes}");

                // Reference: https://msdn.microsoft.com/en-us/library/windows/apps/hh701129.aspx
                TryToSubscribe("Windows.Contoso", messageReceivedHandler);
                
                proximityDevice.DeviceArrived += ProximityDeviceArrived;
                proximityDevice.DeviceDeparted += ProximityDeviceDeparted;

                Log("Proximity device initialized.");

                //publish the message to devices that listen
                PublishInfo();
            }
            else
            {
                Log("Failed to initialized proximity device.");
            }
        
        }

        private void TryToSubscribe(string type, MessageReceivedHandler messageReceivedHandler)
        {
            try
            {
                proximityDevice.SubscribeForMessage(type, messageReceivedHandler);
                Log($"Subscribed to {type}");
            }
            catch (Exception)
            {
                Log($"Cannot subscribe for {type}");
            }
        }

        private void messageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
            Log("Message recieved. Type:" + message.MessageType);
            switch (message.MessageType)
            {
                //here you can do things depending on the message received
                default:
                    break;
            }
        }

        private void ProximityDeviceDeparted(ProximityDevice sender)
        {
            Log("Device departed");
        }

        private void ProximityDeviceArrived(ProximityDevice sender)
        {
            Log("Device arrived");
        }

        private void PublishInfo()
        {
            if (null != proximityDevice
                && 0 == publishingId)
            {
                string info = "I love Surface Hub";

                publishingId = proximityDevice.PublishMessage("Windows.Contoso", info);
                                
                Log("Publishing Info");
            }
        }

        private void Log(string msg)
        {
            if (null != logWindow)
            {
                logWindow.Log(msg);
            }
        }

        private async Task<ProximityDevice> GetNfcDevice()
        {
            ProximityDevice device = null;

            string selectorString = ProximityDevice.GetDeviceSelector();

            DeviceInformationCollection deviceInfoCollection =
                await DeviceInformation.FindAllAsync(selectorString, new List<string>() { "{FB3842CD-9E2A-4F83-8FCC-4B0761139AE9} 2" });

            if (deviceInfoCollection.Count > 0)
            {
                foreach (DeviceInformation info in deviceInfoCollection)
                {
                    Log($"Found: {info.Name} ");
                    Log($"Kind  {info.Kind.ToString()} ");
                    foreach (string prop in info.Properties.Keys)
                    {
                        object value = info.Properties[prop];
                        if (null != value)
                        {
                            if (value is string[])
                            {
                                string output = string.Empty;
                                string[] vals = value as string[];
                                foreach (var v in vals)
                                {
                                    output += v.ToString() + ", ";
                                }
                                Log(prop + ": " + output);
                            }
                            else
                            {
                                Log(prop + ": " + value.ToString());
                            }
                        }
                        if ("{FB3842CD-9E2A-4F83-8FCC-4B0761139AE9} 2" == prop)
                        {
                            Log("Found NFC device");
                        }
                    }
                    device = ProximityDevice.FromId(info.Id);

                }
            }

            return device;
        }

        
    }
}
