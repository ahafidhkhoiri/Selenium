using CygnusAutomation.Cygnus.Models;
using Ganss.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CygnusAutomation.Cygnus.CygnusUtils.BulkyData
{
    public static class DataBulkDialinNumber
    {
        public static void BulkDialinNumberData(CygnusAutomationModel loadConfig, string bulkPath, bool testFailedCase)
        {
            string number = DateTime.UtcNow.Ticks.ToString().Substring(9);

            List<DnrModel> dnrData = new List<DnrModel>();
            if (testFailedCase)
            {
                dnrData.Add(
                    new DnrModel
                    {
                        CountryIso = "IDN",
                        DiscUrn = "Local",
                        IvrLocale = "id",
                        CountryLabel = "IndonesiaTest",
                        Carrier = "test",
                        DnrE164 = "628" + number,
                        DisplayName = "Test Indonesia(Local no)",
                        DisplayNumber = "628" + number,
                        MediaServer = loadConfig.MediaServer,
                        Dnis = "dnis" + number,
                        MobileDnis = "mobileDnis" + number,
                        CustomAllocation = "Custom Allocation test",
                        Notes = "Notes test"
                    }
                    );

            }
            else
            {
                //we'll just bulk provision 2 dial-in numbers.
                for (int i = 1; i < 3; i++)
                {
                    dnrData.Add(
                    new DnrModel
                    {
                        CountryIso = "IDN",
                        DiscUrn = "Local",
                        IvrLocale = "id",
                        CountryLabel = "IndonesiaTest" + i,
                        Carrier = loadConfig.Carrier,
                        DnrE164 = "628" + i + number,
                        DisplayName = "Test Indonesia(Local no)" + i,
                        DisplayNumber = "628" + i + number,
                        MediaServer = loadConfig.MediaServer,
                        Dnis = "dnis" + i + number,
                        MobileDnis = "mobileDnis" + i + number,
                        CustomAllocation = "Custom Allocation test" + i,
                        Notes = "Notes test" + i
                    }
                    );

                }
            }

            new ExcelMapper().Save(bulkPath, dnrData, "DialinNumber");
        }
    }
}
