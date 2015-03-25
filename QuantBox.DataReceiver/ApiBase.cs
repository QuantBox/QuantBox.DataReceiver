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
        public List<InstrumentInfo> InstrumentInfoList;

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
            using (TextWriter writer = new StreamWriter(Path.Combine(path, file)))
            {
                writer.Write("{0}", JsonConvert.SerializeObject(obj, obj.GetType(), jSetting));
                writer.Close();
            }
        }
        protected object Load(string path, string file, object obj)
        {
            try
            {
                object ret;
                using (TextReader reader = new StreamReader(Path.Combine(path, file)))
                {
                    ret = JsonConvert.DeserializeObject(reader.ReadToEnd(), obj.GetType());
                    reader.Close();
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

        public void WaitConnectd()
        {
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
                if (!IsConnected)
                    Thread.Sleep(1000);
            } while (false);
        }

        public void WaitIsLast()
        {
            while (!bIsLast)
                Thread.Sleep(1000);
        }
    }
}
