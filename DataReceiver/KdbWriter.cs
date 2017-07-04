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
        private int TradingDay;

        private Logger Log = LogManager.GetCurrentClassLogger();

        public KdbWriter(string host, string port, string usernameAndPassword, string path, string saveQuote)
        {
            Host = host;
            Port = int.Parse(port);
            UsernameAndPassword = usernameAndPassword;
            Path = path;
            SaveQuote = bool.Parse(saveQuote);
            TradingDay = 0;
            // TradingDay = GetTradingDay(DateTime.Now);
        }

        //private int GetTradingDay(DateTime datetime)
        //{
        //    DateTime dt;
        //    if (datetime.DayOfWeek == DayOfWeek.Friday && datetime.Hour > 16)
        //        dt = datetime.AddDays(3).Date;
        //    else if (datetime.DayOfWeek == DayOfWeek.Saturday)
        //        dt = datetime.AddDays(2).Date;
        //    else if (datetime.DayOfWeek == DayOfWeek.Sunday)
        //        dt = datetime.AddDays(1).Date;
        //    else
        //        dt = datetime.AddHours(8).Date;

        //    return int.Parse(dt.ToString("yyyyMMdd"));
        //}

        public void SetTradingDay(int tradingday)
        {
            // 由在连接Logined时，kdb还没有连接，所以不能做kdb的相关操作
            TradingDay = tradingday;
        }

        public void ChangeTradingDay()
        {
            // 换日后trade与quote中的数据保存后清空.
            // 
            int kdbTradingday = GetKdbTradingDay();
            if (kdbTradingday != TradingDay && kdbTradingday != 0)
            {
                if (Save(kdbTradingday))
                {
                    Log.Info("换交易日了，重新建新表");
                    Create();
                }
            }
            // 登录后，订阅前，需要设置交易日，但是需要确保之前已经保存好了内存中的数据
            if (!SetKdbTradingDay(TradingDay))
                Log.Error("无法设置Kdb 的 tradingday，请检查是否无法连接q.exe 或 无法登录系统.");
        }

        public int GetKdbTradingDay()
        {
            if (KdbExists("tradingday"))
            {
                string checkStr = string.Format("tradingday");
                int result = (int)((long)c.k(checkStr));
                return result;
            }
            else return 0;
        }

        public bool SetKdbTradingDay(int tradingday)
        {
            try
            {
                string checkStr = string.Format("tradingday:{0}", tradingday);
                var s = c.k(checkStr);
                Log.Info("设置Kdb 的 tradingday 为 " + tradingday + " .");
                //long result = (long)s;
                return true;
            }
            catch
            {
            }
            return false;
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
                    if (Host != "127.0.0.1" && Host != "localhost")
                    {
                        c = null;
                        return;
                    }

                    Log.Info("尝试运行 kdb+ 服务.");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = "c:\\q\\w32\\q.exe",
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized,
                        CreateNoWindow = false,
                        Arguments = " -p " + Port.ToString()
                    });

                    // 需要一点时间
                    System.Threading.Thread.Sleep(2000);

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
        }

        public void Init()
        {
            // 表已经存在，不操作
            if (CheckTable())
            {
                return;
            }

            // 表不存在，先加载
            if (Load(TradingDay))
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
                Create();
            }
        }

        public bool CheckTable()
        {
            if (!KdbExists("trade"))
                return false;
            Log.Info("已经存在trade表.");
            if (SaveQuote)
            {
                if (!KdbExists("quote"))
                    return false;
                Log.Info("已经存在quote表.");
            }
            return true;
        }

        private bool KdbExists(string text = "trade")
        {
            if (null == c)
            {
                return false;
            }
            try
            {
                string checkStr = string.Format("type {0}", text);
                var result = c.k(checkStr);
                short s = (short)result;
                if (s == 98 || s == 99 || s == -7 || s == -11)
                    return true;
            }
            catch
            {
            }
            return false;
        }

        public void Create()
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

        public bool Load(int tradingday)
        {
            try
            {
                //object result = c.k(string.Format(@"\l {0}", Path));
                object result1 = c.k(string.Format(@"trade: get `:{0}/{1}/{2}", Path, "trades", tradingday));
                Log.Info("从磁盘加载 trade 表 {0} 成功.", tradingday);
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
                    object result2 = c.k(string.Format(@"quote: get `:{0}/{1}/{2}", Path, "quotes", tradingday));
                    Log.Info("从磁盘加载 quote 表 {0} 成功.", tradingday);
                }
                catch
                {
                    Log.Error("从磁盘加载quote失败.");
                    return false;
                }
            }
            return true;
        }

        public bool Save(int tradingday)
        {
            // 这里会被两处地方调用，一个是Disconnect，二是OnDisconnect
            // 一个是主动发起，另一个是被动触发，担心OnDisconnect在有些API下不会触发，在退出时保存两次也无所谓

            // 比如没有登录成功，就收到断开消息，这里是会出现交易日为0，这种情况下不保存
            if (tradingday == 0)
                return false;

            lock (this)
            {
                try
                {
                    //c.k(string.Format(@"`:{0}{1}/{1}/ set .Q.en[`:{0}{1}/] {2}", Path, "trade", "trade")); 
                    //c.k(string.Format(@"`:{0}{1}/{1}/ set .Q.en[`:{0}{1}/] {2}", Path, "quote", "quote"));
                    c.k(string.Format(@"`:{0}/{1}/{2} set {3}", Path, "trades", tradingday, "trade"));
                    Log.Info("保存 trade 表 {0} 完成.", tradingday);
                    if (SaveQuote)
                    {
                        c.k(string.Format(@"`:{0}/{1}/{2} set {3}", Path, "quotes", tradingday, "quote"));
                        Log.Info("保存 quote 表 {0} 完成.", tradingday);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Log.Error("保存trade/quote表失败.");
                }
            }
            return false;
        }

        public void Disconnect()
        {
            // 一定要保证退出时要运行
            if (null == c)
                return;

            lock (this)
            {
                Save(GetKdbTradingDay());
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
                    // 保存失败，通常是因为网络断开了
                    Log.Error(e.Message);
                    c.Close();
                    c = null;
                }
            }
        }
    }
}
