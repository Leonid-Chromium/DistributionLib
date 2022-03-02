using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionLib
{
    public class Analog1
    {
        public static DataTable MainFun(DataTable historyDT, DataTable InDT, DateTime maxDateTime)
        {
            DataTable OutDT = new DataTable("Output data table");
            try
            {

                return OutDT;
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            return OutDT;
        }
    }
}
