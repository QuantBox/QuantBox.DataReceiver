using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetInstruments
{
    static class Program
    {
        public const string KEY_ConfigPath = "ConfigPath";

        public const string KEY_ConnectionConfigFileName = "ConnectionConfigFileName";
        public const string KEY_InstrumentInfoListFileName = "InstrumentInfoListFileName";

        static void Main(string[] args)
        {
            GetInstruments GetInstruments = new GetInstruments();
            GetInstruments.ConfigPath = ConfigurationManager.AppSettings[KEY_ConfigPath];
            GetInstruments.ConnectionConfigFileName = ConfigurationManager.AppSettings[KEY_ConnectionConfigFileName];
            GetInstruments.InstrumentInfoListFileName = ConfigurationManager.AppSettings[KEY_InstrumentInfoListFileName];
            GetInstruments.Load();
            GetInstruments.Connect();
            GetInstruments.WaitConnectd();
            GetInstruments.ReqQryInstrument();
            GetInstruments.WaitIsLast();
            Console.WriteLine("一共查询到 {0} 条合约", GetInstruments.InstrumentInfoList.Count);
            GetInstruments.Save();
            Console.WriteLine("写入合约列表到 {0}", GetInstruments.InstrumentInfoListFileName);
            GetInstruments.Disconnect();
        }
    }
}
