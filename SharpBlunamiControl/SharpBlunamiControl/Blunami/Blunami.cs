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

    BELL = 0x1, //F1
    LONG_WHISTLE = 0x2, // F2 Diesel - Airhorn
    SHORT_WHISTLE = 0x4,//F3, Diesel - Short Airhorn
    CYLINDER_COCKS = 0x8, // F4, Diesel - Dynamic Brake, Electric - Panto Raise/Lower

    A_GRADE_CROSSING_WHISTLE = 0x1, // F9
    A_BLOWDOWN = 0x2, // F10 , Diesel - Straight to Eight, Electric - Nothing
    A_BRAKE_ENABLED = 0x4, // F11
    A_BRAKE_SELECT = 0x8, // F12

    B_CUTOFF_INCREASE = 0x1,// F5, Diesel - RPM+ / Engine Startup, Electric - Stop request Bell
    B_CUTOFF_DECREASE = 0x2, // F6, Diesel - RPM- / Engine Shutdown, Electric - Pneumatic Doors
    B_DIMMER_ENABLED = 0x4, // F7
    B_SOUND_MUTE = 0x8, // F8

    DE_UNCOUPLE = 0x1, // F13
    DE_MOMENTUM_DISABLE = 0x2, // F14, switching? 
    DE_HANDBRAKE_ENABLE = 0x4, // F15, wheel chains? 
    DE_WATERSTOP = 0x8, // F16, Diesel - HEP Mode, Electric - Nothing
    DE_FUELSTOP = 0x10, // F17, Diesel - Fuel Loading, Electric - Nothing
    DE_ASH_DUMP = 0x20, //F18, Diesel - General Service
    DE_WHEEL_SLIP_ENABLE = 0x40, // F19,  Diesel - Straight to Idle, Electric - Nothing
    DE_FUNCTION_20 = 0x80, // F20,  Diesel - Steam Generator and Auxilary Help Generator

    DF_SANDER_VALVE = 0x1, // F21
    DF_CAB_CHATTER = 0x2, // F22
    DF_ALL_ABOARD = 0x4, // F23
    DF_FX3 = 0x8, // F24
    DF_FX4 = 0x10, // F25
    DF_FX5 = 0x20, // F26
    DF_FX6 = 0x40, // F27
    DF_FX28 = 0x80 // F28
};

enum BlunamiEngineTypes
{
    BLUERAIL_TAMVALLEY = 0,
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

