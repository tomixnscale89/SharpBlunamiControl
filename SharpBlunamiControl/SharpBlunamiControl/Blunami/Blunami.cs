using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

public enum BlunamiEngineEffectCommandTypes
{
    DYNAMO_OFF = 0b10000000,
    DYNAMO_ON = 0b10010000,
    EFFECT_TYPE_A = 0xA0,
    EFFECT_TYPE_B = 0xB0,
    EFFECT_TYPE_DE = 0xDE,
    EFFECT_TYPE_DF = 0xDF,
};
public enum BlunamiEngineEffectCommandParams
{
    RESET = 0xFE,
    RESET2 = 0,

    BELL = 0x1,
    LONG_WHISTLE = 0x2,
    SHORT_WHISTLE = 0x4,
    CYLINDER_COCKS = 0x8,

    A_GRADE_CROSSING_WHISTLE = 0x1,
    A_BLOWDOWN = 0x2,
    A_BRAKE_ENABLED = 0x4,
    A_BRAKE_SELECT = 0x8,

    B_CUTOFF_INCREASE = 0x1,
    B_CUTOFF_DECREASE = 0x2,
    B_DIMMER_ENABLED = 0x4,
    B_SOUND_MUTE = 0x8,

    DE_UNCOUPLE = 0x1,
    DE_MOMENTUM_DISABLE = 0x2,
    DE_HANDBRAKE_ENABLE = 0x4,
    DE_WATERSTOP = 0x8,
    DE_FUELSTOP = 0x10,
    DE_ASH_DUMP = 0x20,
    DE_WHEEL_SLIP_ENABLE = 0x40,
    DE_FUNCTION_20 = 0x80,

    DF_SANDER_VALVE = 0x1,
    DF_CAB_CHATTER = 0x2,
    DF_ALL_ABOARD = 0x4,
    DF_FX3 = 0x8,
    DF_FX4 = 0x10,
    DF_FX5 = 0x20,
    DF_FX6 = 0x40,
    DF_FX28 = 0x80
};

enum BlunamiEngineTypes
{
    // Potential values coming from CV256.
    // From: https://soundtraxx.com/content/Reference/Documentation/Reference-Documents/productID.pdf

    // BLU2200
    BLU2200_STEAM = 90,
    BLU2200_DISESL_EMD = 91,
    BLU2200_DISESL_GE = 92,
    BLU2200_DIESEL_ALCO = 93,
    BLU2200_DIESEL_BALDWIN = 94,
    BLU2200_DIESEL_EMD2 = 95,
    BLU2200_ELECTRIC = 96,

    // BLU4408
    BLU4408_STEAM = 97,
    BLU4408_DISESL_EMD = 98,
    BLU4408_DISESL_GE = 99,
    BLU4408_DIESEL_ALCO = 100,
    BLU4408_DIESEL_BALDWIN = 101,
    BLU4408_DIESEL_EMD2 = 102,
    BLU4408_ELECTRIC = 103,

    // BLUPNP8 (diesel only)
    BLUPNP8_DISESL_EMD = 104,
    BLUPNP8_DISESL_GE = 105,
    BLUPNP8_DIESEL_ALCO = 106,
    BLUPNP8_DIESEL_BALDWIN = 107,
    BLUPNP8_DIESEL_EMD2 = 108,
    BLUPNP8_ELECTRIC = 109,
};

namespace SharpBlunamiControl
{
    internal partial class BlunamiControl
    {
        

