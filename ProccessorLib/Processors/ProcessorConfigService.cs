using BillingERPConn;
using ProccessorLib.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Processors
{
    public class ProcessorConfigService
    {
        public static ProcessorConfig GetConfig(int id)
        {
            DBUtility _idb = new DBUtility();
            ProcessorConfig config = new ProcessorConfig();
            Hashtable ht = new Hashtable { { "ID", id } };
            DataTable dt = _idb.GetDataByProc(ht, "sp_getProcessConfigInfo");
            if (dt.Rows.Count > 0)
            {
                config.StartHour = int.Parse(dt.Rows[0]["StartHour"].ToString());
                config.endHour = int.Parse(dt.Rows[0]["EndHour"].ToString());
                config.Interval = int.Parse(dt.Rows[0]["Interval"].ToString()); // Should be Milisecond
                config.StartMin = int.Parse(dt.Rows[0]["StartMin"].ToString());
                return config;
            }
            return null;
        }
    }
}
