using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using QuantBox.XAPI;
using QuantBox.XAPI.Callback;
using QuantBox;
using QuantBox.Data.Serializer.V2;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Configuration;

namespace DataReceiver
{
    /// <summary>
    /// 需求如下，登录成功后，自动更新合约列表
    /// 自动订阅行情，订阅成功后，自动保存行情
    /// 订阅时需要进行一下区分，比如说
    /// </summary>
    static class Program
    {
        public const string KEY_DataPath = "DataPath";
        public const string KEY_ConfigPath = "ConfigPath";

        public const string KEY_TradeConnectionConfigFileName = "TradeConnectionConfigFileName";
        public const string KEY_TradeInstrumentInfoListFileName = "TradeInstrumentInfoListFileName";

        public const string KEY_MarketDataConnectionConfigListFileName = "MarketDataConnectionConfigListFileName";
        public const string KEY_MarketDataInstrumentInfoListFileName = "MarketDataInstrumentInfoListFileName";
        public const string KEY_MarketDataIncludeFilterListFileName = "MarketDataIncludeFilterListFileName";
        public const string KEY_MarketDataExcludeFilterListFileName = "MarketDataExcludeFilterListFileName";

        static void Main(string[] args)
        {
            GetInstruments GetInstruments = new GetInstruments();
            GetInstruments.ConfigPath = ConfigurationManager.AppSettings[KEY_ConfigPath];
            GetInstruments.ConnectionConfigFileName = ConfigurationManager.AppSettings[KEY_TradeConnectionConfigFileName];
            GetInstruments.InstrumentInfoListFileName = ConfigurationManager.AppSettings[KEY_TradeInstrumentInfoListFileName];
            GetInstruments.Load();
            GetInstruments.Connect();
            GetInstruments.WaitConnectd();
            GetInstruments.ReqQryInstrument();
            GetInstruments.WaitIsLast();
            Console.WriteLine("一共查询到{0}条合约", GetInstruments.InstrumentInfoList.Count);
            GetInstruments.Save();
            Console.WriteLine("写入合约列表到{0}", GetInstruments.InstrumentInfoListFileName);
            GetInstruments.Disconnect();


            // 如何得到交易日？需要登录
            DataReceiver dataReceiver = new DataReceiver();
            dataReceiver.TickWriter = new DRTickWriter(ConfigurationManager.AppSettings[KEY_DataPath]);

            dataReceiver.ConfigPath = ConfigurationManager.AppSettings[KEY_ConfigPath];
            dataReceiver.ConnectionConfigListFileName = ConfigurationManager.AppSettings[KEY_MarketDataConnectionConfigListFileName];
            dataReceiver.InstrumentInfoListFileName = ConfigurationManager.AppSettings[KEY_MarketDataInstrumentInfoListFileName];
            dataReceiver.IncludeFilterListFileName = ConfigurationManager.AppSettings[KEY_MarketDataIncludeFilterListFileName];
            dataReceiver.ExcludeFilterListFileName = ConfigurationManager.AppSettings[KEY_MarketDataExcludeFilterListFileName];
            
            dataReceiver.Load();
            Console.WriteLine("一共读取到{0}条合约", dataReceiver.InstrumentInfoList.Count);
            dataReceiver.Connect();
            dataReceiver.WaitConnectd();

            dataReceiver.Subscribe();

            do
            {
                ConsoleKeyInfo cki = Console.ReadKey();
                if (cki.Key == ConsoleKey.Q)
                    break;
            }while(true);
            

            dataReceiver.Disconnect();

            return;
        }
    }
}
/*
 List<InstrumentFilterConfig> list = new List<InstrumentFilterConfig>();
            list.Add(new InstrumentFilterConfig() { SymbolRegex = @"IF\d{4}$", Time_ssf_Diff = 5});
            list.Add(new InstrumentFilterConfig() { SymbolRegex = @"IO\d{4}-", Time_ssf_Diff = 5});
            list.Add(new InstrumentFilterConfig() { SymbolRegex = @"SR\d{3}$", Time_ssf_Diff = 10});
            list.Add(new InstrumentFilterConfig() { SymbolRegex = @"WR\d{3}[CP]", Time_ssf_Diff = 10});
            list.Add(new InstrumentFilterConfig() { SymbolRegex = @"\d{8}.S", Time_ssf_Diff = 10 });

            List<string> c = new List<string>();
            c.Add("IF1503");
            c.Add("IF1504");
            c.Add("IO1503-C-2500");
            c.Add("SR509");
            c.Add("SR509C2500");
            c.Add("10000000.SSE");
            Regex regex = new Regex(@"\d{8}.SSE");
            foreach(var c1 in c)
            {
                Console.WriteLine(regex.Match(c1).Success);
            }

            Save(@"D:\", "abc.json", list);

            List<ConnectionConfig> list2 = new List<ConnectionConfig>();
            ConnectionConfig cc1 = new ConnectionConfig() { LibPath = @"C:\Program Files\SmartQuant Ltd\OpenQuant 2014\XAPI\CTP\x86\QuantBox_CTP_Quote.dll",SessionLimit = 2, SubscribePerSession = 200 };
            cc1.Server.BrokerID = "1017";
            cc1.Server.Address = "tcp://ctpmn1-front1.citicsf.com:51213";
            cc1.User.UserID = "00000015";
            cc1.User.Password = "123456";
            list2.Add(cc1);

            Save(@"D:\", "abcd.json", list2);

            List<XApi> b = new List<XApi>();

            // 查看有多少种连接
            foreach(var a in list2)
            {
                // 建立多个连接
                for (int i = 0; i < a.SessionLimit;++i)
                {
                    XApi api = new XApi(a.LibPath);
                    api.Server = a.Server;
                    api.User = a.User;

                    api.OnConnectionStatus = OnConnectionStatus;
                    api.OnRtnDepthMarketData = OnRtnDepthMarketData;

                    api.Connect();

                    b.Add(api);
                }
            }
 */