using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnUpdater_V1
{
    public class Log
    {
        public static void Dbg(object? obj)
        {
            System.Diagnostics.Debug.WriteLine(obj);
        }
        public static void ShowSvn(string[] so,string[] se)
        {
            string sdsd = System.IO.Directory.GetCurrentDirectory();
            sdsd += "\nOut:\n";
            for (int i = 0; i < so.Length; i++)
            {
                sdsd += so[i];
            }
            sdsd += "\nErr:\n";
            for (int i = 0; i < se.Length; i++)
            {
                sdsd += se[i];
            }
            Show(sdsd);
        }

        public static void Show(string log)
        {
            Manager.Instance.label1.Text = log;
        }

        /// <summary>
        /// 将序列化的json字符串内容写入Json文件，并且保存
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="jsonConents">Json内容</param>
        public static void WriteJsonFile(string path, string jsonConents)
        {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);

                StreamWriter sw = new StreamWriter(fs);
                sw.Write(jsonConents);
                sw.Flush();
                sw.Close();
        }

        /// <summary>
        /// 获取到本地的Json文件并且解析返回对应的json字符串
        /// </summary>
        /// <param name="filepath">文件路径</param>
        /// <returns>Json内容</returns>
        public static string GetJsonFile(string filepath)
        {
            string json = string.Empty;
            using (FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    json = sr.ReadToEnd().ToString();
                    sr.Close();
                }
            }
            return json;
        }
    }
}
