using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace TCP_Excercise
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataBinding();
            timer1.Interval = TimeSpan.FromSeconds(2);
            timer2.Interval = TimeSpan.FromSeconds(4);
        }



        #region 欄位定義
        int pass_count;
        int fail_count;
        int Totlpass_count;
        int Totlfail_count;

        //  在宣告區先行宣告 Socket 物件 
        Socket[] SckSs;   // 一般而言 Server 端都會設計成可以多人同時連線. 
        Socket SckSPort;  // Client端
        int SckCIndex;    // 定義一個指標用來判斷現下有哪一個空的 Socket 可以分配給 Client 端連線;
        int SPort = 8888;
        private const int RDataLen = 64 * 1024;// 這裡的RDataLen為要傳送資料的長度" 給Client端

        private DispatcherTimer timer1 = new DispatcherTimer();
        private DispatcherTimer timer2 = new DispatcherTimer();
        #endregion

        #region DataBinding
        private BindingData m_ServerStatus, m_ClientStatus, m_ServerReceive, m_ClientReceive, m_pass, m_fail, m_TotlPass, m_TotlFail;
        void DataBinding()
        {
            m_ServerStatus = new BindingData();
            m_ClientStatus = new BindingData();
            m_ServerReceive = new BindingData();
            m_ClientReceive = new BindingData();
            m_pass = new BindingData();
            m_fail = new BindingData();
            m_TotlPass = new BindingData();
            m_TotlFail = new BindingData();
            m_ServerStatus.TextMsg = "處理進度中...";
            m_ClientStatus.TextMsg = "處理進度中...";
            Binding BindingTxtBlk = new Binding() { Source = m_ServerStatus, Path = new PropertyPath("TextMsg") };
            Binding BindingTxtBlk1 = new Binding() { Source = m_ClientStatus, Path = new PropertyPath("TextMsg") };
            Binding BindingTxtBlk2 = new Binding() { Source = m_ServerReceive, Path = new PropertyPath("TextMsg") };
            Binding BindingTxtBlk3 = new Binding() { Source = m_ClientReceive, Path = new PropertyPath("TextMsg") };
            Binding BindingTxtBlk4 = new Binding() { Source = m_pass, Path = new PropertyPath("TextMsg") };
            Binding BindingTxtBlk5 = new Binding() { Source = m_fail, Path = new PropertyPath("TextMsg") };
            Binding BindingTxtBlk6 = new Binding() { Source = m_TotlPass, Path = new PropertyPath("TextMsg") };
            Binding BindingTxtBlk7 = new Binding() { Source = m_TotlFail, Path = new PropertyPath("TextMsg") };
            server_status.SetBinding(TextBlock.TextProperty, BindingTxtBlk);
            client_status.SetBinding(TextBlock.TextProperty, BindingTxtBlk1);
            server_txt.SetBinding(TextBlock.TextProperty, BindingTxtBlk2);
            client_txt.SetBinding(TextBlock.TextProperty, BindingTxtBlk3);
            txt_pass.SetBinding(TextBlock.TextProperty, BindingTxtBlk4);
            txt_fail.SetBinding(TextBlock.TextProperty, BindingTxtBlk5);
            txt_totlpass.SetBinding(TextBlock.TextProperty, BindingTxtBlk6);
            txt_totlfail.SetBinding(TextBlock.TextProperty, BindingTxtBlk7);
        }
        #endregion

        private void server_conn_Click(object sender, RoutedEventArgs e)
        {
            var hostname = Dns.GetHostName();
            var IP = Dns.GetHostAddresses(hostname);
            IPAddress ip = Dns.GetHostAddresses(Dns.GetHostName())[2];
            IPEndPoint ep = new IPEndPoint(ip, 8888);
            m_ServerStatus.TextMsg = $"本機位置: {hostname}     IP: {ip}";

            Listen(ip);

            //Task.Run(() =>
            //{
            //    try
            //    {
            //        var hostname = Dns.GetHostName();
            //        var IP = Dns.GetHostAddresses(hostname);
            //        IPAddress ip = Dns.GetHostAddresses(Dns.GetHostName())[2];
            //        IPEndPoint ep = new IPEndPoint(ip, 8888);
            //        m_ServerStatus.TextMsg = $"本機位置: {hostname}     IP: {ip}";

            //        s.Bind(ep);
            //        s.Listen(10);

            //        Socket h = s.Accept();

            //        var stream = new NetworkStream(h);
            //        var sr = new StreamReader(stream);

            //        Server_msg(sr.ReadLine());
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.ToString());
            //    }
            //});
        }

        private void server_disconn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var s in SckSs)
                {
                    s.Close();
                    s.Dispose();
                    m_ServerStatus.TextMsg = "伺服器中斷";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            ////listener.Stop();
            //s.Shutdown(SocketShutdown.Both);
            //s.Disconnect(true);
            ////s.Dispose();
            //server_status.Text = "伺服器已斷線";
        }

        private void client_conn_Click(object sender, RoutedEventArgs e)
        {
            var ip = txt_ip.Text;
            ConnectServer(ip);
            m_ClientStatus.TextMsg = "已連接至伺服器: " + ip;
        }


        private void btn_inuput_Click(object sender, RoutedEventArgs e)
        {
            var input = txt_input.Text;
            //SckSSend_client(input);
            if (input == "StartP")
            {
                Task.Run(() =>
                {
                    for (var i = 0; i < 200; i++)
                    {
                        Thread.Sleep(200);
                        Parsing(null, null);
                    }
                });
                //timer1.Start();
                //timer1.IsEnabled = true;
                //timer1.Tick += Parsing;
            }
            if (input == "StartF")
            {
                Task.Run(() =>
                {
                    for (var i = 0; i < 50; i++)
                    {
                        Thread.Sleep(500);
                        Parsing1(null, null);
                    }
                });
                //timer2.Start();
                //timer2.IsEnabled = true;
                //timer2.Tick += Parsing1;
            }
            if (input == "StopP")
            {
                timer1.Stop();
                timer1.IsEnabled = false;
            }
            if (input == "StopF")
            {
                timer2.Stop();
                timer2.IsEnabled = false;
            }
        }

        private void Parsing(object sender, EventArgs e)
        {
            SckSSend_client("Pass");
            pass_count++;
            Totlpass_count++;
            m_pass.TextMsg = pass_count.ToString();
            m_TotlPass.TextMsg = Totlpass_count.ToString();

        }
        private void Parsing1(object sender, EventArgs e)
        {
            SckSSend_client("Fail");
            fail_count++;
            Totlfail_count++;
            m_fail.TextMsg = fail_count.ToString();
            m_TotlFail.TextMsg = Totlfail_count.ToString();
        }

        private void Listen(IPAddress ip)
        {

            // 用 Resize 的方式動態增加 Socket 的數目
            Array.Resize(ref SckSs, 1);

            SckSs[0] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SckSs[0].Bind(new IPEndPoint(ip, SPort));

            // 其中 LocalIP 和 SPort 分別為 string 和 int 型態, 前者為 Server 端的IP, 後者為S erver 端的Port
            SckSs[0].Listen(10); // 進行聆聽; Listen( )為允許 Client 同時連線的最大數

            SckSWaitAccept();   // 另外寫一個函數用來分配 Client 端的 Socket
        }

        private void SckSWaitAccept()
        {
            // 判斷目前是否有空的 Socket 可以提供給Client端連線
            bool FlagFinded = false;
            for (int i = 1; i < SckSs.Length; i++)
            {
                // SckSs[i] 若不為 null 表示已被實作過, 判斷是否有 Client 端連線
                if (SckSs[i] != null)
                {
                    // 如果目前第 i 個 Socket 若沒有人連線, 便可提供給下一個 Client 進行連線
                    if (SckSs[i].Connected == false)
                    {
                        SckCIndex = i;
                        FlagFinded = true;
                        break;
                    }
                }
            }

            // 如果 FlagFinded 為 false 表示目前並沒有多餘的 Socket 可供 Client 連線
            if (FlagFinded == false)
            {
                // 增加 Socket 的數目以供下一個 Client 端進行連線
                SckCIndex = SckSs.Length;
                Array.Resize(ref SckSs, SckCIndex + 1);
            }

            // 以下兩行為多執行緒的寫法, 因為接下來 Server 端的部份要使用 Accept() 讓 Cleint 進行連線;
            // 該執行緒有需要時再產生即可, 因此定義為區域性的 Thread. 命名為 SckSAcceptTd;
            // 在 new Thread( ) 裡為要多執行緒去執行的函數. 這裡命名為 SckSAcceptProc;

            Thread SckSAcceptTd = new Thread(SckSAcceptProc);
            SckSAcceptTd.Start();  // 開始執行 SckSAcceptTd 這個執行緒
        }

        // 接收來自Client的連線與Client傳來的資料
        private void SckSAcceptProc()
        {
            // 這裡加入 try 是因為 SckSs[0] 若被 Close 的話, SckSs[0].Accept() 會產生錯誤
            try
            {
                SckSs[SckCIndex] = SckSs[0].Accept();  // 等待Client 端連線

                // 為什麼 Accept 部份要用多執行緒, 因為 SckSs[0] 會停在這一行程式碼直到有 Client 端連上線, 並分配給 SckSs[SckCIndex] 給 Client 連線之後程式才會繼續往下, 若是將 Accept 寫在主執行緒裡, 在沒有Client連上來之前, 主程式將會被hand在這一行無法再做任何事了!!
                // 能來這表示有 Client 連上線. 記錄該 Client 對應的 SckCIndex
                int Scki = SckCIndex;
                // 再產生另一個執行緒等待下一個 Client 連線
                SckSWaitAccept();

                long IntAcceptData;
                byte[] clientData = new byte[RDataLen];  // 其中RDataLen為每次要接受來自 Client 傳來的資料長度

                while (true)
                {
                    // 程式會被 hand 在此, 等待接收來自 Client 端傳來的資料
                    IntAcceptData = SckSs[Scki].Receive(clientData);
                    string S1 = Encoding.Unicode.GetString(clientData).TrimEnd('\0');
                    if (S1 == "join")
                        SckSs[Scki].Send(Encoding.Unicode.GetBytes("Success Connect"));
                    if (S1 == "Pass")
                    {
                        SckSs[Scki].Send(Encoding.Unicode.GetBytes("Receive:Pass"));
                        Totlpass_count++;
                        m_TotlPass.TextMsg = Totlpass_count.ToString();
                    }
                    if (S1 == "Fail")
                    {
                        SckSs[Scki].Send(Encoding.Unicode.GetBytes("Receive:Fail"));
                        Totlfail_count++;
                        m_TotlFail.TextMsg = Totlfail_count.ToString();
                    }
                    m_ServerReceive.TextMsg = S1 + "+1";
                    Console.WriteLine(S1);
                }
            }
            catch (Exception ex)
            {
                //m_ServerStatus.TextMsg = e.ToString();
                Console.WriteLine(ex.ToString());
            }
        }

        private void btn_pass_Click(object sender, RoutedEventArgs e)
        {
            if (client.IsSelected)
            {
                SckSSend_client("Pass");
            }
            pass_count++;
            Totlpass_count++;
            m_pass.TextMsg = pass_count.ToString();
            m_TotlPass.TextMsg = Totlpass_count.ToString();
        }

        private void btn_fail_Click(object sender, RoutedEventArgs e)
        {
            if (client.IsSelected)
            {
                SckSSend_client("Fail");
            }
            fail_count++;
            Totlfail_count++;
            m_fail.TextMsg = fail_count.ToString();
            m_TotlFail.TextMsg = Totlfail_count.ToString();
        }

        private void SckSSend(string SendS)
        {
            for (int Scki = 1; Scki < SckSs.Length; Scki++)
            {
                if (null != SckSs[Scki] && SckSs[Scki].Connected == true)
                {
                    try
                    {
                        // SendS 在這裡為 string 型態, 為 Server 要傳給 Client 的字串, 我測試傳送 字串 "ABCDE" 給Client端
                        SckSs[Scki].Send(Encoding.Unicode.GetBytes(SendS));
                    }
                    catch(Exception ex)
                    {
                        //m_ServerStatus.TextMsg = e.ToString();
                        Console.WriteLine(ex.ToString());
                        // 這裡出錯, 主要是出在 SckSs[Scki] 出問題, 自己加判斷吧~
                    }
                }
            }
        }

        private void btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            pass_count = 0;
            fail_count = 0;
            Totlpass_count = 0;
            Totlfail_count = 0;
            m_pass.TextMsg = "0";
            m_fail.TextMsg = "0";
            m_TotlPass.TextMsg = "0";
            m_TotlFail.TextMsg = "0";
        }

        private void ConnectServer(string ip)
        {
            try
            {
                SckSPort = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SckSPort.Connect(new IPEndPoint(IPAddress.Parse(ip), SPort));
                // RmIp和SPort分別為string和int型態, 前者為Server端的IP, 後者為Server端的Port
                // 同 Server 端一樣要另外開一個執行緒用來等待接收來自 Server 端傳來的資料, 與Server概念同
                Thread SckSReceiveTd = new Thread(SckSReceiveProc);
                SckSReceiveTd.Start();
            }
            catch(Exception ex)
            {
                //m_ClientStatus.TextMsg = e.ToString();
                Console.WriteLine(ex.ToString());
            }
        }

        private void client_disconn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SckSPort.Close();
                SckSPort.Dispose();
                m_ClientStatus.TextMsg = "中止連線";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void SckSReceiveProc()
        {
            try
            {
                long IntAcceptData;
                byte[] clientData = new byte[RDataLen];
                SckSPort.Send(Encoding.Unicode.GetBytes("Join"));
                while (true)
                {
                    // 程式會被 hand 在此, 等待接收來自 Server 端傳來的資料
                    IntAcceptData = SckSPort.Receive(clientData);
                    //string S = Encoding.Default.GetString(clientData);
                    string S1 = Encoding.Unicode.GetString(clientData).TrimEnd('\0');
                    m_ClientReceive.TextMsg = S1;
                    Console.WriteLine(S1);
                    //if (!SckSPort.Connected)
                    //{
                    //    var ip = txt_ip.Text;
                    //    ConnectServer(ip);
                    //}
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                m_ClientStatus.TextMsg = "連線中斷";
            }
        }

        private void SckSSend_client(string SendS)
        {
            try
            {
                SckSPort.Send(Encoding.Unicode.GetBytes(SendS));
            }
            catch (Exception e)
            {
                m_ClientStatus.TextMsg = e.ToString();
            }
        }

        private void btn_inuput_S_Click(object sender, RoutedEventArgs e)
        {
            var Send_str = txt_input_S.Text;
            SckSSend(Send_str);
            if (Send_str == "StartP")
            {
                Task.Run(() =>
                {
                    for (var i = 0; i < 200; i++)
                    {
                        Thread.Sleep(200);
                        Parsing3(null, null);
                    }
                });

                //timer1.Start();
                //timer1.IsEnabled = true;
                //timer1.Tick += Parsing3;
            }
            if (Send_str == "StartF")
            {
                Task.Run(() =>
                {
                    for (var i = 0; i < 50; i++)
                    {
                        Thread.Sleep(500);
                        Parsing4(null, null);
                    }
                });
                //timer2.Start();
                //timer2.IsEnabled = true;
                //timer2.Tick += Parsing4;
            }
            if (Send_str == "StopP")
            {
                timer1.Stop();
                timer1.IsEnabled = false;
            }
            if (Send_str == "StopF")
            {
                timer2.Stop();
                timer2.IsEnabled = false;
            }
        }

        private void Parsing3(object sender, EventArgs e)
        {
            pass_count++;
            Totlpass_count++;
            m_pass.TextMsg = pass_count.ToString();
            m_TotlPass.TextMsg = Totlpass_count.ToString();
        }
        private void Parsing4(object sender, EventArgs e)
        {
            fail_count++;
            Totlfail_count++;
            m_fail.TextMsg = fail_count.ToString();
            m_TotlFail.TextMsg = Totlfail_count.ToString();
        }

    }

    public class BindingData : INotifyPropertyChanged
    {
        private string m_TextMsg;
        public string TextMsg
        {
            set
            {
                if (m_TextMsg != value)
                {
                    m_TextMsg = value;
                    OnPropertyChanged("TextMsg");
                }
            }
            get
            {
                return m_TextMsg;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
