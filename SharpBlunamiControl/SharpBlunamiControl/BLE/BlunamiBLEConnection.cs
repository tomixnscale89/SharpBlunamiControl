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
            Console.WriteLine("Connected to: " + bluetoothLeDevice.Name);
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
                        Console.WriteLine("Stored Blunami: " + device.Name);
                    //    var newSession = await GattSession.FromDeviceIdAsync(device.BluetoothDeviceId);
                    //newSession.MaintainConnection = true;
                    //device.ConnectionStatusChanged += ConnectionStatusChangedHandler;
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



        async Task<GattCharacteristic> GetBlunamiDCCCharacteristic(BluetoothLEDevice device)
        {
            GattDeviceServicesResult result = await device.GetGattServicesAsync();
            GattCharacteristic dccCharacteristic = null;
            if (result.Status == GattCommunicationStatus.Success)
            {
                var services = result.Services;
                foreach (var service in services)
                {
                    if (service.Uuid.ToString() == blunamiServiceStr)
                    {
                        //Console.WriteLine(service.Uuid.ToString());
                        GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync();

                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            var characteristics = characteristicResult.Characteristics;
                            
                            foreach (var characteristic in characteristics)
                            {
                                if(characteristic.Uuid.ToString() == blunamiDCCCharacteristicStr)
                                {
                                    //Console.WriteLine(characteristic.Uuid.ToString());
                                    dccCharacteristic = characteristic;
                                }
                            }
                        }
                    }
                }
            }
            if(dccCharacteristic == null)
            {
                Console.WriteLine("An error occured. Could not find Blunami Characteristic in BLE services list.");
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

                                                    dccAddress = (CV17 << 8) + CV18 - DCC_LONG_ADDRESS_CONSTANT;
                                                    
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
    }
}
