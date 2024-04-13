using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace SharpBlunamiControl.GUI
{
    public partial class BlunamiSearch : Form
    {
        BlunamiControl blunamiControl_Search;
        List<BlunamiEngine> FoundBlunamiDevices_Search = new List<BlunamiEngine> { };
        BluetoothLEAdvertisementWatcher watcher_Search;

        List<ulong> BluetoothDeviceAddresses = new List<ulong> { };
        List<string> FoundBluetoothDevicesNames = new List<string> { };
        List<BluetoothLEDevice> FoundBluetoothDevices = new List<BluetoothLEDevice> { };

        ulong addressFromScanResponse;



        delegate void SetOutputBoxCallback(string text);

        private void AppendText(string text)
        {
            if (this.outputBox.InvokeRequired)
            {
                SetOutputBoxCallback d = new SetOutputBoxCallback(AppendText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                if (this.outputBox.TextLength > 10000)
                    this.outputBox.ResetText();

                this.outputBox.Text += text + Environment.NewLine;
            }
        }

        private void ResetOutputBoxText()
        {
            if (this.outputBox.InvokeRequired)
            {
                SetOutputBoxCallback d = new SetOutputBoxCallback(AppendText);
                this.Invoke(d, new object[] {  });
            }
            else
            {
                this.outputBox.ResetText();
            }
        }

        public BlunamiSearch()
        {
            InitializeComponent();
        }
         public void ShowDialog(BlunamiControl control, List<BlunamiEngine> FoundBlunamiDevices)
        {
            blunamiControl_Search = control;
            FoundBlunamiDevices_Search = FoundBlunamiDevices;
            this.ShowDialog();

            control = blunamiControl_Search;
            FoundBlunamiDevices = FoundBlunamiDevices_Search;
        }



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
            watcher.SignalStrengthFilter.InRangeThresholdInDBm = 127;

            // Set the out-of-range threshold to -75dBm (give some buffer). Used in conjunction with OutOfRangeTimeout
            // to determine when an advertisement is no longer considered "in-range"
            watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -127;

            // Set the out-of-range timeout to be 10 seconds. Used in conjunction with OutOfRangeThresholdInDBm
            // to determine when an advertisement is no longer considered "in-range"
            watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(10000);

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
            //string manufacturerDataString = "";
            //var manufacturerSections = eventArgs.Advertisement.ManufacturerData;
            //if (manufacturerSections.Count > 0)
            //{
            //    // Only print the first one of the list
            //    var manufacturerData = manufacturerSections[0];
            //    var data = new byte[manufacturerData.Data.Length];
            //    using (var reader = DataReader.FromBuffer(manufacturerData.Data))
            //    {
            //        reader.ReadBytes(data);
            //    }
            //    // Print the company ID + the raw data in hex format
            //    manufacturerDataString = string.Format("0x{0}: {1}",
            //        manufacturerData.CompanyId.ToString("X"),
            //        BitConverter.ToString(data));
            //}

            var servicesFound = eventArgs.Advertisement.ServiceUuids;
            //var dataSections = eventArgs.Advertisement.DataSections;

            foreach (var service in servicesFound)
            {
                //Console.WriteLine("Service: {0}", service.ToString());

                if (service.ToString() == BlunamiCommandBase.blunamiServiceStr)
                {
                    AppendText(string.Format("Found a Blunami Device with address: {0},{1}", addressStr, rssi));
                    addressFromScanResponse = address;
                    //Console.WriteLine("Signal strength: {0}", rssi);

                }
            }

            if (addressFromScanResponse == address) // check if the address is the same as the scan response, so we know it's the original device
            {
                if (localName != "") // and the local name itself is not null, because Blunami scan response packets do not contain the name
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
        }

        /// <summary>
        /// Invoked as an event handler when the watcher is stopped or aborted.
        /// </summary>
        /// <param name="watcher">Instance of watcher that triggered the event.</param>
        /// <param name="eventArgs">Event data containing information about why the watcher stopped or aborted.</param>
        private async void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {
            // Notify the user that the watcher was stopped
            if (eventArgs.Error == BluetoothError.RadioNotAvailable)
            {
                AppendText(string.Format("No Bluetooth Radio found."));
                //wantsToExit = true;
            }

            AppendText(string.Format("Watcher stopped or aborted: {0}", eventArgs.Error.ToString()));
        }

        private void BlunamiSearch_Load(object sender, EventArgs e)
        {
            //consoleControl1.StartProcess("Blunami Search", "");
            watcher_Search = new BluetoothLEAdvertisementWatcher();

            BlunamiBLEAdvertisementSetup(watcher_Search);
        }

       
        private void button1_Click(object sender, EventArgs e)
        {
            watcher_Search.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            watcher_Search.Stop();
            Task.Run(async () =>
            {
                await CollectAllBLEDeviesAsyncGUI(FoundBluetoothDevices);

                //BlunamiControl.FoundBluetoothDevices.ForEach(i => var test = ReadDecoderType(i));

                //Console.WriteLine("Looking for decoder type");

                //foreach (var device in BlunamiControl.FoundBluetoothDevices)
                //{
                //    //var characteristic = BlunamiControl.GetBlunamiDCCCharacteristic(device);
                //    //if (!BluetoothDeviceAddresses.Contains(address))
                //    //{
                //    //    BluetoothDeviceAddresses.Add(address);
                //    //}
                //    var type = await BlunamiControl.ReadDecoderType(device);
                //    var address = await BlunamiControl.ReadDecoderAddress(device);
                //    BlunamiEngine engine = new BlunamiEngine(device, address, type, 0);
                //    if (!BlunamiControl.FoundBlunamiDevices.Contains(engine))
                //    {
                //        BlunamiControl.FoundBlunamiDevices.Add(engine);
                //    }

                //}
            }).GetAwaiter().GetResult();            //this.Close();
        }

        public async Task CollectAllBLEDeviesAsyncGUI(List<BluetoothLEDevice> FoundBluetoothDevices)
        {
            ResetOutputBoxText();
            foreach (ulong address in BluetoothDeviceAddresses)
            {
                var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                if (device != null)
                {
                    if (!FoundBluetoothDevices.Contains(device))
                    {
                        FoundBluetoothDevices.Add(device);

                        AppendText(string.Format("Stored {0} to list... ", device.Name));

                        // Try forcing the computer to maintain a connection
                        AppendText(string.Format("Setting MaintainConnection {0}...", device.Name));

                        var newSession = await GattSession.FromDeviceIdAsync(device.BluetoothDeviceId);
                        newSession.MaintainConnection = true;
                        AppendText(string.Format("done...", device.Name));

                        device.ConnectionStatusChanged += ConnectionStatusChangedHandler;


                        // Try some custom pairing stuff to see if this helps with disconnections.
                        // https://github.com/microsoft/Windows-universal-samples/blob/main/Samples/DeviceEnumerationAndPairing/cs/Scenario9_CustomPairDevice.xaml.cs

                        // Get ceremony type and protection level selections
                        // You must select at least ConfirmOnly or the pairing attempt will fail
                        DevicePairingKinds ceremonySelected = DevicePairingKinds.ConfirmOnly;

                        //  Workaround remote devices losing pairing information
                        DevicePairingProtectionLevel protectionLevel = DevicePairingProtectionLevel.None;


                        DeviceInformationCustomPairing customPairing = device.DeviceInformation.Pairing.Custom;

                        // Declare an event handler - you don't need to do much in PairingRequestedHandler since the ceremony is "None"
                        customPairing.PairingRequested += PairingRequestedHandler;
                        AppendText(string.Format("Setting Custom pairing settings: {0}...", device.Name));

                        DevicePairingResult result = await customPairing.PairAsync(ceremonySelected, protectionLevel);
                        AppendText(string.Format("Pairing Settings for {0}: {1}, Protection:", device.Name, result.Status.ToString(), result.ProtectionLevelUsed.ToString()));

                    }



                }
                else
                {
                    AppendText(string.Format("Failed to connect to:" + address));
                }

            }
        }

        private async void ConnectionStatusChangedHandler(BluetoothLEDevice bluetoothLeDevice, object o)
        {
            if (bluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                AppendText(string.Format("Connected to: " + bluetoothLeDevice.Name));
            }
            if (bluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                AppendText(string.Format("Disconnected from: " + bluetoothLeDevice.Name));
                bluetoothLeDevice.Dispose();
            }
        }

        /// <summary>
        /// https://github.com/microsoft/Windows-universal-samples/blob/main/Samples/DeviceEnumerationAndPairing/cs/Scenario9_CustomPairDevice.xaml.cs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PairingRequestedHandler(
            DeviceInformationCustomPairing sender,
            DevicePairingRequestedEventArgs args)
        {
            switch (args.PairingKind)
            {
                case DevicePairingKinds.ConfirmOnly:
                    // Windows itself will pop the confirmation dialog as part of "consent" if this is running on Desktop or Mobile
                    // If this is an App for 'Windows IoT Core' where there is no Windows Consent UX, you may want to provide your own confirmation.
                    args.Accept();
                    break;

                    //case DevicePairingKinds.DisplayPin:
                    //    // We just show the PIN on this side. The ceremony is actually completed when the user enters the PIN
                    //    // on the target device. We automatically accept here since we can't really "cancel" the operation
                    //    // from this side.
                    //    args.Accept();

                    //    // No need for a deferral since we don't need any decision from the user
                    //    await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //    {
                    //        ShowPairingPanel(
                    //            "Please enter this PIN on the device you are pairing with: " + args.Pin,
                    //            args.PairingKind);

                    //    });
                    //    break;

                    //case DevicePairingKinds.ProvidePin:
                    //    // A PIN may be shown on the target device and the user needs to enter the matching PIN on
                    //    // this Windows device. Get a deferral so we can perform the async request to the user.
                    //    var collectPinDeferral = args.GetDeferral();

                    //    await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    //    {
                    //        string pin = await GetPinFromUserAsync();
                    //        if (!string.IsNullOrEmpty(pin))
                    //        {
                    //            args.Accept(pin);
                    //        }

                    //        collectPinDeferral.Complete();
                    //    });
                    //    break;

                    //case DevicePairingKinds.ProvidePasswordCredential:
                    //    var collectCredentialDeferral = args.GetDeferral();
                    //    await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    //    {
                    //        var credential = await GetPasswordCredentialFromUserAsync();
                    //        if (credential != null)
                    //        {
                    //            args.AcceptWithPasswordCredential(credential);
                    //        }
                    //        collectCredentialDeferral.Complete();
                    //    });
                    //    break;

                    //case DevicePairingKinds.ConfirmPinMatch:
                    //    // We show the PIN here and the user responds with whether the PIN matches what they see
                    //    // on the target device. Response comes back and we set it on the PinComparePairingRequestedData
                    //    // then complete the deferral.
                    //    var displayMessageDeferral = args.GetDeferral();

                    //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    //    {
                    //        bool accept = await GetUserConfirmationAsync(args.Pin);
                    //        if (accept)
                    //        {
                    //            args.Accept();
                    //        }

                    //        displayMessageDeferral.Complete();
                    //    });
                    //    break;
            }
        }

    }
}
