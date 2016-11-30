using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XAPI;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace DataReceiver
{
    // <summary>
    /// 连接配置信息
    /// </summary>
    [DataContract]
    public class ConnectionConfig
    {
        /// <summary>
        /// dll地址
        /// </summary>
        [DataMember(Order = 0)]
        public string LibPath;
        /// <summary>
        /// 服务器
        /// </summary>
        [DataMember(Order = 1)]
        public ServerInfoField Server;
        /// <summary>
        /// 账号
        /// </summary>
        [DataMember(Order = 2)]
        public UserInfoField User;
        /// <summary>
        /// 同一服务器和账号最大可登录会话数
        /// </summary>
        [DataMember(Order = 3)]
        public int SessionLimit;
        /// <summary>
        /// 每个会话最大可订阅数
        /// </summary>
        [DataMember(Order = 4)]
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

    /// <summary>
    /// 本来是用的tuple，但没办法用微软内置的转成json，只好自己定义一个
    /// </summary>
    [DataContract]
    public class ScheduleTaskConfig
    {
        [DataMember(Order = 0)]
        public TimeSpan Item1;
        [DataMember(Order = 1)]
        public string Item2;
        [DataMember(Order = 2)]
        public string Item3;
    }
}
