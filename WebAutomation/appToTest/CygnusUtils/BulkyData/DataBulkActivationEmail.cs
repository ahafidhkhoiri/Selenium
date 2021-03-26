using CygnusAutomation.Cygnus.Models;
using Ganss.Excel;
using System;
using System.Collections.Generic;

namespace CygnusAutomation.Cygnus.CygnusUtils.BulkyData
{
    public static class DataBulkActivationEmail
    {
        public static Dictionary<string, string> lastDataInExcel = new Dictionary<string, string>();

        public static Dictionary<string, string> BulkActivationEmailTestData(CygnusAutomationModel loadConfig, string bulkPath, bool testFailedCase)
        {

            List<ActivationEmailModel> userData = new List<ActivationEmailModel>();

            List<ActivationEmailModel> getUserFromDb = CygnusBulkUtils.GetEmailFromDb(loadConfig, false, null);

            if (testFailedCase)
            {
                string invalidUserEmail = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                userData.Add(
                    new ActivationEmailModel { EmailAddress = "testInvalidUser" + invalidUserEmail + "@loopup.co" }
                );
            }
            else
            {
                foreach (ActivationEmailModel aem in getUserFromDb)
                {
                    userData.Add(
                        new ActivationEmailModel { EmailAddress = aem.EmailAddress, LatestActivationEmail = aem.LatestActivationEmail }
                    );

                    lastDataInExcel["email"] = aem.EmailAddress;
                }
            }

            new ExcelMapper().Save(bulkPath, userData, "ActivationEmail");

            return lastDataInExcel;
        }
    }
}