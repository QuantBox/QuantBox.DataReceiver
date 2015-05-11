using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataDownload
{
    class Program
    {
        static void Main(string[] args)
        { 
            var dd = new DataDownload();
            dd.USERNAME = "test1";
            dd.PASSWORD = "ABC";
            dd.BASE_URL = "http://localhost/";
            //dd.PATH_Historical = "";
            var ll = dd.GetHistorical("CFFEX", "IF", "IF1504", 20150330, 20150520);
            foreach(var l in ll)
            {
                Console.WriteLine(l.Item1 + l.Item2);
            }
        }
    }
}
