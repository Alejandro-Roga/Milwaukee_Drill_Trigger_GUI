using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Milwaukee_Drill_Trigger_GUI
{
    class Connect
    {
        private int errorCode;
        private int error;

        public bool IsConnected { get; set; }
        public bool MGH { get; set; }
        public bool MGL { get; set; }

        #region Imported Functions
        [DllImport("breithorn-driver-wrapper.dll", EntryPoint = "init", CallingConvention = CallingConvention.Cdecl)]
        private static extern int init();

        [DllImport("breithorn-driver-wrapper.dll", EntryPoint = "close", CallingConvention = CallingConvention.Cdecl)]
        private static extern int close();

        [DllImport("breithorn-driver-wrapper.dll", EntryPoint = "setMaSiliconVersion", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setMaSiliconVersion(byte version);

        [DllImport("breithorn-driver-wrapper.dll", EntryPoint = "readMaRegister", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte readMaRegister(byte version);

        [DllImport("breithorn-driver-wrapper.dll", EntryPoint = "writeMaRegister", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte writeMaRegister(byte address, byte value);

        [DllImport("breithorn-driver-wrapper.dll", EntryPoint = "readMagAlphaAngularPosition", CallingConvention = CallingConvention.Cdecl)]
        private static extern int readMagAlphaAngularPosition();

        [DllImport("breithorn-driver-wrapper.dll", EntryPoint = "getMaMeanAngularPosition", CallingConvention = CallingConvention.Cdecl)]
        private static extern double getMaMeanAngularPosition();

        [DllImport("breithorn-driver-wrapper.dll", EntryPoint = "storeOneMaRegisterToNvm", CallingConvention = CallingConvention.Cdecl)]
        private static extern int storeOneMaRegisterToNvm(byte address);

        [DllImport("breithorn-driver-wrapper.dll", EntryPoint = "storeAllMaRegistersToNvm", CallingConvention = CallingConvention.Cdecl)]
        private static extern int storeAllMaRegistersToNvm();

        [DllImport("breithorn-driver-wrapper.dll", EntryPoint = "restoreAllMaRegistersFromNvm", CallingConvention = CallingConvention.Cdecl)]
        private static extern int restoreAllMaRegistersFromNvm();
            
        [DllImport("breithorn-driver-wrapper.dll", EntryPoint = "clearAllMaErrorFlags", CallingConvention = CallingConvention.Cdecl)]
        private static extern int clearAllMaErrorFlags();
        #endregion  

        public Connect() { }

        public void InitConnect()
        {
            Console.WriteLine("Connecting...");
            init();
            errorCode = setMaSiliconVersion(4);
            if (errorCode == 0) IsConnected = true;
            if (errorCode == -1) MessageBox.Show("EVKT-MACOM not connected to the computer, check USB connection", "Connection Error");
            if (errorCode == -2) MessageBox.Show("MagAlpha version number not supported", "Connection Error");
            if (errorCode == -3) MessageBox.Show("MagAlpha sensor not connected to the EVKT-MACOM, check the sensor connection", "Connection Error");
            if (errorCode == -4) MessageBox.Show("Auto detection failed to recognize the connected sensor", "Connection Error");

            Console.WriteLine("Connection response: " + errorCode);
        }

        public void CloseConnection() => close();
        
        public double AngularPosition()
        {
            error = readMagAlphaAngularPosition();
            if (error != 0)
                return 12000;
            return getMaMeanAngularPosition();
        }

        public void ReadMagnetFlag()
        {
            byte response = readMaRegister(27);
            int test = response >> 6;
            if(test == 0)
            {
                MGH = false;
                MGL = false;
            }
            else if(test == 1)
            {
                MGH = false;
                MGL = true;
            }
            else if(test == 2)
            {
                MGH = true;
                MGL = false;
            }
            else if(test == 3)
            {
                MGH = true;
                MGL = true;
            }

        }

        public void WriteAtRegister(byte address, byte value)
        {
            Console.WriteLine("Writing at register:" + address + " value: " + value);
            writeMaRegister(address, value);
        }
        
        public byte ReadMagnetValue() => readMaRegister(6);

        public int StoreRegisterToNvm(byte address) => storeOneMaRegisterToNvm(address);

        public int StoreAllRegistersToNvm() => storeAllMaRegistersToNvm();

        public int RestoreRegistersFromNvm() => restoreAllMaRegistersFromNvm();

        public int ClearAllErrorFlags() => clearAllMaErrorFlags();
    }
}
