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
        bool setButtonPressed = false;
        bool lowMomentumPressed,medMomentumPressed,highMomentumPressed = false;

        bool num0Pressed, num1Pressed, num2Pressed, num3Pressed, num4Pressed, num5Pressed, num6Pressed, num7Pressed, num8Pressed, num9Pressed = false;

        //
        int lastFoundTMCCID;
        BlunamiEngine lastUsedEngine;

        float timer;
        float lastPressTime;
        float buttonHoldInterval = 115;
        bool debugString = false;


        int keyPadCVPage = 0;


        /*
         
        PAGE 0:

        SXS , SHORT WHISTLE , CYLINDER COCKS
        CUTOFF+, CUTOFF-, BLOWDOWN
        DIMMER, COUPLE/UNCOUPLE, SWITCHING MODE
                MUTE

        PAGE 1:

        WHEEL CHAINS, WATER STOP, FUEL LOADING
        ASH DUMP, WHEEL SLIP, INJECTOR
        SANDER VALVE, CAB CHATTER, ALL ABOARD/COACH DOORS
                        MUTE

        PAGE 2:

        NOTHING, NOTHING, FX3,
        FX4, FX5, FX6
        FX28, NOTHING, NOTHING
               MUTE
         
         
         
         */


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
            //serialPort.ReadTimeout = 500;
            //serialPort.ReceivedBytesThreshold = 3;
            serialPort.Open();
        }

        void ClosePort()
        {
            serialPort.Close();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            stopWatch.Restart();
            byte[] buf = new byte[3];
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    buf[i] = (byte)serialPort.ReadByte();
                }

                //if (debugString)
                //    Console.WriteLine("Hex: " + BitConverter.ToString(buf));
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

                    if(TMCCPacket != null) // Legacy packets crash here, need to properly parse this
                    {
                        if (TMCCPacket.Item2 == 0xFF & TMCCPacket.Item3 == 0xFF)
                        {
                            //Console.WriteLine("TMCCPacket.Item2 {0} TMCCPacket.Item3 {1}", TMCCPacket.Item2, TMCCPacket.Item3);
                            Console.WriteLine("Stopping all Blunami trains....");
                            foreach(var blunami in FoundBlunamiDevices)
                            {
                                Console.WriteLine("Stopping: {0}....",blunami.BluetoothLeDevice.Name);

                                SetTMCCAbsoluteSpeedState(0, blunami);
                                //Console.Write("done.");


                            }
                            Console.Write("done stopping trains.");

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
                                                SetTMCCActionButtonSates(TMCCPacket.Item2, blunami);
                                                break;
                                            }
                                        case (int)TMCCCommandType.CT_RELATIVE_SPEED:
                                            {
                                                SetTMCCSpeedState(TMCCPacket.Item2, blunami);

                                                break;
                                            }
                                        case (int)TMCCCommandType.CT_ABSOLUTE_SPEED:
                                            {
                                                SetTMCCAbsoluteSpeedState(TMCCPacket.Item2, blunami);

                                                break;
                                            }
                                        case (int)TMCCCommandType.CT_EXTENDED: // SET Button, Momentum buttons
                                            {
                                                SetTMCCExtendedButtonSates(TMCCPacket.Item2, blunami);
                                                break;
                                            }
                                    }

                                }
                            }
                        }
                    }
                }
            }
     
            catch (TimeoutException)
            {
                Console.WriteLine("Serial COM Timeout, too much data received. Please exit application.");
            }
            
        }

        void SetTMCCSpeedState(int dataid, BlunamiEngine loco)
        {
            
            switch (dataid)
            {
                case 0xA:
                    loco.Speed += 5;
                    break;
                case 0x9:
                    loco.Speed += 4;
                    break;
                case 0x8:
                    loco.Speed += 3;
                    break;
                case 0x7:
                    loco.Speed += 2;
                    break;
                case 0x6:
                    loco.Speed += 1;
                    break;
                case 0x5:
                    loco.Speed += 0;
                    break;
                case 0x4:
                    loco.Speed += -1;
                    break;
                case 0x3:
                    loco.Speed += -2;
                    break;
                case 0x2:
                    loco.Speed += -3;
                    break;
                case 0x1:
                    loco.Speed += -4;
                    break;
                case 0x0:
                    loco.Speed += -5;
                    break;
            }

            if (loco.Speed > 126)
                loco.Speed = 126;
            if (loco.Speed < 0)
                loco.Speed = 0;

            WriteBlunamiSpeedCommand(loco);

            if (showDialogueText)
                Console.WriteLine("{0}: Speed: {1}", loco.BluetoothLeDevice.Name, loco.Speed);
        }

        void SetTMCCAbsoluteSpeedState(int dataid, BlunamiEngine loco)
        {

            // Data id here should be the speed from 0 to 31. Let's map the speed steps. It won't be great, but it is something.

            if (dataid == 0)
                loco.Speed = 0;
            else
                loco.Speed = ((126 / 31) * dataid);

            if (loco.Speed > 126)
                loco.Speed = 126;

            WriteBlunamiSpeedCommand(loco);
                            
            if (showDialogueText)
                Console.WriteLine("{0}: Absolute Speed: {1}", loco.BluetoothLeDevice.Name, loco.Speed);
        }

        void SetTMCCActionButtonSates(int dataid, BlunamiEngine loco)
        {
            switch (dataid)
            {
               
                case (int)EngineCommandParams.EC_BLOW_HORN_1:
                case (int)EngineCommandParams.EC_BLOW_HORN_2:
                    {

                        loco.Whistle = true;
                        whistleButtonPressed = true;
                        Task.Run(async () =>
                        {
                            await WriteBlunamiDynamoGroupEffectCommand(loco).ConfigureAwait(false);
                            Console.WriteLine("{0}: Whistle: {1}", loco.BluetoothLeDevice.Name, loco.Whistle ? "On" : "Off");

                        }).GetAwaiter().GetResult();
                        if (debugString)
                            Console.WriteLine("Horn pressed");
                        break;
                    }

                case (int)EngineCommandParams.EC_RING_BELL:
                    {
                        bellButtonPressed = true;
                        if (debugString)
                            Console.WriteLine("Bell pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_TOGGLE_DIRECTION:
                    {
                        directionButtonPressed = true;                                               
                        if (debugString)
                            Console.WriteLine("Toggle direction pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_AUX_1_OPTION_1:
                    {
                        shortWhistleButtonPressed = true;
                        if (debugString)
                            Console.WriteLine("Aux1 pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_AUX_2_OPTION_1:
                    {
                        headlightButtonPressed = true;
                        if (debugString)
                            Console.WriteLine("Aux2 pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_OPEN_FRONT_COUPLER:
                    {
                        frontCouplerButtonPressed = true;
                        if (debugString)
                            Console.WriteLine("Front Coupler pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_OPEN_REAR_COUPLER:
                    {
                        rearCouplerButtonPressed = true;
                        if (debugString)
                            Console.WriteLine("Rear Coupler pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_BOOST_SPEED:
                    {
                        boostButtonPressed = true;
                        if (debugString)
                            Console.WriteLine("Boost pressed\n");
                        break;
                    }

                case (int)EngineCommandParams.EC_BRAKE_SPEED:
                    {
                        brakeButtonPressed = true;
                        if (debugString)
                            Console.WriteLine("Brake pressed\n");
                        break;
                    }

                default:
                    {
                        dataid &= (int)EngineCommandParams.EC_NUMERIC_MASK;
                        if (debugString)
                            Console.WriteLine("Numerical pressed: {0}", dataid);
                        switch (dataid)
                        {
                            case 0:
                                num0Pressed = true;
                                break;
                            case 1:
                                num1Pressed = true;
                                break;
                            case 2:
                                num2Pressed = true;
                                break;
                            case 3:
                                num3Pressed = true;
                                break;
                            case 4:
                                num4Pressed = true;
                                break;
                            case 5:
                                num5Pressed = true;
                                break;
                            case 6:
                                num6Pressed = true;
                                break;
                            case 7:
                                num7Pressed = true;
                                break;
                            case 8:
                                num8Pressed = true;
                                break;
                            case 9:
                                num9Pressed = true;
                                break;
                        }
                        break;
                    }

            }
            lastPressTime = (float)stopWatch.Elapsed.TotalMilliseconds;
        }

        void SetTMCCExtendedButtonSates(int dataid, BlunamiEngine loco)
        {
            switch (dataid)
            {

                case (int)EngineCommandParams.ETC_SET_TMCC_ADDRESS:
                    {
                        if (debugString)
                            Console.WriteLine("Set Address Button", dataid);
                        setButtonPressed = true;
                        break;
                    }

                case (int)EngineCommandParams.ETC_SET_MOMENTUM_HIGH:
                    {
                        if (debugString)
                            Console.WriteLine("Momentum High Button", dataid);
                        break;
                    }

                case (int)EngineCommandParams.ETC_SET_MOMENTUM_MEDIUM:
                    {
                        if (debugString)
                            Console.WriteLine("Momentum Medium Button", dataid);
                        break;
                    }

                case (int)EngineCommandParams.ETC_SET_MOMENTUM_LOW:
                    {
                        if (debugString)
                            Console.WriteLine("Momentum Low Button", dataid);
                        break;
                    }


                default:
                    {
                        break;
                    }

            }
            lastPressTime = (float)stopWatch.Elapsed.TotalMilliseconds;
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
                    int cmdid = ((int)buf[2] & 0b01100000) >> 5;
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
