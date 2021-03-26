using CygnusAutomation.Cygnus.Models;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CygnusAutomation.Cygnus.CygnusUtils
{
    public static class CygnusBulkUtils
    {
        public static void BrowseFileToUpload(IWebDriver driver, string filePath)
        {
            driver.FindElement(By.ClassName(CygnusPages.CygnusPages.BULK_BROWSE_FILE), 5000).Click();
            driver.SwitchTo().ActiveElement().SendKeys(filePath);
            Utils.Pause(5000);
        }

        public static void BulkyProcess(IWebDriver driver, string filePath, bool addBulkItems, bool testFailedCase, string message)
        {
            BrowseFileToUpload(driver, filePath);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_UPLOAD_BTN), 5000).Click();
            Utils.Pause(10000);

            if (testFailedCase)
            {
                bool bulkFailedMessage = driver.FindElement(By.ClassName(CygnusPages.CygnusPages.BULK_FAILED_EXCEL_VALIDATION), 5000)
                    .Text.Contains(message);

                if (!bulkFailedMessage)
                {
                    throw new Exception("Failed to do bulky action.");
                }
                else
                {
                    Console.WriteLine(message);
                }
            }
            else
            {
                driver.FindElement(By.ClassName(CygnusPages.CygnusPages.BULK_PASSED_EXCEL_VALIDATION), 5000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_APPLY_BTN)).Click();

                //some bulky has confirmation popup (like add delegate)
                BulkConfirmationPopUp(driver);
                Utils.Pause(3000);
                RedirectToQueueMonitoringPage(driver, message);
            }
        }

        public static void BulkConfirmationPopUp(IWebDriver driver)
        {
            Utils.Pause(2000);
            try
            {
                bool isConfirmationPopUp = driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_CONFIRM_BUTTON)).Displayed;
                if (isConfirmationPopUp)
                {
                    driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_CONFIRM_BUTTON)).Click();
                }
                else
                {
                    Console.Write("not found");
                }
            }
            catch (Exception e)
            {
            }
        }

        public static void BulkMonitoringPage(IWebDriver driver, string lastEmail, bool salesforceAccount)
        {
            //Refresh the table by Clicking loading button beside the page title
            Utils.Pause(5000);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.REFRESH_BUTTON), 5000).Click();
            var queuedByDropDown = driver.FindElement(By.Id(CygnusPages.CygnusPages.QUEUED_BY_DROP_DOWN));
            
            queuedByDropDown.Click();
            queuedByDropDown.SendKeys(Keys.Down);
            queuedByDropDown.SendKeys(Keys.Enter);

            if (salesforceAccount)
            {
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.COMPLETED_TAB)).Click();
            }
            else
            {
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.FAILED_TAB)).Click();
            }
            Utils.Pause(2000);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_BAR_QUEUE)).Click();
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_BAR_QUEUE)).SendKeys(lastEmail);
        }

        public static string GetExistingEmailDomain(string dbConnectionString)
        {
            string emailDomain = "";
            SqlConnection cnn = new SqlConnection(dbConnectionString);
            cnn.Open();

            String sql = Queries.GetExistingEmailDomain();

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();
            while (dtReader.Read())
            {
                emailDomain = dtReader.GetValue(0).ToString();
            }
            cnn.Close();

            return emailDomain;
        }

        public static void BulkAccessCode(IWebDriver driver, string message)
        {
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.FIRST_USER_CHECKLIST)).Click();
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.EXPIRY_TIME_FIELD)).Click();
            Utils.Pause(3000);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.TODAY_OPTION)).Click();
            driver.FindElement(By.Id(CygnusPages.CygnusPages.RECYCLE_ACCESS_CODE_BUTTON)).Click();
            BulkConfirmationPopUp(driver);
            RedirectToQueueMonitoringPage(driver, message);
        }

        public static List<BillingCodeModel> GetExistingUserBillingCode(CygnusAutomationModel loadConfig)
        {
            List<BillingCodeModel> blkBillingCode = new List<BillingCodeModel>();

            SqlConnection cnn = new SqlConnection(loadConfig.DbConnectionString);
            cnn.Open();

            String sql = Queries.GetExistingUserBillingCode(loadConfig.AccountName);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();
            while (dtReader.Read())
            {
                BillingCodeModel bcm = new BillingCodeModel();

                bcm.EmailAddress = dtReader.GetValue(0).ToString();
                bcm.LeaderCode = dtReader.GetValue(1).ToString();
                bcm.BillingCode = dtReader.GetValue(2).ToString();
                blkBillingCode.Add(bcm);
            }
            cnn.Close();

            return blkBillingCode;
        }

        public static List<TerminateUserModel> GetExistingUser(CygnusAutomationModel loadConfig, bool getResult, string email)
        {
            List<TerminateUserModel> blkTerminateUser = new List<TerminateUserModel>();

            SqlConnection cnn = new SqlConnection(loadConfig.DbConnectionString);
            cnn.Open();

            String sql = "";

            if (getResult)
            {
                sql = Queries.GetTerminateUserResult(email);
            }
            else
            {
                sql = Queries.GetAvailableUserLoopupUser(loadConfig.AccountName);
            }

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();
            while (dtReader.Read())
            {
                TerminateUserModel tum = new TerminateUserModel();

                tum.EmailAddress = dtReader.GetValue(0).ToString();
                tum.UserStatus = Convert.ToInt32(dtReader.GetValue(1));
                blkTerminateUser.Add(tum);
            }
            cnn.Close();

            return blkTerminateUser;
        }

        public static List<DelegateModel> GetExistingDelegate(CygnusAutomationModel loadConfig, bool getResult, string email)
        {
            List<DelegateModel> delegateList = new List<DelegateModel>();

            SqlConnection cnn = new SqlConnection(loadConfig.DbConnectionString);
            cnn.Open();

            String sql = "";

            if (getResult)
            {
               sql = Queries.GetBulkDelegateResult(email);
            }
            else
            {
               sql = Queries.GetAvailableDelegateUser(loadConfig.AccountName);
            }

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();

            while (dtReader.Read())
            {
                DelegateModel dm = new DelegateModel();

                dm.EmailAddress = dtReader.GetValue(0).ToString();
                dm.DelegateEmailAddress = dtReader.GetValue(1).ToString();
                delegateList.Add(dm);
            }
            cnn.Close();

            return delegateList;
        }

        public static List<UserModel> CheckProvisionUser(CygnusAutomationModel loadConfig, string email)
        {
            List<UserModel> blkUser = new List<UserModel>();
            UserModel um = new UserModel();

            SqlConnection cnn = new SqlConnection(loadConfig.DbConnectionString);
            cnn.Open();

            String sql = Queries.CheckProvisionUser(email); ;

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();
            while (dtReader.Read())
            {
                um.FirstName = dtReader.GetValue(0).ToString();
                um.LastName = dtReader.GetValue(1).ToString();
                um.EmailAddress = dtReader.GetValue(2).ToString();
                um.BusinessPhone = dtReader.GetValue(3).ToString();
                um.MobilePhone = dtReader.GetValue(4).ToString();
                um.BillingGroup = dtReader.GetValue(5).ToString();
                um.Office = dtReader.GetValue(6).ToString();
                um.DialInTemplate = dtReader.GetValue(7).ToString();
                um.BillingCode = dtReader.GetValue(8).ToString();
                um.LeaderStartsConference = dtReader.GetValue(9).ToString();
                um.LeaderEndsConference = dtReader.GetValue(10).ToString();
                um.RollCall = dtReader.GetValue(11).ToString();
                um.LectureMode = dtReader.GetValue(12).ToString();
                um.AllowParticipantUnMute = dtReader.GetValue(13).ToString();
                um.JoinTones = dtReader.GetValue(14).ToString();
                um.AutoRecord = dtReader.GetValue(15).ToString();
                um.UfkUser = dtReader.GetValue(16).ToString();
                blkUser.Add(um);
            }
            cnn.Close();

            return blkUser;
        }

        public static List<ActivationEmailModel> GetEmailFromDb(CygnusAutomationModel loadConfig, bool getResult, string email)
        {
            List<ActivationEmailModel> blkActEmail = new List<ActivationEmailModel>();
            String sql = "";
            SqlConnection cnn = new SqlConnection(loadConfig.DbConnectionString);
            cnn.Open();

            if (getResult)
            {
                sql = Queries.GetLatestActivationEmail(email);
            }
            else
            {
                sql = Queries.GetExistingUserActivationEmail(loadConfig.AccountName);
            }

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();
            while (dtReader.Read())
            {
                ActivationEmailModel ae = new ActivationEmailModel();

                ae.EmailAddress = dtReader.GetValue(0).ToString();
                ae.LatestActivationEmail = dtReader.GetValue(1).ToString();
                blkActEmail.Add(ae);
            }
            cnn.Close();

            return blkActEmail;
        }

        public static List<AccessCodeModel> CheckAccessCode(CygnusAutomationModel loadConfig, string email)
        {
            List<AccessCodeModel> accessCodeList = new List<AccessCodeModel>();

            SqlConnection cnn = new SqlConnection(loadConfig.DbConnectionString);
            cnn.Open();

            String sql = Queries.GetAccessCode(email);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();

            while (dtReader.Read())
            {
                AccessCodeModel acm = new AccessCodeModel();

                acm.EmailAddress = dtReader.GetValue(0).ToString();
                acm.AccessCode = dtReader.GetValue(1).ToString();
                accessCodeList.Add(acm);
            }
            cnn.Close();

            return accessCodeList;
        }

        public static List<ExtendedProperty> CheckUserSettings(CygnusAutomationModel loadConfig, string email, string bulkUserSettingKey)
        {
            List<ExtendedProperty> userSettingsList = new List<ExtendedProperty>();

            SqlConnection cnn = new SqlConnection(loadConfig.DbConnectionString);
            cnn.Open();

            String sql = Queries.GetUpdateUserSettings(email, bulkUserSettingKey);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();

            while (dtReader.Read())
            {
                ExtendedProperty usm = new ExtendedProperty();

                usm.IntegerValue = dtReader.GetValue(0).ToString();
                usm.StringValue = dtReader.GetValue(1).ToString();
                userSettingsList.Add(usm);
            }
            cnn.Close();

            return userSettingsList;
        }

        public static void RedirectToQueueMonitoringPage(IWebDriver driver, String successMessage)
        {
            Utils.Pause(8000);
            if (driver.FindElement(By.LinkText(CygnusPages.CygnusPages.BULK_REDIRECT_TO_QUEUE_MONITOR), 5000).Displayed)
            {
                Console.WriteLine(successMessage);
            }
            Utils.Pause(2000);
            driver.FindElement(By.LinkText(CygnusPages.CygnusPages.BULK_REDIRECT_TO_QUEUE_MONITOR), 5000).Click();
        }
    }
}
