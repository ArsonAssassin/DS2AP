using Archipelago.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS2AP
{
    public static class Helpers
    {
        public static ulong GetBaseAddress()
        {
            var address = (ulong)Memory.GetBaseAddress("DarkSoulsII");
            if (address == 0)
            {
                Console.WriteLine("Could not find Base Address");
            }
            return address;
        }
        public static ulong GetBaseAOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x58, 0x38, 0x48, 0x85, 0xDB, 0x74, 0x00, 0xF6};
            string mask = "xxx????xxxxxxxx?x";
            IntPtr getBaseAAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getBaseAAddress + 3), 4), 0);
            IntPtr baseCAddress = getBaseAAddress + offset + 7;

            return (ulong)baseCAddress;
        }
        public static ulong GetBaseCOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x45, 0x33, 0xED, 0x48, 0x8B, 0xF1, 0x48, 0x85, 0xC0 };
            string mask = "xxx????xxxxxxxxxx";
            IntPtr getBaseCAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getBaseCAddress + 3), 4), 0);
            IntPtr baseCAddress = getBaseCAddress + offset + 7;

            return (ulong)baseCAddress;
        }
        public static ulong GetItemGiveFuncOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = HexStringToByteArray("48 89 5C 24 18 56 57 41 56 48 83 EC 30 45 8B F1 41");
            string mask = "xxxxxxxxxxxxxxxxx";
            IntPtr getItemFuncAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getItemFuncAddress + 3), 4), 0);
            IntPtr itemFuncAddress = getItemFuncAddress + offset + 7;

            return (ulong)itemFuncAddress;
        }
        public static ulong GetItemStruct2dDisplayOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = HexStringToByteArray("40 53 48 83 EC 20 45 33 D2 45 8B D8 48 8B D9 44 89 11");
            string mask = "xxxxxxxxxxxxxxxxxx";
            IntPtr getItemStruct2dDisplayAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getItemStruct2dDisplayAddress + 3), 4), 0);
            IntPtr itemItemStruct2dDisplayAddress = getItemStruct2dDisplayAddress + offset + 7;

            return (ulong)itemItemStruct2dDisplayAddress;
        }
        public static ulong GetDisplayItemOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = HexStringToByteArray("48 8B 89 D8 00 00 00 48 85 C9 0F 85 40 5E 00 00");
            string mask = "xxxxxxxxxxxxxxxx";
            IntPtr getDisplayItemAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getDisplayItemAddress + 3), 4), 0);
            IntPtr displayItemAddress = getDisplayItemAddress + offset + 7;

            return (ulong)displayItemAddress;
        }
        public static ulong GetItemGiveWindow()
        {
            var baseAddress = GetBaseAOffset();
            return baseAddress + 0x22E0;
        }
        public static ulong GetEventFlagsOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x99, 0x33, 0xC2, 0x45, 0x33, 0xC0, 0x2B, 0xC2, 0x8D, 0x50, 0xF6 };
            string mask = "xxx????xxxxxxxxxxx";
            IntPtr getEFAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getEFAddress + 3), 4), 0);
            IntPtr eventFlagsAddress = getEFAddress + offset + 7;

            return (ulong)(BitConverter.ToInt32(Memory.ReadFromPointer((ulong)eventFlagsAddress, 4, 2)));
        }
        public static PlayerPointers GetPlayerPointers()
        {
            var baseA = GetBaseAOffset();
            PlayerPointers pointers = new PlayerPointers() 
            {
                Name = baseA + 0xA8,
                AvailableBag = baseA + 0xA8 + 0x10
            };
            return pointers;
            
        }
        public static bool GetIsPlayerOnline()
        {
            var baseCOffset = GetBaseCOffset();
            ulong onlineFlagOffset = 0xB7D;

            var isOnline = Memory.ReadByte(baseCOffset + onlineFlagOffset) != 0;
            return isOnline;

        }
        public static string OpenEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string file = reader.ReadToEnd();
                return file;
            }
        }
        public static byte[] GetItemCommand()
        {
            return [0x48, 0x83, 0xEC, 0x28, 0x41, 0xB8, 0x08, 0x00, 0x00, 0x00, 0x49, 0xBF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x49, 0x8D, 0x17, 0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x45, 0x31, 0xC9, 0x49, 0xBE, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x28, 0xC3];
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            // Remove any spaces from the string
            hex = hex.Replace(" ", "");

            // Check if the string has an even number of characters
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("The hexadecimal string must have an even number of digits.");
            }

            // Convert the string to a byte array
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
