using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArchiveData
{
    //"D:\Program Files\7-Zip\7z.exe" a -t7z "D:\test\Data_TradingDay\20150326.7z" "D:\test\Data_TradingDay\20150326\*" -m0=PPMd -mx=9
    //"D:\Program Files\7-Zip\7z.exe" a -t7z "D:\test\Data_TradingDay\20150326.7z" "D:\test\Data_TradingDay\20150326\*" -m0=PPMd -mx=9

    class PathHelper
    {

        public static bool SplitFileName(string fileName, out string exchange, out string product,
            out string instrument, out string date)
        {
            exchange = string.Empty;
            product = string.Empty;
            instrument = string.Empty;
            date = string.Empty;
            // IF1503.CFFEX_20150430
            // IF1503._20150430
            // IO1506-C-2768._20150430
            // SR503C1507.CZCE_20150430
            // 600000.SSE_20150506
            // SPC a1506&b1507._20150501
            Regex regex1 = new Regex(@"([A-za-z0-9\-& ]*)\.([A-za-z]*)_(\d{4,8})");
            Match match1 = regex1.Match(fileName);
            if (match1.Success)
            {
                instrument = match1.Groups[1].ToString();
                exchange = match1.Groups[2].ToString();
                date = match1.Groups[3].ToString();
                
                if (instrument.IndexOf("&") > -1)
                {
                    // SPC a1506&b1507
                    char[] arr = instrument.ToCharArray();
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (!(arr[i] >= '0' && arr[i] <= '9'))
                        {
                            product += arr[i];
                        }
                    }
                    return true;
                }
                else
                {
                    // IF1503
                    // SR503
                    // IO1503-C-2500
                    // SR503C500
                    // 600000
                    Regex regex2 = new Regex(@"([A-za-z]*)([0-9]+)([-CP]?)");
                    Match match2 = regex2.Match(instrument);
                    if (match2.Success)
                    {
                        if (string.IsNullOrEmpty(match2.Groups[3].ToString()))
                        {
                            product = match2.Groups[1].ToString();
                        }
                        else
                        {
                            product = match2.Groups[1].ToString() + match2.Groups[2].ToString();
                        }
                        return true;
                    }
                }
            }
            return false;
        }


        public static string SevenZipDirectory(string exe, string target, string directory)
        {
            // 注意当7z中已经有文件时是添加

            //"D:\Program Files\7-Zip\7z.exe" a -t7z "D:\test\Data_TradingDay\20150326.7z" "D:\test\Data_TradingDay\20150326\*" -m0=PPMd -mx=9
            string cmd = string.Format("a -t7z \"{0}\" \"{1}\\*\" -m0=PPMd -mx=9",target, directory);
            var proc = Process.Start(exe,cmd);
            proc.WaitForExit();
            return cmd;
        }

        public static string SevenZipFile(string exe, string target, string source)
        {
            //"D:\Program Files\7-Zip\7z.exe" a -t7z "D:\test\Data_TradingDay\20150326.7z" "D:\test\Data_TradingDay\20150326\*" -m0=PPMd -mx=9
            string cmd = string.Format("a -t7z \"{0}\" \"{1}\" -m0=PPMd -mx=9", target, source);
            ProcessStartInfo ps = new ProcessStartInfo(exe);
            ps.UseShellExecute = false;
            ps.CreateNoWindow = true;
            ps.Arguments = cmd;
            var proc = Process.Start(ps);
            proc.WaitForExit();
            return cmd;
        }
    }
}
