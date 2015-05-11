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
            Console.WriteLine(dd.GetHistorical("CFFEX", "IF", "IF1450", 20150410));
        }
    }
}
