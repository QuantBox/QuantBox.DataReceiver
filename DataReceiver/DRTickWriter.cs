using Ideafixxxer.Generics;
using QuantBox;
using QuantBox.Data.Serializer.V2;
using QuantBox.XAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataReceiver
{
    public class DRTickWriter:TickWriter
    {
        public DRTickWriter(string path):base(path)
        {

        }
        
        // 目前先不处理港股的tickSize变化的那种行情
        PbTick CreateTick(ref DepthMarketDataField pDepthMarketData, PbTickCodec codec)
        {
            var tick = new PbTick();
            tick.DepthList = new List<DepthItem>();
            tick.Config = codec.Config;

            tick.TradingDay = pDepthMarketData.TradingDay;
            tick.ActionDay = pDepthMarketData.ActionDay;
            tick.Time_HHmm = pDepthMarketData.UpdateTime / 100;
            tick.Time_____ssf__ = pDepthMarketData.UpdateTime % 100 * 10 + pDepthMarketData.UpdateMillisec / 100;
            tick.Time________ff = pDepthMarketData.UpdateMillisec % 100;
            // 数据接收器时计算本地与交易所的行情时间差
            // 1.这个地方是否保存？
            // 2.到底是XAPI中提供还是由接收器提供？
            //tick.LocalTime_Msec = (int)(DateTime.Now - codec.GetActionDayDateTime(tick)).TotalMilliseconds;

            codec.SetSymbol(tick, pDepthMarketData.Symbol);
            if (pDepthMarketData.Exchange != ExchangeType.Undefined)
                codec.SetExchange(tick, Enum<ExchangeType>.ToString(pDepthMarketData.Exchange));
            codec.SetLowerLimitPrice(tick, pDepthMarketData.LowerLimitPrice);
            codec.SetUpperLimitPrice(tick, pDepthMarketData.UpperLimitPrice);

            codec.SetOpen(tick, pDepthMarketData.OpenPrice);
            codec.SetHigh(tick, pDepthMarketData.HighestPrice);
            codec.SetLow(tick, pDepthMarketData.LowestPrice);
            codec.SetClose(tick, pDepthMarketData.ClosePrice);

            codec.SetVolume(tick, (long)pDepthMarketData.Volume);
            codec.SetOpenInterest(tick, (long)pDepthMarketData.OpenInterest);
            codec.SetTurnover(tick, pDepthMarketData.Turnover);//一定要设置合约乘数才能最优保存
            codec.SetAveragePrice(tick, pDepthMarketData.AveragePrice);
            codec.SetLastPrice(tick, pDepthMarketData.LastPrice);
            codec.SetSettlementPrice(tick, pDepthMarketData.SettlementPrice);

            do
            {

                if (pDepthMarketData.BidVolume1 == 0)
                    break;
                tick.DepthList.Insert(0, new DepthItem(codec.PriceToTick(pDepthMarketData.BidPrice1), pDepthMarketData.BidVolume1, 0));

                // 先记录一个假的，防止只有买价没有卖价的情况
                codec.SetAskPrice1(tick, pDepthMarketData.BidPrice1);
                tick.AskPrice1 += 1;

                if (pDepthMarketData.BidVolume2 == 0)
                    break;
                tick.DepthList.Insert(0, new DepthItem(codec.PriceToTick(pDepthMarketData.BidPrice2), pDepthMarketData.BidVolume2, 0));

                if (pDepthMarketData.BidVolume3 == 0)
                    break;
                tick.DepthList.Insert(0, new DepthItem(codec.PriceToTick(pDepthMarketData.BidPrice3), pDepthMarketData.BidVolume3, 0));

                if (pDepthMarketData.BidVolume4 == 0)
                    break;
                tick.DepthList.Insert(0, new DepthItem(codec.PriceToTick(pDepthMarketData.BidPrice4), pDepthMarketData.BidVolume4, 0));

                if (pDepthMarketData.BidVolume5 == 0)
                    break;
                tick.DepthList.Insert(0, new DepthItem(codec.PriceToTick(pDepthMarketData.BidPrice5), pDepthMarketData.BidVolume5, 0));

            } while (false);

            do
            {
                if (pDepthMarketData.AskVolume1 == 0)
                    break;
                tick.DepthList.Add(new DepthItem(codec.PriceToTick(pDepthMarketData.AskPrice1), pDepthMarketData.AskVolume1, 0));
                // 记录卖一价
                codec.SetAskPrice1(tick, pDepthMarketData.AskPrice1);

                if (pDepthMarketData.AskVolume2 == 0)
                    break;
                tick.DepthList.Add(new DepthItem(codec.PriceToTick(pDepthMarketData.AskPrice2), pDepthMarketData.AskVolume2, 0));

                if (pDepthMarketData.AskVolume3 == 0)
                    break;
                tick.DepthList.Add(new DepthItem(codec.PriceToTick(pDepthMarketData.AskPrice3), pDepthMarketData.AskVolume3, 0));

                if (pDepthMarketData.AskVolume4 == 0)
                    break;
                tick.DepthList.Add(new DepthItem(codec.PriceToTick(pDepthMarketData.AskPrice4), pDepthMarketData.AskVolume4, 0));

                if (pDepthMarketData.AskVolume5 == 0)
                    break;
                tick.DepthList.Add(new DepthItem(codec.PriceToTick(pDepthMarketData.AskPrice5), pDepthMarketData.AskVolume5, 0));

            } while (false);

            return tick;
        }

        public bool Write(ref DepthMarketDataField pDepthMarketData)
        {
            QuantBox.Data.Serializer.V2.TickWriter.WriterDataItem item;
            if (Items.TryGetValue(pDepthMarketData.Symbol, out item))
            {
                item.Tick = CreateTick(ref pDepthMarketData, item.Serializer.Codec);
                base.Write(item, item.Tick);
                return true;
            }
            return false;
        }
    }
}
