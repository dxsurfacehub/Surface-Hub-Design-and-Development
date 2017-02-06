using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.Networking.Proximity;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace NfcConnect.Common
{
    public class NFC
    {
        ProximityDevice proximityDevice;
        MainPage page;

        public NFC(MainPage p)
        {
            page = p;
        }

        private void Log(string msg)
        {
            page.Log(msg);
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
                    Log("Found: " + info.Name);
                    foreach(string prop in info.Properties.Keys)
                    {
                        object value = info.Properties[prop];
                        if (null != value)
                        {
                            Log(prop + ": " + value.ToString());
                        }
                    }
                    device = ProximityDevice.FromId(info.Id);
                    
                }
            }

            return device;
        }

        private void PublishLaunchApp(ProximityDevice proximityDevice)
        {
            //var proximityDevice = Windows.Networking.Proximity.ProximityDevice.GetDefault();

            if (proximityDevice != null)
            {
                // The format of the app launch string is:
                // <args>\tWindows\t<AppFamilyBasedName>\tWindowsPhone\t<AppGUID>
                // The string is tab delimited.

                // The <args> string must have at least one character.
                string launchArgs = "user=default";


                // The format of the AppFamilyBasedName is: PackageFamilyName!PRAID.
                string praid = "App"; // The Application Id value from your package.appxmanifest.
                string appFamilyBasedName = Windows.ApplicationModel.Package.Current.Id.FamilyName + "!" + praid;


                // GUID is PhoneProductId value from you package.appxmanifest 
                // NOTE: The GUID will change after you associate app to the app store
                // Consider using windows.applicationmodel.store.currentapp.appid after the app is associated to the store.
                string appGuid = "{55d006ef-be06-4019-bc6d-ec38f28a5304}";

                string launchAppMessage = launchArgs +
                "\tWindows\t" + appFamilyBasedName +
                "\tWindowsPhone\t" + appGuid;

                var dataWriter = new Windows.Storage.Streams.DataWriter();
                dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
                dataWriter.WriteString(launchAppMessage);
                var launchAppPubId = proximityDevice.PublishBinaryMessage("LaunchApp:WriteTag", dataWriter.DetachBuffer());
            }
        }

        private void CreateDeviceWatcher()
        {
            string selectorString = ProximityDevice.GetDeviceSelector();
            DeviceWatcher watcher = DeviceInformation.CreateWatcher(selectorString);
            watcher.Added += Watcher_Added;
            watcher.Removed += Watcher_Removed;
            watcher.Updated += Watcher_Updated;
            watcher.EnumerationCompleted += Watcher_EnumerationCompleted;

            watcher.Start();
        }

        private void Watcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            Log("First batch enumerated" );
        }

        private void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate info)
        {
            Log("Updated: " + info.Id);
        }

        private void Watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate info)
        {
            Log("Removed: " + info.Id);
        }

        private void Watcher_Added(DeviceWatcher sender, DeviceInformation info)
        {
            Log("Found: " + info.Name);
            foreach (string prop in info.Properties.Keys)
            {
                object value = info.Properties[prop];
                if (null != value)
                {
                    Log(prop + ": " + value.ToString());
                }
            }
        }

        public async Task Init()
        {
            //CreateDeviceWatcher();
            //return;

            //FindDevice();
            proximityDevice = await GetNfcDevice();


            if (proximityDevice != null)
            {
                string deviceInfo = "Successfully retrieved Device!\r\n" +
                "Device ID: " + proximityDevice.DeviceId + "\r\n" +
                "Bits Per Second: " + proximityDevice.BitsPerSecond + "\r\n" +
                "Max Message Bytes: " + proximityDevice.MaxMessageBytes + "\r\n";
                Log(deviceInfo);

                //PublishLaunchApp(proximityDevice);
                
                // Reference: https://msdn.microsoft.com/en-us/library/windows/apps/hh701129.aspx
                //proximityDevice.SubscribeForMessage("Windows", messageReceivedHandler);
                //proximityDevice.SubscribeForMessage("WindowsURI", messageReceivedHandler);
                //proximityDevice.SubscribeForMessage("WindowsMime", messageReceivedHandler);
                //proximityDevice.SubscribeForMessage("WriteableTag", messageReceivedHandler);

                proximityDevice.SubscribeForMessage("NDEF", messageReceivedHandler);
                //proximityDevice.SubscribeForMessage("NDEF: ext", messageReceivedHandler);
                //proximityDevice.SubscribeForMessage("NDEF:MIME", messageReceivedHandler);
                //proximityDevice.SubscribeForMessage("NDEF:URI", messageReceivedHandler);

                // Note: Below are not supported on Surface Hub. Future updates to this app should
                // autodetect capabilities and then register message handlers for them

                //proximityDevice.SubscribeForMessage("Pairing: Bluetooth", messageReceivedHandler);
                //proximityDevice.SubscribeForMessage("NDEF:WriteTag", messageReceivedHandler);
                //proximityDevice.SubscribeForMessage("NDEF:Unknown", messageReceivedHandler);

                proximityDevice.DeviceArrived += ProximityDeviceArrived;
                proximityDevice.DeviceDeparted += ProximityDeviceDeparted;
                
                Log("Proximity device initialized.\n");
            }
            else
            {
                Log("Failed to initialize proximity device.\n");
            }

        }

        private async void FindDevice()
        {
            string selectorString = ProximityDevice.GetDeviceSelector();

            DeviceInformationCollection deviceInfoCollection =
                await DeviceInformation.FindAllAsync(selectorString, null);

            if (deviceInfoCollection.Count == 0)
            {
                Log("No proximity devices found.");
            }
            else
            {
                foreach (DeviceInformation dev in deviceInfoCollection)
                {
                    Log($"Proximity Device id {dev.Id}  Kind  {dev.Kind.ToString()} ");
                    proximityDevice = ProximityDevice.FromId(dev.Id);
                }
            }
        }


        private void ProximityDeviceDeparted(ProximityDevice sender)
        {
            Log("Device departed");
            //throw new NotImplementedException();
        }

        private void ProximityDeviceArrived(ProximityDevice sender)
        {
            Log("Device arrived: " + sender.DeviceId);
            
            //throw new NotImplementedException();
        }

        private void messageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
            Log("Message recieved " + message.MessageType);
            switch (message.MessageType)
            {
                case "WriteableTag":
                    var tagSize = BitConverter.ToInt32(message.Data.ToArray(), 0);
                    Log("Writeable tag size: " + tagSize);
                    break;
                case "NDEF":
                    Log("Recieved: " + message.DataAsString);
                    break;
                case "Windows":
                    Log("Recieved: " + message.DataAsString);
                    
                    break;
                default:
                    break;
            }
        }




    }
}
