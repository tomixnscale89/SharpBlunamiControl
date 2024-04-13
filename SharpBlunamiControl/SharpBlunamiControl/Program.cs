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
using System.Timers;
using System.Diagnostics;

namespace SharpBlunamiControl
{

    public partial class BlunamiControl
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


            /*
             
            Add BLE Radio Checking Here 

             */



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

                    Console.WriteLine("When you are finished, type Q to shut down the application.");
                    Console.WriteLine("To control your trains, please use the number AFTER the name of your Blunami decoder above.");
                }
                else
                {
                    BlunamiControl.wantsToExit = true;
                    Console.WriteLine("Found no devices. Exiting....");

                }


                // https://stackoverflow.com/questions/3382409/console-readkey-async-or-callback
                Task.Factory.StartNew(() =>
                {
                    while (Console.ReadKey().Key != ConsoleKey.Q) ;
                    BlunamiControl.wantsToExit = true;
                });

                while (!BlunamiControl.wantsToExit)
                {


                    Task.Run(async () =>
                    {
                        if(BlunamiControl.lastUsedEngine != null && BlunamiControl.lastUsedEngine.Whistle)
                        {
                            await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.lastUsedEngine).ConfigureAwait(false);
                            Console.WriteLine("{0}: Whistle: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Whistle ? "On" : "Off");

                        }
                        //Console.WriteLine(BlunamiControl.stopWatch.Elapsed.TotalMilliseconds - BlunamiControl.lastPressTime);
                        if (BlunamiControl.stopWatch.Elapsed.TotalMilliseconds - BlunamiControl.lastPressTime >= BlunamiControl.buttonHoldInterval)
                        {
                            if (BlunamiControl.lastUsedEngine != null && !BlunamiControl.stopWatch.IsRunning)
                            {
                                // Normal Whistle Enable/Disable
                                if (BlunamiControl.lastUsedEngine.Whistle)
                                {
                                    BlunamiControl.lastUsedEngine.Whistle = false;
                                    BlunamiControl.whistleButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.lastUsedEngine).ConfigureAwait(false);
                                    Console.WriteLine("{0}: Whistle: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Whistle ? "On" : "Off");

                                }

                                // Bell Enable/Disable
                                if (BlunamiControl.bellButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.Bell = !BlunamiControl.lastUsedEngine.Bell;
                                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.lastUsedEngine).ConfigureAwait(false);
                                    Console.WriteLine("{0}: Bell: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Bell ? "On" : "Off");
                                    BlunamiControl.bellButtonPressed = false;
                                }


                                // Headlight Enable/Disable
                                if (BlunamiControl.headlightButtonPressed)
                                {
                                    BlunamiControl.headlightButtonPressed = false;

                                    BlunamiControl.lastUsedEngine.Headlight = !BlunamiControl.lastUsedEngine.Headlight;
                                    await BlunamiControl.WriteBlunamiDynamoGroupEffectCommand(BlunamiControl.lastUsedEngine).ConfigureAwait(false);
                                    Console.WriteLine("{0}: Headlight: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Headlight ? "On" : "Off");

                                }


                                // Direction Enable/Disable
                                if (BlunamiControl.directionButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.Direction = !BlunamiControl.lastUsedEngine.Direction;
                                    BlunamiControl.directionButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiDirectionCommand(BlunamiControl.lastUsedEngine).ConfigureAwait(false);

                                    Console.WriteLine("{0}: Direction: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Direction ? "Forward" : "Reverse");

                                }

                                // Brake Selection 
                                if (BlunamiControl.boostButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.BrakeSelection = !BlunamiControl.lastUsedEngine.BrakeSelection;
                                    BlunamiControl.boostButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiAGroupEffectCommand(BlunamiControl.lastUsedEngine).ConfigureAwait(false);

                                    Console.WriteLine("{0}: Brake Selection: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.BrakeSelection ? "Train" : "Independent");

                                }

                                // Brake Enable/Disable
                                if (BlunamiControl.brakeButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.Brake = !BlunamiControl.lastUsedEngine.Brake;
                                    BlunamiControl.brakeButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiAGroupEffectCommand(BlunamiControl.lastUsedEngine).ConfigureAwait(false);

                                    Console.WriteLine("{0}, Brake: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.Brake ? "On" : "Off");

                                }

                                // Front Coupler - Hooked on FX3
                                if(BlunamiControl.frontCouplerButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.FX3 = !BlunamiControl.lastUsedEngine.FX3;
                                    BlunamiControl.frontCouplerButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiDFGroupEffectCommand(BlunamiControl.lastUsedEngine).ConfigureAwait(false);

                                    Console.WriteLine("{0}: FX3: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.FX3 ? "On" : "Off");

                                }

                                // Rear Coupler - Hooked on FX4
                                if (BlunamiControl.rearCouplerButtonPressed)
                                {
                                    BlunamiControl.lastUsedEngine.FX4 = !BlunamiControl.lastUsedEngine.FX4;
                                    BlunamiControl.rearCouplerButtonPressed = false;
                                    await BlunamiControl.WriteBlunamiDFGroupEffectCommand(BlunamiControl.lastUsedEngine).ConfigureAwait(false);

                                    Console.WriteLine("{0}: FX4: {1}", BlunamiControl.lastUsedEngine.BluetoothLeDevice.Name, BlunamiControl.lastUsedEngine.FX4 ? "On" : "Off");

                                }


                                await BlunamiControl.KeyPadCommands(BlunamiControl.keyPadCVPage).ConfigureAwait(false);

                                // Set Button
                                if (BlunamiControl.setButtonPressed)
                                {
                                    BlunamiControl.setButtonPressed = false;
                                    BlunamiControl.keyPadCVPage++;
                                    if (BlunamiControl.keyPadCVPage > 2)
                                        BlunamiControl.keyPadCVPage = 0;
                                    Console.WriteLine("Using Keypad Page:{0}", BlunamiControl.keyPadCVPage);
                                    
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

                            Console.WriteLine("{0}: Grade Crossing Horn: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.GradeCrossingWhistle ? "On" : "Off");

                        }

                        // Short Whistle Enable/Disable
                        if (num2Pressed)
                        {
                            lastUsedEngine.ShortWhistle = !lastUsedEngine.ShortWhistle;
                            num2Pressed = false;
                            await WriteBlunamiDynamoGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Short Horn: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.ShortWhistle ? "On" : "Off");

                        }

                        // Cylinder Cocks Enable/Disable
                        if (num3Pressed)
                        {
                            lastUsedEngine.CylinderCocks = !lastUsedEngine.CylinderCocks;
                            num3Pressed = false;
                            await WriteBlunamiDynamoGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: CylinderCocks/DynamicBrake/Panto: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.CylinderCocks ? "On" : "Off");

                        }

                        // Cutoff+ Enable/Disable
                        if (num4Pressed)
                        {
                            lastUsedEngine.CutoffIncrease = !lastUsedEngine.CutoffIncrease;
                            num4Pressed = false;
                            await WriteBlunamiBGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Cutoff +/RPM/Startup: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.CutoffIncrease ? "On" : "Off");

                        }

                        // Cutoff- Enable/Disable
                        if (num5Pressed)
                        {
                            lastUsedEngine.CutoffDecrease = !lastUsedEngine.CutoffDecrease;
                            num5Pressed = false;
                            await WriteBlunamiBGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Cutoff -/RPM/Shutdown: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.CutoffDecrease ? "On" : "Off");

                        }


                        // Blowdown Enable/Disable
                        if (num6Pressed)
                        {
                            lastUsedEngine.Blowdown = !lastUsedEngine.Blowdown;
                            num6Pressed = false;
                            await WriteBlunamiAGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Blowdown/StraightToEight: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.Blowdown ? "On" : "Off");

                        }

                        // Dimmer Enable/Disable
                        if (num7Pressed)
                        {
                            lastUsedEngine.Dimmer = !lastUsedEngine.Dimmer;
                            num7Pressed = false;
                            await WriteBlunamiBGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Dimmer: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.Dimmer ? "On" : "Off");

                        }

                        // Couple Enable/Disable
                        if (num8Pressed)
                        {
                            lastUsedEngine.Uncouple = !lastUsedEngine.Uncouple;
                            num8Pressed = false;
                            await WriteBlunamiDEGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Couple: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.Uncouple ? "On" : "Off");

                        }

                        // Momentum (Switching Mode) Mode Enable/Disable
                        if (num9Pressed)
                        {
                            lastUsedEngine.Momentum = !lastUsedEngine.Momentum;
                            num9Pressed = false;
                            await WriteBlunamiDEGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Switching Mode: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.Momentum ? "On" : "Off");

                        }

                    }
                    break;
                    
                case 1:
                    {
                        // HandBrake Enable/Disable
                        if (num1Pressed)
                        {
                            lastUsedEngine.HandBrake = !lastUsedEngine.HandBrake;
                            num1Pressed = false;
                            await WriteBlunamiAGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: HandBrake: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.HandBrake ? "On" : "Off");

                        }

                        // WaterStop Enable/Disable
                        if (num2Pressed)
                        {
                            lastUsedEngine.Waterstop = !lastUsedEngine.Waterstop;
                            num2Pressed = false;
                            await WriteBlunamiDEGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: WaterStop: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.Waterstop ? "On" : "Off");

                        }

                        // Fuel Loading Enable/Disable
                        if (num3Pressed)
                        {
                            lastUsedEngine.FuelStop = !lastUsedEngine.FuelStop;
                            num3Pressed = false;
                            await WriteBlunamiDEGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: FuelLoad: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.FuelStop ? "On" : "Off");

                        }

                        // Ashdump Enable/Disable
                        if (num4Pressed)
                        {
                            lastUsedEngine.AshDump = !lastUsedEngine.AshDump;
                            num4Pressed = false;
                            await WriteBlunamiDEGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: Ash Dump: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.AshDump ? "On" : "Off");

                        }

                        // Wheel Slip- Enable/Disable
                        if (num5Pressed)
                        {
                            lastUsedEngine.Wheelslip = !lastUsedEngine.Wheelslip;
                            num5Pressed = false;
                            await WriteBlunamiDEGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: WheelSlip: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.Wheelslip ? "On" : "Off");

                        }


                        // Injector/ F20 Enable/Disable
                        if (num6Pressed)
                        {
                            lastUsedEngine.F20 = !lastUsedEngine.F20;
                            num6Pressed = false;
                            await WriteBlunamiDEGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: F20: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.F20 ? "On" : "Off");

                        }

                        // Sander Enable/Disable
                        if (num7Pressed)
                        {
                            lastUsedEngine.SanderValve = !lastUsedEngine.SanderValve;
                            num7Pressed = false;
                            await WriteBlunamiDFGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: SanderValve: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.SanderValve ? "On" : "Off");

                        }

                        // Cab Chatter Enable/Disable
                        if (num8Pressed)
                        {
                            lastUsedEngine.CabChatter = !lastUsedEngine.CabChatter;
                            num8Pressed = false;
                            await WriteBlunamiDFGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: CabChatter: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.CabChatter ? "On" : "Off");

                        }

                        // All Aboard Mode Enable/Disable
                        if (num9Pressed)
                        {
                            lastUsedEngine.AllAboard = !lastUsedEngine.AllAboard;
                            num9Pressed = false;
                            await WriteBlunamiDFGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: AllAboard: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.AllAboard ? "On" : "Off");

                        }
                    }
                    break;
                case 2:
                    {
                        if (num1Pressed)
                        {
                            num1Pressed = false;

                        }

                        if (num2Pressed)
                        {
                            num2Pressed = false;
                        }

                        // FX3 Enable/Disable
                        if (num3Pressed)
                        {
                            lastUsedEngine.FX3 = !lastUsedEngine.FX3;
                            num3Pressed = false;
                            await WriteBlunamiDFGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: FX3: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.FX3 ? "On" : "Off");

                        }

                        // FX4 Enable/Disable
                        if (num4Pressed)
                        {
                            lastUsedEngine.FX4 = !lastUsedEngine.FX4;
                            num4Pressed = false;
                            await WriteBlunamiDFGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: FX4: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.FX4 ? "On" : "Off");

                        }

                        // FX5 Enable/Disable
                        if (num5Pressed)
                        {
                            lastUsedEngine.FX5 = !lastUsedEngine.FX5;
                            num5Pressed = false;
                            await WriteBlunamiDFGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: FX5: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.FX5 ? "On" : "Off");

                        }


                        // FX6 Enable/Disable
                        if (num6Pressed)
                        {
                            lastUsedEngine.FX6 = !lastUsedEngine.FX6;
                            num6Pressed = false;
                            await WriteBlunamiDFGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: FX6: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.FX6 ? "On" : "Off");

                        }

                        // FX28 Enable/Disable
                        if (num7Pressed)
                        {
                            lastUsedEngine.FX28 = !lastUsedEngine.FX28;
                            num7Pressed = false;
                            await WriteBlunamiDFGroupEffectCommand(lastUsedEngine);

                            Console.WriteLine("{0}: FX28: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.FX28 ? "On" : "Off");

                        }

                        if (num8Pressed)
                        {
                            num8Pressed = false;
                        }

                        if (num9Pressed)
                        {
                            num9Pressed = false;
                        }
                    }
                    break;

            }

            // Momentum Mode Enable/Disable
            if (num0Pressed)
            {
                lastUsedEngine.Mute = !lastUsedEngine.Mute;
                num0Pressed = false;
                await WriteBlunamiBGroupEffectCommand(lastUsedEngine);

                Console.WriteLine("{0}: Mute: {1}", lastUsedEngine.BluetoothLeDevice.Name, lastUsedEngine.Mute ? "On" : "Off");

            }
        }

    }

}
