using QuantBox.XAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataReceiver
{
    // <summary>
    /// 连接配置信息
    /// </summary>
    public class ConnectionConfig
    {
        /// <summary>
        /// dll地址
        /// </summary>
        public string LibPath;
        /// <summary>
        /// 服务器
        /// </summary>
        public ServerInfoField Server;
        /// <summary>
        /// 账号
        /// </summary>
        public UserInfoField User;
        /// <summary>
        /// 同一服务器和账号最大可登录会话数
        /// </summary>
        public int SessionLimit;
        /// <summary>
        /// 每个会话最大可订阅数
        /// </summary>
        public int SubscribePerSession;
    }

    /// <summary>
    /// 查询得到的合约信息
    /// </summary>
    public class InstrumentInfo
    {
        public string Symbol;
        public string Instrument;
        public string Exchange;
        public string ProductID;
        public double TickSize;
        public double Factor;
        [JsonIgnore]
        public int Time_ssf_Diff;

        public override bool Equals(object obj)
        {
            if (obj is InstrumentInfo)
                return this.GetHashCode().Equals(((InstrumentInfo)obj).GetHashCode());
            else return false;
        }

        public override int GetHashCode()
        {
            int hashID = Instrument.GetHashCode();
            int hashValue = Exchange.GetHashCode();
            return hashID ^ hashValue;
        }
    }

    /// <summary>
    /// 写文件时要用到的信息，有了此文件可以进行文件大小的优化
    /// 同时
    /// </summary>
    public class InstrumentFilterConfig
    {
        public string SymbolRegex;
        /// <summary>
        /// 只是为了节省文件大小
        /// </summary>
        public int Time_ssf_Diff;
    }
}
