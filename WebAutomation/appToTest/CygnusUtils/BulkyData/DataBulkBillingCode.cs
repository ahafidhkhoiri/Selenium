using CygnusAutomation.Cygnus.Models;
using Ganss.Excel;
using System;
using System.Collections.Generic;

namespace CygnusAutomation.Cygnus.CygnusUtils.BulkyData
{
    public static class DataBulkBillingCode
    {
        public static Dictionary<string, string> lastDataInExcel = new Dictionary<string, string>();

        public static Dictionary<string, string> BulkBillingCodeTestData(CygnusAutomationModel loadConfig, string bulkPath, bool testFailedCase)
        {
            string billingCodeFormat = DateTime.Now.ToString("HHmmss");

            List<BillingCodeModel> userData = new List<BillingCodeModel>();

            List<BillingCodeModel> getUserFromDb = CygnusBulkUtils.GetExistingUserBillingCode(loadConfig);

            if (testFailedCase)
            {
                string invalidUserEmail = $"Invalid_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
                string invalidAccessCode = DateTime.Now.ToString("HHmmss");

                userData.Add(
                    new BillingCodeModel { EmailAddress = invalidUserEmail, LeaderCode = invalidAccessCode, BillingCode = $"BC_{billingCodeFormat}" }
                );
            }
            else
            {
                int i = 0;
                foreach(BillingCodeModel bcm in getUserFromDb)
                {
                    string bc = $"BC_{++i}{billingCodeFormat}";
                    userData.Add(
                        new BillingCodeModel { EmailAddress = bcm.EmailAddress, LeaderCode = bcm.LeaderCode, BillingCode = bc}
                    );

                    lastDataInExcel["email"] = bcm.EmailAddress;
                    lastDataInExcel["bCode"] = bc;
                }
            }

            new ExcelMapper().Save(bulkPath, userData, "BillingCode");

            return lastDataInExcel;
        }
    }
}
