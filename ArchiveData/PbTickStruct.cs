using QuantBox.Data.Serializer.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PbTickStruct5
    {
        public int TradingDay;
        public int ActionDay;
        public int UpdateTime;
        public int UpdateMillisec;

        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Symbol;
        /// <summary>
        /// 交易所代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string Exchange;

        /// <summary>
        /// 最新价
        /// </summary>
        public float LastPrice;
        /// <summary>
        /// 数量
        /// </summary>
        public float Volume;
        /// <summary>
        /// 成交金额
        /// </summary>
        public float Turnover;
        /// <summary>
        /// 持仓量
        /// </summary>
        public float OpenInterest;
        /// <summary>
        /// 当日均价
        /// </summary>
        public float AveragePrice;


        /// <summary>
        /// 今开盘
        /// </summary>
        public float OpenPrice;
        /// <summary>
        /// 最高价
        /// </summary>
        public float HighestPrice;
        /// <summary>
        /// 最低价
        /// </summary>
        public float LowestPrice;
        /// <summary>
        /// 今收盘
        /// </summary>
        public float ClosePrice;
        /// <summary>
        /// 本次结算价
        /// </summary>
        public float SettlementPrice;

        /// <summary>
        /// 涨停板价
        /// </summary>
        public float UpperLimitPrice;
        /// <summary>
        /// 跌停板价
        /// </summary>
        public float LowerLimitPrice;
        /// <summary>
        /// 昨收盘
        /// </summary>
        public float PreClosePrice;
        /// <summary>
        /// 上次结算价
        /// </summary>
        public float PreSettlementPrice;
        /// <summary>
        /// 昨持仓量
        /// </summary>
        public float PreOpenInterest;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public float[] Price;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public int[] Size;
    }

    public class DataConvert
    {
        static PbTickCodec codec = new PbTickCodec();

        public static PbTickStruct5 toStruct(PbTickView input)
        {
            var o = default(PbTickStruct5);

            codec.GetUpdateTime(input,out o.UpdateTime,out o.UpdateMillisec);

            o.TradingDay = input.TradingDay;
            o.ActionDay = input.ActionDay;
            o.LastPrice = (float)input.LastPrice;
            o.Volume = input.Volume;

            o.Turnover = (float)input.Turnover;
            o.OpenInterest = (float)input.OpenInterest;
            o.AveragePrice = (float)input.AveragePrice;

            if (input.Bar != null)
            {
                o.OpenPrice = (float)input.Bar.Open;
                o.HighestPrice = (float)input.Bar.High;
                o.LowestPrice = (float)input.Bar.Low;
                o.ClosePrice = (float)input.Bar.Close;
            }

            if (input.Static != null)
            {
                o.LowerLimitPrice = (float)input.Static.LowerLimitPrice;
                o.UpperLimitPrice = (float)input.Static.UpperLimitPrice;
                o.SettlementPrice = (float)input.Static.SettlementPrice;
                o.Symbol = input.Static.Symbol;
                o.Exchange = input.Static.Exchange;
                //o.Symbol = "IF";
                //o.Exchange = "CFFEX";
                o.PreClosePrice = (float)input.Static.PreClosePrice;
                o.PreSettlementPrice = (float)input.Static.PreSettlementPrice;
                o.PreOpenInterest = input.Static.PreOpenInterest;
            }

            o.Price = new float[10];
            o.Size = new int[10];

            int count = input.DepthList == null ? 0 : input.DepthList.Count;
            if (count > 0)
            {
                int AskPos = DepthListHelper.FindAsk1Position(input.DepthList, input.AskPrice1); // 卖一位置
                int BidPos = AskPos - 1; // 买一位置，在数组中的位置
                int BidCount = BidPos + 1;
                int AskCount = count - AskPos;


                if (BidCount > 0)
                {
                    int j = 0;
                    for (int i = BidPos; i >= 0; --i)
                    {
                        o.Price[4 - j] = (float)input.DepthList[i].Price;
                        o.Size[4 - j] = input.DepthList[i].Size;

                        ++j;
                    }
                }

                if (AskCount > 0)
                {
                    int j = 0;
                    for (int i = AskPos; i < count; ++i)
                    {
                        o.Price[5 + j] = (float)input.DepthList[i].Price;
                        o.Size[5 + j] = input.DepthList[i].Size;

                        ++j;
                    }
                }
            }

            return o;
        }
    }
}
