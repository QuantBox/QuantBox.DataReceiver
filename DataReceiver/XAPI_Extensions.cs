using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XAPI.Callback;

namespace DataReceiver
{
    public static class XAPI_Extensions
    {
        public static Logger GetLog(this XApi api)
        {
            return (api.Log as Logger);
        }
    }
}
