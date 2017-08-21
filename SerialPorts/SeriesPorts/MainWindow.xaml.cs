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


/*
Reference:
[Serial Port Introduction]https://www.codeproject.com/Articles/678025/Serial-Comms-in-Csharp-for-Beginners
[Time Format]             https://msdn.microsoft.com/zh-tw/library/bb882581(v=vs.110).aspx
[receive thread conflict] http://www.yaoguangkeji.com/a_nbLZPlKk.html     
[Paragraph coloring]      http://www.it1352.com/375841.html
[MetroApps]               http://mahapps.com/guides/quick-start.html
[Color]                   http://www.cnblogs.com/ProJKY/archive/2011/12/27/2303879.html     
     */

namespace SeriesPorts
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class MainWindow : MetroWindow
    {
        ComController ComCtr ;
        private DispatcherTimer ShowTimer;
        private Int32 recCount = 0;
        private Int32 sndCount = 0;


        public delegate void SendEventHandler(object sender, RoutedEventArgs e);
        public event SendEventHandler comSendEvent = null;


        public MainWindow()
        {
            InitializeComponent();
            InitialzeUI();
            ComCtr = new ComController(this);
            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowCurTimer);//起个Timer一直获取当前时间
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ShowTimer.Start();

            comSendEvent += new SendEventHandler(method_SendCom);


        }

        public void ShowCurTimer(object sender, EventArgs e)
        {
            ShowTime();
        }

        private void ShowTime()
        {
         //   this.date1tbx.Text = DateTime.Now.ToString("yyyy/MM/dd");  
            this.timetbx.Text = DateTime.Now.ToString("HH:mm:ss");

        }

        private void InitialzeUI()
        {
            Baucbx.Items.Add(9600);
            Baucbx.Items.Add(14400);
            Baucbx.Items.Add(115200);
            Baucbx.Text = Baucbx.Items[0].ToString();

            Datcbx.Items.Add(7);
            Datcbx.Items.Add(8);
            Datcbx.Items.Add(9);
            Datcbx.Text = Datcbx.Items[0].ToString();

            Stopcbx.Items.Add("None");
            Stopcbx.Items.Add("One");
            Stopcbx.Items.Add("Two");
            Stopcbx.Items.Add("OnePointFive");
            Stopcbx.Text = Stopcbx.Items[1].ToString();

            Parcbx.Items.Add("None");
            Parcbx.Items.Add("Odd");
            Parcbx.Items.Add("Even");
            Parcbx.Items.Add("Mark");
            Parcbx.Items.Add("Space");
            Parcbx.Text = Parcbx.Items[0].ToString();

            Handcbx.Items.Add("None");
            Handcbx.Items.Add("XOnXOff");
            Handcbx.Items.Add("RequestToSend");
            Handcbx.Items.Add("RequestToSendXOnXOff");
            Handcbx.Text = Handcbx.Items[0].ToString();

            sendbtn1.IsEnabled = false;
            sendbtn2.IsEnabled = false;
            sendtbx1.Clear();
            sendtbx2.Clear();

            this.rtb1.Document.Blocks.Clear();
            rtb1.AppendText("\n");

            sendtbx.Text = "Sent:" + sndCount.ToString();
            recetbx.Text = "Received:" + recCount.ToString();

        }

        private void Refbtn_Click(object sender, RoutedEventArgs e)
        {
            string[] ArrayComPortsNames = null;

            ArrayComPortsNames = SerialPort.GetPortNames();
            if (ArrayComPortsNames.Length == 0)
            {
                contbx.Text = "No ComPorts Found!";
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
            
            if (comSendEvent != null)
            {
                comSendEvent(sender, e);
            }

        }

        private void sendbtn2_Click(object sender, RoutedEventArgs e)
        {
            if (comSendEvent != null)
            {
                comSendEvent(sender, e);
            }
        }

        private void ocbtn_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToString(ocbtn.Content) == "OPEN")
            {
                ComCtr.OpenCom(
                    COMcbx.Text,
                    Baucbx.Text,
                    Datcbx.Text,
                    Stopcbx.Text,
                    Parcbx.Text,
                    Handcbx.Text
                    );
            }
            else if (Convert.ToString(ocbtn.Content) == "CLOSE")
            {
                ComCtr.CloseCom();
            }
        }

        public void method_OpenCom(Object sender, ComController.SerialPortEventArgs e)
        {
            ocbtn.Content = "CLOSE";
            contbx.Text = "Connected";
            sendbtn1.IsEnabled = true;
            sendbtn2.IsEnabled = true;
        }


        public void method_CloseCom(Object sender, ComController.SerialPortEventArgs e)
        {
            ocbtn.Content = "OPEN";
            contbx.Text = "Disconnected";
            sendbtn1.IsEnabled = false;
            sendbtn2.IsEnabled = false;


        }


        public void method_RecCom(Object sender, ComController.SerialPortEventArgs e)
        {

            this.Dispatcher.Invoke(new Action(() => {

                TextRange rangeOfWord = new TextRange(rtb1.Document.ContentEnd, rtb1.Document.ContentEnd);
                rangeOfWord.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff") + " [RX] - ";
                rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);

                rangeOfWord = new TextRange(rtb1.Document.ContentEnd, rtb1.Document.ContentEnd);
                rangeOfWord.Text = Encoding.Default.GetString(e.receivedBytes);
                rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkOrange);

                rtb1.AppendText("\n");
                rtb1.ScrollToEnd();

                recCount += e.receivedBytes.Length;
                recetbx.Text = "Received:" + recCount.ToString();
            }));

        }


        private void method_SendCom(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string DataBuffer = null;
            if (btn.Name == "sendbtn1")
            {
                 DataBuffer = sendtbx1.Text;

            }
            else if (btn.Name == "sendbtn2")
            {
                 DataBuffer = sendtbx2.Text;

            }
            if (sehex.IsChecked == true)
            {
                byte[] data = new byte[DataBuffer.Length];
                for (int i = 0; i < DataBuffer.Length; i++)
                {
                    data[i] = Convert.ToByte(DataBuffer.Substring(i, 1));
                }
                ComCtr.ComSend(data,0, DataBuffer.Length);
            }

            else
            {
                ComCtr.ComSend(DataBuffer);

            }

            TextRange rangeOfWord = new TextRange(rtb1.Document.ContentEnd, rtb1.Document.ContentEnd);
            rangeOfWord.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff") + " [TX] - ";
            rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);

            rangeOfWord = new TextRange(rtb1.Document.ContentEnd, rtb1.Document.ContentEnd);
            rangeOfWord.Text = DateTime.Now.ToString(DataBuffer);
            rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.SteelBlue);

            rtb1.AppendText("\n");
            rtb1.ScrollToEnd();

            sndCount += DataBuffer.Length;
            sendtbx.Text = "Sent:" + sndCount.ToString();




        }

        private void clrbtn_Click(object sender, RoutedEventArgs e)
        {
            this.rtb1.Document.Blocks.Clear();
            rtb1.AppendText("\n");
            sndCount = 0;
            recCount = 0;
            sendtbx.Text = "Sent:" + sndCount.ToString();
            recetbx.Text = "Received:" + recCount.ToString();



        }
    }


}
