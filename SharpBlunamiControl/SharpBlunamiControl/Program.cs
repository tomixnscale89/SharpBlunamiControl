using System;
using System.Collections.Generic;
using Windows.Storage.Streams;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth;
using System.IO.Ports;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using SharpBlunamiControl.GUI;
using System.Windows.Forms;

namespace SharpBlunamiControl
{

    internal partial class BlunamiControl
    {
        BluetoothLEAdvertisementWatcher watcher;
        List<BlunamiEngine> FoundBlunamiDevices = new List<BlunamiEngine> { };

        [STAThread]
        static void Main(string[] args)
        {
            

            var BlunamiControl = new BlunamiControl();
            // Create and initialize a new watcher instance.
            BlunamiControl.watcher = new BluetoothLEAdvertisementWatcher();

            if (BlunamiControl.SelectSerialPort())
            {
                BlunamiControl.ConfigureSerialPort();

                BlunamiControl.BlunamiBLEAdvertisementSetup(BlunamiControl.watcher);
                Console.WriteLine("Ready to start scanning?");
                Console.ReadLine();
                BlunamiControl.watcher.Start();

                Console.WriteLine("Press enter to stop scanning and exit");

                Console.ReadLine();
                BlunamiControl.watcher.Stop();
                BlunamiControl.FoundBluetoothDevicesNames.ForEach(i => Console.WriteLine("{0}\t", i));
                Console.WriteLine("Found the following devices.. ok to connect?");
                Console.ReadLine();


                Task.Run(async () =>
                {
                    await BlunamiControl.CollectAllBLEDeviesAsync(BlunamiControl.FoundBluetoothDevices);

                    //BlunamiControl.FoundBluetoothDevices.ForEach(i => var test = ReadDecoderType(i));

                    //Console.WriteLine("Looking for decoder type");

                    foreach (var device in BlunamiControl.FoundBluetoothDevices)
                    {
                        //var characteristic = BlunamiControl.GetBlunamiDCCCharacteristic(device);
                        //if (!BluetoothDeviceAddresses.Contains(address))
                        //{
                        //    BluetoothDeviceAddresses.Add(address);
                        //}
                        var type = await BlunamiControl.ReadDecoderType(device);
                        var address = await BlunamiControl.ReadDecoderAddress(device);
                        BlunamiEngine engine = new BlunamiEngine(device, address, type, 0);
                        if (!BlunamiControl.FoundBlunamiDevices.Contains(engine))
                        {
                            BlunamiControl.FoundBlunamiDevices.Add(engine);
                        }

                    }
                }).GetAwaiter().GetResult();

                BlunamiControl.FoundBlunamiDevices.ForEach(i => Console.WriteLine("{0},{1},{2}", i.BluetoothLeDevice.Name, i.Id, BlunamiControl.PrintDecoderName(i.DecoderType)));


                ////await BlunamiControl.QueryBlunamiServices(BlunamiControl.FoundBluetoothDevices[0]);
                //await ReadDecoderType(BlunamiControl.FoundBluetoothDevices[0]);


               

                Task.Run(async () =>
                {
                    Console.WriteLine("Whistle On");
                    BlunamiControl.FoundBlunamiDevices[0].DynamoFlags ^= BlunamiEngineEffectCommandParams.LONG_WHISTLE;

                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.FoundBlunamiDevices[0]);

                }).GetAwaiter().GetResult();

                Console.ReadLine();

                Task.Run(async () =>
                {
                    Console.WriteLine("Whistle Off");
                    BlunamiControl.FoundBlunamiDevices[0].DynamoFlags ^= BlunamiEngineEffectCommandParams.LONG_WHISTLE;

                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.FoundBlunamiDevices[0]);

                }).GetAwaiter().GetResult();

            }

            Console.ReadLine();

            BlunamiControl.FoundBluetoothDevices.ForEach(i => i.Dispose()); // disconnect and clean up all remaining BLE connections
            if(BlunamiControl.serialEnabled)
                BlunamiControl.ClosePort(); // close Serial port

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }

        
    }

}
