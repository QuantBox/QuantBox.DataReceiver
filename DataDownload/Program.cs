using ArchiveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

//using HDF.PInvoke;
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
    }
}
