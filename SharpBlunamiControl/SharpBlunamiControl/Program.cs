﻿using System;
using System.Collections.Generic;
using Windows.Storage.Streams;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth;
using System.IO.Ports;


namespace SharpBlunamiControl
{

    internal partial class BlunamiControl
    {
        BluetoothLEAdvertisementWatcher watcher;

        bool test = false;
        static void Main(string[] args)
        {
            var BlunamiControl = new BlunamiControl();
            // Create and initialize a new watcher instance.
            BlunamiControl.watcher = new BluetoothLEAdvertisementWatcher();


            BlunamiControl.SelectSerialPort();
            BlunamiControl.ConfigureSerialPort();

            if (BlunamiControl.test)
            {
                BlunamiControl.BlunamiBLEAdvertisementSetup(BlunamiControl.watcher);
                Console.WriteLine("Ready to start scanning?");
                Console.ReadLine();
                BlunamiControl.watcher.Start();

                Console.WriteLine("Press enter to stop scanning and exit");

                Console.ReadLine();
                BlunamiControl.watcher.Stop();
                BlunamiControl.FoundBluetoothDevicesNames.ForEach(i => Console.WriteLine("{0}\t", i));
            }

            Console.ReadLine();

            BlunamiControl.ClosePort();
        }

        
    }

}
