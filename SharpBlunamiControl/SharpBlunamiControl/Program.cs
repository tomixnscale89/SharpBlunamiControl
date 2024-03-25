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

            Console.WriteLine("Welcome to SharpBlunamiControl.");
            Console.WriteLine("Please connect your DB9 Cable into a SER2, BASE1, BASE1L, BASE 2, or BASE3.");
            Console.WriteLine("");



            var BlunamiControl = new BlunamiControl();


            // Create and initialize a new watcher instance.
            BlunamiControl.watcher = new BluetoothLEAdvertisementWatcher();

            if (BlunamiControl.SelectSerialPort())
            {
                BlunamiControl.ConfigureSerialPort();
                Console.WriteLine("");

                BlunamiControl.BlunamiBLEAdvertisementSetup(BlunamiControl.watcher);
                Console.WriteLine("Ready to start scanning? \nLet the scan run as long until you see all of the engines you wish to connect to. \nThen press enter to stop scanning.");
                Console.ReadLine();
                BlunamiControl.watcher.Start();

                Console.WriteLine("Press enter to stop scanning and exit....");

                Console.ReadLine();
                BlunamiControl.watcher.Stop();
                //BlunamiControl.FoundBluetoothDevicesNames.ForEach(i => Console.WriteLine("{0}\t", i));
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

                Console.WriteLine("");
                Console.WriteLine("Found the following Blunami engines:");
                BlunamiControl.FoundBlunamiDevices.ForEach(i => Console.WriteLine("{0},TMCC ID: {1}, Decoder: {2}", i.BluetoothLeDevice.Name, i.Id, BlunamiControl.PrintDecoderName(i.DecoderType)));
                BlunamiControl.stopWatch.Start();
                Console.WriteLine("");

                Console.WriteLine("When you are finished, press the HALT button to shut down the application.");
                Console.WriteLine("To control your trains, please use the number AFTER the name of your Blunami decoder above.");



                ////await BlunamiControl.QueryBlunamiServices(BlunamiControl.FoundBluetoothDevices[0]);
                //await ReadDecoderType(BlunamiControl.FoundBluetoothDevices[0]);

                while (!BlunamiControl.wantsToExit)
                {


                    Task.Run(async () =>
                    {
                        //Console.WriteLine(BlunamiControl.stopWatch.Elapsed.TotalMilliseconds - BlunamiControl.lastPressTime);
                        if (BlunamiControl.stopWatch.Elapsed.TotalMilliseconds - BlunamiControl.lastPressTime >= BlunamiControl.buttonHoldInterval)
                        {
                            if (BlunamiControl.lastUsedEngine != null && !BlunamiControl.stopWatch.IsRunning)
                            {
                                // Normal Whistle Enable/Disable
                                if (BlunamiControl.lastUsedEngine.Whistle)
                                {
                                    Console.WriteLine("{0}: Whistle", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Whistle);
                                    BlunamiControl.lastUsedEngine.Whistle = false;
                                    BlunamiControl.whistleButtonPressed = false;

                                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.lastUsedEngine);
                                }

                                // Bell Enable/Disable
                                if (BlunamiControl.bellButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.Bell = !BlunamiControl.lastUsedEngine.Bell;
                                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.lastUsedEngine);
                                    Console.WriteLine("{0}: Bell: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Bell);
                                    BlunamiControl.bellButtonPressed = false;
                                }


                                // Headlight Enable/Disable
                                if (BlunamiControl.headlightButtonPressed)
                                {
                                    BlunamiControl.headlightButtonPressed = false;

                                    BlunamiControl.lastUsedEngine.Headlight = !BlunamiControl.lastUsedEngine.Headlight;
                                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.lastUsedEngine);
                                    Console.WriteLine("{0}: Headlight: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Headlight);

                                }


                                // Direction Enable/Disable
                                if (BlunamiControl.directionButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.Direction = !BlunamiControl.lastUsedEngine.Direction;
                                    BlunamiControl.directionButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiDirectionCommand(BlunamiControl.lastUsedEngine);

                                    Console.WriteLine("{0}: Direction: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Direction);

                                }

                                // Brake Selection 
                                if (BlunamiControl.boostButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.BrakeSelection = !BlunamiControl.lastUsedEngine.BrakeSelection;
                                    BlunamiControl.boostButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiAGroupEffectCommand(BlunamiControl.lastUsedEngine);

                                    Console.WriteLine("{0}: Brake Selection: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.BrakeSelection);

                                }

                                // Brake Enable/Disable
                                if (BlunamiControl.brakeButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.Brake = !BlunamiControl.lastUsedEngine.Brake;
                                    BlunamiControl.brakeButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiAGroupEffectCommand(BlunamiControl.lastUsedEngine);

                                    Console.WriteLine("{0}, Brake: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Brake);

                                }

                                // Short Whistle Enable/Disable
                                if (BlunamiControl.num3Pressed)
                                {
                                    BlunamiControl.lastUsedEngine.ShortWhistle = !BlunamiControl.lastUsedEngine.ShortWhistle;
                                    BlunamiControl.num3Pressed = false;
                                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.lastUsedEngine);

                                    Console.WriteLine("{0}: Short Whistle: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.ShortWhistle);

                                }

                                // Grade Crossing Whistle Enable/Disable
                                if (BlunamiControl.num2Pressed)
                                {
                                    BlunamiControl.lastUsedEngine.GradeCrossingWhistle = !BlunamiControl.lastUsedEngine.GradeCrossingWhistle;
                                    BlunamiControl.num2Pressed = false;
                                    await BlunamiControl.WriteBlunamiAGroupEffectCommand(BlunamiControl.lastUsedEngine);

                                    Console.WriteLine("{0}: Grade Crossing Whistle: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.GradeCrossingWhistle);

                                }


                            }

                            else
                            {
                                BlunamiControl.stopWatch.Stop();
                            }


                        }

                    }
                    ).GetAwaiter().GetResult();

                }

            }

            BlunamiControl.stopWatch.Stop();



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
            if (BlunamiControl.serialEnabled)
                BlunamiControl.ClosePort(); // close Serial port

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }


    }

}
