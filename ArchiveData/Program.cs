using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArchiveData
{
    class Program
    {
        public const string KEY_DataPath = "DataPath";
        public const string KEY_OutputPath_TradingDay = "OutputPath_TradingDay";
        public const string KEY_OutputPath_Instrument = "OutputPath_Instrument";
        public const string KEY_DefaultExchange = "DefaultExchange";
        public const string KEY_SevenZipExePath = "SevenZipExePath";

        public const string KEY_Clear_DataPath = "Clear_DataPath";
        public const string KEY_Clear_OutputPath_TradingDay = "Clear_OutputPath_TradingDay";

        static void Main(string[] args)
        {
            // 遍历，某个目录
            Logger Log = LogManager.GetCurrentClassLogger();

            string DataPath = ConfigurationManager.AppSettings[KEY_DataPath];
            string OutputPath_TradingDay = ConfigurationManager.AppSettings[KEY_OutputPath_TradingDay];
            string OutputPath_Instrument = ConfigurationManager.AppSettings[KEY_OutputPath_Instrument];
            string DefaultExchange = ConfigurationManager.AppSettings[KEY_DefaultExchange];
            string SevenZipExePath = ConfigurationManager.AppSettings[KEY_SevenZipExePath];
            bool Clear_DataPath = bool.Parse(ConfigurationManager.AppSettings[KEY_Clear_DataPath]);
            bool Clear_OutputPath_TradingDay = bool.Parse(ConfigurationManager.AppSettings[KEY_Clear_OutputPath_TradingDay]);

            HashSet<string> Set_TradingDay = new HashSet<string>();

            var files = new DirectoryInfo(DataPath).GetFiles("*", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                string exchange = string.Empty;
                string product = string.Empty;
                string instrument = string.Empty;
                string symbol = string.Empty;
                string date = string.Empty;
                if (PathHelper.SplitFileName(f.Name, out exchange, out product, out instrument, out date))
                {
                    if (string.IsNullOrEmpty(exchange))
                    {
                        exchange = DefaultExchange;
                    }

                    DirectoryInfo DI_TradingDay = new DirectoryInfo(Path.Combine(OutputPath_TradingDay, date));
                    if (!DI_TradingDay.Exists)
                        DI_TradingDay.Create();
                    string Path_TradingDay = Path.Combine(DI_TradingDay.FullName, f.Name);

                    // 将当前目录下内容复制到指定日期目录
                    File.Copy(f.FullName, Path_TradingDay, true);

                    
                    {
                        DirectoryInfo DI_Instrument = new DirectoryInfo(Path.Combine(OutputPath_Instrument, exchange, product, instrument));
                        if (!DI_Instrument.Exists)
                            DI_Instrument.Create();
                        string Path_Instrument = Path.Combine(DI_Instrument.FullName, f.Name);
                        // 将当前目录下内容压缩到指定合约目录下

                        // 文件已经打开的情况下，无法进行压缩，这个地方处理一下
                        PathHelper.SevenZipFile(SevenZipExePath, Path_Instrument + ".7z", Path_TradingDay);
                    }

                    // 只有备份了一份，才会去删除
                    if (Clear_DataPath)
                    {
                        File.Delete(f.FullName);
                    }

                    // 记录下处理了哪些交易日
                    Set_TradingDay.Add(DI_TradingDay.FullName);

                    Log.Info("处理完:{0}", f.FullName);
                }
                else
                {
                    //无法识别，需人工处理
                    Log.Info("无法识别:{0}", f.FullName);
                }
            }

            // 对交易日进行压缩
            foreach (var d in Set_TradingDay)
            {
                Log.Info("压缩交易日目录:{0}", d);
                PathHelper.SevenZipDirectory(SevenZipExePath, d + ".7z", d);
            }

            // 删除
            if (Clear_OutputPath_TradingDay)
            {
                foreach (var d in Set_TradingDay)
                {
                    Log.Info("删除交易日目录:{0}", d);
                    var di = new DirectoryInfo(d);
                    if(di.Exists)
                        di.Delete(true);
                }
            }
            
        }
    }
}
