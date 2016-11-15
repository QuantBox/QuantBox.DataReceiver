using ArchiveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using HDF.PInvoke;
using System.Runtime.InteropServices;

namespace DataDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            var dd = new DataDownload();
            dd.USERNAME = "guest";
            dd.PASSWORD = "guest";
            dd.BASE_URL = "http://localhost/";
            //dd.PATH_Historical = "";
            var ll = dd.GetHistorical("CFFEX", "IF&IF", "IF1504&IF1505", 20150330, 20150520);
            var l2 = dd.GetHistorical("CFFEX", "IF", "IF1504", 20150330, 20150520);
            foreach(var l in ll)
            {
                Console.WriteLine(l.Item1 + l.Item2);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ComType
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Name;
            public int x_pos;
            public int y_pos;
            public float Mass;
            public double Temperature;
        }

        static void Main_Read(string[] args)
        {
            int DATA_ARRAY_LENGTH = 5;

            //var h5 = H5F.open(@"E:\HDF5\HDF5DotNet-src\examples\CSharpExample\CSharpExample1\table.h5", H5F.ACC_RDONLY);

            //var h5 = H5F.open(@"D:\test.h5", H5F.ACC_RDONLY);
            //var h5 = H5F.open(@"E:\HDF5\Hdf5DotnetTools-master\ToolTest\bin\Debug\table.h5", H5F.ACC_RDONLY);
            var h5 = H5F.open(@"E:\HDF5\test_gzip.h5", H5F.ACC_RDONLY);

            var dataset = H5D.open(h5, "trans_detail/20160929");
            var spaceid = H5D.get_space(dataset);
            var npoints = H5S.get_simple_extent_npoints(spaceid);
            //var dims = H5S.get_simple_extent_dims(spaceid);
            int rank = H5S.get_simple_extent_ndims(spaceid);

            // 是不是不能用自己的type
            var dtype = H5D.get_type(dataset);
            var dtcls = H5T.get_class(dtype);
            var size = H5T.get_size(dtype);
            var sz = Marshal.SizeOf(typeof(ComType));

            var dtype_n = H5T.get_nmembers(dtype);
            
            for(uint i=0;i<dtype_n;++i)
            {
                var x = H5T.get_member_name(dtype, i);
                var x4 = Marshal.PtrToStringAnsi(x);
                var y = H5T.get_member_type(dtype, i);
                var z = H5T.get_class(y);
                var x1 = H5T.get_member_offset(dtype, i);
                var x3 = H5T.get_size(y);
                
                Console.WriteLine(x4);
                Console.WriteLine(z);
                Console.WriteLine(x1);
                //var x2 = Marshal.OffsetOf(typeof(ComType), x4).ToInt32();
                //Console.WriteLine(x2);
                Console.WriteLine(x3);
            }

            int ss1 = Marshal.SizeOf(typeof(ComType));
            IntPtr p = Marshal.AllocHGlobal(ss1*11);
            H5D.read(dataset, dtype, H5S.ALL,H5S.ALL,H5P.DEFAULT, p);
            var s = Marshal.PtrToStructure(p, typeof(ComType));
            Console.WriteLine(s);
            var s2 = Marshal.PtrToStructure(p+ss1, typeof(ComType));
            Console.WriteLine(s2);
            var s3 = Marshal.PtrToStructure(p + ss1*4, typeof(ComType));
            Console.WriteLine(s3);
            var s4 = Marshal.PtrToStructure(p + ss1 * 5, typeof(ComType));
            Console.WriteLine(s4);
            var s6 = Marshal.PtrToStructure(p + ss1 * 10, typeof(ComType));
            Console.WriteLine(s6);
        }

        static void Main2222(string[] args)
        {
            var h5 = H5F.create(@"D:\test.h5", H5F.ACC_TRUNC);

            var typeId = H5T.create(H5T.class_t.COMPOUND, new IntPtr(40));

            var strtype = H5T.copy(H5T.C_S1);
            H5T.set_size(strtype, new IntPtr(16));

            H5T.insert(typeId, "Name", new IntPtr(0), strtype);
            H5T.insert(typeId, "x_pos", new IntPtr(16), H5T.NATIVE_INT32);
            H5T.insert(typeId, "y_pos", new IntPtr(20), H5T.NATIVE_INT32);
            H5T.insert(typeId, "Mass", new IntPtr(24), H5T.NATIVE_FLOAT);
            H5T.insert(typeId, "Temperature", new IntPtr(32), H5T.NATIVE_DOUBLE);

            ulong[] dims = new ulong[] { 10000 };
            ulong[] chunk_size = new ulong[] { 1000};

            var spaceid = H5S.create_simple(dims.Length, dims, null);


            var dcpl = H5P.create(H5P.DATASET_CREATE);

            H5P.set_layout(dcpl, H5D.layout_t.COMPACT);
            H5P.set_deflate(dcpl, 6);

            H5P.set_chunk(dcpl, chunk_size.Length, chunk_size);





            var datasetid = H5D.create(h5, "Table1", typeId, spaceid, H5P.DEFAULT, dcpl);

            ComType ct = new ComType()
            {
                Name = "aabb",
                x_pos = 2,
                y_pos = 1,
                Mass = 1.24F,
                Temperature = 45.7,
            };

            IntPtr p = Marshal.AllocHGlobal(40 * (int)dims[0]);
            Marshal.StructureToPtr(ct, p, false);



            H5D.write(datasetid, typeId, spaceid, H5S.ALL, H5P.DEFAULT, p);

            H5F.close(h5);
        }
    }
}
