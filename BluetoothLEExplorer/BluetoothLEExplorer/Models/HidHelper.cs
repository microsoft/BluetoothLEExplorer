using System;

namespace BluetoothLEExplorer.Models
{
    public enum KeyEvent
    {
        KeyMake,
        KeyBreak
    }

    class HidHelper
    {
        public static byte GetHidUsageFromPs2Set1(uint ps2Set1ScanCode)
        {
            switch (ps2Set1ScanCode)
            {
                case 0x00: return 0x00;  // Not used
                case 0x29: return 0x35;  // Key location 1  ( ~ ` )
                case 0x02: return 0x1E;  // Key location 2  ( ! 1 )
                case 0x03: return 0x1F;  // Key location 3  ( @ 2 )
                case 0x04: return 0x20;  // Key location 4  ( # 3 )
                case 0x05: return 0x21;  // Key location 5  ( $ 4 )
                case 0x06: return 0x22;  // Key location 6  ( % 5 )
                case 0x07: return 0x23;  // Key location 7  ( ^ 6 )
                case 0x08: return 0x24;  // Key location 8  ( & 7 )
                case 0x09: return 0x25;  // Key location 9  ( * 8 )
                case 0x0A: return 0x26;  // Key location 10 ( ( 9 )
                case 0x0B: return 0x27;  // Key location 11 ( ) 0 )
                case 0x0C: return 0x2D;  // Key location 12 ( _ - )
                case 0x0D: return 0x2E;  // Key location 13 ( + = )
                case 0x0E: return 0x2A;  // Key location 15 ( Backspace )
                case 0x0F: return 0x2B;  // Key location 16 ( Tab )
                case 0x10: return 0x14;  // Key location 17 ( Q )
                case 0x11: return 0x1A;  // Key location 18 ( W )
                case 0x12: return 0x08;  // Key location 19 ( E )
                case 0x13: return 0x15;  // Key location 20 ( R )
                case 0x14: return 0x17;  // Key location 21 ( T )
                case 0x15: return 0x1C;  // Key location 22 ( Y )
                case 0x16: return 0x18;  // Key location 23 ( U )
                case 0x17: return 0x0C;  // Key location 24 ( I )
                case 0x18: return 0x12;  // Key location 25 ( O )
                case 0x19: return 0x13;  // Key location 26 ( P )
                case 0x1A: return 0x2F;  // Key location 27 ( { [ )
                case 0x1B: return 0x30;  // Key location 28 ( } ] )
                case 0x2B: return 0x31;  // Key location 29* ( | \ )
                case 0x3A: return 0x39;  // Key location 30 ( Caps Lock )
                case 0x1E: return 0x04;  // Key location 31 ( A )
                case 0x1F: return 0x16;  // Key location 32 ( S )
                case 0x20: return 0x07;  // Key location 33 ( D )
                case 0x21: return 0x09;  // Key location 34 ( F )
                case 0x22: return 0x0A;  // Key location 35 ( G )
                case 0x23: return 0x0B;  // Key location 36 ( H )
                case 0x24: return 0x0D;  // Key location 37 ( J )
                case 0x25: return 0x0E;  // Key location 38 ( K )
                case 0x26: return 0x0F;  // Key location 39 ( L )
                case 0x27: return 0x33;  // Key location 40 ( : ; )
                case 0x28: return 0x34;  // Key location 41 ( “ ‘ )
                case 0x1C: return 0x28;  // Key location 43 ( Enter )
                case 0x2A: return 0xE1;  // Key location 44 ( L SHIFT )
                case 0x56: return 0x64;  // Key location 45 ( NONE ) **
                case 0x2C: return 0x1D;  // Key location 46 ( Z )
                case 0x2D: return 0x1B;  // Key location 47 ( X )
                case 0x2E: return 0x06;  // Key location 48 ( C )
                case 0x2F: return 0x19;  // Key location 49 ( V )
                case 0x30: return 0x05;  // Key location 50 ( B )
                case 0x31: return 0x11;  // Key location 51 ( N )
                case 0x32: return 0x10;  // Key location 52 ( M )
                case 0x33: return 0x36;  // Key location 53 ( < , )
                case 0x34: return 0x37;  // Key location 54 ( > . )
                case 0x35: return 0x38;  // Key location 55 ( ? / )
                case 0x73: return 0x87;  // Key location 56 ( NONE ) ***
                case 0x36: return 0xE5;  // Key location 57 ( R SHIFT )
                case 0x1D: return 0xE0;  // Key location 58 ( L CTRL )
                case 0xE05B: return 0xE3; // Key location 59 ( L WIN )
                case 0x38: return 0xE2; // Key location 60 ( L ALT )
                case 0x39: return 0x2C; // Key location 61 ( Space Bar )
                case 0xE038: return 0xE6; // Key location 62 ( R ALT )
                case 0xE05C: return 0xE7; // Key location 63 ( R WIN )
                case 0xE01D: return 0xE4; // Key location 64 ( R CTRL )
                case 0xE05D: return 0x65; // Key location 65 ( APP )
                case 0xE052: return 0x49; // Key location 75 ( Insert )
                case 0xE053: return 0x4C; // Key location 76 ( Delete )
                case 0xE04B: return 0x50; // Key location 79 ( Left Arrow )
                case 0xE047: return 0x4A; // Key location 80 ( Home )
                case 0xE04F: return 0x4D; // Key location 81 ( End )
                case 0xE048: return 0x52; // Key location 83 ( Up Arrow )
                case 0xE050: return 0x51; // Key location 84 ( Dn Arrow )
                case 0xE049: return 0x4B; // Key location 85 ( Page Up )
                case 0xE051: return 0x4E; // Key location 86 ( Page Down )
                case 0xE04D: return 0x4F; // Key location 89 ( Right Arrow )
                case 0x45: return 0x53; // Key location 90 ( Num Lock )
                case 0x47: return 0x5F; // Key location 91 ( Numeric 7 )
                case 0x4B: return 0x5C; // Key location 92 ( Numeric 4 )
                case 0x4F: return 0x59; // Key location 93 ( Numeric 1 )
                case 0xE035: return 0x54; // Key location 95 ( Numeric / )
                case 0x48: return 0x60; // Key location 96 ( Numeric 8 )
                case 0x4C: return 0x5D; // Key location 97 ( Numeric 5 )
                case 0x50: return 0x5A; // Key location 98 ( Numeric 2 )
                case 0x52: return 0x62; // Key location 99 ( Numeric 0 )
                case 0x37: return 0x55; // Key location 100 ( Numeric * )
                case 0x49: return 0x61; // Key location 101 ( Numeric 9 )
                case 0x4D: return 0x5E; // Key location 102 ( Numeric 6 )
                case 0x51: return 0x5B; // Key location 103 ( Numeric 3 )
                case 0x53: return 0x63; // Key location 104 ( Numeric . )
                case 0x4A: return 0x56; // Key location 105 ( Numeric - )
                case 0x4E: return 0x57; // Key location 106 ( Numeric + )
                case 0x7E: return 0x85; // Key location 107 ( NONE ) ***
                case 0xE01C: return 0x58; // Key location 108 ( Numeric Enter )
                case 0x01: return 0x29; // Key location 110 ( Esc )
                case 0x3B: return 0x3A; // Key location 112 ( F1 )
                case 0x3C: return 0x3B; // Key location 113 ( F2 )
                case 0x3D: return 0x3C; // Key location 114 ( F3 )
                case 0x3E: return 0x3D; // Key location 115 ( F4 )
                case 0x3F: return 0x3E; // Key location 116 ( F5 )
                case 0x40: return 0x3F; // Key location 117 ( F6 )
                case 0x41: return 0x40; // Key location 118 ( F7 )
                case 0x42: return 0x41; // Key location 119 ( F8 )
                case 0x43: return 0x42; // Key location 120 ( F9 )
                case 0x44: return 0x43; // Key location 121 ( F10 )
                case 0x57: return 0x44; // Key location 122 ( F11 )
                case 0x58: return 0x45; // Key location 123 ( F12 )
                case 0xE037: return 0x46; // Key location 124 ( Print Screen )
                case 0x46: return 0x47; // Key location 125 ( Scroll Lock )
                case 0xE11D45: return 0x48; // Key location 126 ( Pause )
                default:
                    throw new ArgumentException("PS2 Set 1 code not recognized");
            }
        }

        public static bool IsMofifierKey(byte hidUsageScanCode)
        {
            // Modifier keys are defined as:
            //   E0: Left Ctrl
            //   E1: Left Shift
            //   E2: Left Alt
            //   E3: Left Windows Key
            //   E4: Right Ctrl
            //   E5: Right Shift
            //   E6: Right Alt
            //   E7: Right Windows Key
            return ((hidUsageScanCode >= 0xE0) && (hidUsageScanCode <= 0xE7));
        }

        public static byte GetFlagOfModifierKey(byte hidUsageScanCode)
        {
            if (!IsMofifierKey(hidUsageScanCode))
            {
                throw new ArgumentException("Key given is not a modifier key");
            }

            // Modifier keys by bit position in modifier key bitfield:
            //   0: Left Ctrl
            //   1: Left Shift
            //   2: Left Alt
            //   3: Left Windows Key
            //   4: Right Ctrl
            //   5: Right Shift
            //   6: Right Alt
            //   7: Right Windows Key
            return (byte)(0x1 << (hidUsageScanCode - 0xE0));
        }
    }
}
