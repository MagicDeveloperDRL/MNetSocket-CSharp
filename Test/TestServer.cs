/****************************************************
    文件：TestServer.cs
    作者：MRL Liu（Lab）
    邮箱: lpyaonuli@163.com
    日期：2021/4/7 18:24:36
    功能：用来测试ServerSocket提供的接口
*****************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MNetSocket
{
	public class TestServer : MonoBehaviour {
        private ServerSocket server = null;
        private bool init = false; //是否初始化Socket
        private int index = 0;
      
        /// <summary>
        /// 绘制按钮
        /// </summary>
        void OnGUI()
        {
            if (GUI.Button(new Rect(30, 20, 120, 60), "Server Start"))//连接服务端
            {
                if (!init)
                {
                    init = true;
                    server = new ServerSocket();
                    server.SetIPAddress("127.0.0.1", 5006);// 设置IP地址和端口号
                    server.Init_Socket();
                    server.conn_client(OnRecv);// 连接客户端
                    //Debug.Log("客户端已经初始化");
                }
            }
            if (GUI.Button(new Rect(30, 130, 120, 60), "Send all Client"))//发送字符串
            {
                if (init)
                {
                    string msg = "Hello Client" + index++;
                    server.SendAll((int)MsgCmd.INFORM, msg);
                    //Debug.Log("send:" + msg);
                }
            }
            if (GUI.Button(new Rect(30, 240, 120, 60), "Close all client"))//发送字符串  
            {
                if (init)
                {
                    server.CloseAllClient();//关闭所有客户端
                }
            }
            if (GUI.Button(new Rect(30, 350, 120, 60), "Server Exit"))
            {
                OnApplicationQuit();
            }
        }
        /// <summary>
        /// 消息处理函数
        /// </summary>
        /// <param name="cmd">命令类型</param>
        /// <param name="body">消息正文</param>
        /// <param name="recv">是否回复</param>
        public void OnRecv(int cmd, dynamic body, int recv)
        {
            if ((MsgCmd)cmd == MsgCmd.REQUEST)
            {
                DebugHelper.Log("消息正文:" + body,Color.green);
                if (recv==1) {
                    string msg = "this is response from server";
                    server.SendAll((int)MsgCmd.INFORM, msg);//回复客户端
                }
            }
            else if ((MsgCmd)cmd == MsgCmd.INFORM)
            {

                DebugHelper.Log("消息正文:" + body, Color.green);//显示客户端消息
            }
            else if ((MsgCmd)cmd == MsgCmd.PARAM)
            {
                DebugHelper.Log("成功收到环境参数", Color.green);//提示收到参数
            }
            else if ((MsgCmd)cmd == MsgCmd.EXIT)
            {
                server.CloseAllClient();//关闭所有客户端
                DebugHelper.Log("已经关闭所有客户端" + body, Color.green);
            }
            else
            {
                DebugHelper.Log("未知的消息类型:" + body, Color.green);
            }
        }
        /// <summary>
        /// 服务端退出时
        /// </summary>
        private void OnApplicationQuit()
        {
            if (init && server != null)
            {
                server.Close();
            }
        }
    }
}