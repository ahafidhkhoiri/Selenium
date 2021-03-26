using CygnusAutomation.Cygnus.Models;
using Ganss.Excel;
using System;
using System.Collections.Generic;

namespace CygnusAutomation.Cygnus.CygnusUtils.BulkyData
{
    public static class DataBulkDelegate
    {
        public static Dictionary<string, string> lastDataInExcel = new Dictionary<string, string>();

        public static Dictionary<string, string> BulkDelegateTestData(CygnusAutomationModel loadConfig, string bulkPath, bool testFailedCase)
        {
            List<DelegateModel> userData = new List<DelegateModel>();

            List<DelegateModel> getUserFromDb = CygnusBulkUtils.GetExistingDelegate(loadConfig, false, null);

            if (testFailedCase)
            {
                string invalidUserEmail = $"Invalid_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
                string invalidDelegateEmail = $"InvalidDelegate_{DateTime.Now.ToString("HHmmss")}";

                userData.Add(
                    new DelegateModel { EmailAddress = invalidUserEmail, DelegateEmailAddress = invalidDelegateEmail }
                );
            }
            else
            {
                foreach (DelegateModel dm in getUserFromDb)
                {
                    userData.Add(
                        new DelegateModel { EmailAddress = dm.EmailAddress, DelegateEmailAddress = dm.DelegateEmailAddress }
                    );

                    lastDataInExcel["email"] = dm.EmailAddress;
                    lastDataInExcel["delegateEmail"] = dm.DelegateEmailAddress;
                }
            }

            new ExcelMapper().Save(bulkPath, userData, "Delegate");

            return lastDataInExcel;
        }
    }
}