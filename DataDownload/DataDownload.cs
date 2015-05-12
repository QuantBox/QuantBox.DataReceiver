using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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

        private string _USERNAME;
        public string USERNAME
        {
            get { return _USERNAME; }
            set {
                _USERNAME = value;

                Cookies = (Dictionary<string, Cookie>)Load(_USERNAME, Cookies);
                foreach (var cook in Cookies.Values)
                {
                    CookieContainer.Add(cook);
                }
            }
        }
        public string PASSWORD { get; set; }

        private CookieContainer CookieContainer = new CookieContainer();
        private Dictionary<string, Cookie> Cookies = new Dictionary<string, Cookie>();

        public Tuple<int, string> GetRealtime(string exchange, string instrument)
        {
            string url = string.Format(BASE_URL + URL_Realtime, HttpUtility.UrlEncode(exchange), HttpUtility.UrlEncode(instrument));
            return DownloadFile(url, PATH_Realtime);
        }

        public Tuple<int, string> GetTradingDay(int tradingDay)
        {
            string url = string.Format(BASE_URL + URL_TradingDay,tradingDay);
            return DownloadFile(url, PATH_TradingDay);
        }

        public Tuple<int, string> GetHistorical(string exchange, string product, string instrument, int tradingDay)
        {
            string url = string.Format(BASE_URL + URL_Historical, HttpUtility.UrlEncode(exchange), HttpUtility.UrlEncode(product), HttpUtility.UrlEncode(instrument), tradingDay);
            string newpath = Path.Combine(PATH_Historical, exchange, product, instrument);
            return DownloadFile(url, newpath);
        }

        public Tuple<int, string> GetHistorical(string exchange, string product, string instrument, DateTime tradingDay)
        {
            int date = tradingDay.Year * 10000 + tradingDay.Month * 100 + tradingDay.Day;
            return GetHistorical(exchange, product, instrument, date);
        }

        public List<Tuple<int,string>> GetHistorical(string exchange, string product, string instrument, int datatime1,int datatime2)
        {
            List<Tuple<int, string>> file_paths = new List<Tuple<int, string>>();

            DateTime _datetime1 = new DateTime(datatime1 / 10000, datatime1 % 10000 / 100, datatime1 % 100);
            DateTime _datatime2 = new DateTime(datatime2 / 10000, datatime2 % 10000 / 100, datatime2 % 100);

            for (DateTime datetime = _datetime1; datetime <= _datatime2; datetime = datetime.AddDays(1))
            {
                if (datetime.DayOfWeek == DayOfWeek.Saturday || datetime.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                file_paths.Add(GetHistorical(exchange, product, instrument, datetime));
            }

            return file_paths;
        }

        #region Cookie写入
        private static JsonSerializerSettings jSetting = new JsonSerializerSettings()
        {
            // json文件格式使用非紧凑模式
            //NullValueHandling = NullValueHandling.Ignore,
            //DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        protected void Save(string file, object obj)
        {
            IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
            using (FileStream fs = myIsolatedStorage.OpenFile(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (TextWriter writer = new StreamWriter(fs))
                {
                    writer.Write("{0}", JsonConvert.SerializeObject(obj, obj.GetType(), jSetting));
                    writer.Close();
                }
            }
        }
        protected object Load(string file, object obj)
        {
            try
            {
                object ret;
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
                using (FileStream fs = myIsolatedStorage.OpenFile(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (TextReader reader = new StreamReader(fs))
                    {
                        ret = JsonConvert.DeserializeObject(reader.ReadToEnd(), obj.GetType());
                        reader.Close();
                    }
                }

                return ret;
            }
            catch
            {
            }
            return obj;
        }

        private void SaveCookie(HttpWebResponse rsp)
        {
            if (rsp.Headers["Set-Cookie"] != null)
            {
                foreach (Cookie cook in rsp.Cookies)
                {
                    Cookies[cook.Name] = cook;
                    Save(_USERNAME, Cookies);

                    //Console.WriteLine("Cookie:");
                    //Console.WriteLine("{0} = {1}", cook.Name, cook.Value);
                    //Console.WriteLine("Domain: {0}", cook.Domain);
                    //Console.WriteLine("Path: {0}", cook.Path);
                    //Console.WriteLine("Port: {0}", cook.Port);
                    //Console.WriteLine("Secure: {0}", cook.Secure);

                    //Console.WriteLine("When issued: {0}", cook.TimeStamp);
                    //Console.WriteLine("Expires: {0} (expired? {1})",
                    //    cook.Expires, cook.Expired);
                    //Console.WriteLine("Don't save: {0}", cook.Discard);
                    //Console.WriteLine("Comment: {0}", cook.Comment);
                    //Console.WriteLine("Uri for comments: {0}", cook.CommentUri);
                    //Console.WriteLine("Version: RFC {0}", cook.Version == 1 ? "2109" : "2965");

                    //// Show the string representation of the cookie.
                    //Console.WriteLine("String: {0}", cook.ToString());
                }
            }
        }
        #endregion

        protected Tuple<int,string> DownloadFile(string url, string local_path)
        {
            var httpStatusCode = 200;
            string file_fullname = "";
            string target = "";

            // 由于不知道本地文件名是多少，没法进行断点继传
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            NetworkCredential nc = new NetworkCredential(USERNAME, PASSWORD);
            req.Credentials = nc;
            req.CookieContainer = CookieContainer;

            try
            {
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
                        DirectoryInfo di = new DirectoryInfo(local_path);
                        if (!di.Exists)
                            di.Create();

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

                    SaveCookie(wr);
                }
            }
            catch (WebException ex)
            {
                var rsp = ex.Response as HttpWebResponse;
                httpStatusCode = (int)rsp.StatusCode;
                file_fullname = string.Format("{0} - {1}", ex.Message, url);

                SaveCookie(rsp);
            }
            catch (Exception ex)
            {
                httpStatusCode = 0;
                file_fullname = string.Format("{0} - {1}", ex.Message, url);
            }

            return new Tuple<int,string>(httpStatusCode,file_fullname);
        }

    }
}
