using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace SharpBlunamiControl
{
    internal partial class BlunamiControl
    {
        static DeviceInformation information = null;
        List<BluetoothLEDevice> FoundBluetoothDevices = new List<BluetoothLEDevice> { };


        private async void ConnectionStatusChangedHandler(BluetoothLEDevice bluetoothLeDevice, object o)
        {
            if(bluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                Console.WriteLine("Connected to: " + bluetoothLeDevice.Name);
            }
            if(bluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                Console.WriteLine("Disconnected from: " + bluetoothLeDevice.Name);
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

        async Task CollectAllBLEDeviesAsync(List<BluetoothLEDevice> FoundBluetoothDevices)
        {

            foreach (ulong address in BluetoothDeviceAddresses)
            {
                var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                if(device != null)
                {
                    if (!FoundBluetoothDevices.Contains(device))
                    {
                        FoundBluetoothDevices.Add(device);
                        Console.WriteLine("Stored {0} to list... ",device.Name);

                        // Try forcing the computer to maintain a connection
                        Console.Write("Setting MaintainConnection {0}...",device.Name);

                        var newSession = await GattSession.FromDeviceIdAsync(device.BluetoothDeviceId);
                        newSession.MaintainConnection = true;
                        Console.WriteLine("done...", device.Name);

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
                        Console.WriteLine("Setting Custom pairing settings: {0}...", device.Name);

                        DevicePairingResult result = await customPairing.PairAsync(ceremonySelected, protectionLevel);
                        Console.WriteLine("Pairing Settings for {0}: {1}, Protection:", device.Name,result.Status.ToString(), result.ProtectionLevelUsed.ToString());

                    }

                    

                }
                else
                {
                    Console.WriteLine("Failed to connect to:" + address);
                }
                
            }
        }

        async Task QueryBlunamiServices(BluetoothLEDevice device)
        {
            if(device != null)
            {
                GattDeviceServicesResult result = await device.GetGattServicesAsync();
                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    foreach (var service in services)
                    {
                        Console.WriteLine(service.Uuid);
                    }
                }
            }
        }


        public async Task<GattCharacteristic> GetBlunamiDCCCharacteristic(BluetoothLEDevice device)
        {
            GattDeviceServicesResult result = await device.GetGattServicesAsync();
            GattCharacteristic dccCharacteristic = null;
            try
            {
                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    foreach (var service in services)
                    {
                        if (service.Uuid.ToString() == BlunamiCommandBase.blunamiServiceStr)
                        {
                            //Console.WriteLine(service.Uuid.ToString());
                            GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync();

                            if (result.Status == GattCommunicationStatus.Success)
                            {
                                var characteristics = characteristicResult.Characteristics;

                                foreach (var characteristic in characteristics)
                                {
                                    if (characteristic.Uuid.ToString() == BlunamiCommandBase.blunamiDCCCharacteristicStr)
                                    {
                                        //Console.WriteLine(characteristic.Uuid.ToString());
                                        dccCharacteristic = characteristic;
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("GetBlunamiDCCCharacteristic: Error Reading Blunami Characteristic:{0}", result.Status.ToString());
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("GetBlunamiDCCCharacteristic: Error Writing to Blunami Characteristic:{0}", result.Status.ToString());
                }
                if (dccCharacteristic == null)
                {
                    Console.WriteLine("GetBlunamiDCCCharacteristic: An error occured. Could not find Blunami Characteristic in BLE services list.");
                }
            }
            catch(System.ObjectDisposedException)
            {
                Console.WriteLine("System.ObjectDisposedException on: {0}, {1}", device.Name, device.ConnectionStatus);
            }
            
            
            return dccCharacteristic;
        }



        public async Task<int> ReadShortDecoderAddress(BluetoothLEDevice device)
        {
            int shortID = -1;
            if (device != null)
            {
                try
                {
                    byte[] readCVCommand = new BlunamiCommandBase().baseReadCVCommand;
                    // to read the short address, we just need to read CV1. 
                    readCVCommand[4] = 1;
                    var writer = new DataWriter();
                    writer.WriteBytes(readCVCommand);

                    var characteristic = await GetBlunamiDCCCharacteristic(device);

                    if (characteristic != null)
                    {
                        GattWriteResult result = await characteristic.WriteValueWithResultAsync(writer.DetachBuffer());
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            // Successfully wrote to Blunami. Now, we have to read what the result from the same characteristic
                            GattReadResult readResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                            if (readResult.Status == GattCommunicationStatus.Success)
                            {
                                var reader = DataReader.FromBuffer(readResult.Value);
                                byte[] input = new byte[reader.UnconsumedBufferLength];
                                reader.ReadBytes(input);
                                shortID = input[5];
                                //Console.WriteLine(DecoderType);
                                //Console.WriteLine(PrintDecoderName(DecoderType));
                                // Utilize the data as needed
                            }
                            else
                            {
                                Console.WriteLine("ReadShortDecoderAddress: Error Reading Blunami Characteristic:{0}", result.Status.ToString());
                            }
                        }
                        else
                        {
                            Console.WriteLine("ReadShortDecoderAddress: Error Writing to Blunami Characteristic:{0}", result.Status.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not find Blunami Characteristic returning invalid shortID");

                    }

                }
                catch
                {

                }
            }
            else
            {
                Console.WriteLine("Could not find Blunami Device. Returning invalid shortID");

                shortID = -1;
            }
            return shortID;
        }

        public async Task<int> ReadDecoderAddress(BluetoothLEDevice device)
        {
            int dccAddress = -1;
            var writer = new DataWriter();
            GattWriteResult result;
            GattReadResult readResult;
            DataReader reader;
            byte[] input;

            if (device != null)
            {
                try
                {
                    // First thing we need to determine is if the locomotive uses a 4-digit address (extended)
                    // or a simple digit address. The way we can determine this is from the bits of CV29.
                    // https://www.2mm.org.uk/articles/cv29%20calculator.htm

                    byte[] readCVCommand = new BlunamiCommandBase().baseReadCVCommand;
                    readCVCommand[4] = 29;
                    writer.WriteBytes(readCVCommand);

                    var characteristic = await GetBlunamiDCCCharacteristic(device);

                    if (characteristic != null)
                    {
                         result = await characteristic.WriteValueWithResultAsync(writer.DetachBuffer());
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            // Successfully wrote to Blunami. Now, we have to read what the result from the same characteristic
                            readResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                            if (readResult.Status == GattCommunicationStatus.Success)
                            {
                                reader = DataReader.FromBuffer(readResult.Value);
                                input = new byte[reader.UnconsumedBufferLength];
                                reader.ReadBytes(input);
                                int cv29 = input[5];
                                cv29 &= 32; // let's and CV29 with 32 to see if there is a one or zero.
                                //Console.WriteLine("CV29: " + cv29);
                                if (cv29 == 32)
                                {
                                    // we are using long loco address. Read CV17, and CV18.
                                    readCVCommand[4] = 17; // read cv17
                                    writer.WriteBytes(readCVCommand);
                                    result = await characteristic.WriteValueWithResultAsync(writer.DetachBuffer());

                                    if (result.Status == GattCommunicationStatus.Success)
                                    {
                                        readResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                                        if (readResult.Status == GattCommunicationStatus.Success)
                                        {
                                            reader = DataReader.FromBuffer(readResult.Value);
                                            input = new byte[reader.UnconsumedBufferLength];
                                            reader.ReadBytes(input);
                                            int CV17 = input[5];
                                            //Console.WriteLine("CV17: " + CV17);
                                            // Now with CV17 read, let's read CV18.

                                            readCVCommand[4] = 18; // read cv18
                                            writer.WriteBytes(readCVCommand);
                                            result = await characteristic.WriteValueWithResultAsync(writer.DetachBuffer());

                                            if (result.Status == GattCommunicationStatus.Success) // if write was successful, let's read the buffer
                                            {
                                                readResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                                                if (readResult.Status == GattCommunicationStatus.Success) // if the buffer read was successful, let's peek at the data. 
                                                {
                                                    reader = DataReader.FromBuffer(readResult.Value);
                                                    input = new byte[reader.UnconsumedBufferLength];
                                                    reader.ReadBytes(input);
                                                    int CV18 = input[5]; // 
                                                                         // 49152 is DCC Long address offset
                                                    //Console.WriteLine("CV18: " + CV18);

                                                    dccAddress = (CV17 << 8) + CV18 - BlunamiCommandBase.DCC_LONG_ADDRESS_CONSTANT;
                                                    
                                                    // This is only for communicating with Lionel devices. TMCC cannot handle 4 digit so we are just ignoring the last two values.
                                                    //dccAddress = dccAddress / 100; // get rid of the last two digits by dividing by 100 and forgetting the decimal portions
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Loco is not using long loco address. Only read CV1.
                                    // to read the short address, we just need to read CV1. 
                                    readCVCommand[4] = 1;
                                    writer.WriteBytes(readCVCommand);
                                    result = await characteristic.WriteValueWithResultAsync(writer.DetachBuffer());
                                    if (result.Status == GattCommunicationStatus.Success)
                                    {
                                        // Successfully wrote to Blunami. Now, we have to read what the result from the same characteristic
                                        readResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                                        if (readResult.Status == GattCommunicationStatus.Success)
                                        {
                                            reader = DataReader.FromBuffer(readResult.Value);
                                            input = new byte[reader.UnconsumedBufferLength];
                                            reader.ReadBytes(input);
                                            dccAddress = input[5];
                                        }
                                    }

                                }




                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not find Blunami Characteristic returning invalid dccAddress");

                    }

                }
                catch
                {

                }
            }
            else
            {
                Console.WriteLine("Could not find Blunami Device. Returning invalid dccAddress");

                dccAddress = -1;
            }
            return dccAddress;
        }

    public async Task<int> ReadDecoderType(BluetoothLEDevice device)
        {
            int DecoderType = -1;
            if (device != null)
            {
                try
                {
                    byte[] readCVCommand = new BlunamiCommandBase().baseReadCVCommand;
                    // to read Decoder type, this comes from CV256. We must change the 3rd byte to be 0x75 to start reading
                    // CV256 and beyond. Since CV256 is the first cv in this new range, we just set the 4th byte to 0. If we read
                    // CV257 instead, we'd set the 4th byte to 1. 
                    readCVCommand[3] = 0x75;
                    readCVCommand[4] = 0;
                    var writer = new DataWriter();
                    writer.WriteBytes(readCVCommand);

                    var characteristic = await GetBlunamiDCCCharacteristic(device);

                    if(characteristic != null)
                    {
                        GattWriteResult result = await characteristic.WriteValueWithResultAsync(writer.DetachBuffer());
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            // Successfully wrote to Blunami. Now, we have to read what the result from the same characteristic
                            GattReadResult readResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                            if (readResult.Status == GattCommunicationStatus.Success)
                            {
                                var reader = DataReader.FromBuffer(readResult.Value);
                                byte[] input = new byte[reader.UnconsumedBufferLength];
                                reader.ReadBytes(input);
                                DecoderType = input[5];
                                // Utilize the data as needed
                            }
                            else
                            {
                                Console.WriteLine("Error Reading from Blunami Characteristic:{0}", result.Status.ToString());
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error Writing to Blunami Characteristic:{0}", result.Status.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not find Blunami Characteristic returning invalid DecoderType");

                    }

                }
                catch
                {

                }
            }
            else
            {
                Console.WriteLine("Could not find Blunami Device. Returning invalid DecoderType");

                DecoderType = - 1;
            }
            return DecoderType;
        }

        public async Task ResetBlunamiFunctions()
        {
            
            }
        }
    }