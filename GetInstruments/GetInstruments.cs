using DataReceiver;
using NLog;
using XAPI;
using XAPI.Callback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetInstruments
{
    /// <summary>
    /// 登录得到最新的合约列表
    /// </summary>
    public class GetInstruments : ApiBase
    {
        public string ConnectionConfigFileName = @"ConnectionConfig.json";

        public ConnectionConfig ConnectionConfig;

        public void Save()
        {
            // 连接信息没有必要重新保存
            //Save(ConfigPath, ConnectionConfigFileName, ConnectionConfig);
            Save(ConfigPath, InstrumentInfoListFileName, InstrumentInfoList);
        }

        public void Load()
        {
            ConnectionConfig = new ConnectionConfig();
            InstrumentInfoList = new List<InstrumentInfo>();

            ConnectionConfig = (ConnectionConfig)Load(ConfigPath, ConnectionConfigFileName, ConnectionConfig);
            InstrumentInfoList = (List<InstrumentInfo>)Load(ConfigPath, InstrumentInfoListFileName, InstrumentInfoList);

            if (ConnectionConfig == null)
                ConnectionConfig = new ConnectionConfig();
            if (InstrumentInfoList == null)
                InstrumentInfoList = new List<InstrumentInfo>();
        }

        public void Connect()
        {
            XApi api = new XApi(ConnectionConfig.LibPath);

            api.Server = ConnectionConfig.Server;
            api.User = ConnectionConfig.User;

            api.Log = LogManager.GetLogger(string.Format("{0}.{1}", api.Server.BrokerID, api.User.UserID));
            api.MaxSubscribedInstrumentsCount = ConnectionConfig.SubscribePerSession;

            api.OnConnectionStatus = OnConnectionStatus;
            api.OnRspQryInstrument = OnRspQryInstrument;

            api.Connect();

            XApiList.Add(api);
        }

        public void ReqQryInstrument()
        {
            bIsLast = false;
            InstrumentInfoList.Clear();

            foreach (var api in XApiList)
            {
                ReqQueryField query = default(ReqQueryField);
                api.ReqQuery(QueryType.ReqQryInstrument, ref query);
            }
        }

        private void OnRspQryInstrument(object sender, ref InstrumentField instrument, int size1, bool bIsLast)
        {
            if (size1 > 0)
            {
                InstrumentInfoList.Add(new InstrumentInfo()
                {
                    Symbol = instrument.Symbol,
                    Instrument = instrument.InstrumentID,
                    Exchange = instrument.ExchangeID,
                    ProductID = instrument.ProductID,
                    TickSize = instrument.PriceTick,
                    Factor = instrument.VolumeMultiple
                });
            }

            this.bIsLast = bIsLast;
        }
    }
}
