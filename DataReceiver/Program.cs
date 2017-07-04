using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using XAPI;
using XAPI.Callback;
using QuantBox.Data.Serializer.V2;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Configuration;
using NLog;
using System.Runtime.InteropServices;

namespace DataReceiver
{
    /// <summary>
    /// 需求如下，登录成功后，自动更新合约列表
    /// 自动订阅行情，订阅成功后，自动保存行情
    /// 订阅时需要进行一下区分，比如说
    /// </summary>
    class Program
    {
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);// 删除菜单

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);// 获取系统菜单句柄

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();// 获取控制台窗口句柄


        public const string KEY_OutputFormat = "OutputFormat";
        public const string KEY_DataPath = "DataPath";
        public const string KEY_ConfigPath = "ConfigPath";

        public const string KEY_ConnectionConfigListFileName = "ConnectionConfigListFileName";
        public const string KEY_InstrumentInfoListFileName = "InstrumentInfoListFileName";
        public const string KEY_IncludeFilterListFileName = "IncludeFilterListFileName";
        public const string KEY_ExcludeFilterListFileName = "ExcludeFilterListFileName";

        public const string KEY_SaveAsFilteredInstrumentInfoListFileName = @"SaveAsFilteredInstrumentInfoListFileName";
        public const string KEY_SaveAsSubscribedInstrumentInfoListFileName = @"SaveAsSubscribedInstrumentInfoListFileName";
        public const string KEY_SaveAsTradingDayFileName = @"SaveAsTradingDayFileName";

        public const string KEY_ScheduleTasksListFileName = "ScheduleTasksListFileName";

        public const string KEY_Kdb_Enable = "Kdb_Enable";
        public const string KEY_Kdb_DataPath = "Kdb_DataPath";
        public const string KEY_Kdb_Host = "Kdb_Host";
        public const string KEY_Kdb_Port = "Kdb_Port";
        public const string KEY_Kdb_UsernameAndPassword = "Kdb_UsernameAndPassword";
        public const string KEY_Kdb_Save_Quote = "Kdb_Save_Quote";

        static void Main(string[] args)
        {
            // 禁用控制台的关闭按钮
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);

            DRTickWriter TickWriter = new DRTickWriter(
                ConfigurationManager.AppSettings[KEY_DataPath],
                ConfigurationManager.AppSettings[KEY_OutputFormat]
                );

            Tom.Kdb.KdbWriter KdbWriter = new Tom.Kdb.KdbWriter(
                ConfigurationManager.AppSettings[KEY_Kdb_Host],
                ConfigurationManager.AppSettings[KEY_Kdb_Port],
                ConfigurationManager.AppSettings[KEY_Kdb_UsernameAndPassword],
                ConfigurationManager.AppSettings[KEY_Kdb_DataPath],
                ConfigurationManager.AppSettings[KEY_Kdb_Save_Quote]
                );

            DataReceiver dataReceiver = new DataReceiver();
            dataReceiver.TickWriter = TickWriter;
            if(bool.Parse(ConfigurationManager.AppSettings[KEY_Kdb_Enable]))
            {
                dataReceiver.KdbWriter = KdbWriter;
            }
            


            dataReceiver.ConfigPath = ConfigurationManager.AppSettings[KEY_ConfigPath];
            dataReceiver.DataPath = ConfigurationManager.AppSettings[KEY_DataPath];
            dataReceiver.ConnectionConfigListFileName = ConfigurationManager.AppSettings[KEY_ConnectionConfigListFileName];
            dataReceiver.InstrumentInfoListFileName = ConfigurationManager.AppSettings[KEY_InstrumentInfoListFileName];
            dataReceiver.IncludeFilterListFileName = ConfigurationManager.AppSettings[KEY_IncludeFilterListFileName];
            dataReceiver.ExcludeFilterListFileName = ConfigurationManager.AppSettings[KEY_ExcludeFilterListFileName];

            dataReceiver.SaveAsFilteredInstrumentInfoListFileName = ConfigurationManager.AppSettings[KEY_SaveAsFilteredInstrumentInfoListFileName];
            dataReceiver.SaveAsSubscribedInstrumentInfoListFileName = ConfigurationManager.AppSettings[KEY_SaveAsSubscribedInstrumentInfoListFileName];
            dataReceiver.SaveAsTradingDayFileName = ConfigurationManager.AppSettings[KEY_SaveAsTradingDayFileName];

            dataReceiver.ScheduleTasksListFileName = ConfigurationManager.AppSettings[KEY_ScheduleTasksListFileName];
            
            dataReceiver.LoadConnectionConfig();
            dataReceiver.Connect();

            // 由于会建立多个，所以超时时间可以长一些
            if (dataReceiver.WaitConnectd(30 * 1000))
            {
                // 可以在这里加入一些订阅前的准备工作
                // 这里已经登录成功了，并获得了交易日时间，所以可以用来做别的工作
                dataReceiver.BeforeSubscribe();

                dataReceiver.WatcherStrat(dataReceiver.ConfigPath, "*.json");
                // 监控配置文件
                dataReceiver.ProcessConfig(null);
                dataReceiver.ProcessScheduleTasks(null);
                Console.WriteLine("开始接收，按Ctrl+Q退出");
            }
            else
            {
                Console.WriteLine("登录超时，按Ctrl+Q退出");
            }

            do
            {
                ConsoleKeyInfo cki = Console.ReadKey();
                Console.WriteLine(cki);
                if (cki.Key == ConsoleKey.Q && cki.Modifiers == ConsoleModifiers.Control)
                    break;
            }while(true);


            dataReceiver.WatcherStop();
            dataReceiver.Disconnect();
            Console.WriteLine("按任意键退出");
            Console.ReadKey();

            return;
        }
    }
}