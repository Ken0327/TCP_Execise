using System;
using System.Collections;
using System.Collections.Generic;
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

namespace TCP_Excercise
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        int pass_count;
        int fail_count;


        #region 欄位定義
        //伺服器程式使用的埠，預設為8888
        private int _port = 8888;
        //接收資料緩衝區大小64K
        private const int Datas = 64 * 1024;
        //伺服器端的監聽器
        private TcpListener listener = null;
        //保存所有用戶端會話的雜湊表
        private Hashtable ht = new Hashtable();
        #endregion
        private TcpClient _tcpclient = new TcpClient();

        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private void server_conn_Click(object sender, RoutedEventArgs e)
        {
            //var hostname = Dns.GetHostName();
            //var IP = Dns.GetHostAddresses(hostname);
            //IPAddress ip = Dns.GetHostAddresses(Dns.GetHostName())[1];
            //listener = new TcpListener(ip, 8888);
            //listener.Start();
            //server_status.Text = "伺服器已啟動，正在監聽...\n";
            //server_status.Text = "伺服器模式:等待封包資料...本機IP: " + string.Format("{0}\t埠號：{1}\n", ip, _port);
            //Server();
            Task.Run(() =>
            {
                try
                {
                    var hostname = Dns.GetHostName();
                    var IP = Dns.GetHostAddresses(hostname);
                    IPAddress ip = Dns.GetHostAddresses(Dns.GetHostName())[1];
                    IPEndPoint ep = new IPEndPoint(ip, 8888);
                    s.Bind(ep);
                    s.Listen(10);

                    Socket h = s.Accept();

                    var stream = new NetworkStream(h);
                    var sr = new StreamReader(stream);

                    Server_msg(sr.ReadLine());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            });
        }

        private void server_disconn_Click(object sender, RoutedEventArgs e)
        {
            //listener.Stop();
            s.Shutdown(SocketShutdown.Both);
            s.Disconnect(true);
            //s.Dispose();
            server_status.Text = "伺服器已斷線";
        }

        private void client_conn_Click(object sender, RoutedEventArgs e)
        {
            Client_msg("Client Started");
            var ip = txt_ip.Text;
            //_tcpclient.Connect(ip, 8888);
            c.Connect(ip, 8888);
            client_status.Text = "Client Connected ...";
            Client("join");

        }


        private void btn_inuput_Click(object sender, RoutedEventArgs e)
        {
            var input = txt_input.Text;
            try
            {
                var ip = txt_ip.Text;
                Task.Run(() =>
                {
                    Client(input);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void Server()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    byte[] bytes = new byte[Datas];
                    Socket socket = listener.AcceptSocket();
                    socket.Receive(bytes);
                    string MbrName = Encoding.Unicode.GetString(bytes).TrimEnd('\0');
                    //success
                    if (MbrName == "conn")
                        socket.Send(Encoding.Unicode.GetBytes("Data Source=127.0.0.1;Initial Catalog=ooo163;user id=sa;password=000"));
                    else if (MbrName == "join")
                    {
                        socket.Send(Encoding.Unicode.GetBytes("success"));
                    }
                    else if (MbrName == "Pass")
                    {
                        socket.Send(Encoding.Unicode.GetBytes("Receive Pass"));
                        pass_count++;
                    }
                    else if (MbrName == "Fail")
                    {
                        socket.Send(Encoding.Unicode.GetBytes("Receive Fail"));
                        fail_count++;
                    }
                    else
                        socket.Send(Encoding.Unicode.GetBytes("error"));
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        server_txt.Text = MbrName.ToString();
                        pass.Text = pass_count.ToString();
                        fail.Text = fail_count.ToString();
                    });
                }
            });
        }

        private void Client(string input)
        {
            try
            {
                //socket
                var stream = new NetworkStream(c);
                var sr = new StreamReader(stream);
                var sw = new StreamWriter(stream);
                //tcpclient
                //NetworkStream stream = _tcpclient.GetStream();
                sw.WriteLine(input);
                sw.Flush();

                Client_msg(sr.ReadLine());

                //byte[] outStream = Encoding.Unicode.GetBytes(input);
                //NetStream.Write(outStream, 0, outStream.Length);
                //NetStream.Flush();

                //byte[] inStream = new byte[50];
                //NetStream.Read(inStream, 0, inStream.Length);//(int)clientSocket.ReceiveBufferSize);
                //string returndata = Encoding.Unicode.GetString(inStream).TrimEnd('\0');
                ////string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                //msg(returndata);
                //txt_input.Text = "";
                //txt_input.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void Client_msg(string mesg)
        {
            client_txt.Text = client_txt.Text + Environment.NewLine + " >> " + mesg;
        }

        public void Server_msg(string mesg)
        {
            server_txt.Text = server_txt.Text + Environment.NewLine + " >> " + mesg;
        }
    }
}