        string PrintDecoderName(int type)
        {
            switch (type)
            {
                case (int)BlunamiEngineTypes.BLU2200_STEAM: return "BLU2200 Steam2";
                case (int)BlunamiEngineTypes.BLU2200_DIESEL_ALCO: return "BLU2200 DIESEL ALCO";
                case (int)BlunamiEngineTypes.BLU2200_DIESEL_BALDWIN: return "BLU2200 DIESEL BALDWIN";
                case (int)BlunamiEngineTypes.BLU2200_DIESEL_EMD2: return "BLU2200 DISESL EMD-2";
                case (int)BlunamiEngineTypes.BLU2200_DISESL_EMD: return "BLU2200 DIESEL EMD";
                case (int)BlunamiEngineTypes.BLU2200_DISESL_GE: return "BLU2200 DIESEL GE";
                case (int)BlunamiEngineTypes.BLU2200_ELECTRIC: return "BLU2200 ELECTRIC";
                case (int)BlunamiEngineTypes.BLU4408_STEAM: return "BLU4408 Steam2";
                case (int)BlunamiEngineTypes.BLU4408_DIESEL_ALCO: return "BLU4408 DIESEL ALCO";
                case (int)BlunamiEngineTypes.BLU4408_DIESEL_BALDWIN: return "BLU4408 DIESEL BALDWIN";
                case (int)BlunamiEngineTypes.BLU4408_DIESEL_EMD2: return "BLU4408 DISESL EMD-2";
                case (int)BlunamiEngineTypes.BLU4408_DISESL_EMD: return "BLU4408 DIESEL EMD";
                case (int)BlunamiEngineTypes.BLU4408_DISESL_GE: return "BLU4408 DIESEL GE";
                case (int)BlunamiEngineTypes.BLU4408_ELECTRIC: return "BLU4408 ELECTRIC";
                case (int)BlunamiEngineTypes.BLUPNP8_DIESEL_ALCO: return "BLUPNP8 DIESEL ALCO";
                case (int)BlunamiEngineTypes.BLUPNP8_DIESEL_BALDWIN: return "BLUPNP8 DIESEL BALDWIN";
                case (int)BlunamiEngineTypes.BLUPNP8_DIESEL_EMD2: return "BLUPNP8 DISESL EMD-2";
                case (int)BlunamiEngineTypes.BLUPNP8_DISESL_EMD: return "BLUPNP8 DIESEL EMD";
                case (int)BlunamiEngineTypes.BLUPNP8_DISESL_GE: return "BLUPNP8 DIESEL GE";
                default: return "Error Occured, likely data was interrupted by another fire_and_forget";
            }
        }



        public async void ReadDecoderType(BlunamiEngine loco)
        {
            if (loco.BluetoothLeDevice != null)
            {
                try
                {
                    var readCVCommand = new BlunamiCommandBase().baseReadCVCommand;
                    // to read Decoder type, this comes from CV256. We must change the 3rd byte to be 0x75 to start reading
                    // CV256 and beyond. Since CV256 is the first cv in this new range, we just set the 4th byte to 0. If we read
                    // CV257 instead, we'd set the 4th byte to 1. 
                    readCVCommand[3] = 0x75;
                    readCVCommand[4] = 0;
                    var writer = new DataWriter();
                    writer.WriteBytes(readCVCommand);

                    GattCommunicationStatus result = await loco.BlunamiCharactertisic.WriteValueAsync(writer.DetachBuffer());
                    if (result == GattCommunicationStatus.Success)
                    {
                        // Successfully wrote to Blunami. Now, we have to read what the result from the same characteristic
                        GattReadResult readResult = await loco.BlunamiCharactertisic.ReadValueAsync();
                        if (readResult.Status == GattCommunicationStatus.Success)
                        {
                            var reader = DataReader.FromBuffer(readResult.Value);
                            byte[] input = new byte[reader.UnconsumedBufferLength];
                            reader.ReadBytes(input);
                            loco.DecoderType = input[5];
                            Console.WriteLine(PrintDecoderName(loco.DecoderType));
                            // Utilize the data as needed
                        }
                    }
                }
                catch
                {

                }
            }
        }

         async Task<int> ReadCVFromDecoder(BlunamiEngine loco, int cv)
        {
            int cvData = -1;
            if(loco.BluetoothLeDevice != null)
            {
                try
                {
                    byte[] readCVCommand = new BlunamiCommandBase().baseReadCVCommand;
                    readCVCommand[4] = (byte)cv;

                    cvData = await WriteThenReadDataToBlunami(loco, readCVCommand);
                }
                catch
                {

                }
            }
            return cvData;
        }

        async Task WriteDataOnlyToBlunami(BlunamiEngine loco, byte[] data)
        {
            var writer = new DataWriter();
            if (loco.BluetoothLeDevice != null)
            {
                try
                {
                    if(loco.BlunamiCharactertisic != null)
                    {
                        GattWriteResult result = await loco.BlunamiCharactertisic.WriteValueWithResultAsync(writer.DetachBuffer()); ;
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            // Successfully wrote to device
                        }
                    }
                }
                catch
                {

                }
            }
        }

