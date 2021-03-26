using CygnusAutomation.Cygnus.Models;
using Ganss.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CygnusAutomation.Cygnus.CygnusUtils.BulkyData
{
    public static class DataBulkUser
    {

        public static String lastDataInExcel = "";
        public static String BulkUserTestData(CygnusAutomationModel loadConfig, string bulkPath, bool testFailedCase)
        {
            string bulkEmailFormat = DateTime.Now.ToString("yyyyMMddHHmmss");
            string userLastNameFormat = DateTime.Now.ToString("HHmmss");
            string billingCodeFormat = DateTime.Now.ToString("HHmmss");

            List<UserModel> userData = new List<UserModel>();
            if (testFailedCase)
            {
                userData.Add(
                    new UserModel
                    {
                        FirstName = "Automation",
                        LastName = "usrInvalid" + userLastNameFormat,
                        EmailAddress = "auto.test.invalid" + bulkEmailFormat + "@loopup.co",
                        BusinessPhone = "14152112212",
                        MobilePhone = "14152001114",
                        BillingGroup = "test",
                        Office = "test",
                        DialInTemplate = "",
                        BillingCode = "bc_" + billingCodeFormat,
                        LeaderStartsConference = "0",
                        LeaderEndsConference = "0",
                        RollCall = "1",
                        LectureMode = "0",
                        AllowParticipantUnMute = "0",
                        JoinTones = "1",
                        AutoRecord = "1",
                        UfkUser = "ufk_au_" + bulkEmailFormat
                    }
                    );

            }
            else
            {
                //we'll just bulk provision 3 users.
                for (int i = 1; i < 4; i++)
                {
                    userData.Add(
                    new UserModel
                    {
                        FirstName = "Automation",
                        LastName = "usr" + i + userLastNameFormat,
                        EmailAddress = "auto.test." + i + bulkEmailFormat + "@loopup.co",
                        BusinessPhone = "14152112212",
                        MobilePhone = "14152001114",
                        BillingGroup = loadConfig.BillingGroup,
                        Office = loadConfig.OfficeName,
                        DialInTemplate = "",
                        BillingCode = "bc_" + i + billingCodeFormat,
                        LeaderStartsConference = "0",
                        LeaderEndsConference = "0",
                        RollCall = "1",
                        LectureMode = "0",
                        AllowParticipantUnMute = "0",
                        JoinTones = "1",
                        AutoRecord = "1",
                        UfkUser = "ufk_au_" + i + bulkEmailFormat
                    }
                    );

                    lastDataInExcel = "auto.test." + i + bulkEmailFormat + "@loopup.co";
                }
            }

            new ExcelMapper().Save(bulkPath, userData, "ProvisionUser");

            return lastDataInExcel;
        }
    }
}
