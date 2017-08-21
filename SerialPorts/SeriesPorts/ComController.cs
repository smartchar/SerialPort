using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Windows.Threading;
using MahApps.Metro.Controls;

namespace SeriesPorts
{
    public class ComController
    {
        MainWindow MW;

        SerialPort ComPort = new SerialPort();
        private Object thisLock = new Object();

        public delegate void SerialPortEventHandler(Object sender, SerialPortEventArgs e);
        public event SerialPortEventHandler comReceiveDataEvent = null;
        public event SerialPortEventHandler comOpenEvent = null;
        public event SerialPortEventHandler comCloseEvent = null;

        public class SerialPortEventArgs : EventArgs
        {
            public bool isOpend = false;
            public Byte[] receivedBytes = null;
        }

        public ComController(MainWindow MAINWINDOW)
        {

            //https://stackoverflow.com/questions/41151478/how-to-access-mainwindow-from-another-class
            MW = MAINWINDOW;
            comCloseEvent += new SerialPortEventHandler(MW.method_CloseCom);
            comOpenEvent += new SerialPortEventHandler(MW.method_OpenCom);
            comReceiveDataEvent += new SerialPortEventHandler(MW.method_RecCom);
        }

        public void OpenCom(string PortName, string BaudRate,
            string DataBits, string StopBits, string Parity, string HandShake)
        {
            SerialPortEventArgs args = new SerialPortEventArgs();
            if (!ComPort.IsOpen)
            {
                ComPort.PortName = Convert.ToString(PortName);
                ComPort.BaudRate = Convert.ToInt32(BaudRate);
                ComPort.DataBits = Convert.ToInt16(DataBits);
                ComPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBits);
                ComPort.Parity = (Parity)Enum.Parse(typeof(Parity), Parity);
                ComPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), HandShake);
                ComPort.Open();

                ComPort.DataReceived += new SerialDataReceivedEventHandler(ComReceived);
            }
            else 
            {
                return;
            }

            args.isOpend = true;
            if (comOpenEvent != null)
            {
                comOpenEvent.Invoke(this, args);
            }

        }

        public void CloseCom()
        {
            SerialPortEventArgs args = new SerialPortEventArgs();

            if (ComPort.IsOpen)
            {
                ComPort.Close();
            }
            args.isOpend = false;
            if (comCloseEvent != null)
            {
                comCloseEvent.Invoke(this, args);
            }
        }

        private void ComReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (ComPort.BytesToRead <= 0)
            {
                return;
            }
            //Thread Safety explain in MSDN:
            // Any public static (Shared in Visual Basic) members of this type are thread safe. 
            // Any instance members are not guaranteed to be thread safe.
            // So, we need to synchronize I/O
            lock (thisLock)
            {
                int len = ComPort.BytesToRead;
                Byte[] data = new Byte[len];
                try
                {
                    ComPort.Read(data, 0, len);
                }
                catch (System.Exception)
                {
                    //catch read exception
                }
                SerialPortEventArgs args = new SerialPortEventArgs();
                args.receivedBytes = data;
                if (comReceiveDataEvent != null)
                {
                    comReceiveDataEvent.Invoke(this, args);
                }
            }
        }

        public void ComSend(byte[] buffer, int offset, int count)
        {
            ComPort.Write(buffer,offset,count);

        }

        public void ComSend(string sendbuf)
        {
            ComPort.Write(sendbuf);

        }

    }
}