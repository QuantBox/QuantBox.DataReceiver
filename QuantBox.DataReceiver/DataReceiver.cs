using Newtonsoft.Json;
using QuantBox;
using QuantBox.XAPI;
using QuantBox.XAPI.Callback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataReceiver
{
    public class DataReceiver:ApiBase
    {
        public DRTickWriter TickWriter;

        public List<ConnectionConfig> ConnectionConfigList;
        public List<InstrumentFilterConfig> IncludeFilterList;
        public List<InstrumentFilterConfig> ExcludeFilterList;

        public string ConnectionConfigListFileName = @"ConnectionConfigList.json";
        public string IncludeFilterListFileName = @"IncludeFilterList.json";
        public string ExcludeFilterListFileName = @"ExcludeFilterList.json";

        public ActionBlock<DepthMarketDataField> Input;

        public DataReceiver()
        {
            Input = new ActionBlock<DepthMarketDataField>((x) => OnInputMarketData(x));
        }

        public void Save()
        {
            Save(ConfigPath, ConnectionConfigListFileName, ConnectionConfigList);
            Save(ConfigPath, InstrumentInfoListFileName, InstrumentInfoList);
            Save(ConfigPath, IncludeFilterListFileName, IncludeFilterList);
            Save(ConfigPath, ExcludeFilterListFileName, ExcludeFilterList);
        }

        public void Load()
        {
            ConnectionConfigList = new List<ConnectionConfig>();
            InstrumentInfoList = new List<InstrumentInfo>();
            IncludeFilterList = new List<InstrumentFilterConfig>();
            ExcludeFilterList = new List<InstrumentFilterConfig>();

            ConnectionConfigList = (List<ConnectionConfig>)Load(ConfigPath, ConnectionConfigListFileName, ConnectionConfigList);
            InstrumentInfoList = (List<InstrumentInfo>)Load(ConfigPath, InstrumentInfoListFileName, InstrumentInfoList);
            IncludeFilterList = (List<InstrumentFilterConfig>)Load(ConfigPath, IncludeFilterListFileName, IncludeFilterList);
            ExcludeFilterList = (List<InstrumentFilterConfig>)Load(ConfigPath, ExcludeFilterListFileName, ExcludeFilterList);

            if (ConnectionConfigList == null)
                ConnectionConfigList = new List<ConnectionConfig>();
            if(InstrumentInfoList == null)
                InstrumentInfoList = new List<InstrumentInfo>();
            if (IncludeFilterList == null)
                IncludeFilterList = new List<InstrumentFilterConfig>();
            if (ExcludeFilterList == null)
                ExcludeFilterList = new List<InstrumentFilterConfig>();
        }

        public void Connect()
        {
            // 查看有多少种连接
            foreach (var cc in ConnectionConfigList)
            {
                // 建立多个连接
                for (int i = 0; i < cc.SessionLimit; ++i)
                {
                    XApi api = new XApi(cc.LibPath);
                    api.Server = cc.Server;
                    api.User = cc.User;
                    api.MaxSubscribedInstrumentsCount = cc.SubscribePerSession;

                    api.OnConnectionStatus = OnConnectionStatus;
                    api.OnRtnDepthMarketData = OnRtnDepthMarketData;

                    api.Connect();

                    XApiList.Add(api);
                }
            }
        }

        public bool Contains(string szInstrument, string szExchange)
        {
            foreach (var api in XApiList)
            {
                if (api.SubscribedInstrumentsContains(szInstrument,szExchange))
                {
                    return true;
                }
            }
            return false;
        }

        public void Subscribe()
        {
            // 如何处理程序正在跑，但修改了配置文件，有新加合约时

            // 遍历合约列表
            foreach (var i in InstrumentInfoList)
            {
                // 查看是否需要订阅
                InstrumentFilterConfig match1 = Match(i.Symbol, IncludeFilterList);
                
                if(match1 != null)
                {
                    i.Time_ssf_Diff = match1.Time_ssf_Diff;
                    // 包含，需要订阅

                    InstrumentFilterConfig match2 = Match(i.Symbol, ExcludeFilterList);
                    if(match2 == null)
                    {
                        // 不包含，没有被排除，需要订阅
                        TickWriter.AddInstrument(string.Format("{0}.{1}",i.Instrument,i.Exchange), i.TickSize, i.Factor, i.Time_ssf_Diff);
                        TickWriter.AddInstrument(string.Format("{0}.", i.Instrument), i.TickSize, i.Factor, i.Time_ssf_Diff);
                        TickWriter.AddInstrument(string.Format("{0}", i.Instrument), i.TickSize, i.Factor, i.Time_ssf_Diff);

                        if (Contains(i.Instrument, i.Exchange))
                        {
                            Console.WriteLine("已经订阅了{0}.{1}", i.Instrument, i.Exchange);
                        }
                        else
                        {
                            // 得到API,直接订阅
                            foreach (var api in XApiList)
                            {
                                if (api.SubscribedInstrumentsCount < api.MaxSubscribedInstrumentsCount)
                                {
                                    api.Subscribe(i.Instrument, i.Exchange);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("合约{0}匹配于{1}排除列表", i.Symbol, ExcludeFilterListFileName);
                    }
                }
                else
                {
                    Console.WriteLine("合约{0}不匹配于{1}包含列表", i.Symbol, IncludeFilterListFileName);
                }
            }
        }

        private InstrumentFilterConfig Match(string symbol, List<InstrumentFilterConfig> list)
        {
            foreach(var l in list)
            {
                Regex regex = new Regex(l.SymbolRegex);
                if (regex.Match(symbol).Success)
                {
                    return l;
                }
            }
            return null;
        }

        private void OnConnectionStatus(object sender, ConnectionStatus status, ref RspUserLoginField userLogin, int size1)
        {
            Console.WriteLine("登录状态：" + status + userLogin.ErrorMsg());
        }

        private void OnRtnDepthMarketData(object sender, ref DepthMarketDataField marketData)
        {
            Input.Post(marketData);
        }

        public void OnInputMarketData(DepthMarketDataField pDepthMarketData)
        {
            TickWriter.Write(ref pDepthMarketData);
        }
    }
}
