 /****************************************************
    文件：ClientSocket.cs
    作者：MRL Liu（Lab）
    邮箱: lpyaonuli@163.com
    日期：2021/3/31 15:10:47
    功能：提供客户端TCP套接字通信功能的接口
*****************************************************/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace MNetSocket
{
    
    public class ClientSocket {
        private Socket socket;//客户端套接字
        private string ip = "127.0.0.1";//ip地址
        private int port = 5006;//端口号
        private byte[] data = new byte[1024];//接收到的消息存储空间
        Queue<byte[]> m_DataQueue = new Queue<byte[]>();//消息字节队列
        private MsgProtol msgProtol = new MsgProtol();//网络协议
        // 函数模板
        public RecvEventHandler recvHandler;

        public  void SetIPAddress(string _ip ,int _port= 5006) {
            ip = _ip;
            port = _port;
        }

        /// <summary>
        /// 初始化套接字并进行连接
        /// </summary>
        public void Connect(RecvEventHandler onrecv)
        {
            recvHandler = onrecv;//接收到消息的处理函数
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);// 创建一个TCP套接字
            socket.Connect(ip, port);//连接指定IP地址
            DebugHelper.Log("客户端套接字初始化IP地址为："+ip+":"+port);
        }

        

        /// <summary>
        /// 同步发送消息
        /// </summary>
        /// <param name="msg"></param>
        private void SendSync(int cmd, dynamic msg,int recv=0)
        {
            try
            {
                byte[] bytes = msgProtol.pack(cmd, msg, recv);//将消息进行打包
                socket.Send(bytes);//尝试发送
                DebugHelper.Log("发送信息：" + msg);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="msg"></param>
        private void SendAsync(int cmd, dynamic msg, int recv = 0)
        {
            try
            {
                byte[] bytes = msgProtol.pack(cmd, msg, recv);//将消息进行打包
                socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, asyncResult =>
                {
                    socket.EndSend(asyncResult);
                    DebugHelper.Log("发送信息：" + msg);
                }, null);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
            }
        }

        /// <summary>
        /// 同步接收消息
        /// </summary>
        /// <param name="msg"></param>
        private void ReceiveSync()
        {
            int length = socket.Receive(data);
            byte[] dataByte = new byte[length];
            for (int i = 0; i < length; i++)
            {
                dataByte[i] = data[i];
            }
            if (m_DataQueue != null)
            {
                Monitor.Enter(m_DataQueue);
                m_DataQueue.Enqueue(data);//将接收到的消息存储起来
                DebugHelper.Log("client rcv data,type " + data[0] + "msg:" + data[1]);// 接收到消息
                Monitor.Exit(m_DataQueue);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="paramer"></param>
        /// <param name="async"></param>
        public void Send(int cmd, dynamic msg, int recv = 0, bool async=true)
        {
            if (async)
            {
                SendAsync(cmd, msg, recv);
            }
            else
            {
                SendSync(cmd, msg, recv);
            }
            if (recv==1)
            {
                ReceiveAsync();
            }
        }

        /// <summary>
        /// 异步接收消息
        /// </summary>
        private void ReceiveAsync()
        {
            try
            {
                socket.BeginReceive(data, 0, data.Length, SocketFlags.None,
                asyncResult =>
                {
                    try
                    {
                        int length = socket.EndReceive(asyncResult);//结束本次接收
                        byte[] dataByte = new byte[length];
                        for (int i = 0; i < length; i++)
                        {
                            dataByte[i] = data[i];
                        }
                        // 添加到消息字节队列
                        if (m_DataQueue != null && dataByte.Length>0)
                        {
                            Monitor.Enter(m_DataQueue);
                            m_DataQueue.Enqueue(dataByte);//将接收到的消息存储起来
                            DebugHelper.Log("添加1个数据包(" + length + "bytes)到消息队列");
                            Monitor.Exit(m_DataQueue);
                        }
                    }
                    catch (SocketException e)
                    {
                        if (e.ErrorCode == 10054)
                        {
                            Close();
                            Debug.Log("server has closed!");
#if UNITY_EDITOR
                            EditorApplication.isPlaying = false;
#endif
                        }
                        else
                        {
                            Debug.LogError(e.Message);
                        }
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
            }
        }
        

        /// <summary>
        /// 更新处理消息队列中的信息
        /// </summary>
        public void updateHandleMsg()
        {
            if (m_DataQueue != null && m_DataQueue.Count > 0 ) {
                Monitor.Enter(m_DataQueue);
                byte[] dataByte = m_DataQueue.Dequeue();
                DebugHelper.Log("从消息队列取出1个数据包(" + dataByte.Length + "bytes)");
                bool flag = msgProtol.unpack(dataByte,recvHandler);//对消息进行解包
                if (flag)
                {
                    ReceiveAsync();//继续接收消息
                }
                Monitor.Exit(m_DataQueue);
            }
        }

        /// <summary>
        /// 关闭套接字
        /// </summary>
        public void Close()
        {
            try
            {
                if (socket != null)
                {
                    socket.Close();
                    DebugHelper.Log("关闭客户端套接字");
                }
            }
            catch (SocketException e)
            {
                Debug.LogError("套接字关闭出错:" + e.Message);
            }
        }
        
        
        


    }
}