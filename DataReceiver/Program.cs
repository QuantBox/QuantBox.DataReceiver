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
using NLog;

namespace DataReceiver
{
    /// <summary>
    /// 需求如下，登录成功后，自动更新合约列表
    /// 自动订阅行情，订阅成功后，自动保存行情
    /// 订阅时需要进行一下区分，比如说
    /// </summary>
    class Program
    {
        public const string KEY_DataPath = "DataPath";
        public const string KEY_ConfigPath = "ConfigPath";

        public const string KEY_ConnectionConfigListFileName = "ConnectionConfigListFileName";
        public const string KEY_InstrumentInfoListFileName = "InstrumentInfoListFileName";
        public const string KEY_IncludeFilterListFileName = "IncludeFilterListFileName";
        public const string KEY_ExcludeFilterListFileName = "ExcludeFilterListFileName";

        public const string KEY_SaveAsInstrumentInfoListName = @"SaveAsInstrumentInfoListName";
        public const string KEY_SaveAsTradingDayName = @"SaveAsTradingDayName";

        static void Main(string[] args)
        {
            DRTickWriter TickWriter = new DRTickWriter(ConfigurationManager.AppSettings[KEY_DataPath]);

            DataReceiver dataReceiver = new DataReceiver();
            dataReceiver.TickWriter = TickWriter;

            dataReceiver.ConfigPath = ConfigurationManager.AppSettings[KEY_ConfigPath];
            dataReceiver.ConnectionConfigListFileName = ConfigurationManager.AppSettings[KEY_ConnectionConfigListFileName];
            dataReceiver.InstrumentInfoListFileName = ConfigurationManager.AppSettings[KEY_InstrumentInfoListFileName];
            dataReceiver.IncludeFilterListFileName = ConfigurationManager.AppSettings[KEY_IncludeFilterListFileName];
            dataReceiver.ExcludeFilterListFileName = ConfigurationManager.AppSettings[KEY_ExcludeFilterListFileName];
            dataReceiver.SaveAsInstrumentInfoListName = Path.Combine(
                ConfigurationManager.AppSettings[KEY_DataPath],
                ConfigurationManager.AppSettings[KEY_SaveAsInstrumentInfoListName]);
            dataReceiver.SaveAsTradingDayName = Path.Combine(
                ConfigurationManager.AppSettings[KEY_DataPath],
                ConfigurationManager.AppSettings[KEY_SaveAsTradingDayName]);

            dataReceiver.LoadConnectionConfig();
            dataReceiver.Connect();
            // 由于会建立多个，所以超时时间可以长一些
            if (dataReceiver.WaitConnectd(20 * 1000))
            {
                dataReceiver.WatcherStrat(dataReceiver.ConfigPath, "*.json");
                // 复制老列表
                dataReceiver.ProcessConfig(null);
            }

            Console.WriteLine("开始接收，按Ctrl+Q退出");

            do
            {
                ConsoleKeyInfo cki = Console.ReadKey();
                Console.WriteLine(cki);
                if (cki.Key == ConsoleKey.Q && cki.Modifiers == ConsoleModifiers.Control)
                    break;
            }while(true);


            dataReceiver.WatcherStop();
            dataReceiver.Disconnect();

            return;
        }
    }
}