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

        

        bool showDialogueText = true;

        Stopwatch stopWatch = new Stopwatch();


        [STAThread]
        static void Main(string[] args)
        {
            //Application.Run(new SharpBlunamiControl.GUI.SharpBlunamiControl());



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

                if (BlunamiControl.FoundBluetoothDevices.Count > 0)
                {
                    

                    Console.WriteLine("");
                    Console.WriteLine("Found the following Blunami engines:");
                    BlunamiControl.FoundBlunamiDevices.ForEach(i => Console.WriteLine("{0},TMCC ID: {1}, Decoder: {2}", i.BluetoothLeDevice.Name, i.TMCCID, BlunamiControl.PrintDecoderName(i.DecoderType)));
                    BlunamiControl.stopWatch.Start();
                    Console.WriteLine("");

                    Console.WriteLine("When you are finished, press the HALT button to shut down the application.");
                    Console.WriteLine("To control your trains, please use the number AFTER the name of your Blunami decoder above.");
                }
                else
                {
                    BlunamiControl.wantsToExit = true;
                    Console.WriteLine("Found no devices. Exiting....");

                }

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
                                    Console.WriteLine("{0}: Headlight: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Headlight ? "On" : "Off");

                                }


                                // Direction Enable/Disable
                                if (BlunamiControl.directionButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.Direction = !BlunamiControl.lastUsedEngine.Direction;
                                    BlunamiControl.directionButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiDirectionCommand(BlunamiControl.lastUsedEngine);

                                    Console.WriteLine("{0}: Direction: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Direction ? "Forward" : "Reverse");

                                }

                                // Brake Selection 
                                if (BlunamiControl.boostButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.BrakeSelection = !BlunamiControl.lastUsedEngine.BrakeSelection;
                                    BlunamiControl.boostButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiAGroupEffectCommand(BlunamiControl.lastUsedEngine);

                                    Console.WriteLine("{0}: Brake Selection: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.BrakeSelection ? "Train" : "Independent");

                                }

                                // Brake Enable/Disable
                                if (BlunamiControl.brakeButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.Brake = !BlunamiControl.lastUsedEngine.Brake;
                                    BlunamiControl.brakeButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiAGroupEffectCommand(BlunamiControl.lastUsedEngine);

                                    Console.WriteLine("{0}, Brake: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Brake ? "On" : "Off");

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

        async Task KeyPadCommands(int keyPadPage)
        {
            switch(keyPadPage)
            {
                default:
                case 0:
                    {
                        // Grade Crossing Whistle Enable/Disable
                        if (num1Pressed)
                        {
                            lastUsedEngine.GradeCrossingWhistle = !lastUsedEngine.GradeCrossingWhistle;
                            num1Pressed = false;
                            await WriteBlunamiAGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Grade Crossing Whistle: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.GradeCrossingWhistle ? "On" : "Off");

                        }

                        // Grade Crossing Whistle Enable/Disable
                        if (num2Pressed)
                        {
                           lastUsedEngine.GradeCrossingWhistle = !lastUsedEngine.GradeCrossingWhistle;
                            num2Pressed = false;
                            await WriteBlunamiAGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Grade Crossing Whistle: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.GradeCrossingWhistle ? "On" : "Off");

                        }

                        // Short Whistle Enable/Disable
                        if (num3Pressed)
                        {
                            lastUsedEngine.ShortWhistle = !lastUsedEngine.ShortWhistle;
                            num3Pressed = false;
                            await WriteBlunamiDynamoGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Short Whistle: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.ShortWhistle ? "On" : "Off");

                        }

                        // Cutoff+ Enable/Disable
                        if (num4Pressed)
                        {
                            lastUsedEngine.ShortWhistle = !lastUsedEngine.ShortWhistle;
                            num4Pressed = false;
                            await WriteBlunamiDynamoGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Cutoff +: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.ShortWhistle ? "On" : "Off");

                        }

                    }
                    break;
                    
                case 1:
                    {

                    }
                    break;
                case 2:
                    {

                    }
                    break;

            }
        }

    }

}
