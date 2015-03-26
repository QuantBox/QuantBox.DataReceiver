using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetInstruments
{
    /// <summary>
    /// 由计划任务启动
    /// 每天晚上9点前启动，每天早上9点前也启动
    /// 如果登录失败等情况，就不更新合约列表文件
    /// 运行完就退出
    /// </summary>
    class Program
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
            if(!GetInstruments.WaitConnectd(10*1000))
            {
                Console.WriteLine("连接超时退出");
                GetInstruments.Disconnect();
                return;
            }
            GetInstruments.ReqQryInstrument();
            // 这个超时是否过短？因为在LTS模拟中可能要5分钟
            if(GetInstruments.WaitIsLast(60*1000))
            {
                Console.WriteLine("一共查询到 {0} 条合约", GetInstruments.InstrumentInfoList.Count);
                if(GetInstruments.InstrumentInfoList.Count>0)
                {
                    GetInstruments.Save();
                    Console.WriteLine("写入合约列表到 {0}", GetInstruments.InstrumentInfoListFileName);
                }
            }
            else
            {
                Console.WriteLine("查询合约超时");
            }
            
            GetInstruments.Disconnect();
        }
    }
}
