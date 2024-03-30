using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth;
using Windows.Storage.Streams;


namespace SharpBlunamiControl
{
    internal partial class BlunamiControl
    {
        List<ulong> BluetoothDeviceAddresses = new List<ulong> { };
        List<string> FoundBluetoothDevicesNames = new List<string> { };

        ulong addressFromScanResponse;


        //Dictionary<BluetoothLEAdvertisementReceivedEventArgs, BluetoothLEAdvertisementReceivedEventArgs> events = new Dictionary<BluetoothLEAdvertisementReceivedEventArgs, BluetoothLEAdvertisementReceivedEventArgs>;
        private async void BlunamiBLEAdvertisementSetup(BluetoothLEAdvertisementWatcher watcher)
        {

            var manufacturerData = new BluetoothLEManufacturerData();
            // Then, set the company ID for the manufacturer data. Here use the Blunami Manufacturer (Verifone Systems): 0x200
            //manufacturerData.CompanyId = 0x200;

            //watcher.AllowExtendedAdvertisements = true;
            watcher.ScanningMode = BluetoothLEScanningMode.Active;

            //watcher.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);

            // Part 1B: Configuring the signal strength filter for proximity scenarios

            // Configure the signal strength filter to only propagate events when in-range
            // Please adjust these values if you cannot receive any advertisement 
            // Set the in-range threshold to -70dBm. This means advertisements with RSSI >= -70dBm 
            // will start to be considered "in-range".
            watcher.SignalStrengthFilter.InRangeThresholdInDBm = -75;

            // Set the out-of-range threshold to -75dBm (give some buffer). Used in conjunction with OutOfRangeTimeout
            // to determine when an advertisement is no longer considered "in-range"
            watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -80;

            // Set the out-of-range timeout to be 2 seconds. Used in conjunction with OutOfRangeThresholdInDBm
            // to determine when an advertisement is no longer considered "in-range"
            watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(2000);

            // By default, the sampling interval is set to zero, which means there is no sampling and all
            // the advertisement received is returned in the Received event

            // Attach a handler to process the received advertisement. 
            // The watcher cannot be started without a Received handler attached
            watcher.Received += OnAdvertisementReceived;

            // Attach a handler to process watcher stopping due to various conditions,
            // such as the Bluetooth radio turning off or the Stop method was called
            watcher.Stopped += OnAdvertisementWatcherStopped;

        }

        private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            // We can obtain various information about the advertisement we just received by accessing 
            // the properties of the EventArgs class

            // The timestamp of the event
            DateTimeOffset timestamp = eventArgs.Timestamp;

            // The type of advertisement
            BluetoothLEAdvertisementType advertisementType = eventArgs.AdvertisementType;

            // The received signal strength indicator (RSSI)
            Int16 rssi = eventArgs.RawSignalStrengthInDBm;

            // The local name of the advertising device contained within the payload, if any
            string localName = eventArgs.Advertisement.LocalName;

            // Get the Bluetooth address to later connect to
            ulong address = eventArgs.BluetoothAddress;
            string addressStr = eventArgs.BluetoothAddress.ToString();

            // Check if there are any manufacturer-specific sections.
            // If there is, print the raw data of the first manufacturer section (if there are multiple).
            string manufacturerDataString = "";
            var manufacturerSections = eventArgs.Advertisement.ManufacturerData;
            if (manufacturerSections.Count > 0)
            {
                // Only print the first one of the list
                var manufacturerData = manufacturerSections[0];
                var data = new byte[manufacturerData.Data.Length];
                using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                {
                    reader.ReadBytes(data);
                }
                // Print the company ID + the raw data in hex format
                manufacturerDataString = string.Format("0x{0}: {1}",
                    manufacturerData.CompanyId.ToString("X"),
                    BitConverter.ToString(data));
            }

            var servicesFound = eventArgs.Advertisement.ServiceUuids;
            var dataSections = eventArgs.Advertisement.DataSections;

            foreach (var service in servicesFound)
            {
                //Console.WriteLine("Service: {0}", service.ToString());

                if (service.ToString() == BlunamiCommandBase.blunamiServiceStr)
                {
                    Console.WriteLine("Found a Blunami Device with address: {0}", addressStr);
                    addressFromScanResponse = address;

                }
            }

            if(addressFromScanResponse == address) // check if the address is the same as the scan response, so we know it's the original device
            {
                if(localName != "") // and the local name itself is not null, because Blunami scan response packets do not contain the name
                {
                    //Console.WriteLine(string.Format("[{0}]: type={1}, rssi={2}, name={3}, manufacturerData=[{4}, address={5}, services count:{6}]",
                    //                    timestamp.ToString("hh\\:mm\\:ss\\.fff"),
                    //                    advertisementType.ToString(),
                    //                    rssi.ToString(),
                    //                    localName,
                    //                    manufacturerDataString, addressStr, servicesFound.Count));


                    if (!BluetoothDeviceAddresses.Contains(address))
                    {
                        BluetoothDeviceAddresses.Add(address);
                    }
                    if (!FoundBluetoothDevicesNames.Contains(localName))
                    {
                        FoundBluetoothDevicesNames.Add(localName);
                    }
                }
            }


            //if (!foundBlunamiAdvertisementService)
            //{
            //    foreach (var dataSection in dataSections)
            //    {
            //        var data = new byte[dataSection.Data.Length];
            //        using (var reader = DataReader.FromBuffer(dataSection.Data))
            //        {
            //            reader.ReadBytes(data);
            //        }
            //        //Console.WriteLine("Data section: {0}, {1}", dataSection.DataType, System.Text.Encoding.Default.GetString(data));
            //        //Console.WriteLine("Data section: {0}, {1}", dataSection.DataType, BitConverter.ToString(data));
            //    }



            //    // Serialize UI update to the main UI thread
            //    //await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //    //{
            //    // Display these information on the list
            //    Console.WriteLine(string.Format("[{0}]: type={1}, rssi={2}, name={3}, manufacturerData=[{4}, address={5}, services count:{6}]",
            //        timestamp.ToString("hh\\:mm\\:ss\\.fff"),
            //        advertisementType.ToString(),
            //        rssi.ToString(),
            //        localName,
            //        manufacturerDataString, addressStr, servicesFound.Count));

            //    //foreach (var serviceUuid in servicesFound)
            //    //{
            //    //    Console.WriteLine(serviceUuid.ToString());
            //    //}

            //    if (!BluetoothDeviceAddresses.Contains(address))
            //    {
            //        BluetoothDeviceAddresses.Add(address);
            //    }
            //    if (!FoundBluetoothDevicesNames.Contains(localName))
            //    {
            //        FoundBluetoothDevicesNames.Add(localName);
            //    }
            //}



            //})
        }

        /// <summary>
        /// Invoked as an event handler when the watcher is stopped or aborted.
        /// </summary>
        /// <param name="watcher">Instance of watcher that triggered the event.</param>
        /// <param name="eventArgs">Event data containing information about why the watcher stopped or aborted.</param>
        private async void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {
            // Notify the user that the watcher was stopped
            if(eventArgs.Error == BluetoothError.RadioNotAvailable)
            {
                Console.WriteLine("No Bluetooth Radio found.");
                wantsToExit = true;
            }
            
            Console.WriteLine(string.Format("Watcher stopped or aborted: {0}", eventArgs.Error.ToString()));
        }
    }
}
