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
            catch(Exception e)
            {
                Log.Error(e.Message);
                return;
            }
            
            Log.Info("连接kdb+成功：{0}:{1}", Host, Port);

            try
            {
                short result1 = (short)c.k(@"type trade");
                short result2 = (short)c.k(@"type quote");
                if (result1 == 98 && result2 == 98)
                {
                    // 表已经存在，不操作
                    Log.Info("已经存在trade/quote两张表");
                    return;
                }
            }
            catch (Exception e)
            {
                try
                {
                    // 表不存在，先加载
                    //object result = c.k(string.Format(@"\l {0}", Path));
                    object result1 = c.k(string.Format(@"load `:{0}/{1}", Path, "trade"));
                    object result2 = c.k(string.Format(@"load `:{0}/{1}", Path, "quote"));
                }
                catch(Exception e1)
                {

                }
            }
            

            try
            {
                short result1 = (short)c.k(@"type trade");
                short result2 = (short)c.k(@"type quote");
                if (result1 == 98 && result2 == 98)
                {
                    // 表已经存在，不操作
                    Log.Info("从磁盘加载trade/quote两张表成功");
                    return;
                }
            }
            catch (Exception e)
            {
                // 加载也是失败的，只能创建了
                c.ks(@"trade:([]datetime:`datetime$();sym:`symbol$();price:`float$();volume:`long$())");
                c.ks(@"quote:([]datetime:`datetime$();sym:`symbol$();bid:`float$();ask:`float$();bsize:`int$();asize:`int$())");
                Log.Info("创建trade/quote两张表完成");
            }
        }

        public void Save()
        {
            lock(this)
            {
                try
                {
                    //c.k(string.Format(@"`:{0}{1}/{1}/ set .Q.en[`:{0}{1}/] {2}", Path, "trade", "trade"));
                    //c.k(string.Format(@"`:{0}{1}/{1}/ set .Q.en[`:{0}{1}/] {2}", Path, "quote", "quote"));
                    c.k(string.Format(@"`:{0}/{1} set {2}", Path, "trade", "trade"));
                    c.k(string.Format(@"`:{0}/{1} set {2}", Path, "quote", "quote"));
                    Log.Info("保存trade/quote两张表完成");
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
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
                Save();
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
            
            double bid = 0;
            double ask = 0;
            int bsize = 0;
            int asize = 0;

            string trade_str = string.Format("`trade insert({0};`$\"{1}\";{2}f;{3}j)", dateTime, symbol, price, volume);
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
                catch(Exception e)
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
