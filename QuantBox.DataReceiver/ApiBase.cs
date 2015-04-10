using Newtonsoft.Json;
using QuantBox.XAPI.Callback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataReceiver
{
    public class ApiBase
    {
        public string ConfigPath;
        public string InstrumentInfoListFileName = @"InstrumentInfoList.json";
        public List<InstrumentInfo> InstrumentInfoList = new List<InstrumentInfo>();

        protected bool bIsLast;

        public List<XApi> XApiList = new List<XApi>();

        private static JsonSerializerSettings jSetting = new JsonSerializerSettings()
        {
            // json文件格式使用非紧凑模式
            //NullValueHandling = NullValueHandling.Ignore,
            //DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        protected void Save(string path, string file, object obj)
        {
            using (FileStream fs = File.Open(Path.Combine(path, file), FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (TextWriter writer = new StreamWriter(fs))
                {
                    writer.Write("{0}", JsonConvert.SerializeObject(obj, obj.GetType(), jSetting));
                    writer.Close();
                }
            }
        }
        protected object Load(string path, string file, object obj)
        {
            try
            {
                object ret;
                using (FileStream fs = File.Open(Path.Combine(path, file), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (TextReader reader = new StreamReader(fs))
                    {
                        ret = JsonConvert.DeserializeObject(reader.ReadToEnd(), obj.GetType());
                        reader.Close();
                    }
                }
                
                return ret;
            }
            catch
            {
            }
            return obj;
        }

        public void Disconnect()
        {
            foreach (var api in XApiList)
            {
                api.Disconnect();
            }
        }

        /// <summary>
        /// 超时退出返回false
        /// 正常退出返回true
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool WaitConnectd(int timeout)
        {
            DateTime dt = DateTime.Now;
            
            do
            {
                bool IsConnected = true;
                foreach (var api in XApiList)
                {
                    if (!api.IsConnected)
                    {
                        IsConnected = false;
                        break;
                    }
                }

                if (IsConnected)
                    return true;

                // 超时退出
                if ((DateTime.Now - dt).TotalMilliseconds >= timeout)
                {
                    return false;
                }
                Thread.Sleep(1000);
            } while (true);
        }

        /// <summary>
        /// 超时退出返回false
        /// 正常退出返回true
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool WaitIsLast(int timeout)
        {
            DateTime dt = DateTime.Now;
            while (!bIsLast)
            {
                if ((DateTime.Now - dt).TotalMilliseconds >= timeout)
                {
                    return false;
                }
                Thread.Sleep(1000);
            }
            return true;
        }
    }
}
