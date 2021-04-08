/****************************************************
    文件：ServerSocket.cs
    作者：MRL Liu（Lab）
    邮箱: lpyaonuli@163.com
    日期：2021/4/7 18:24:16
    功能：提供服务端TCP套接字通信功能的接口
*****************************************************/
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using System.Collections.Generic;

namespace MNetSocket
{
	public class ServerSocket {
        private Socket socket;//服务端套接字
        private string ip = "127.0.0.1";//ip地址
        private int port = 5006;//端口号
        public bool is_Debug = true;//是否进行Debug，打印关键信息
        private static byte[] dataBuffer = new byte[102400];// 接收数据缓存
        private MsgProtol msgProtol = new MsgProtol();//网络协议
        private List<Socket> clientList = new List<Socket>();
        // 函数模板
        public RecvEventHandler recvHandler=null;
        public void SetIPAddress(string _ip, int _port = 5006)
        {
            ip = _ip;
            port = _port;
        }

        public void Init_Socket() {
            try
            {
                // 创建一个TCP套接字
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //为套接字绑定IP地址和端口号
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
                socket.Bind(ipEndPoint);//绑定ip和端口号
                //开始监听端口号
                socket.Listen(1);
                DebugHelper.Log("服务端套接字初始化IP地址为："+ip+":"+port, Color.green);
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }
        public void conn_client(RecvEventHandler onrecv) {
            if (recvHandler == null) {
                recvHandler = onrecv;//接收到消息的处理函数
            }
            try
            {
                // 异步监听客户端的连接请求，立即返回，程序继续执行
                socket.BeginAccept(AcceptCallBack, socket);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        // 程序接收到客户端连接的处理方法
        private void AcceptCallBack(IAsyncResult ar)
        {
            Socket serverSocket = ar.AsyncState as Socket;
            Socket clientSocket = serverSocket.EndAccept(ar);
            DebugHelper.Log("接收到一个客户端连接："+ clientSocket, Color.green);
            // 开始异步接收客户端消息
            clientSocket.BeginReceive(dataBuffer, 0, 1024, SocketFlags.None, ReceiveCallBack, clientSocket);
            clientList.Add(clientSocket);
            // 再次监听新的接收客户端连接
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);
        }

        // 程序接收到客户端消息的处理方法
        private  void ReceiveCallBack(IAsyncResult ar)
        {
            Socket clientSocket = null;
            try
            {
                clientSocket = ar.AsyncState as Socket;
                int length = clientSocket.EndReceive(ar);
                // 检查是否收到数据
                if (length == 0)
                {
                    clientSocket.Close();
                    return;
                }
                // 从缓存中提取接收到的字节数据
                byte[] dataByte = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    dataByte[i] = dataBuffer[i];
                }
                // 将数据包进行解包并交由分发函数处理
                msgProtol.unpack(dataByte, recvHandler);
                // 再次监听新的接收客户端消息
                clientSocket.BeginReceive(dataBuffer, 0, 1024, SocketFlags.None, ReceiveCallBack, clientSocket);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (clientSocket != null)
                {
                    clientSocket.Close();
                }
            }
        }
        // 向指定的客户端广播消息
        public void Send(Socket client,int cmd, dynamic msg, int recv = 0) {
            var data = msgProtol.pack(cmd, msg, recv);
            client.Send(data);//回复客户端
        }
        // 向所有的客户端广播消息
        public void SendAll(int cmd, dynamic msg, int recv = 0)
        {
            foreach (var client in clientList)
            {
                if (client != null)
                {
                    Send(client,cmd, msg, recv);//向指定客户端发送
                }
            }
        }
        // 关闭所有客户端
        public void CloseAllClient() {
            foreach (var client in clientList)
            {
                if (client != null) {
                    Send(client, (int)MsgCmd.EXIT,"",0);//通知所有客户端关闭
                    client.Close();//关闭本地客户端
                }
            }
            clientList.Clear();//清空所有客户端
        }
        public void Close() {
            try
            {
                if (socket != null)
                {
                    socket.Close();
                    DebugHelper.Log("关闭服务端套接字",Color.green);
                }
            }
            catch (SocketException e)
            {
                Debug.LogError("套接字关闭出错:" + e.Message);
            }
        }
    }
}