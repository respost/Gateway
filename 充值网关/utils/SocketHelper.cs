using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace 充值网关
{
    public class SocketHelper
    {
        //服务端
        private Socket ServerSocket = null;
        //tcp客户端字典
        public Dictionary<string, Session> dic_ClientSocket = new Dictionary<string, Session>();
        //线程字典,每新增一个连接就添加一条线程
        private Dictionary<string, Thread> dic_ClientThread = new Dictionary<string, Thread>();
        //监听客户端连接的标志
        private bool Flag_Listen = true;

        //定义的委托将新增的Socket连接传给主窗体的ListView
        public delegate void ClientSocketChanged(int status, Session sesson);
        public event ClientSocketChanged OnClientSocketChanged;

        //定义委托将客户端传过来的消息在主界面上展示出来
        public delegate void SocketText(Session sesson, string text);
        public event SocketText getSocketText;

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="port">端口号</param>
        public bool OpenServer(int port)
        {
            try
            {
                Flag_Listen = true;
                // 创建负责监听的套接字，注意其中的参数；
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // 创建包含ip和端口号的网络节点对象；
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                try
                {
                    // 将负责监听的套接字绑定到唯一的ip和端口上；
                    ServerSocket.Bind(endPoint);
                }
                catch
                {
                    return false;
                }
                // 设置监听队列的长度；
                ServerSocket.Listen(100);
                // 创建负责监听的线程；
                Thread Thread_ServerListen = new Thread(ListenConnecting);
                Thread_ServerListen.IsBackground = true;
                Thread_ServerListen.Start();

                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 关闭服务
        /// </summary>
        public void CloseServer()
        {
            lock (dic_ClientSocket)
            {
                foreach (var item in dic_ClientSocket)
                {
                    item.Value.Close();//关闭每一个连接
                }
            }
            lock (dic_ClientThread)
            {
                foreach (var item in dic_ClientThread)
                {
                    item.Value.Abort();//停止线程
                }
                dic_ClientThread.Clear();
            }
            Flag_Listen = false;
            //ServerSocket.Shutdown(SocketShutdown.Both);//服务端不能主动关闭连接,需要把监听到的连接逐个关闭
            if (ServerSocket != null)
                ServerSocket.Close();

        }
        /// <summary>
        /// 监听客户端请求的方法；
        /// </summary>
        private void ListenConnecting()
        {
            while (Flag_Listen)  // 持续不断的监听客户端的连接请求；
            {
                try
                {
                    // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的 套接字；
                    Socket sokConnection = ServerSocket.Accept();
                    // 将与客户端连接的 套接字 对象添加到集合中；
                    string str_EndPoint = sokConnection.RemoteEndPoint.ToString();
                    Session myTcpClient = new Session() { TcpSocket = sokConnection };
                    //创建线程接收数据
                    Thread th_ReceiveData = new Thread(ReceiveData);
                    th_ReceiveData.IsBackground = true;
                    th_ReceiveData.Start(myTcpClient);
                    //把线程及客户连接加入字典
                    dic_ClientThread.Add(str_EndPoint, th_ReceiveData);
                    dic_ClientSocket.Add(str_EndPoint, myTcpClient);
                    OnClientSocketChanged(1, myTcpClient);
                }
                catch
                {

                }
                Thread.Sleep(200);
            }
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sokConnectionparn"></param>
        private void ReceiveData(object sokConnectionparn)
        {
            Session tcpClient = sokConnectionparn as Session;
            Socket socketClient = tcpClient.TcpSocket;
            bool Flag_Receive = true;
            while (Flag_Receive)
            {
                try
                {
                    // 定义一个2M的缓存区；
                    byte[] arrMsgRec = new byte[1024 * 1024 * 2];
                    // 将接受到的数据存入到输入  arrMsgRec中；
                    int length = -1;
                    try
                    {
                        length = socketClient.Receive(arrMsgRec); // 接收数据，并返回数据的长度；
                    }
                    catch
                    {
                        Flag_Receive = false;
                        // 从通信线程集合中删除被中断连接的通信线程对象；
                        string keystr = socketClient.RemoteEndPoint.ToString();
                        dic_ClientThread[keystr].Abort();//关闭线程
                        dic_ClientThread.Remove(keystr);//删除字典中该线程

                        tcpClient = null;
                        socketClient = null;
                        break;
                    }
                    byte[] buf = new byte[length];
                    Array.Copy(arrMsgRec, buf, length);
                    lock (tcpClient.m_Buffer)
                    {
                        string text = System.Text.Encoding.Default.GetString(buf);
                        if (!string.IsNullOrEmpty(text))
                        {                        
                            getSocketText(tcpClient, text);
                            //业务需要，收到客户端的消息后，回复进行确认
                            //tcpClient.Send(System.Text.Encoding.Default.GetBytes("[{\"code\":1,\"age\":\"成功\"}]"));
                        }
                    }
                }
                catch
                {
                }
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// 发送数据给指定的客户端
        /// </summary>
        /// <param name="_endPoint">客户端套接字</param>
        /// <param name="_buf">发送的数组</param>
        /// <returns></returns>
        public bool SendData(string _endPoint, byte[] _buf)
        {
            Session myT = new Session();
            if (dic_ClientSocket.TryGetValue(_endPoint, out myT))
            {
                myT.Send(_buf);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}
