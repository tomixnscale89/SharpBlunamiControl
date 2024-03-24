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
        private enum TMCCCommandType
        {
            CT_ACTION = 0x0,
            CT_EXTENDED = 0x1,
            CT_RELATIVE_SPEED = 0x2,
            CT_ABSOLUTE_SPEED = 0x3,
            CT_MASK = 0x3,
        };

        private enum EngineCommandParams
        {
            EC_FORWARD_DIRECTION = 0b00000,
            EC_TOGGLE_DIRECTION = 0b00001,
            EC_REVERSE_DIRECTION = 0b00011,
            EC_BOOST_SPEED = 0b00100,
            EC_BRAKE_SPEED = 0b00111,
            EC_OPEN_FRONT_COUPLER = 0b00101,
            EC_OPEN_REAR_COUPLER = 0b00110,
            EC_BLOW_HORN_1 = 0b11100,
            EC_RING_BELL = 0b11101,
            EC_LET_OFF_SOUND = 0b11110,
            EC_BLOW_HORN_2 = 0b11111,
            EC_AUX_1_OFF = 0b01000,
            EC_AUX_1_OPTION_1 = 0b01001,
            EC_AUX_1_OPTION_2 = 0b01010,
            EC_AUX_1_ON = 0b01011,
            EC_AUX_2_OFF = 0b01100,
            EC_AUX_2_OPTION_1 = 0b01101,
            EC_AUX_2_OPTION_2 = 0b01110,
            EC_AUX_2_ON = 0b01111,
            EC_NUMERIC_FLAG = 0b10000,
            EC_NUMERIC_MASK = 0b01111,

            // extended commands
            EEC_ASSIGN_TO_TRAIN_FLAG = 0b10000,
            EEC_ASSIGN_TO_TRAIN_MASK = 0b01111,
            EEC_ASSIGN_AS_SINGLE_UNIT_FORWARD = 0b00000,
            EEC_ASSIGN_AS_SINGLE_UNIT_REVERSE = 0b00100,
            EEC_ASSIGN_AS_HEAD_END_UNIT_FORWARD = 0b00001,
            EEC_ASSIGN_AS_HEAD_END_UNIT_REVERSE = 0b00101,
            EEC_ASSIGN_AS_MIDDLE_UNIT_FORWARD = 0b00010,
            EEC_ASSIGN_AS_MIDDLE_UNIT_REVERSE = 0b00110,
            EEC_ASSIGN_AS_REAR_END_UNIT_FORWARD = 0b00011,
            EEC_ASSIGN_AS_REAR_END_UNIT_REVERSE = 0b00111,

            EEC_SET_MOMENTUM_LOW = 0b01000,
            EEC_SET_MOMENTUM_MEDIUM = 0b01001,
            EEC_SET_MOMENTUM_HIGH = 0b01010,
            EEC_SET_ENGINE_ADDRESS = 0b01011,

            // extended train commands
            ETC_SET_MOMENTUM_LOW = 0b01000,
            ETC_SET_MOMENTUM_MEDIUM = 0b01001,
            ETC_SET_MOMENTUM_HIGH = 0b01010,
            ETC_SET_TMCC_ADDRESS = 0b01011,
            ETC_CLEAR_CONSIST = 0b01100,
        };

        string[] availablePorts;
        SerialPort serialPort;
        int comPortIndex = 0;
        bool canContinueBeyondSerialPortSelection = false;
        bool serialEnabled = false;

        bool bellButtonPressed = false;
        bool whistleButtonPressed = false;
        bool shortWhistleButtonPressed = false;
        bool headlightButtonPressed = false;
        bool frontCouplerButtonPressed = false;
        bool rearCouplerButtonPressed = false;
        bool boostButtonPressed = false;
        bool brakeButtonPressed = false;
        bool directionButtonPressed = false;

        //
        int lastFoundTMCCID;
        BlunamiEngine lastUsedEngine;

        float timer;
        float lastPressTime;
        float buttonHoldInterval = 100;
        bool debugString = true;

        bool SelectSerialPort()
        {
            availablePorts = SerialPort.GetPortNames(); // Get all available Serial Ports
            if (availablePorts.Length != 0) // Make sure the ports are more than zero
            {
                while (!canContinueBeyondSerialPortSelection) // stick in this loop until user selects a real port
                {
                    Console.WriteLine("Please enter the correct COM Port to use:");

                    for (int i = 0; i < availablePorts.Length; i++) // enumerate through all ports
                    {
                        Console.WriteLine("[" + i.ToString() + "] " + availablePorts[i]);
                    }

                    ConsoleKeyInfo keyEntered = Console.ReadKey(); // read key
                    Console.ReadLine();
                    if (keyEntered != null) // as long as key is not null
                    {
                        if (Char.IsDigit(keyEntered.KeyChar)) // check if the key is a digit. if not, tell them to start over
                        {
                            comPortIndex = int.Parse(keyEntered.KeyChar.ToString()); // convert key to a int
                            try // does this entered port exist in our array?
                            {
                                serialPort = new SerialPort(availablePorts[comPortIndex], 9600, Parity.None, 8, StopBits.One);
                                canContinueBeyondSerialPortSelection |= true;
                                Console.WriteLine("Using COM port: " + availablePorts[comPortIndex]);
                                serialEnabled = true;
                            }
                            catch (System.IO.IOException)
                            {
                                Console.WriteLine("Serial Port does not exist. Please enter again.");
                            }
                            catch (System.IndexOutOfRangeException)
                            {
                                Console.WriteLine("Serial Port does not exist. Please enter again.");
                            }

                        }
                    }
                }
                return true;

            }

            else // No ports found at all, shut down program
            {
                Console.WriteLine("No COM ports found. Exiting....");
                return false;
            }
        }

        void ConfigureSerialPort()
        {
            serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
            serialPort.ReadTimeout = 500;
            serialPort.Open();
        }

        void ClosePort()
        {
            serialPort.Close();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buf = new byte[3];
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    buf[i] = (byte)serialPort.ReadByte();
                }

                if (debugString)
                    Console.WriteLine("Hex: " + BitConverter.ToString(buf));
                if (buf.Length == 3)
                {
                    /*
                     * Idea for this:
                     * 1. TMCC Packet comes over serial. Parse it.
                     * 2. Deconstruct the command byte, data byte, and engine ID.
                     * 3. Store the engine ID, and then go set which buttons were pressed.
                     * 4. back in the main loop, when a button is pressed, we check all of our
                     * BlunamiEngine list to find a engine that has the same ID. If it exists,
                     * then connect and send the data over.
                     * 
                     */

                    // Item1 - CommandType
                    // Item2 - TMCC ID
                    // Item3 - Data ID

                    var TMCCPacket = DetermineTMCCCommand(buf);

                    if (TMCCPacket.Item2 == 0xFF & TMCCPacket.Item3 == 0xFF)
                    {
                        //Console.WriteLine("TMCCPacket.Item2 {0} TMCCPacket.Item3 {1}", TMCCPacket.Item2, TMCCPacket.Item3);
                        Console.WriteLine("Program now exiting....");
                        wantsToExit = true; // exit thread
                    }
                    else
                    {
                        lastFoundTMCCID = TMCCPacket.Item3;
                        foreach (var blunami in FoundBlunamiDevices)
                        {
                            if (blunami.TMCCID == lastFoundTMCCID)
                            {
                                lastUsedEngine = blunami;
                                switch (TMCCPacket.Item1)
                                {
                                    case (int)TMCCCommandType.CT_ACTION:
                                        {
                                            SetTMCCButtonSates(TMCCPacket.Item2, blunami);
                                            break;
                                        }
                                    case (int)TMCCCommandType.CT_RELATIVE_SPEED:
                                        {
                                            break;
                                        }
                                    case (int)TMCCCommandType.CT_ABSOLUTE_SPEED:
                                        {
                                            break;
                                        }
                                }
                            }
                        }
                    }
                    
                }
            }
     
            catch (TimeoutException)
            {
                Console.WriteLine("Button's not pressed");
            }
            
        }

        void TestStates(int dataid)
        {
            switch (dataid)
            {
                case (int)EngineCommandParams.EC_BLOW_HORN_1:
                case (int)EngineCommandParams.EC_BLOW_HORN_2:
                    {
                        whistleButtonPressed = true;
                        lastPressTime = timer;
                        if (debugString)
                            Console.WriteLine("Horn pressed");
                        break;
                    }

                case (int)EngineCommandParams.EC_RING_BELL:
                    {
                        bellButtonPressed = true;
                        lastPressTime = timer;
                        if (debugString)
                            Console.WriteLine("Bell pressed\n");
                        break;
                    }
            }
        }

        void SetTMCCButtonSates(int dataid, BlunamiEngine loco)
        {
            switch (dataid)
            {
                case (int)EngineCommandParams.EC_BLOW_HORN_1:
                case (int)EngineCommandParams.EC_BLOW_HORN_2:
                    {
                        loco.DynamoFlags |= BlunamiEngineEffectCommandParams.LONG_WHISTLE;
                        loco.Whistle = true;
                        whistleButtonPressed = true;
                        lastPressTime = stopWatch.ElapsedMilliseconds;
                        if (debugString)
                            Console.WriteLine("Horn pressed");
                        break;
                    }

                case (int)EngineCommandParams.EC_RING_BELL:
                    {
                        bellButtonPressed = true;
                        lastPressTime = stopWatch.ElapsedMilliseconds;
                        if (debugString)
                            Console.WriteLine("Bell pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_TOGGLE_DIRECTION:
                    {
                        directionButtonPressed = true;
                        lastPressTime = stopWatch.ElapsedMilliseconds;
                        if (debugString)
                            Console.WriteLine("Toggle direction pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_AUX_1_OPTION_1:
                    {
                        shortWhistleButtonPressed = true;
                        lastPressTime = stopWatch.ElapsedMilliseconds;
                        if (debugString)
                            Console.WriteLine("Aux1 pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_AUX_2_OPTION_1:
                    {
                        headlightButtonPressed = true;
                        lastPressTime = (float)stopWatch.Elapsed.TotalMilliseconds;
                        if (debugString)
                            Console.WriteLine("Aux2 pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_OPEN_FRONT_COUPLER:
                    {
                        frontCouplerButtonPressed = true;
                        lastPressTime = stopWatch.ElapsedMilliseconds;
                        if (debugString)
                            Console.WriteLine("Front Coupler pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_OPEN_REAR_COUPLER:
                    {
                        rearCouplerButtonPressed = true;
                        lastPressTime = stopWatch.ElapsedMilliseconds;
                        if (debugString)
                            Console.WriteLine("Rear Coupler pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_BOOST_SPEED:
                    {
                        boostButtonPressed = true;
                        lastPressTime = stopWatch.ElapsedMilliseconds;
                        if (debugString)
                            Console.WriteLine("Boost pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_BRAKE_SPEED:
                    {
                        brakeButtonPressed = true;
                        lastPressTime = stopWatch.ElapsedMilliseconds;
                        if (debugString)
                            Console.WriteLine("Brake pressed\n");
                        break;
                    }

                default:
                    {
                        dataid &= (int)EngineCommandParams.EC_NUMERIC_MASK;
                        if (debugString)
                            Console.WriteLine("Numerical pressed: %d\n", dataid);
                        break;

                        switch (dataid)
                        {
                            case 0:
                                break;
                            case 1:
                                break;
                            case 2:
                                break;
                            case 3:
                                break;
                            case 4:
                                break;
                            case 5:
                                break;
                            case 6:
                                break;
                            case 7:
                                break;
                            case 8:
                                break;
                            case 9:
                                break;
                        }
                    }

            }
        }

        private Tuple<int , int, int> DetermineTMCCCommand(byte[] buf)
        {
            if ((int)buf[0] == 0xFE)
            {
                //printf("TMCC Byte\n");
                //std::cout << std::bitset<8>((int)data[1]) << std::endl;
                Tuple<int, int, int> commandData;

                if (buf[1] == 0xFF & buf[2] == 0xFF)
                {
                   commandData = new Tuple<int, int, int>(buf[0], buf[1], buf[2]);
                }
                else
                {
                    int id_0 = ((int)buf[1] & 0b00111111) << 1;
                    int id_1 = (((int)buf[2] & 0b10000000) >> 7) & 1;
                    int engineid = id_0 | id_1;

                    //if (true)
                    //std::cout << "engine id: " << engineid << std::endl;
                    //if (engineid == loco.id) // if our command matches a known loco id
                    //{
                    int cmdid = (int)buf[2] & 0b01100000;
                    int dataid = (int)buf[2] & 0b00011111;

                    commandData = new Tuple<int, int, int>(cmdid, dataid, engineid);
                }

                
                return commandData;
            }
            else
                return null;
        }
    }
}
