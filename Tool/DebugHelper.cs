/****************************************************
    文件：DebugHelper.cs
    作者：MRL Liu（Lab）
    邮箱: lpyaonuli@163.com
    日期：2021/4/7 22:2:18
    功能：基于Unity环境的日志调试工具，提供自定义颜色功能
*****************************************************/
using UnityEngine;

/* Example:
 * DebugHelper.Log("first log");
   DebugHelper.Log("test red",Color.red);
   DebugHelper.Log("test green",Color.green);
   DebugHelper.Log("test blue",Color.blue);
   DebugHelper.Log("%d:custom color and format",new Color(1,0.5f,0.5f),4);
 */

namespace MNetSocket
{
	public class DebugHelper{
        public static void Log(object message)
        {
            string msg = message.ToString();
            Debug.Log(msg);
        }
        public static void Log(object message, Color color)
        {
            string msg = message.ToString();//将对象转换成字符串形式
            if (color != null) {
                string colHtmlString = ColorUtility.ToHtmlStringRGB(color);//将Color转换为颜色编码
                string colorTagStart = "-><color=#{0}>";// 前缀标签形式
                string colorTagEnd = "</color>";// 后缀标签形式
                msg = string.Format(colorTagStart, colHtmlString) + msg + colorTagEnd;//组成带颜色标记的日志输出
            }
            Debug.Log(msg);//输出信息
        }
        public static void Log(object msg, Color color, params object[] args)
        {
            Log(_format(msg, args), color);
        }
        private static string _format(object msg, params object[] args)
        {
            string fmt = msg as string;
            if (args.Length == 0 || string.IsNullOrEmpty(fmt))
            {
                return msg.ToString();
            }
            else
            {
                return string.Format(fmt, args);
            }
        }
    }
}