using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using XAPI;
using kx;
using NLog;

namespace Tom.Kdb
{
    public class KdbWriter
    {
        public c c = null;

        private string Host;
        private int Port;
        private string UsernameAndPassword;
        private string Path;
        private bool SaveQuote;

        private Logger Log = LogManager.GetCurrentClassLogger();

        public KdbWriter(string host, string port, string usernameAndPassword, string path, string saveQuote)
        {
            Host = host;
            Port = int.Parse(port);
            UsernameAndPassword = usernameAndPassword;
            Path = path;
            SaveQuote = bool.Parse(saveQuote);
        }

        private string TradingDay(DateTime datetime)
        {
            DateTime dt;
            dt = datetime.AddHours(8).Date;

            if (datetime.DayOfWeek == DayOfWeek.Friday && datetime.Hour > 16)
                dt = datetime.AddDays(3).Date;
            else if (datetime.DayOfWeek == DayOfWeek.Saturday && datetime.Hour < 4)
                dt = datetime.AddDays(2).Date;
            else
                dt = datetime.AddHours(8).Date;
            return dt.ToString("yyyyMMdd");
        }
        public void Connect()
        {
            if (null != c)
                return;

            try
            {
                if (string.IsNullOrEmpty(UsernameAndPassword))
                    c = new c(Host, Port);
                else
                    c = new c(Host, Port, UsernameAndPassword);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);

                try
                {
                    Log.Info("尝试运行 kdb+ 服务.");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = "c:\\q\\w32\\q.exe",
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized,
                        CreateNoWindow = false,
                        Arguments = " -p " + Port.ToString()
                    });
                    if (string.IsNullOrEmpty(UsernameAndPassword))
                        c = new c(Host, Port);
                    else
                        c = new c(Host, Port, UsernameAndPassword);
                }
                catch (Exception arg)
                {
                    Log.Error("运行 q.exe 失败.");
                    return;
                }
            }

            Log.Info("连接 kdb+ 成功：{0}:{1}", Host, Port);

            // 表已经存在，不操作
            if (CheckTable())
            {
                return;
            }

            // 表不存在，先加载
            if (Load(DateTime.Now))
            {
                // 需要一点时间
                System.Threading.Thread.Sleep(500);

                if (CheckTable())
                {
                    // 表已经存在，不操作
                    return;
                }
            }
            else
            {
                // 加载也是失败的，只能创建了
                Init();
            }
        }
        public bool CheckTable()
        {
            try
            {
                short result1 = (short)c.k(@"type trade");
                if (result1 != 98)
                    return false;
                Log.Info("已经存在trade表.");
                if (SaveQuote)
                {
                    short result2 = (short)c.k(@"type quote");
                    if (result2 != 98)
                        return false;
                    Log.Info("已经存在quote表.");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Init()
        {
            lock (this)
            {
                try
                {
                    c.ks(@"trade:([]datetime:`datetime$();sym:`symbol$();price:`float$();volume:`long$();openint:`long$())");
                    Log.Info("创建trade表完成.");
                    if (SaveQuote)
                    {
                        c.ks(@"quote:([]datetime:`datetime$();sym:`symbol$();bid:`float$();ask:`float$();bsize:`int$();asize:`int$())");
                        Log.Info("创建quote表完成.");
                    }
                }
                catch (Exception e)
                {
                    Log.Error("创建trade/quote表失败.");
                }
            }
        }
        public bool Load(DateTime dt)
        {
            try
            {
                //object result = c.k(string.Format(@"\l {0}", Path));
                object result1 = c.k(string.Format(@"trade: get `:{0}/{1}/{2}", Path, "trades", TradingDay(dt)));
                Log.Info("从磁盘加载 trade 表 {0} 成功.", TradingDay(dt));
            }
            catch
            {
                Log.Error("从磁盘加载trade失败.");
                return false;
            }
            if (SaveQuote)
            {
                try
                {
                    object result2 = c.k(string.Format(@"quote: get `:{0}/{1}/{2}", Path, "quotes", TradingDay(dt)));
                    Log.Info("从磁盘加载 quote 表 {0} 成功.", TradingDay(dt));
                }
                catch
                {
                    Log.Error("从磁盘加载quote失败.");
                    return false;
                }
            }
            return true;
        }
        public void Save(DateTime dt)
        {
            if (dt.Hour > 16 & dt.Hour < 21) // 夜市开盘前 重连，可以保证 这个初始化
            {
                // 检查 存档文件 的 最后存盘时间 如果 15:30 没有保存过，保存一次
                string path = Path + @"/trades/" + TradingDay(dt.AddHours(-8));
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.LastWriteTime < dt.Date.AddHours(15).AddMinutes(30))
                {
                    Save(dt.AddHours(-8));
                }

                Init();
            }

            lock (this)
            {
                try
                {
                    //c.k(string.Format(@"`:{0}{1}/{1}/ set .Q.en[`:{0}{1}/] {2}", Path, "trade", "trade"));
                    //c.k(string.Format(@"`:{0}{1}/{1}/ set .Q.en[`:{0}{1}/] {2}", Path, "quote", "quote"));
                    c.k(string.Format(@"`:{0}/{1}/{2} set {3}", Path, "trades", TradingDay(dt), "trade"));
                    Log.Info("保存 trade 表 {0} 完成.", TradingDay(dt));
                    if (SaveQuote)
                    {
                        c.k(string.Format(@"`:{0}/{1}/{2} set {3}", Path, "quotes", TradingDay(dt), "quote"));
                        Log.Info("保存 quote 表 {0} 完成.", TradingDay(dt));
                    }
                }
                catch (Exception e)
                {
                    Log.Error("保存trade/quote表失败.");
                }
            }
        }
        public void Disconnect()
        {
            // 一定要保证退出时要运行
            if (null == c)
                return;

            lock (this)
            {
                //Save(DateTime.Now);
                c.Close();
                c = null;
            }
        }

        public void Write(ref DepthMarketDataNClass pDepthMarketData)
        {
            // 如果盘中不小心把服务端关了也不再开启
            if (null == c)
                return;

            string dateTime = pDepthMarketData.ExchangeDateTime().ToString("yyyy.MM.ddTHH:mm:ss.fff");
            string symbol = pDepthMarketData.InstrumentID;
            double price = pDepthMarketData.LastPrice;
            long volume = (long)(pDepthMarketData.Volume);
            long openint = (long)(pDepthMarketData.OpenInterest);

            double bid = 0;
            double ask = 0;
            int bsize = 0;
            int asize = 0;

            string trade_str = string.Format("`trade insert({0};`$\"{1}\";{2}f;{3}j;{4}j)", dateTime, symbol, price, volume, openint);
            string quote_str = null;

            if (SaveQuote)
            {
                if (pDepthMarketData.Bids.Length > 0)
                {
                    bid = pDepthMarketData.Bids[0].Price;
                    bsize = pDepthMarketData.Bids[0].Size;
                }
                if (pDepthMarketData.Asks.Length > 0)
                {
                    ask = pDepthMarketData.Asks[0].Price;
                    asize = pDepthMarketData.Asks[0].Size;
                }
                quote_str = string.Format("`quote insert({0};`$\"{1}\";{2}f;{3}f;{4};{5})", dateTime, symbol, bid, ask, bsize, asize);
            }

            // 有一个问题
            lock (this)
            {
                try
                {
                    // trade
                    c.ks(trade_str);

                    // quote
                    if (SaveQuote)
                    {
                        c.ks(quote_str);
                    }
                }
                catch (Exception e)
                {
                    // 保存失败，通常时因为
                    Log.Error(e.Message);
                    c.Close();
                    c = null;
                }
            }
        }
    }
}
