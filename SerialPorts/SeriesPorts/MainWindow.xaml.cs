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



/*
Reference:
[Serial Port Introduction]https://www.codeproject.com/Articles/678025/Serial-Comms-in-Csharp-for-Beginners
[Time Format]             https://msdn.microsoft.com/zh-tw/library/bb882581(v=vs.110).aspx
[receive thread conflict] http://www.yaoguangkeji.com/a_nbLZPlKk.html     
[Paragraph coloring]      http://www.it1352.com/375841.html
     
     
     */

namespace SeriesPorts
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort ComPort = new SerialPort();
        string dateString = "7/16/2008 8:32:45.126 AM";
        private DispatcherTimer ShowTimer;
        private Object thisLock = new Object();
        private Int32 recCount = 0;
        private Int32 sndCount = 0;

        public delegate void SerialPortEventHandler(Object sender, SerialPortEventArgs e);
        public event SerialPortEventHandler comReceiveDataEvent = null;
        public event SerialPortEventHandler comOpenEvent = null;
        public event SerialPortEventHandler comCloseEvent = null;

        public class SerialPortEventArgs : EventArgs
        {
            public bool isOpend = false;
            public Byte[] receivedBytes = null;
        }

        public MainWindow()
        {
            InitializeComponent();
            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowCurTimer);//起个Timer一直获取当前时间
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ShowTimer.Start();

            this.comCloseEvent += new SerialPortEventHandler(method_CloseCom);
            this.comOpenEvent += new SerialPortEventHandler(method_OpenCom);
            this.comReceiveDataEvent += new SerialPortEventHandler(method_RecCom);
        }
        public void method_CloseCom(Object sender, SerialPortEventArgs e)
        {

        }
        public void method_OpenCom(Object sender, SerialPortEventArgs e)
        {
 
        }
        public void method_RecCom(Object sender, SerialPortEventArgs e)
        {

            this.Dispatcher.Invoke(new Action(() => {

                TextRange rangeOfWord = new TextRange(rtb1.Document.ContentEnd, rtb1.Document.ContentEnd);
                rangeOfWord.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff") + " [TX] - ";
                rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);

                rangeOfWord = new TextRange(rtb1.Document.ContentEnd, rtb1.Document.ContentEnd);
                rangeOfWord.Text = Encoding.Default.GetString(e.receivedBytes);
                rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.PowderBlue);

                rtb1.AppendText("\n");
                rtb1.ScrollToEnd();

                recCount += e.receivedBytes.Length;
                rectbx.Text = recCount.ToString();
            }));




        }


        public void ShowCurTimer(object sender, EventArgs e)
        {
            ShowTime();
        }

        private void ShowTime()
        {
            //获得年月日
            this.date1tbx.Text = DateTime.Now.ToString("yyyy/MM/dd");  
            //获得时分秒
            this.date2tbx.Text = DateTime.Now.ToString("HH:mm:ss");

        }

        private void ocbtn_Click(object sender, RoutedEventArgs e)
        {
            SerialPortEventArgs args = new SerialPortEventArgs();
            if (Convert.ToString(ocbtn.Content) == "OPEN")
            {
                ocbtn.Content = "CLOSE";
                ComPort.PortName = Convert.ToString(COMcbx.Text);
                ComPort.BaudRate = Convert.ToInt32(Baucbx.Text);
                ComPort.DataBits = Convert.ToInt16(Datcbx.Text);
                ComPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), Stopcbx.Text);
                ComPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), Flocbx.Text);
                ComPort.Parity = (Parity)Enum.Parse(typeof(Parity), Parcbx.Text);
                ComPort.Open();
            }
            else if (Convert.ToString(ocbtn.Content) == "CLOSE")
            {
                ocbtn.Content = "OPEN";
                ComPort.Close();
            }

            if (ComPort.IsOpen)
            {
                statustbk.Text = "Connected";
                sendbtn1.IsEnabled = true;
                sendbtn2.IsEnabled = true;
                ComPort.DataReceived += new SerialDataReceivedEventHandler(ComReceived);

                args.isOpend = true;
                if (comOpenEvent != null)
                {
                    comOpenEvent.Invoke(this, args);
                }

            }
            else
            {
                statustbk.Text = "Disconnected";
                args.isOpend = false;
                if (comCloseEvent != null)
                {
                    comCloseEvent.Invoke(this, args);
                }
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

        private void Baucbx_Initialized(object sender, EventArgs e)
        {
            Baucbx.Items.Add(9600);
            Baucbx.Items.Add(14400);
            Baucbx.Items.Add(115200);
            Baucbx.Text = Baucbx.Items[0].ToString();
        }

        private void Datcbx_Initialized(object sender, EventArgs e)
        {
            Datcbx.Items.Add(7);
            Datcbx.Items.Add(8);
            Datcbx.Items.Add(9);
            Datcbx.Text = Datcbx.Items[0].ToString();
        }

        private void Stopcbx_Initialized(object sender, EventArgs e)
        {
            Stopcbx.Items.Add("None");
            Stopcbx.Items.Add("One");
            Stopcbx.Items.Add("Two");
            Stopcbx.Items.Add("OnePointFive");
            Stopcbx.Text = Stopcbx.Items[1].ToString();
        }

        private void Parcbx_Initialized(object sender, EventArgs e)
        {
            Parcbx.Items.Add("None");
            Parcbx.Items.Add("Odd");
            Parcbx.Items.Add("Even");
            Parcbx.Items.Add("Mark");
            Parcbx.Items.Add("Space");
            Parcbx.Text = Parcbx.Items[0].ToString();
        }

        private void Flocbx_Initialized(object sender, EventArgs e)
        {
            Flocbx.Items.Add("None");
            Flocbx.Items.Add("XOnXOff");
            Flocbx.Items.Add("RequestToSend");
            Flocbx.Items.Add("RequestToSendXOnXOff");
            Flocbx.Text = Flocbx.Items[0].ToString();
        }

        private void Refbtn_Click(object sender, RoutedEventArgs e)
        {
            string[] ArrayComPortsNames = null;

            ArrayComPortsNames = SerialPort.GetPortNames();
            if(ArrayComPortsNames.Length == 0)
            {
                statustbk.Text = "No ComPorts Found!";
                ocbtn.IsEnabled = false;

            }
            else
            {
                Array.Sort(ArrayComPortsNames);
                for (int i = 0; i < ArrayComPortsNames.Length; i++)
                {
                    COMcbx.Items.Add(ArrayComPortsNames[i]);
                }
                COMcbx.Text = ArrayComPortsNames[0];
                ocbtn.IsEnabled = true;


            }
        }


        private void sendbtn1_Click(object sender, RoutedEventArgs e)
        {
            string DataBuffer = sendtbx1.Text;
            TextRange rangeOfWord = new TextRange(rtb1.Document.ContentEnd, rtb1.Document.ContentEnd);
            rangeOfWord.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff") + " [TX] - " ;
            rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);

            rangeOfWord = new TextRange(rtb1.Document.ContentEnd, rtb1.Document.ContentEnd);
            rangeOfWord.Text = DateTime.Now.ToString(DataBuffer);
            rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Pink);

            rtb1.AppendText("\n");
            rtb1.ScrollToEnd();
            ComPort.Write(DataBuffer);
        }



        private void StatusBar_Initialized(object sender, EventArgs e)
        {
            statustbk.Text = "disconnected";
            rectbx.Text = "0";
            senttbk.Text = "0";
        }





        private void sendGroup_Initialized(object sender, EventArgs e)
        {
            sendbtn1.IsEnabled = false;
            sendbtn2.IsEnabled = false;
            sendtbx1.Clear();
            sendtbx2.Clear();
        }

        private void recGroup_Initialized(object sender, EventArgs e)
        {
            this.rtb1.Document.Blocks.Clear();
            rtb1.AppendText("\n");
        }
    }
}