    //BLU-21PNEM8
    BLU21PNEM8_STEAM = 109,
    BLU21PNEM8_DISESL_EMD = 110,
    BLU21PNEM8_DISESL_GE = 111,
    BLU21PNEM8_DIESEL_ALCO = 112,
    BLU21PNEM8_DIESEL_BALDWIN = 113,
    BLU21PNEM8_DIESEL_EMD2 = 114,
    BLU21PNEM8_ELECTRIC = 115,

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
                case (int)BlunamiEngineTypes.BLUERAIL_TAMVALLEY: return "BLUERAIL BY TAMVALLEY";
                default: return "Unknown Board Type";
            }
        }



        public async void ReadDecoderType(BlunamiEngine loco)
        {
            if (loco.BluetoothLeDevice != null && loco.BluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
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
            if(loco.BluetoothLeDevice != null && loco.BluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
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
            if (loco.BluetoothLeDevice != null && loco.BluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
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
            if (loco.BluetoothLeDevice != null && loco.BluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
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

            if (loco.BluetoothLeDevice != null && loco.BluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                try
                {
                    byte[] baseCommand = new BlunamiCommandBase().baseCommand;

                    //switch (loco.DecoderType)
                    //{
                    //    // for 2200 decoders, the byte 2 should be 0x02. 
                    //    case (int)BlunamiEngineTypes.BLU2200_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU2200_ELECTRIC:
                    //    case (int)BlunamiEngineTypes.BLUERAIL_TAMVALLEY:

                    //        {
                    //            baseCommand[1] = 0x02;
                    //            break;
                    //        }
                    //    // for 4408 decoders, the byte 2 should be 0x03. 
                    //    case (int)BlunamiEngineTypes.BLU4408_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU4408_ELECTRIC:
                    //        {
                    //            baseCommand[1] = 0x03;
                    //            break;
                    //        }

                    //    default:
                    //        Console.WriteLine("Failure to determine Decoder type: WriteBlunamiDynamoGroupEffectCommand");
                    //        return;

                    //}

                    if (loco.UsesLongAddress)
                    {
                        baseCommand[1] = 0x03;

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
                        baseCommand[1] = 0x02;
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
                            if(debugString)
                                Console.WriteLine("Successfully wrote DynamoEffectPacket to {0}",loco.BluetoothLeDevice.Name);
                        }
                        else
                        {
                            Console.WriteLine("WriteBlunamiDynamoGroupEffectCommand: Error Writing to Blunami Characteristic:{0}", result.Status.ToString());
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

        async Task WriteBlunamiAGroupEffectCommand(BlunamiEngine loco)
        {
            if (loco.BluetoothLeDevice != null && loco.BluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                try
                {
                    byte[] baseCommand = new BlunamiCommandBase().baseCommand;

                    //switch (loco.DecoderType)
                    //{
                    //    // for 2200 decoders, the byte 2 should be 0x02. 
                    //    case (int)BlunamiEngineTypes.BLU2200_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU2200_ELECTRIC:
                    //    case (int)BlunamiEngineTypes.BLUERAIL_TAMVALLEY:

                    //        {
                    //            baseCommand[1] = 0x02;
                    //            break;
                    //        }
                    //    // for 4408 decoders, the byte 2 should be 0x03. 
                    //    case (int)BlunamiEngineTypes.BLU4408_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU4408_ELECTRIC:
                    //        {
                    //            baseCommand[1] = 0x03;
                    //            break;
                    //        }

                    //    default:
                    //        Console.WriteLine("Failure to determine Decoder type: WriteBlunamiAGroupEffectCommand");
                    //        return;

                    //}

                    if (loco.UsesLongAddress)
                    {
                        baseCommand[1] = 0x03;
                        baseCommand[2] = (byte)loco.CV17;
                        baseCommand[3] = (byte)loco.CV18;
                        baseCommand[4] = (byte)0xA0;
                        baseCommand[4] += (byte)loco.AFlags;
                    }
                    else
                    {
                        baseCommand[1] = 0x02;
                        baseCommand[2] = (byte)loco.Id;
                        baseCommand[3] += (byte)0xA0;
                        baseCommand[3] += (byte)loco.AFlags;

                        //std.wcout << (int)dataToWrite[3] << std.endl;

                    }

                    var writer = new DataWriter();
                    writer.WriteBytes(baseCommand);
                    if (loco.BlunamiCharactertisic != null)
                    {
                        GattWriteResult result = await loco.BlunamiCharactertisic.WriteValueWithResultAsync(writer.DetachBuffer());
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            //Console.WriteLine("Successfully wrote AEffectPacket to train");
                        }
                        else
                        {
                            Console.WriteLine("WriteBlunamiAGroupEffectCommand: Error Writing to Blunami Characteristic:{0}", result.Status.ToString());
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

        async Task WriteBlunamiBGroupEffectCommand(BlunamiEngine loco)
        {
            if (loco.BluetoothLeDevice != null && loco.BluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                try
                {
                    byte[] baseCommand = new BlunamiCommandBase().baseCommand;

                    //switch (loco.DecoderType)
                    //{
                    //    // for 2200 decoders, the byte 2 should be 0x02. 
                    //    case (int)BlunamiEngineTypes.BLU2200_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU2200_ELECTRIC:
                    //    case (int)BlunamiEngineTypes.BLUERAIL_TAMVALLEY:

                    //        {
                    //            baseCommand[1] = 0x02;
                    //            break;
                    //        }
                    //    // for 4408 decoders, the byte 2 should be 0x03. 
                    //    case (int)BlunamiEngineTypes.BLU4408_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU4408_ELECTRIC:
                    //        {
                    //            baseCommand[1] = 0x03;
                    //            break;
                    //        }

                    //    default:
                    //        Console.WriteLine("Failure to determine Decoder type: WriteBlunamiBGroupEffectCommand");
                    //        return;

                    //}

                    if (loco.UsesLongAddress)
                    {
                        baseCommand[1] = 0x03;
                        baseCommand[2] = (byte)loco.CV17;
                        baseCommand[3] = (byte)loco.CV18;
                        baseCommand[4] = (byte)0xB0;
                        baseCommand[4] += (byte)loco.BFlags;
                    }
                    else
                    {
                        baseCommand[1] = 0x02;
                        baseCommand[2] = (byte)loco.Id;
                        baseCommand[3] += (byte)0xB0;
                        baseCommand[3] += (byte)loco.BFlags;

                        //std.wcout << (int)dataToWrite[3] << std.endl;

                    }

                    var writer = new DataWriter();
                    writer.WriteBytes(baseCommand);
                    if (loco.BlunamiCharactertisic != null)
                    {
                        GattWriteResult result = await loco.BlunamiCharactertisic.WriteValueWithResultAsync(writer.DetachBuffer());
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            //Console.WriteLine("Successfully wrote AEffectPacket to train");
                        }
                        else
                        {
                            Console.WriteLine("WriteBlunamiBGroupEffectCommand: Error Writing to Blunami Characteristic:{0}", result.Status.ToString());
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

        async Task WriteBlunamiDEGroupEffectCommand(BlunamiEngine loco)
        {
            if (loco.BluetoothLeDevice != null && loco.BluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                try
                {
                    byte[] baseCommand = new BlunamiCommandBase().baseCommand;

                    //switch (loco.DecoderType)
                    //{
                    //    // for 2200 decoders, the byte 2 should be 0x02. 
                    //    case (int)BlunamiEngineTypes.BLU2200_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU2200_ELECTRIC:
                    //    case (int)BlunamiEngineTypes.BLUERAIL_TAMVALLEY:

                    //        {
                    //            baseCommand[1] = 0x03;
                    //            break;
                    //        }
                    //    // for 4408 decoders, the byte 2 should be 0x03. 
                    //    case (int)BlunamiEngineTypes.BLU4408_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU4408_ELECTRIC:
                    //        {
                    //            baseCommand[1] = 0x04;
                    //            break;
                    //        }

                    //    default:
                    //        Console.WriteLine("Failure to determine Decoder type: WriteBlunamiDEGroupEffectCommand");
                    //        return;

                    //}

                    if (loco.UsesLongAddress)
                    {
                        baseCommand[1] = 0x04;

                        baseCommand[2] = (byte)loco.CV17;
                        baseCommand[3] = (byte)loco.CV18;
                        baseCommand[4] = (byte)0xDE;
                        baseCommand[5] += (byte)loco.DEFlags;
                    }
                    else
                    {
                        baseCommand[1] = 0x03;

                        baseCommand[2] = (byte)loco.Id;
                        baseCommand[3] = (byte)0xDE;
                        baseCommand[4] += (byte)loco.DEFlags;

                        //std.wcout << (int)dataToWrite[3] << std.endl;

                    }

                    var writer = new DataWriter();
                    writer.WriteBytes(baseCommand);
                    if (loco.BlunamiCharactertisic != null)
                    {
                        GattWriteResult result = await loco.BlunamiCharactertisic.WriteValueWithResultAsync(writer.DetachBuffer());
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            //Console.WriteLine("Successfully wrote AEffectPacket to train");
                        }
                        else
                        {
                            Console.WriteLine("WriteBlunamiDEGroupEffectCommand: Error Writing to Blunami Characteristic:{0}", result.Status.ToString());
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

        async Task WriteBlunamiDFGroupEffectCommand(BlunamiEngine loco)
        {
            if (loco.BluetoothLeDevice != null && loco.BluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                try
                {
                    byte[] baseCommand = new BlunamiCommandBase().baseCommand;

                    //switch (loco.DecoderType)
                    //{
                    //    // for 2200 decoders, the byte 2 should be 0x02. 
                    //    case (int)BlunamiEngineTypes.BLU2200_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU2200_ELECTRIC:
                    //    case (int)BlunamiEngineTypes.BLUERAIL_TAMVALLEY:

                    //        {
                    //            baseCommand[1] = 0x03;
                    //            break;
                    //        }
                    //    // for 4408 decoders, the byte 2 should be 0x03. 
                    //    case (int)BlunamiEngineTypes.BLU4408_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU4408_ELECTRIC:
                    //        {
                    //            baseCommand[1] = 0x04;
                    //            break;
                    //        }

                    //    default:
                    //        Console.WriteLine("Failure to determine Decoder type: WriteBlunamiDFGroupEffectCommand");
                    //        return;

                    //}

                    if (loco.UsesLongAddress)
                    {
                        baseCommand[1] = 0x04;
                        baseCommand[2] = (byte)loco.CV17;
                        baseCommand[3] = (byte)loco.CV18;
                        baseCommand[4] = (byte)0xDF;
                        baseCommand[5] += (byte)loco.DFFlags;
                    }
                    else
                    {
                        baseCommand[1] = 0x03;
                        baseCommand[2] = (byte)loco.Id;
                        baseCommand[3] = (byte)0xDF;
                        baseCommand[4] += (byte)loco.DFFlags;

                        //std.wcout << (int)dataToWrite[3] << std.endl;

                    }

                    var writer = new DataWriter();
                    writer.WriteBytes(baseCommand);
                    if (loco.BlunamiCharactertisic != null)
                    {
                        GattWriteResult result = await loco.BlunamiCharactertisic.WriteValueWithResultAsync(writer.DetachBuffer());
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            //Console.WriteLine("Successfully wrote AEffectPacket to train");
                        }
                        else
                        {
                            Console.WriteLine("WriteBlunamiDFGroupEffectCommand: Error Writing to Blunami Characteristic:{0}", result.Status.ToString());
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

        async Task WriteBlunamiSpeedCommand(BlunamiEngine loco)
        {
            if (loco.BluetoothLeDevice != null && loco.BluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                try
                {
                    byte[] baseCommand = new BlunamiCommandBase().baseSpeedCommand;

                    //switch (loco.DecoderType)
                    //{
                    //    // for 2200 decoders, the byte 2 should be 0x02. 
                    //    case (int)BlunamiEngineTypes.BLU2200_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU2200_ELECTRIC:
                    //    case (int)BlunamiEngineTypes.BLUERAIL_TAMVALLEY:

                    //        {
                    //            baseCommand[1] = 0x03;
                    //            break;
                    //        }
                    //    // for 4408 decoders, the byte 2 should be 0x03. 
                    //    case (int)BlunamiEngineTypes.BLU4408_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU4408_ELECTRIC:
                    //        {
                    //            baseCommand[1] = 0x04;
                    //            break;
                    //        }

                    //    default:
                    //        Console.WriteLine("Failure to determine Decoder type: WriteBlunamiSpeedCommand");
                    //        return;

                    //}

                    if (loco.UsesLongAddress)
                    {
                        baseCommand[1] = 0x04;
                        baseCommand[2] = (byte)loco.CV17;
                        baseCommand[3] = (byte)loco.CV18;
                        baseCommand[4] = 0x3F;

                        if (loco.Direction)
                        {
                            baseCommand[5] = (byte)(0x80 + loco.Speed);
                        }
                        else
                        {
                            baseCommand[5] = (byte)(0x00 + loco.Speed);
                        }

                    }
                    else
                    {
                        baseCommand[1] = 0x03;
                        baseCommand[2] = (byte)loco.Id;
                        baseCommand[3] = 0x3F;

                        if (loco.Direction)
                        {
                            baseCommand[4] = (byte)(0x80 + loco.Speed);
                        }
                        else
                        {
                            baseCommand[4] = (byte)(0x00 + loco.Speed);
                        }

                    }

                    var writer = new DataWriter();
                    writer.WriteBytes(baseCommand);
                    if (loco.BlunamiCharactertisic != null)
                    {
                        GattWriteResult result = await loco.BlunamiCharactertisic.WriteValueWithResultAsync(writer.DetachBuffer());
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            //Console.WriteLine("Successfully wrote SpeedPacket to train");
                        }
                        else
                        {
                            Console.WriteLine("WriteBlunamiSpeedCommand: Error Writing to Blunami Characteristic:{0}", result.Status.ToString());
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

        async Task WriteBlunamiDirectionCommand(BlunamiEngine loco)
        {
            if (loco.BluetoothLeDevice != null && loco.BluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                try
                {
                    byte[] baseCommand = new BlunamiCommandBase().baseSpeedCommand;

                    //switch (loco.DecoderType)
                    //{
                    //    // for 2200 decoders, the byte 2 should be 0x02. 
                    //    case (int)BlunamiEngineTypes.BLU2200_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU2200_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU2200_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU2200_ELECTRIC:
                    //    case (int)BlunamiEngineTypes.BLUERAIL_TAMVALLEY:
                    //        {
                    //            baseCommand[1] = 0x03;
                    //            break;
                    //        }
                    //    // for 4408 decoders, the byte 2 should be 0x03. 
                    //    case (int)BlunamiEngineTypes.BLU4408_STEAM:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_ALCO:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_BALDWIN:
                    //    case (int)BlunamiEngineTypes.BLU4408_DIESEL_EMD2:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_EMD:
                    //    case (int)BlunamiEngineTypes.BLU4408_DISESL_GE:
                    //    case (int)BlunamiEngineTypes.BLU4408_ELECTRIC:
                    //        {
                    //            baseCommand[1] = 0x04;
                    //            break;
                    //        }

                    //    default:
                    //        Console.WriteLine("Failure to determine Decoder type: WriteBlunamiSpeedCommand");
                    //        return;

                    //}

                    if (loco.UsesLongAddress)
                    {
                        baseCommand[1] = 0x04;
                        baseCommand[2] = (byte)loco.CV17;
                        baseCommand[3] = (byte)loco.CV18;
                        baseCommand[4] = 0x3F;

                        if (loco.Direction)
                        {
                            baseCommand[5] = (byte)(0x81);
                        }
                        else
                        {
                            baseCommand[5] = (byte)(0x01);
                        }

                    }
                    else
                    {
                        baseCommand[1] = 0x03;
                        baseCommand[2] = (byte)loco.Id;
                        baseCommand[3] = 0x3F;

                        if (loco.Direction)
                        {
                            baseCommand[4] = (byte)(0x81);
                        }
                        else
                        {
                            baseCommand[4] = (byte)(0x01);
                        }

                    }

                    var writer = new DataWriter();
                    writer.WriteBytes(baseCommand);
                    if (loco.BlunamiCharactertisic != null)
                    {
                        GattWriteResult result = await loco.BlunamiCharactertisic.WriteValueWithResultAsync(writer.DetachBuffer());
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            //Console.WriteLine("Successfully wrote DirectionPacket to train");
                        }
                        else
                        {
                            Console.WriteLine("WriteBlunamiDirectionCommand: Error Writing to Blunami Characteristic:{0}", result.Status.ToString());
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
        int TMCCIDVal = 3;
        int CV17Value = 0;
        int CV18Value = 0;
        int decoderType = 90; // set default to 2200 Steam2
        int speed = 0;

        BlunamiEngineEffectCommandParams dynamoFlags = (BlunamiEngineEffectCommandParams)0;
        BlunamiEngineEffectCommandParams aFlags = (BlunamiEngineEffectCommandParams)0;
        BlunamiEngineEffectCommandParams bFlags = (BlunamiEngineEffectCommandParams)0;
        BlunamiEngineEffectCommandParams deFlags = (BlunamiEngineEffectCommandParams)0;
        BlunamiEngineEffectCommandParams dfFlags = (BlunamiEngineEffectCommandParams)0;

        bool shortWhistleOn = false;
        bool headlightOn = false;
        bool direction = true; // true - forward, false - backward
                               //bool cylinderCocksOn = false;

        bool brakeSelection = false; // false - Independent Brake, True - Train Brake

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


        /// <summary>
        /// Dynamo region flags
        /// </summary>
        /// <returns></returns>
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

        bool isCylinderCocksOn()
        {
            return (dynamoFlags & BlunamiEngineEffectCommandParams.CYLINDER_COCKS) != (BlunamiEngineEffectCommandParams)0;
        }

        /// <summary>
        /// A group region flags
        /// </summary>
        /// <returns></returns>

        bool isGradeCrossingWhistleOn()
        {
            return (aFlags & BlunamiEngineEffectCommandParams.A_GRADE_CROSSING_WHISTLE) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isBlowdownOn()
        {
            return (aFlags & BlunamiEngineEffectCommandParams.A_BLOWDOWN) != (BlunamiEngineEffectCommandParams)0;
        }

        bool BrakeMode()
        {
            return (aFlags & BlunamiEngineEffectCommandParams.A_BRAKE_SELECT) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isBrakeOn()
        {
            return (aFlags & BlunamiEngineEffectCommandParams.A_BRAKE_ENABLED) != (BlunamiEngineEffectCommandParams)0;
        }

        /// <summary>
        /// B region flags
        /// </summary>
        /// <returns></returns>

        bool isCutoffPlusOn()
        {
            return (bFlags & BlunamiEngineEffectCommandParams.B_CUTOFF_INCREASE) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isCutoffMinusOn()
        {
            return (bFlags & BlunamiEngineEffectCommandParams.B_CUTOFF_DECREASE) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isDimmerOn()
        {
            return (bFlags & BlunamiEngineEffectCommandParams.B_DIMMER_ENABLED) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isMuted()
        {
            return (bFlags & BlunamiEngineEffectCommandParams.B_SOUND_MUTE) != (BlunamiEngineEffectCommandParams)0;
        }


        /// <summary>
        /// DE Region flags
        /// </summary>
        /// <returns></returns>
        bool isCoupleOn()
        {
            return (deFlags & BlunamiEngineEffectCommandParams.DE_UNCOUPLE) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isMomentumOn()
        {
            return (deFlags & BlunamiEngineEffectCommandParams.DE_MOMENTUM_DISABLE) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isHandBrakeOn()
        {
            return (deFlags & BlunamiEngineEffectCommandParams.DE_HANDBRAKE_ENABLE) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isWaterStopOn()
        {
            return (deFlags & BlunamiEngineEffectCommandParams.DE_WATERSTOP) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isFuelStopOn()
        {
            return (deFlags & BlunamiEngineEffectCommandParams.DE_FUELSTOP) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isAshDumpOn()
        {
            return (deFlags & BlunamiEngineEffectCommandParams.DE_ASH_DUMP) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isWheelSlipOn()
        {
            return (deFlags & BlunamiEngineEffectCommandParams.DE_WHEEL_SLIP_ENABLE) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isF20On() 
        {
            return (deFlags & BlunamiEngineEffectCommandParams.DE_FUNCTION_20) != (BlunamiEngineEffectCommandParams)0;
        }

        /// <summary>
        /// DF Region flags
        /// </summary>
        /// <returns></returns>
        bool isSanderOn()
        {
            return (dfFlags & BlunamiEngineEffectCommandParams.DF_SANDER_VALVE) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isCabChatterOn()
        {
            return (dfFlags & BlunamiEngineEffectCommandParams.DF_CAB_CHATTER) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isAllAboardOn()
        {
            return (dfFlags & BlunamiEngineEffectCommandParams.DF_ALL_ABOARD) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isFX3On()
        {
            return (dfFlags & BlunamiEngineEffectCommandParams.DF_FX3) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isFX4On()
        {
            return (dfFlags & BlunamiEngineEffectCommandParams.DF_FX4) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isFX5On()
        {
            return (dfFlags & BlunamiEngineEffectCommandParams.DF_FX5) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isFX6On()
        {
            return (dfFlags & BlunamiEngineEffectCommandParams.DF_FX6) != (BlunamiEngineEffectCommandParams)0;
        }

        bool isFX28On() // This enables both FX7 and FX8
        {
            return (dfFlags & BlunamiEngineEffectCommandParams.DF_FX28) != (BlunamiEngineEffectCommandParams)0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bluetoothLEDevice"></param>
        /// <param name="address"></param>
        /// <param name="decoderType"></param>
        /// <param name="speed"></param>

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
                //Console.WriteLine("CV17{0},CV18{1}", this.CV17Value, this.CV18Value);
                // This is only for communicating with Lionel devices. TMCC cannot handle 4 digit so we are just ignoring the last two values.
                this.TMCCIDVal = this.Id / 100; // get rid of the last two digits by dividing by 100 and forgetting the decimal portions
            }
            else
            {
                this.usesLongAddress = false;
                // This is only for communicating with Lionel devices. TMCC cannot handle 4 digit so we are just ignoring the last two values.
                this.TMCCIDVal = this.Id; // get rid of the last two digits by dividing by 100 and forgetting the decimal portions
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

        public int TMCCID
        {
            get
            {
                return TMCCIDVal;
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
                return isWhistleOn();
            }
            set
            {
                if(value)
                    DynamoFlags |= BlunamiEngineEffectCommandParams.LONG_WHISTLE;
                else
                    DynamoFlags ^= BlunamiEngineEffectCommandParams.LONG_WHISTLE;
            }
        }

        public bool Bell
        {
            get
            {
                return isBellOn();
            }
            set
            {
                if (value)
                    DynamoFlags |= BlunamiEngineEffectCommandParams.BELL;
                else
                    DynamoFlags ^= BlunamiEngineEffectCommandParams.BELL;
            }
        }

        public bool ShortWhistle
        {
            get
            {
                return isShortWhistleOn();
            }
            set
            {
                if (value)
                    DynamoFlags |= BlunamiEngineEffectCommandParams.SHORT_WHISTLE;
                else
                    DynamoFlags ^= BlunamiEngineEffectCommandParams.SHORT_WHISTLE;
            }

        }

        

        public bool CylinderCocks
        {
            get
            {
                return isCylinderCocksOn();
            }
            set
            {
                if (value)
                    DynamoFlags |= BlunamiEngineEffectCommandParams.CYLINDER_COCKS;
                else
                    DynamoFlags ^= BlunamiEngineEffectCommandParams.CYLINDER_COCKS;
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

        /// <summary>
        /// A RANGE
        /// </summary>

        public bool GradeCrossingWhistle
        {
            get
            {
                return isGradeCrossingWhistleOn();
            }
            set
            {
                if (value)
                    AFlags |= BlunamiEngineEffectCommandParams.A_GRADE_CROSSING_WHISTLE;
                else
                    AFlags ^= BlunamiEngineEffectCommandParams.A_GRADE_CROSSING_WHISTLE;
            }

        }

        public bool Blowdown
        {
            get
            {
                return isBlowdownOn();
            }
            set
            {
                if (value)
                    AFlags |= BlunamiEngineEffectCommandParams.A_BLOWDOWN;
                else
                    AFlags ^= BlunamiEngineEffectCommandParams.A_BLOWDOWN;
            }
        }

        public bool Brake
        {
            get
            {
                return isBrakeOn();
            }
            set
            {
                if (value)
                    AFlags |= BlunamiEngineEffectCommandParams.A_BRAKE_ENABLED;
                else
                    AFlags ^= BlunamiEngineEffectCommandParams.A_BRAKE_ENABLED;
            }
        }

        public bool BrakeSelection
        {
            get
            {
                return BrakeMode();
            }
            set
            {
                if (value)
                    AFlags |= BlunamiEngineEffectCommandParams.A_BRAKE_SELECT;
                else
                    AFlags ^= BlunamiEngineEffectCommandParams.A_BRAKE_SELECT;
            }
        }

        /// <summary>
        /// B RANGE
        /// </summary>
        public bool CutoffIncrease
        {
            get
            {
                return isCutoffPlusOn();
            }
            set
            {
                if (value)
                    BFlags |= BlunamiEngineEffectCommandParams.B_CUTOFF_INCREASE;
                else
                    BFlags ^= BlunamiEngineEffectCommandParams.B_CUTOFF_INCREASE;
            }
        }

        public bool CutoffDecrease
        {
            get
            {
                return isCutoffMinusOn();
            }
            set
            {
                if (value)
                    BFlags |= BlunamiEngineEffectCommandParams.B_CUTOFF_DECREASE;
                else
                    BFlags ^= BlunamiEngineEffectCommandParams.B_CUTOFF_DECREASE;
            }
        }

        public bool Dimmer
        {
            get
            {
                return isDimmerOn();
            }
            set
            {
                if (value)
                    BFlags |= BlunamiEngineEffectCommandParams.B_DIMMER_ENABLED;
                else
                    BFlags ^= BlunamiEngineEffectCommandParams.B_DIMMER_ENABLED;
            }
        }

        public bool Mute
        {
            get
            {
                return isMuted();
            }
            set
            {
                if (value)
                    BFlags |= BlunamiEngineEffectCommandParams.B_SOUND_MUTE;
                else
                    BFlags ^= BlunamiEngineEffectCommandParams.B_SOUND_MUTE;
            }
        }

        /// <summary>
        /// DE RANGE
        /// </summary>
        /// 
        public bool Uncouple
        {
            get
            {
                return isCoupleOn();
            }
            set
            {
                if (value)
                    DEFlags |= BlunamiEngineEffectCommandParams.DE_UNCOUPLE;
                else
                    DEFlags ^= BlunamiEngineEffectCommandParams.DE_UNCOUPLE;
            }
        }

        public bool Momentum
        {
            get
            {
                return isMomentumOn();
            }
            set
            {
                if (value)
                    DEFlags |= BlunamiEngineEffectCommandParams.DE_MOMENTUM_DISABLE;
                else
                    DEFlags ^= BlunamiEngineEffectCommandParams.DE_MOMENTUM_DISABLE;
            }
        }

        public bool Waterstop
        {
            get
            {
                return isWaterStopOn();
            }
            set
            {
                if (value)
                    DEFlags |= BlunamiEngineEffectCommandParams.DE_WATERSTOP;
                else
                    DEFlags ^= BlunamiEngineEffectCommandParams.DE_WATERSTOP;
            }
        }

        public bool FuelStop
        {
            get
            {
                return isFuelStopOn();
            }
            set
            {
                if (value)
                    DEFlags |= BlunamiEngineEffectCommandParams.DE_FUELSTOP;
                else
                    DEFlags ^= BlunamiEngineEffectCommandParams.DE_FUELSTOP;
            }
        }

        public bool AshDump
        {
            get
            {
                return isAshDumpOn();
            }
            set
            {
                if (value)
                    DEFlags |= BlunamiEngineEffectCommandParams.DE_ASH_DUMP;
                else
                    DEFlags ^= BlunamiEngineEffectCommandParams.DE_ASH_DUMP;
            }
        }

        public bool HandBrake
        {
            get
            {
                return isHandBrakeOn();
            }
            set
            {
                if (value)
                    DEFlags |= BlunamiEngineEffectCommandParams.DE_HANDBRAKE_ENABLE;
                else
                    DEFlags ^= BlunamiEngineEffectCommandParams.DE_HANDBRAKE_ENABLE;
            }
        }

        public bool Wheelslip
        {
            get
            {
                return isWheelSlipOn();
            }
            set
            {
                if (value)
                    DEFlags |= BlunamiEngineEffectCommandParams.DE_WHEEL_SLIP_ENABLE;
                else
                    DEFlags ^= BlunamiEngineEffectCommandParams.DE_WHEEL_SLIP_ENABLE;
            }
        }

        public bool F20
        {
            get
            {
                return isF20On();
            }
            set
            {
                if (value)
                    DEFlags |= BlunamiEngineEffectCommandParams.DE_FUNCTION_20;
                else
                    DEFlags ^= BlunamiEngineEffectCommandParams.DE_FUNCTION_20;
            }
        }

        /// <summary>
        /// DF RANGE
        /// </summary>
        public bool SanderValve
        {
            get
            {
                return isSanderOn();
            }
            set
            {
                if (value)
                    DFFlags |= BlunamiEngineEffectCommandParams.DF_SANDER_VALVE;
                else
                    DFFlags ^= BlunamiEngineEffectCommandParams.DF_SANDER_VALVE;
            }
        }

        public bool CabChatter
        {
            get
            {
                return isCabChatterOn();
            }
            set
            {
                if (value)
                    DFFlags |= BlunamiEngineEffectCommandParams.DF_CAB_CHATTER;
                else
                    DFFlags ^= BlunamiEngineEffectCommandParams.DF_CAB_CHATTER;
            }
        }

        public bool AllAboard
        {
            get
            {
                return isAllAboardOn();
            }
            set
            {
                if (value)
                    DFFlags |= BlunamiEngineEffectCommandParams.DF_ALL_ABOARD;
                else
                    DFFlags ^= BlunamiEngineEffectCommandParams.DF_ALL_ABOARD;
            }
        }

        public bool FX3
        {
            get
            {
                return isFX3On();
            }
            set
            {
                if (value)
                    DFFlags |= BlunamiEngineEffectCommandParams.DF_FX3;
                else
                    DFFlags ^= BlunamiEngineEffectCommandParams.DF_FX3;
            }
        }

        public bool FX4
        {
            get
            {
                return isFX4On();
            }
            set
            {
                if (value)
                    DFFlags |= BlunamiEngineEffectCommandParams.DF_FX4;
                else
                    DFFlags ^= BlunamiEngineEffectCommandParams.DF_FX4;
            }
        }

        public bool FX5
        {
            get
            {
                return isFX5On();
            }
            set
            {
                if (value)
                    DFFlags |= BlunamiEngineEffectCommandParams.DF_FX5;
                else
                    DFFlags ^= BlunamiEngineEffectCommandParams.DF_FX5;
            }
        }

        public bool FX6
        {
            get
            {
                return isFX6On();
            }
            set
            {
                if (value)
                    DFFlags |= BlunamiEngineEffectCommandParams.DF_FX6;
                else
                    DFFlags ^= BlunamiEngineEffectCommandParams.DF_FX6;
            }
        }

        public bool FX28
        {
            get
            {
                return isFX28On();
            }
            set
            {
                if (value)
                    DFFlags |= BlunamiEngineEffectCommandParams.DF_FX28;
                else
                    DFFlags ^= BlunamiEngineEffectCommandParams.DF_FX28;
            }
        }

        /// <summary>
        /// 
        /// </summary>

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

        public BlunamiEngineEffectCommandParams AFlags
        {
            get
            {
                return aFlags;
            }
            set
            {
                aFlags = (BlunamiEngineEffectCommandParams)value;
            }
        }

        public BlunamiEngineEffectCommandParams BFlags
        {
            get
            {
                return bFlags;
            }
            set
            {
                bFlags = (BlunamiEngineEffectCommandParams)value;
            }
        }

        public BlunamiEngineEffectCommandParams DEFlags
        {
            get
            {
                return deFlags;
            }
            set
            {
                deFlags = (BlunamiEngineEffectCommandParams)value;
            }
        }

        public BlunamiEngineEffectCommandParams DFFlags
        {
            get
            {
                return dfFlags;
            }
            set
            {
                dfFlags = (BlunamiEngineEffectCommandParams)value;
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