        async Task<int> WriteThenReadDataToBlunami(BlunamiEngine loco, byte[] data)
        {
            int dataFound = -1;
            if (loco.BluetoothLeDevice != null)
            {
                try
                {
                    byte[] readCVCommand = new BlunamiCommandBase().baseReadCVCommand;
                    // to read Decoder type, this comes from CV256. We must change the 3rd byte to be 0x75 to start reading
                    // CV256 and beyond. Since CV256 is the first cv in this new range, we just set the 4th byte to 0. If we read
                    // CV257 instead, we'd set the 4th byte to 1. 
                    readCVCommand[3] = 0x75;
                    readCVCommand[4] = 0;
                    var writer = new DataWriter();
                    writer.WriteBytes(readCVCommand);

                    if (loco.BlunamiCharactertisic != null)
                    {
                        GattWriteResult result = await loco.BlunamiCharactertisic.WriteValueWithResultAsync(writer.DetachBuffer());
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            // Successfully wrote to Blunami. Now, we have to read what the result from the same characteristic
                            GattReadResult readResult = await loco.BlunamiCharactertisic.ReadValueAsync(BluetoothCacheMode.Uncached);
                            if (readResult.Status == GattCommunicationStatus.Success)
                            {
                                var reader = DataReader.FromBuffer(readResult.Value);
                                byte[] input = new byte[reader.UnconsumedBufferLength];
                                reader.ReadBytes(input);
                                dataFound = input[5];
                                // Utilize the data as needed
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not find Blunami Characteristic returning invalid dataFound");
                    }

                }
                catch
                {

                }
            }
            else
            {
                Console.WriteLine("Could not find Blunami Device. Returning invalid dataFound");

                dataFound = -1;
            }
            return dataFound;
        }




        public async Task<GattCharacteristic> GetBlunamiDCCCharacteristic(BlunamiEngine loco)
        {
            GattDeviceServicesResult result = await loco.BluetoothLeDevice.GetGattServicesAsync();
            GattCharacteristic dccCharacteristic = null;
            if (result.Status == GattCommunicationStatus.Success)
            {
                var services = result.Services;
                foreach (var service in services)
                {
                    if (service.Uuid.ToString() == BlunamiCommandBase.blunamiServiceStr)
                    {
                        //Console.WriteLine(service.Uuid.ToString());
                        GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync();

                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            var characteristics = characteristicResult.Characteristics;

                            foreach (var characteristic in characteristics)
                            {
                                if (characteristic.Uuid.ToString() == BlunamiCommandBase.blunamiDCCCharacteristicStr)
                                {
                                    //Console.WriteLine(characteristic.Uuid.ToString());
                                    dccCharacteristic = characteristic;
                                }
                            }
                        }
                    }
                }
            }
            if (dccCharacteristic == null)
            {
                Console.WriteLine("An error occured. Could not find Blunami Characteristic in BLE services list.");
            }
            return dccCharacteristic;
        }

