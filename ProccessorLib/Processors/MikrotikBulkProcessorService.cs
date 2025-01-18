using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Processors
{
   public class MikrotikBulkProcessorService
    {
        ServiceManager svrManager;
        public MikrotikBulkProcessorService()
        {
            svrManager = new ServiceManager();
            svrManager.MikrotikBulkProcessor();
        }

    }
}
