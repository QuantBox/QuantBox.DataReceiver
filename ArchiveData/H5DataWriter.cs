using HDF.PInvoke;
using QuantBox.Data.Serializer.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveData
{
    public class H5DataWriter
    {
        private long h5;

        public void Open(string OutputFile)
        {
            h5 = H5F.create(OutputFile, H5F.ACC_TRUNC, H5P.DEFAULT, H5P.DEFAULT);
        }

        public void Close()
        {
            H5F.close(h5);
        }

        public void Writer(List<PbTickView> list,string dataset_name)
        {
            var t = typeof(PbTickStruct5);
            var size = Marshal.SizeOf(t);

            var typeId = create_type();

            // chunk一定得小于dim数据量，多了会出错
            // 如果数据100条左右，按
            var log10 = (int)Math.Log10(list.Count);
            ulong pow = (ulong)Math.Pow(10, log10);
            ulong c_s = Math.Min(1000, pow);
            ulong[] chunk_size = new ulong[] { c_s };

            ulong[] dims = new ulong[] { (ulong)list.Count };

            long dcpl = 0;
            if (list.Count == 0 || log10 == 0)
            { }
            else
            {
                dcpl = create_property(chunk_size);
            }

            var spaceid = H5S.create_simple(dims.Length, dims, null);
            var datasetid = H5D.create(h5, dataset_name, typeId, spaceid, H5P.DEFAULT, dcpl);

            IntPtr p = Marshal.AllocHGlobal(size * (int)dims[0]);

            int i = 0;
            foreach (var b in list)
            {
                var s = DataConvert.toStruct(b);
                Marshal.StructureToPtr(s, new IntPtr(p.ToInt32() + size * i), false);
                ++i;
            }

            H5D.write(datasetid, typeId, spaceid, H5S.ALL, H5P.DEFAULT, p);

            H5D.close(datasetid);
            H5S.close(spaceid);
            H5T.close(typeId);
            H5P.close(dcpl);
            

            Marshal.FreeHGlobal(p);
        }

        private long create_type()
        {
            var t = typeof(PbTickStruct5);
            var size = Marshal.SizeOf(t);
            var float_size = Marshal.SizeOf(typeof(float));
            var int_size = Marshal.SizeOf(typeof(int));

            var typeId = H5T.create(H5T.class_t.COMPOUND, new IntPtr(size));


            H5T.insert(typeId, "TradingDay", Marshal.OffsetOf(t, "TradingDay"), H5T.NATIVE_INT32);
            H5T.insert(typeId, "ActionDay", Marshal.OffsetOf(t, "ActionDay"), H5T.NATIVE_INT32);
            H5T.insert(typeId, "UpdateTime", Marshal.OffsetOf(t, "UpdateTime"), H5T.NATIVE_INT32);
            H5T.insert(typeId, "UpdateMillisec", Marshal.OffsetOf(t, "UpdateMillisec"), H5T.NATIVE_INT32);

            var Symbol_type = H5T.copy(H5T.C_S1);
            H5T.set_size(Symbol_type, new IntPtr(64));
            var Exchange_type = H5T.copy(H5T.C_S1);
            H5T.set_size(Exchange_type, new IntPtr(9));
            H5T.insert(typeId, "Symbol", Marshal.OffsetOf(t, "Symbol"), Symbol_type);
            H5T.insert(typeId, "Exchange", Marshal.OffsetOf(t, "Exchange"), Exchange_type);

            H5T.insert(typeId, "LastPrice", Marshal.OffsetOf(t, "LastPrice"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "Volume", Marshal.OffsetOf(t, "Volume"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "Turnover", Marshal.OffsetOf(t, "Turnover"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "OpenInterest", Marshal.OffsetOf(t, "OpenInterest"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "AveragePrice", Marshal.OffsetOf(t, "AveragePrice"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "OpenPrice", Marshal.OffsetOf(t, "OpenPrice"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "HighestPrice", Marshal.OffsetOf(t, "HighestPrice"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "LowestPrice", Marshal.OffsetOf(t, "LowestPrice"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "ClosePrice", Marshal.OffsetOf(t, "ClosePrice"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "SettlementPrice", Marshal.OffsetOf(t, "SettlementPrice"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "UpperLimitPrice", Marshal.OffsetOf(t, "UpperLimitPrice"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "LowerLimitPrice", Marshal.OffsetOf(t, "LowerLimitPrice"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "PreClosePrice", Marshal.OffsetOf(t, "PreClosePrice"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "PreSettlementPrice", Marshal.OffsetOf(t, "PreSettlementPrice"), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "PreOpenInterest", Marshal.OffsetOf(t, "PreOpenInterest"), H5T.NATIVE_FLOAT);

            var price_intptr = Marshal.OffsetOf(t, "Price");
            H5T.insert(typeId, "BidPrice5", price_intptr + float_size * 0, H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "BidPrice4", price_intptr + float_size * 1, H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "BidPrice3", price_intptr + float_size * 2, H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "BidPrice2", price_intptr + float_size * 3, H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "BidPrice1", price_intptr + float_size * 4, H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "AskPrice1", price_intptr + float_size * 5, H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "AskPrice2", price_intptr + float_size * 6, H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "AskPrice3", price_intptr + float_size * 7, H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "AskPrice4", price_intptr + float_size * 8, H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "AskPrice5", price_intptr + float_size * 9, H5T.NATIVE_FLOAT);

            var size_intptr = Marshal.OffsetOf(t, "Size");
            H5T.insert(typeId, "BidSize5", size_intptr + int_size * 0, H5T.NATIVE_INT32);
            H5T.insert(typeId, "BidSize4", size_intptr + int_size * 1, H5T.NATIVE_INT32);
            H5T.insert(typeId, "BidSize3", size_intptr + int_size * 2, H5T.NATIVE_INT32);
            H5T.insert(typeId, "BidSize2", size_intptr + int_size * 3, H5T.NATIVE_INT32);
            H5T.insert(typeId, "BidSize1", size_intptr + int_size * 4, H5T.NATIVE_INT32);
            H5T.insert(typeId, "AskSize1", size_intptr + int_size * 5, H5T.NATIVE_INT32);
            H5T.insert(typeId, "AskSize2", size_intptr + int_size * 6, H5T.NATIVE_INT32);
            H5T.insert(typeId, "AskSize3", size_intptr + int_size * 7, H5T.NATIVE_INT32);
            H5T.insert(typeId, "AskSize4", size_intptr + int_size * 8, H5T.NATIVE_INT32);
            H5T.insert(typeId, "AskSize5", size_intptr + int_size * 9, H5T.NATIVE_INT32);

            return typeId;
        }

        private long create_property(ulong[] chunk_size)
        {
            var dcpl = H5P.create(H5P.DATASET_CREATE);
            H5P.set_layout(dcpl, H5D.layout_t.CHUNKED);
            H5P.set_chunk(dcpl, chunk_size.Length, chunk_size);
            H5P.set_deflate(dcpl, 6);
            return dcpl;
        }
    }
}
