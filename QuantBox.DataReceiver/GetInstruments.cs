using Newtonsoft.Json;
using QuantBox;
using QuantBox.XAPI;
using QuantBox.XAPI.Callback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataReceiver
{
    /// <summary>
    /// 登录得到最新的合约列表
    /// </summary>
    public class GetInstruments:ApiBase
    {
        public string ConnectionConfigFileName = @"ConnectionConfig.json";
        
        public ConnectionConfig ConnectionConfig;

        public void Save()
        {
            Save(ConfigPath, ConnectionConfigFileName, ConnectionConfig);
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
                api.ReqQryInstrument("","");
            }
        }

        private void OnConnectionStatus(object sender, ConnectionStatus status, ref RspUserLoginField userLogin, int size1)
        {
            Console.WriteLine("登录状态：" + status + userLogin.ErrorMsg());
        }

        private void OnRspQryInstrument(object sender, ref InstrumentField instrument, int size1, bool bIsLast)
        {
            InstrumentInfoList.Add(new InstrumentInfo()
            {
                Symbol = instrument.Symbol,
                Instrument = instrument.InstrumentID,
                Exchange = instrument.ExchangeID,
                TickSize = instrument.PriceTick,
                Factor = instrument.VolumeMultiple
            });
            this.bIsLast = bIsLast;
        }
    }
}