        async Task WriteBlunamiDynamoGroupEffectCommand(BlunamiEngine loco)
        {
            if (loco.BluetoothLeDevice != null)
            {
                try
                {
                    byte[] baseCommand = new BlunamiCommandBase().baseCommand;

                    switch (loco.DecoderType)
                    {
                        // for 2200 decoders, the byte 2 should be 0x02. 
                        case (int)BlunamiEngineTypes.BLU2200_STEAM:
                        case (int)BlunamiEngineTypes.BLU2200_DIESEL_ALCO:
                        case (int)BlunamiEngineTypes.BLU2200_DIESEL_BALDWIN:
                        case (int)BlunamiEngineTypes.BLU2200_DIESEL_EMD2:
                        case (int)BlunamiEngineTypes.BLU2200_DISESL_EMD:
                        case (int)BlunamiEngineTypes.BLU2200_DISESL_GE:
                        case (int)BlunamiEngineTypes.BLU2200_ELECTRIC:
                            {
                                baseCommand[1] = 0x02;
                                break;
                            }
                        // for 4408 decoders, the byte 2 should be 0x03. 
                        case (int)BlunamiEngineTypes.BLU4408_STEAM:
                        case (int)BlunamiEngineTypes.BLU4408_DIESEL_ALCO:
                        case (int)BlunamiEngineTypes.BLU4408_DIESEL_BALDWIN:
                        case (int)BlunamiEngineTypes.BLU4408_DIESEL_EMD2:
                        case (int)BlunamiEngineTypes.BLU4408_DISESL_EMD:
                        case (int)BlunamiEngineTypes.BLU4408_DISESL_GE:
                        case (int)BlunamiEngineTypes.BLU4408_ELECTRIC:
                            {
                                baseCommand[1] = 0x03;
                                break;
                            }

                        default:
                            Console.WriteLine("Failure to determine Decoder type: WriteBlunamiDynamoGroupEffectCommand");
                            return;

                    }

                    if (loco.UsesLongAddress)
                    {
                        baseCommand[2] = (byte)loco.CV17;
                        baseCommand[3] = (byte)loco.CV18;

                        if (loco.Headlight)
                            baseCommand[4] = (byte)BlunamiEngineEffectCommandTypes.DYNAMO_ON;
                        else
                            baseCommand[4] = (byte)BlunamiEngineEffectCommandTypes.DYNAMO_OFF;

                        baseCommand[4] += (byte)loco.DynamoFlags;
                    }
                    else
                    {
                        baseCommand[2] = (byte)loco.Id;
                        if (loco.Headlight)
                            baseCommand[3] = (byte)BlunamiEngineEffectCommandTypes.DYNAMO_ON;
                        else
                            baseCommand[3] = (byte)BlunamiEngineEffectCommandTypes.DYNAMO_OFF;

                        baseCommand[3] += (byte)loco.DynamoFlags;

                        //std.wcout << (int)dataToWrite[3] << std.endl;

                    }

                    var writer = new DataWriter();
                    writer.WriteBytes(baseCommand);
                    if (loco.BlunamiCharactertisic != null)
                    {
                        GattWriteResult result = await loco.BlunamiCharactertisic.WriteValueWithResultAsync(writer.DetachBuffer());
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            Console.WriteLine("Successfully wrote DynamoEffectPacket to train");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not find Blunami Characteristic returning invalid shortID");

                    }

                }
                catch
                {

                }
            }

        }
    }



    class BlunamiCommandBase
    {
        public byte[] baseCommand = new byte[] { 0x02, 0x02, 0x00, 0x00, 0x00, 0x00 }; // replace third byte with engine ID
        public byte[] baseSpeedCommand = new byte[] { 0x02, 0x03, 0x00, 0x3F, 0x00, 0x00 }; // replace third byte with engine ID. Change the 4th bit to 0x3F if short digit, 5th bit to 0x3F if long digit. 
        public byte[] baseReadCVCommand = new byte[] { 0x03, 0x04, 0x00, 0x74, 0xFF, 0x01, 0x00 }; // replace FF with CV in Hex to read
        public static int DCC_LONG_ADDRESS_CONSTANT = 49152;
        public static string blunamiServiceStr = "f688fd00-da9a-2e8f-d003-ebcdcc259af9";
        public static string blunamiDCCCharacteristicStr = "f688fd1d-da9a-2e8f-d003-ebcdcc259af9";
    }

    public class BlunamiEngine
    {
        BluetoothLEDevice bluetoothLeDevice = null;
        GattCharacteristic blunamiCharacteristic = null;
        int id = 3; // DCC defaults at 3
        int longID = 1234; // This is a quickID used for engines that make use of the long address. 
        int CV17Value = 0;
        int CV18Value = 0;
        int decoderType = 90; // set default to 2200 Steam2
        int speed = 0;

        BlunamiEngineEffectCommandParams dynamoFlags = (BlunamiEngineEffectCommandParams)0;
        BlunamiEngineEffectCommandParams aFlags = (BlunamiEngineEffectCommandParams)0;
        BlunamiEngineEffectCommandParams bFlags = (BlunamiEngineEffectCommandParams)0;
        BlunamiEngineEffectCommandParams deFlags = (BlunamiEngineEffectCommandParams)0;
        BlunamiEngineEffectCommandParams dfFlags = (BlunamiEngineEffectCommandParams)0;

        bool whistleOn = false;
        bool shortWhistleOn = false;
        bool bellOn = false;
        bool headlightOn = false;
        bool direction = true; // true - forward, false - backward
                               //bool cylinderCocksOn = false;

        //// A range
        //bool gradeCrossingWhistle = false;
        //bool blowdownEnabled = false;
        //bool brakeEnabled = false;
        //bool brakeSelectTrainInd = false;

        //// B range
        //bool cutoffIncrease = false;
        //bool cutoffDecrease = false;
        //bool dimmerOn = false;
        //bool muted = false;

        //// DE range
        //bool uncoupleOn = false;
        //bool momentumOn = false;
        //bool handbrakeOn = false;
        //bool waterStopOn = false;
        //bool fuelStopOn = false;
        //bool ashDumpOn = false;
        //bool wheelSlipOn = false;
        //bool f20On = false;

        //// DF range
        //bool sanderValveOn = false;
        //bool cabChatterOn = false;
        //bool allAboard = false;
        //bool FX3On = false;
        //bool FX4On = false;
        //bool FX5On = false;
        //bool FX6On = false;
        //bool FX28On = false;

        bool usesLongAddress = false;

        bool isBellOn()
        {
            return (dynamoFlags & BlunamiEngineEffectCommandParams.BELL) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isWhistleOn()
        {
            return (dynamoFlags & BlunamiEngineEffectCommandParams.LONG_WHISTLE) != (BlunamiEngineEffectCommandParams)0;
        }
        bool isShortWhistleOn()
        {
            return (dynamoFlags & BlunamiEngineEffectCommandParams.SHORT_WHISTLE) != (BlunamiEngineEffectCommandParams)0;
        }

        public BlunamiEngine(BluetoothLEDevice bluetoothLEDevice, int address, int decoderType, int speed)
        {
            this.bluetoothLeDevice = bluetoothLEDevice;
            //blunamiCharacteristic = await GetBlunamiDCCCharacteristic(bluetoothLEDevice);

            //this.blunamiCharacteristic = characteristic;
            this.id = address;
            //this.longID = longID;
            this.decoderType = decoderType;
            this.speed = speed;

            Task.Run(async () =>
            {
              this.blunamiCharacteristic = await GetBlunamiDCCCharacteristic(bluetoothLEDevice);
            }).GetAwaiter().GetResult();

            if (id > 98)
            {
                this.usesLongAddress = true;
                this.CV17Value = (this.id + 49152) >> 8;
                this.CV18Value = (this.id + 49152) & 0xFF;
                Console.WriteLine("CV17{0},CV18{1}", this.CV17Value, this.CV18Value);
            }
            else
            {
                this.usesLongAddress = false;
            }

        }

        private async Task<GattCharacteristic> GetBlunamiDCCCharacteristic(BluetoothLEDevice bluetoothLEDevice)
        {
            GattDeviceServicesResult result = await bluetoothLEDevice.GetGattServicesAsync();
            GattCharacteristic dccCharacteristic = null;
            if (result.Status == GattCommunicationStatus.Success)
            {
                var services = result.Services;
                foreach (var service in services)
                {
                    if (service.Uuid.ToString() == BlunamiCommandBase.blunamiServiceStr)
                    {
                        //Console.WriteLine(service.Uuid.ToString());
                        GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync();

                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            var characteristics = characteristicResult.Characteristics;

                            foreach (var characteristic in characteristics)
                            {
                                if (characteristic.Uuid.ToString() == BlunamiCommandBase.blunamiDCCCharacteristicStr)
                                {
                                    //Console.WriteLine(characteristic.Uuid.ToString());
                                    dccCharacteristic = characteristic;
                                }
                            }
                        }
                    }
                }
            }
            if (dccCharacteristic == null)
            {
                Console.WriteLine("An error occured. Could not find Blunami Characteristic in BLE services list.");
            }
            return dccCharacteristic;
        }

        public BluetoothLEDevice BluetoothLeDevice
        {
            get
            {
                return bluetoothLeDevice;
            }
            set
            {
                bluetoothLeDevice = value;
            }
        }

        public GattCharacteristic BlunamiCharactertisic
        {
            get
            {
                return blunamiCharacteristic;
            }
            set
            {
                blunamiCharacteristic = value;
            }
        }

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public int LongID
        {
            get
            {
                return longID;
            }
            set
            {
                longID = value;
            }
        }

        public int CV17
        {
            get
            {
                return CV17Value;
            }
            set
            {
                CV17Value = value;
            }
        }

        public int CV18
        {
            get
            {
                return CV18Value;
            }
            set
            {
                CV18Value = value;
            }
        }

        public int DecoderType
        {
            get
            {
                return decoderType;
            }
            set
            {
                decoderType = value;
            }
        }

        public int Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        public bool Whistle
        {
            get
            {
                return whistleOn;
            }
            set
            {
                whistleOn = value;
            }
        }

        public bool ShortWhistle
        {
            get
            {
                return shortWhistleOn;
            }
            set
            {
                shortWhistleOn = value;
            }

        }

        public bool Headlight
        {
            get
            {
                return headlightOn;
            }
            set
            {
                headlightOn = value;
            }
        }

        public bool Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
            }
        }

        public bool UsesLongAddress
        {
            get
            {
                return usesLongAddress;
            }
            set
            {
                usesLongAddress = value;
            }
        }

        public BlunamiEngineEffectCommandParams DynamoFlags
        {
            get
            {
                return dynamoFlags;
            }
            set
            {
                dynamoFlags = (BlunamiEngineEffectCommandParams)value;
            }
        }

        public int DCC_LONG_ADDRESS_CONSTANT
        {
            get
            {
                return DCC_LONG_ADDRESS_CONSTANT;
            }
        }
    }
}


