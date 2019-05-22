using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Milwaukee_Drill_Trigger_GUI
{
    class Connect
    {
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
        #endregion  

        public Connect() { }

        public void InitConnect()
        {
            Console.WriteLine("test");
            init();
            int errorCode;
            errorCode = setMaSiliconVersion(4);
            if (errorCode == 0)
                IsConnected = true;
            if(errorCode == -1)
                MessageBox.Show("EVKT-MACOM not connected to the computer, check USB connection", "Connection Error");
            if (errorCode == -2)
                MessageBox.Show("MagAlpha version number not supported", "Connection Error");
            if (errorCode == -3)
                MessageBox.Show("MagAlpha sensor not connected to the EVKT-MACOM, check the sensor connection", "Connection Error");
            if (errorCode == -4)
                MessageBox.Show("Auto detection failed to recognize the connected sensor", "Connection Error");

            Console.WriteLine(errorCode);
        }

        public void CloseConnection()
        {
            close();
        }
        public double AngularPosition()
        {
            int error = readMagAlphaAngularPosition();
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
            }else if(test == 1)
            {
                MGH = false;
                MGL = true;
            }else if(test == 2)
            {
                MGH = true;
                MGL = false;
            }

        }

        public void WriteAtRegister(byte address, byte value)
        {
            writeMaRegister(address, value);
        }

        public byte ReadMagnetValue()
        {
            return readMaRegister(6);
        }
    }
}
