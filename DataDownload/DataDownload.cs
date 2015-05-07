using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataDownload
{
    public class DataDownload
    {
        public const string URL_Realtime = "download.php?type=realtime&exchange={0}&instrument={1}";
        public const string URL_TradingDay = "download.php?type=tradingday&tradingday={0}";
        public const string URL_Historical = "download.php?type=historical&exchange={0}&product={1}&instrument={2}&tradingday={3}";

        public string BASE_URL = "http://localhost/";

        public string PATH_Realtime = @"E:\test\Data";
        public string PATH_TradingDay = @"E:\test\Data_TradingDay";
        public string PATH_Historical = @"E:\test\Data_Instrument";
        
        public string USERNAME = "guest";
        public string PASSWORD = "guest";

        public string GetRealtime(string exchange, string instrument)
        {
            string url = string.Format(BASE_URL+URL_Realtime,exchange,instrument);
            DirectoryInfo di = new DirectoryInfo(PATH_Realtime);
            if (!di.Exists)
                di.Create();
            return DownloadFile(url, PATH_Realtime);
        }

        public string GetTradingDay(int tradingDay)
        {
            string url = string.Format(BASE_URL + URL_TradingDay,tradingDay);
            DirectoryInfo di = new DirectoryInfo(PATH_TradingDay);
            if (!di.Exists)
                di.Create();
            return DownloadFile(url, PATH_TradingDay);
        }

        public string GetHistorical(string exchange, string product, string instrument, int tradingDay)
        {
            string url = string.Format(BASE_URL + URL_Historical, exchange, product, instrument, tradingDay);
            string newpath = Path.Combine(PATH_Historical, exchange, product, instrument);
            DirectoryInfo di = new DirectoryInfo(newpath);
            if (!di.Exists)
                di.Create();
            return DownloadFile(url, newpath);
        }

        protected string DownloadFile(string url,string local_path)
        {
            string file_fullname = "";
            string target = "";

            // 由于不知道本地文件名是多少，没法进行断点继传
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            NetworkCredential nc = new NetworkCredential(USERNAME, PASSWORD);
            req.Credentials = nc;
            
            using (HttpWebResponse wr = (HttpWebResponse)req.GetResponse())
            {
                string desc = wr.Headers["Content-Disposition"];
                if (desc != null)
                {
                    string fstr = "filename=";
                    int pos1 = desc.IndexOf(fstr);
                    if (pos1 > 0)
                    {
                        string fn = desc.Substring(pos1 + fstr.Length);
                        if (fn != "")
                            target = fn;
                    }
                }
                using (Stream stream = wr.GetResponseStream())
                {
                    file_fullname = Path.Combine(local_path, target);
                    //文件流，流信息读到文件流中，读完关闭
                    using (FileStream fs = File.Create(file_fullname))
                    {
                        //建立字节组，并设置它的大小是多少字节
                        byte[] bytes = new byte[10240];
                        int n = 0;
                        do
                        {
                            //一次从流中读多少字节，并把值赋给Ｎ，当读完后，Ｎ为０,并退出循环
                            n = stream.Read(bytes, 0, 1024);
                            fs.Write(bytes, 0, n);　//将指定字节的流信息写入文件流中
                        } while (n > 0);
                    }
                    File.SetLastWriteTime(file_fullname, wr.LastModified);
                }
            }
            return file_fullname;
        }
    }
}
