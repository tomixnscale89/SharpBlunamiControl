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
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;

namespace SharpBlunamiControl
{

    internal partial class BlunamiControl
    {
        BluetoothLEAdvertisementWatcher watcher;
        List<BlunamiEngine> FoundBlunamiDevices = new List<BlunamiEngine> { };
        bool wantsToExit = false;
        static bool resetBlunamiSettings = false;

        Stopwatch stopWatch = new Stopwatch();


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
                //Console.WriteLine("Found the following devices.. ok to connect?");

                
                //Console.WriteLine("Wanting to exit");

                //Console.ReadLine();


                // Collect all Blunami Devices and add them to our Blunami Class

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
                BlunamiControl.stopWatch.Start();


                ////await BlunamiControl.QueryBlunamiServices(BlunamiControl.FoundBluetoothDevices[0]);
                //await ReadDecoderType(BlunamiControl.FoundBluetoothDevices[0]);

                while (!BlunamiControl.wantsToExit)
                {

                    if (BlunamiControl.stopWatch.Elapsed.TotalMilliseconds - BlunamiControl.lastPressTime >= BlunamiControl.buttonHoldInterval)
                    {
                        Task.Run(async () =>
                        {
                            //Console.WriteLine(BlunamiControl.stopWatch.Elapsed.TotalMilliseconds - BlunamiControl.lastPressTime);

                            if (BlunamiControl.lastUsedEngine != null)
                            {
                                if (BlunamiControl.lastUsedEngine.Whistle)
                                {
                                    // printf("Horn stopped\n");
                                    Console.WriteLine("{0}, Whistle: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Whistle);

                                    BlunamiControl.lastUsedEngine.DynamoFlags ^= BlunamiEngineEffectCommandParams.LONG_WHISTLE;
                                    BlunamiControl.lastUsedEngine.Whistle = false;
                                    Console.WriteLine("{0}, Whistle: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Whistle);

                                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.lastUsedEngine);
                                }

                                if (BlunamiControl.bellButtonPressed)
                                {
                                    //printf("Short Horn button stopped\n");
                                    //loco.dynamoFlags ^= BlunamiEngineEffectCommandParams::SHORT_WHISTLE;
                                    //Console.WriteLine("Blunami Short Whistle state %d\n", loco.shortWhistleOn);
                                    BlunamiControl.lastUsedEngine.DynamoFlags ^= BlunamiEngineEffectCommandParams.BELL;
                                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.lastUsedEngine);
                                    Console.WriteLine("{0}, Bell: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Headlight);

                                    Console.WriteLine("Bell stopped");
                                    //WriteBlunamiDynamoGroupEffectCommand(loco);

                                    BlunamiControl.bellButtonPressed = false;
                                }


                                if (BlunamiControl.headlightButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.Headlight = !BlunamiControl.lastUsedEngine.Headlight;
                                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.lastUsedEngine);
                                    Console.WriteLine("{0}, Headlight: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Headlight);

                                    BlunamiControl.headlightButtonPressed = false;
                                }

                                if (BlunamiControl.directionButtonPressed)
                                {
                                    Console.WriteLine("{0}, Direction: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Direction);
                                    BlunamiControl.lastUsedEngine.Direction = !BlunamiControl.lastUsedEngine.Direction;
                                    BlunamiControl.directionButtonPressed = false;

                                }


                            }
                        }
                        ).GetAwaiter().GetResult();
                    }

                    }

                }




                //Console.ReadLine();

                //Task.Run(async () =>
                //{
                //    Console.WriteLine("Whistle Off");

                //    for (int i = 0; i < BlunamiControl.FoundBluetoothDevices.Count; i++)
                //    {
                //        BlunamiControl.FoundBlunamiDevices[i].DynamoFlags ^= BlunamiEngineEffectCommandParams.LONG_WHISTLE;
                //        await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.FoundBlunamiDevices[i]);
                //    }

                //}).GetAwaiter().GetResult();


            //Console.ReadLine();
            BlunamiControl.stopWatch.Stop();

            BlunamiControl.FoundBluetoothDevices.ForEach(i => i.Dispose()); // disconnect and clean up all remaining BLE connections
            if(BlunamiControl.serialEnabled)
                BlunamiControl.ClosePort(); // close Serial port

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }

        
    }

}
