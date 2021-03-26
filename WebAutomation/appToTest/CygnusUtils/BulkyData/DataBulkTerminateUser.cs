using CygnusAutomation.Cygnus.Models;
using Ganss.Excel;
using System;
using System.Collections.Generic;

namespace CygnusAutomation.Cygnus.CygnusUtils.BulkyData
{
    public static class DataBulkTerminateUser
    {
        public static Dictionary<string, string> lastDataInExcel = new Dictionary<string, string>();

        public static Dictionary<string, string> BulkTerminateUserTestData(CygnusAutomationModel loadConfig, string bulkPath, bool testFailedCase)
        {
            string billingCodeFormat = DateTime.Now.ToString("HHmmss");

            List<TerminateUserModel> userData = new List<TerminateUserModel>();

            List<TerminateUserModel> getUserFromDb = CygnusBulkUtils.GetExistingUser(loadConfig, false, null);

            if (testFailedCase)
            {
                string invalidUserEmail = $"Invalid_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";

                userData.Add(new TerminateUserModel { EmailAddress = invalidUserEmail });
            }
            else
            {
                foreach(TerminateUserModel tum in getUserFromDb)
                {
                    userData.Add(new TerminateUserModel { EmailAddress = tum.EmailAddress });

                    lastDataInExcel["email"] = tum.EmailAddress;
                }
            }

            new ExcelMapper().Save(bulkPath, userData, "TerminateUser");

            return lastDataInExcel;
        }
    }
}
