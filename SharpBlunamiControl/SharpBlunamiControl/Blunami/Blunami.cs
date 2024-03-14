using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

enum BlunamiEngineEffectCommandTypes
{
    DYNAMO_OFF = 0b10000000,
    DYNAMO_ON = 0b10010000,
    EFFECT_TYPE_A = 0xA0,
    EFFECT_TYPE_B = 0xB0,
    EFFECT_TYPE_DE = 0xDE,
    EFFECT_TYPE_DF = 0xDF,
};
enum BlunamiEngineEffectCommandParams
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
        const int DCC_LONG_ADDRESS_CONSTANT = 49152;
        byte[] baseCommand = { 0x02, 0x02, 0x00, 0x00, 0x00, 0x00 }; // replace third byte with engine ID
        byte[] baseSpeedCommand = { 0x02, 0x03, 0x00, 0x3F, 0x00, 0x00 }; // replace third byte with engine ID. Change the 4th bit to 0x3F if short digit, 5th bit to 0x3F if long digit. 
        byte[] baseReadCVCommand = { 0x03, 0x04, 0x00, 0x74, 0xFF, 0x01, 0x00 }; // replace FF with CV in Hex to read
        string blunamiServiceStr = "{f688fd00-da9a-2e8f-d003-ebcdcc259af9}";
        string blunamiDCCCharacteristicStr = "{f688fd1d-da9a-2e8f-d003-ebcdcc259af9}";


    }
}
