/****************************************************
    文件：TestClient.cs
    作者：MRL Liu（Lab）
    邮箱: lpyaonuli@163.com
    日期：2021/3/31 15:55:58
    功能：用来测试ClientSocket提供的接口
*****************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace MNetSocket
{
    
    public class TestClient : MonoBehaviour {
        private ClientSocket client = null;
        private bool init = false; //是否初始化Socket
        private int index = 0;

        
        /// <summary>
        /// 绘制按钮
        /// </summary>
        void OnGUI()
        {
            if (GUI.Button(new Rect(Screen.width - 150, 20, 120, 60), "Client connect"))//连接服务端
            {
                if (!init) {
                    init = true;
                    client = new ClientSocket();
                    client.SetIPAddress("127.0.0.1", 5006);// 设置IP地址和端口号
                    client.Connect(OnRecv);
                    SendParameters();// 发送环境初始化参数
                 }
            }
            if (GUI.Button(new Rect(Screen.width - 150, 130, 120, 60), "Send Hello Server"))//发送字符串
            {
                if (init)
                {
                    string msg = "Hello Server " + index++;
                    client.Send((int)MsgCmd.INFORM,msg);
                }
            }
            if (GUI.Button(new Rect(Screen.width - 150, 240, 120, 60), "Send Request"))//发送字符串  
            {
                if (init)
                {
                    string msg = "this is request";
                    client.Send((int)MsgCmd.REQUEST, msg,1);
                }
            }
            if (GUI.Button(new Rect(Screen.width - 150, 350, 120, 60), "Client exit"))
            {
                string msg = "EXIT";
                client.Send((int)MsgCmd.EXIT, msg);
                OnApplicationQuit();
            }
        }

        public void Update()
        {
            if (client != null) {
                client.updateHandleMsg();
            }
        }

        // 发送初始化对象
        private void SendParameters()
        {
            // 创建一个Parameters对象
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["logPath"] = "param1";
            dict["brainNames"] = "param2".ToString();
            client.Send((int)MsgCmd.PARAM, dict);
        }
        
        public void OnRecv(int cmd, dynamic body, int recv)
        {
            //int res = 0;
            if ((MsgCmd)cmd == MsgCmd.REQUEST)
            {
                Debug.Log("data msg:" + body);
            }
            else if ((MsgCmd)cmd == MsgCmd.INFORM)
            {
                Debug.Log("data msg:" + body);
            }
            else if ((MsgCmd)cmd == MsgCmd.PARAM)
            {
                Debug.Log("data msg:" + body);
            }
            else if ((MsgCmd)cmd == MsgCmd.EXIT)
            {
                Debug.Log("data msg:" + body);
                client.Close();
            }
            else 
            {
                Debug.Log("unknown data type:" + body);
            }
        }
        private void OnApplicationQuit()
        {
            if (init && client != null)
            {
                client.Close();
            }
        }

    }
}