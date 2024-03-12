using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace SharpBlunamiControl
{
    internal partial class BlunamiControl
    {
        string[] availablePorts;
        SerialPort serialPort;
        int comPortIndex = 0;
        bool canContinueBeyondSerialPortSelection = false;
        void SelectSerialPort()
        {
            availablePorts = SerialPort.GetPortNames();
            if (availablePorts.Length != 0)
            {
                while (!canContinueBeyondSerialPortSelection)
                {
                    Console.WriteLine("Please enter the correct COM Port to use:");

                    for (int i = 0; i < availablePorts.Length; i++)
                    {
                        Console.WriteLine("[" + i.ToString() + "] " + availablePorts[i]);
                    }
                    ConsoleKeyInfo keyEntered = Console.ReadKey();
                    Console.ReadLine();
                    if (keyEntered != null)
                    {
                        if (Char.IsDigit(keyEntered.KeyChar))
                        {
                            comPortIndex = int.Parse(keyEntered.KeyChar.ToString());
                            Console.WriteLine("Using COM port: " + availablePorts[comPortIndex]);
                            if (availablePorts[comPortIndex] != null)
                            {
                                serialPort = new SerialPort(availablePorts[comPortIndex], 9600, Parity.None, 8, StopBits.One);
                                canContinueBeyondSerialPortSelection |= true;
                            }
                            else
                            {
                                Console.WriteLine("Serial Port does not exist. Please enter again.");
                            }
                        }
                    }
                }
            }

            else
            {
                Console.WriteLine("No COM ports found. Exiting....");
                return;
            }
        }
    }
}
