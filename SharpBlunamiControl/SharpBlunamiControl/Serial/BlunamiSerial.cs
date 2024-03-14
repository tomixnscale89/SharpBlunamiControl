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

        bool bellButtonPressed = false;
        bool whistleButtonPressed = false;
        bool shortWhistleButtonPressed = false;
        bool headlightButtonPressed = false;
        bool frontCouplerButtonPressed = false;
        bool rearCouplerButtonPressed = false;
        bool boostButtonPressed = false;
        bool brakeButtonPressed = false;
        bool directionButtonPressed = false;
        float timer;
        float lastPressTime;
        float buttonHoldInterval = 0.2f;
        bool debugString = true;

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
                            if (availablePorts[comPortIndex] != null)
                            {
                                serialPort = new SerialPort(availablePorts[comPortIndex], 9600, Parity.None, 8, StopBits.One);
                                canContinueBeyondSerialPortSelection |= true;
                                Console.WriteLine("Using COM port: " + availablePorts[comPortIndex]);
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

        void ConfigureSerialPort()
        {
            serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
            serialPort.Open();
        }

        void ClosePort()
        {
            serialPort.Close();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Console.WriteLine(serialPort.ReadExisting());
            SerialPort sp = (SerialPort)sender;
            byte[] buf = new byte[sp.BytesToRead];
            Console.WriteLine("Received data," + buf.Length);
            sp.Read(buf, 0, buf.Length);
            //buf.ToList().ForEach(b => recievedData.Enqueue(b));
            if (buf.Length == 3)
                DetermineTMCCCommand(buf);
        }

        private void DetermineTMCCCommand(byte[] buf)
        {
            if ((int)buf[0] == 0xFE)
            {
                //printf("TMCC Byte\n");
                //std::cout << std::bitset<8>((int)data[1]) << std::endl;

                int id_0 = ((int)buf[1] & 0b00111111) << 1;
                int id_1 = (((int)buf[2] & 0b10000000) >> 7) & 1;
                int engineid = id_0 | id_1;

                //if (true)
                //std::cout << "engine id: " << engineid << std::endl;
                //if (engineid == loco.id) // if our command matches a known loco id
                //{
                int cmdid = (int)buf[2] & 0b01100000;
                int dataid = (int)buf[2] & 0b00011111;
                switch (cmdid)
                {

                    case (int)TMCCCommandType.CT_ACTION:
                        {
                            switch (dataid)
                            {
                                case (int)EngineCommandParams.EC_BLOW_HORN_1:
                                    {
                                        //loco.dynamoFlags |= BlunamiEngineEffectCommandParams.LONG_WHISTLE;
                                        //loco.whistleOn = true;
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

                                case (int)EngineCommandParams.EC_TOGGLE_DIRECTION:
                                    {
                                        directionButtonPressed = true;
                                        lastPressTime = timer;
                                        if (debugString)
                                            Console.WriteLine("Toggle direction pressed\n");
                                        break;
                                    }

                                case (int)EngineCommandParams.EC_AUX_1_OPTION_1:
                                    {
                                        shortWhistleButtonPressed = true;
                                        lastPressTime = timer;
                                        if (debugString)
                                            Console.WriteLine("Aux1 pressed\n");
                                        break;
                                    }

                                case (int)EngineCommandParams.EC_AUX_2_OPTION_1:
                                    {
                                        headlightButtonPressed = true;
                                        lastPressTime = timer;
                                        if (debugString)
                                            Console.WriteLine("Aux2 pressed\n");
                                        break;
                                    }

                                case (int)EngineCommandParams.EC_OPEN_FRONT_COUPLER:
                                    {
                                        frontCouplerButtonPressed = true;
                                        lastPressTime = timer;
                                        if (debugString)
                                            Console.WriteLine("Front Coupler pressed\n");
                                        break;
                                    }

                                case (int)EngineCommandParams.EC_OPEN_REAR_COUPLER:
                                    {
                                        rearCouplerButtonPressed = true;
                                        lastPressTime = timer;
                                        if (debugString)
                                            Console.WriteLine("Rear Coupler pressed\n");
                                        break;
                                    }

                                case (int)EngineCommandParams.EC_BOOST_SPEED:
                                    {
                                        boostButtonPressed = true;
                                        lastPressTime = timer;
                                        if (debugString)
                                            Console.WriteLine("Boost pressed\n");
                                        break;
                                    }

                                case (int)EngineCommandParams.EC_BRAKE_SPEED:
                                    {
                                        brakeButtonPressed = true;
                                        lastPressTime = timer;
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
                            break;
                        }

                    case (int)TMCCCommandType.CT_RELATIVE_SPEED:
                        {
                            if (debugString)
                                Console.WriteLine("Speed command\n");

                            switch (dataid)
                            {
                                case 0xA:
                                    //loco.speed += 5;
                                    break;
                                case 0x9:
                                    //loco.speed += 4;
                                    break;
                                case 0x8:
                                    //loco.speed += 3;
                                    break;
                                case 0x7:
                                    //loco.speed += 2;
                                    break;
                                case 0x6:
                                    //loco.speed += 1;
                                    break;
                                case 0x5:
                                    // loco.speed += 0;
                                    break;
                                case 0x4:
                                    // loco.speed += -1;
                                    break;
                                case 0x3:
                                    //  loco.speed += -2;
                                    break;
                                case 0x2:
                                    // loco.speed += -3;
                                    break;
                                case 0x1:
                                    //  loco.speed += -4;
                                    break;
                                case 0x0:
                                    //  loco.speed += -5;
                                    break;
                            }

                            //if (loco.speed > 126)
                            // loco.speed = 126;
                            //if (loco.speed < 0)
                            // loco.speed = 0;

                            // WriteBlunamiSpeedCommand(loco);
                            break;
                        }
                }
                //}

                //}
            }
        }
    }
}
