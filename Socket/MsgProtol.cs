/****************************************************
    文件：MsgProtol.cs
    作者：MRL Liu（Lab）
    邮箱: lpyaonuli@163.com
    日期：2021/4/1 20:54:34
    功能：提供服务端和客户端的通信协议
          特别提醒：该API暂不支持处理包含中文的消息包的处理
*****************************************************/
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MNetSocket
{
    public enum MsgCmd { INFORM = 1, REQUEST, PARAM, EXIT };//消息类型
    public delegate void RecvEventHandler(int cmd, dynamic body, int recv);//消息处理方法
    public class MsgProtol {
        
        // 函数模板
       
        private ArrayList dataBuffer = new ArrayList();
        private int intSize = 4;
        private byte[] intBytes = new byte[4];


        public byte[] pack(int cmd, dynamic _body, int _recv= 1)
        {
            string Jsonbody = JsonConvert.SerializeObject(_body,Formatting.Indented);//将消息正文的字符串转换为Json格式
            //Debug.Log("打包了一个数据包，jsonStr:" + Jsonbody);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(Jsonbody);//将消息正文的字符串转换为字节编码
            byte[] bodySizeBytes = System.BitConverter.GetBytes(Jsonbody.Length);//将消息正文字符长度转化为字节编码
            byte[] cmdBytes = System.BitConverter.GetBytes(cmd);//将cnd转化为字节编码
            byte[] recvBytes = System.BitConverter.GetBytes(_recv);//将cnd转化为字节编码
            byte[] newBytes = bodySizeBytes.Concat(cmdBytes).Concat(recvBytes).Concat(bodyBytes).ToArray();//将字节长度和字节编码结合到一起
            //Debug.Log("打包了一个数据包，_bodySize:" + Jsonbody.Length);
            return newBytes;
        }

        public bool unpack(byte[] data, RecvEventHandler recvHandler) {
            if (data.Length > 0){
                foreach(byte bt in data){
                    dataBuffer.Add(bt);
                }
                while (true) {
                    if (dataBuffer.Count < intSize* 3)
                    {
                        //DebugHelper.Log("数据包不够消息头大小");
                        break;
                    }
                    //解码消息正文大小
                    for (int i=0;i<intSize;i++) {
                        intBytes[i] = (byte)dataBuffer.GetRange(0, intSize)[i];
                    }
                    int bodySize = System.BitConverter.ToInt32(intBytes, 0);
                    //解码消息cmd大小
                    for (int i = 0; i < intSize; i++)
                    {
                        intBytes[i] = (byte)dataBuffer.GetRange(4, intSize)[i];
                    }
                    int cmd = System.BitConverter.ToInt32(intBytes, 0);
                    //解码消息recv大小
                    for (int i = 0; i < intSize; i++)
                    {
                        intBytes[i] = (byte)dataBuffer.GetRange(8, intSize)[i];
                    }
                    int recv = System.BitConverter.ToInt32(intBytes, 0);
                    //判断是否超过一个消息长度，处理分包情况
                    if (dataBuffer.Count < intSize * 3+ bodySize)
                    {
                        //DebugHelper.Log("数据包不够消息正文大小");
                        break;
                    }
                    // 解码消息正文
                    byte[] bodyBytes = new byte[bodySize];
                    for (int i = 0; i < bodySize; i++)
                    {
                        bodyBytes[i] = (byte)dataBuffer.GetRange(12, bodySize)[i];
                    }
                    string _Jsonbody = Encoding.UTF8.GetString(bodyBytes);
                    //DebugHelper.Log("解码得到消息正文："+ _Jsonbody);
                    var _body = JsonConvert.DeserializeObject<dynamic>(_Jsonbody);
                    //DebugHelper.Log("JsonConvert转换得到消息正文：" + _body);
                    // 处理消息正文
                    if (recvHandler != null)
                    {
                        DebugHelper.Log("接收到消息包->cmd:" + (MsgCmd)cmd + " bodySize:" + bodySize.ToString() + " recv:" + recv.ToString(), Color.green);
                        recvHandler(cmd, _body, recv);// 交给处理消息的接口
                    }
                    else {
                        DebugHelper.Log("无法分发消息包，可能消息处理函数为空");
                    }
                    // 移除已经处理的消息，处理粘包情况
                    dataBuffer.RemoveRange(0, bodySize + intSize * 3);
                }
                    

                return true;
            }
            else {
                return false;
            }
        }
        

    }
}