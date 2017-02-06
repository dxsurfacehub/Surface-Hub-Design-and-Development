using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Networking.Proximity;

namespace Contoso_Jobs.Common
{
    public class NFC
    {
        ProximityDevice proximityDevice;

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
                    Debug.WriteLine("Found: " + info.Name);
                    foreach (string prop in info.Properties.Keys)
                    {
                        object value = info.Properties[prop];
                        if (null != value)
                        {
                            Debug.WriteLine(prop + ": " + value.ToString());
                        }
                    }
                    device = ProximityDevice.FromId(info.Id);
                }
            }
            return device;
        }

        public async Task Init()
        {
            proximityDevice = await GetNfcDevice();

            if (proximityDevice != null)
            {
                string deviceInfo = "Successfully retrieved Device!\r\n" +
                "Device ID: " + proximityDevice.DeviceId + "\r\n" +
                "Bits Per Second: " + proximityDevice.BitsPerSecond + "\r\n" +
                "Max Message Bytes: " + proximityDevice.MaxMessageBytes + "\r\n";
                Log(deviceInfo);

                // Reference: https://msdn.microsoft.com/en-us/library/windows/apps/hh701129.aspx
                TryToSubscribe("Windows.Contoso", messageReceivedHandler);

                proximityDevice.DeviceArrived += ProximityDeviceArrived;
                proximityDevice.DeviceDeparted += ProximityDeviceDeparted;

                Log("Proximity device initialized.\n");
            }
            else
            {
                Log("Failed to initialized proximity device.");
            }

        }

        private void Log(string msg)
        {
            Debug.WriteLine(msg);
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

        private void ProximityDeviceDeparted(ProximityDevice sender)
        {
            Log("Device departed");
        }

        private void ProximityDeviceArrived(ProximityDevice sender)
        {
            Log($"Device arrived: {sender.DeviceId}");
        }

        private void messageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
            Log($"Recieved {message.MessageType}");

            if ("Windows.Contoso" == message.MessageType)
            {
                Log($"Message is: {message.DataAsString}");
            }
        }


    }
}
