using QuantBox.Data.Serializer.V2;
using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveData
{
    public class Pd0DataReader
    {
        public QuantBox.Data.Serializer.PbTickSerializer Serializer = new QuantBox.Data.Serializer.PbTickSerializer();

        public List<PbTickView> ReadOneFile(FileInfo file)
        {
            Stream _stream = new MemoryStream();
            var fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            {
                try
                {
                    using (var zip = new SevenZipExtractor(fileStream))
                    {
                        zip.ExtractFile(0, _stream);
                        _stream.Seek(0, SeekOrigin.Begin);
                    }
                }
                catch (Exception ex)
                {
                    _stream = fileStream;
                    try
                    {
                        // 有部分文件会出现zip的密码错误，然后把流关闭了，所以需要重新打开一次
                        // 如果是写部分不会出问题，读的部分可能出问题
                        _stream.Seek(0, SeekOrigin.Begin);
                    }
                    catch(Exception ex2)
                    {
                        _stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
                }
            }
            return Serializer.Read2View(_stream);
        }
    }
}
