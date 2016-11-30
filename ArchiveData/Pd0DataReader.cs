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
                    _stream.Seek(0, SeekOrigin.Begin);
                }
            }
            return Serializer.Read2View(_stream);
        }
    }
}
