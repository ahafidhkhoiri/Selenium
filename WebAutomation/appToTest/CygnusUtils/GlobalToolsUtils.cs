using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using CygnusAutomation.Cygnus.CygnusUtils;
using CygnusAutomation.Cygnus.Models;
using OpenQA.Selenium.Interactions;
using System.Reflection;
using System.Data.SqlClient;
using NUnit.Framework;
using System.Data;

namespace CygnusAutomation.Cygnus.CygnusUtils
{
    public static class GlobalToolsUtils
    {
        public static void AddDnr(IWebDriver driver, DnrModel dialInNumber)
        {
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_BUTTON)).Click();
            Utils.Pause(5000);

            var dialInCategory = new SelectElement(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_CATEGORY_DROPDOWN)));
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_CATEGORY_DROPDOWN)).Click();
            dialInCategory.SelectByText(dialInNumber.DialInCategory);

            var carrier = new SelectElement(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CARRIER_DROPDOWN)));
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CARRIER_DROPDOWN)).Click();
            carrier.SelectByText(dialInNumber.Carrier);

            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DNRE164_FIELD)).SendKeys(dialInNumber.DnrE164);
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DISPLAY_LABEL_FIELD)).SendKeys(dialInNumber.DisplayName);

            var mediaServer = new SelectElement(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.MEDIA_SERVER_DROPDOWN)));
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.MEDIA_SERVER_DROPDOWN)).Click();
            mediaServer.SelectByText(dialInNumber.MediaServer);


            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DNIS_FIELD)).SendKeys(dialInNumber.Dnis);
            driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.APPLY_MOBILE_DNIS)).Click();
            Utils.Pause(3000);
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.MOBILE_DNIS_FIELD)).SendKeys(dialInNumber.MobileDnis);
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CUSTOM_ALLOCATION_FIELD)).SendKeys(dialInNumber.CustomAllocation);
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.NOTES_FIELD)).SendKeys(dialInNumber.Notes);
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();

            Utils.Pause(10000);
        }

        public static List<DnrModel> AddEditDialInNumberSets(IWebDriver driver, DialInNumberSetsModel dialInNumberSets, CygnusAutomationModel loadConfig, bool isTemplateSelected, bool isEdited, bool isDnrActive, bool isDnrSetException)
        {
            List<DnrModel> ufkDnrs = new List<DnrModel>();

            Utils.Pause(5000);
            driver.FindElement(By.Name(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_NAME)).Clear();
            driver.FindElement(By.Name(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_NAME)).SendKeys(dialInNumberSets.DialInNumberSetName);

            if (!isEdited && !isDnrSetException)
            {
                var applicationServer = new SelectElement(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.APPLICATION_SERVER_DROPDOWN)));
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.APPLICATION_SERVER_DROPDOWN)).Click();
                applicationServer.SelectByText(dialInNumberSets.ApplicationServer);

                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SET_APP_SERVERS_BUTTON)).Click();
            }

            if (!isEdited && isDnrSetException)
            {
                var parentDnrSet = new SelectElement(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.MASTER_DIAL_IN_NUMBER_SET_DROPDOWN)));
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.MASTER_DIAL_IN_NUMBER_SET_DROPDOWN)).Click();
                Utils.Pause(3000);
                string dnrgName = dialInNumberSets.DialInNumberSetName;
                string parentDnrg = dnrgName.Substring(dnrgName.LastIndexOf(':') + 1);
                parentDnrSet.SelectByText(parentDnrg);

                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CREATE_NUMBER_SET_EXCEPTION_BUTTON)).Click();
                Utils.Pause(5000);
            }

            if (!isTemplateSelected)
            {
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(3000);

                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CONFIRMATION_MESSAGE))
                    .Text.Contains("Please select at least one dial-in number"));
            }
            else
            {
                string dnrs = "";
                if (isDnrSetException)
                {
                    string _dnrgName = dialInNumberSets.DialInNumberSetName;
                    string dnrgName = _dnrgName.Substring(_dnrgName.LastIndexOf(':') + 1);
                    List<DialInNumberSetsModel> dnrg = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dnrgName);

                    // Now we wants to exclude dnrs from parent dnrg and add it to its exception
                    List<DnrModel> dnrsList = dnrg[0].DialInNumbers;
                    List<string> usedUfkDnrsList = new List<string>();
                    foreach (var tmp in dnrsList)
                    {
                        usedUfkDnrsList.Add(tmp.UfkDnr);
                    }

                    dnrs = "'" + String.Join("','", usedUfkDnrsList) + "'";
                    
                }
                ufkDnrs = GetUfkDnrByAppServer(loadConfig, isEdited, isDnrActive, isDnrSetException, dnrs);

                foreach (var ufk in ufkDnrs)
                {
                    driver.FindElement(By.XPath($"//option[@value='{ufk.UfkDnr}']")).Click();
                    Utils.Pause(1000);
                }

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(8000);
            }
            return ufkDnrs;
        }

        public static void AddEditTemplate(IWebDriver driver, string templateName, bool isEdited)
        {
            if (!isEdited)
            {
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION)).Click();
                Utils.Pause(5000);

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_FIELD)).SendKeys(templateName);

                var primaryNumber = new SelectElement(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PRIMARY_NUMBER_DROPDOWN)));
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PRIMARY_NUMBER_DROPDOWN)).Click();
                primaryNumber.SelectByText("Indonesia (Toll-Free)");

                var secondaryNumber = new SelectElement(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SECONDARY_NUMBER_DROPDOWN)));
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SECONDARY_NUMBER_DROPDOWN)).Click();
                secondaryNumber.SelectByText("United States (Toll)");
            }
            else
            {
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_FIELD)).Clear();
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_FIELD)).SendKeys(templateName + "updated");
            }
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.AVAILABLE_TOLL_CHECKBOX)).Click();
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.AVAILABLE_TOLL_FREE_CHECKBOX)).Click();
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DEFAULT_TOLL_CHECKBOX)).Click();
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DEFAULT_TOLL_FREE_CHECKBOX)).Click();
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
            Utils.Pause(10000);
        }

        public static void AddEditAccountTemplate(IWebDriver driver, string accountTemplateName, bool isEdited)
        {
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ACCOUNT_TEMPLATE_NAME_FIELD)).Clear();
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ACCOUNT_TEMPLATE_NAME_FIELD)).SendKeys(accountTemplateName);

            if (isEdited)
            {
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_UK_TOLL_FREE)).Click(); 
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SET_PRIMARY_NUMBER_UK_TOLL_FREE)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_IDN_TOLL_FREE)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SET_SECONDARY_NUMBER_IDN_TOLL_FREE)).Click();
                Utils.Pause(2000);
            }
            else
            {
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_IDN_TOLL_FREE)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SET_PRIMARY_NUMBER_IDN_TOLL_FREE)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_UK_TOLL_FREE)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SET_SECONDARY_NUMBER_UK_TOLL_FREE)).Click();
                Utils.Pause(2000);
            }
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
            Utils.Pause(5000);
        }

        public static List<DialInTemplateModel> CheckTemplate(CygnusAutomationModel loadConfig, string templateNameOrUfkTemplate, bool checkAllSelection)
        {
            List<DialInTemplateModel> templateList = new List<DialInTemplateModel>();

            SqlConnection cnn = new SqlConnection(loadConfig.DnrDbConnectionString);
            cnn.Open();

            String sql = Queries.GetTemplate(templateNameOrUfkTemplate, checkAllSelection);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();

            while (dtReader.Read())
            {
                DialInTemplateModel tm = new DialInTemplateModel();

                tm.DialInTemplateName = dtReader.GetValue(0).ToString();
                tm.DiscUrn = dtReader.GetValue(1).ToString();
                tm.Type = dtReader.GetValue(2).ToString();
                tm.UfkTemplate = dtReader.GetValue(3).ToString();
                templateList.Add(tm);
            }
            cnn.Close();

            return templateList;
        }

        public static List<DnrModel> GetUfkDnrByAppServer(CygnusAutomationModel loadConfig, bool isEdited,
            bool isDnrActive, bool isDnrSetException, string dnrs)
        {
            List<DnrModel> ufkDnr = new List<DnrModel>();
            SqlConnection cnn = new SqlConnection(loadConfig.DbConnectionString);
            cnn.Open();

            string sql = Queries.GetActiveUfkDnrByAppServer(loadConfig.ApplicationServer, isEdited, isDnrActive, isDnrSetException, dnrs);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();
            while (dtReader.Read())
            {
                DnrModel dm = new DnrModel();

                dm.UfkDnr = dtReader.GetValue(0).ToString();
                dm.DisplayName = dtReader.GetValue(1).ToString();
                ufkDnr.Add(dm);
            }
            cnn.Close();

            return ufkDnr;
        }

        public static void BrowseFileToUpload(IWebDriver driver, string filePath)
        {
            driver.FindElement(By.ClassName(CygnusPages.GlobalToolsPages.BULK_BROWSE_FILE), 5000).Click();
            driver.SwitchTo().ActiveElement().SendKeys(filePath);
            Utils.Pause(5000);
        }

        public static void BulkConfirmationPopUp(IWebDriver driver)
        {
            Utils.Pause(2000);
            try
            {
                bool isConfirmationPopUp = driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.BULK_CONFIRM_BUTTON)).Displayed;
                if (isConfirmationPopUp)
                {
                    driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.BULK_CONFIRM_BUTTON)).Click();
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

        public static void DnrBulkyProcess(IWebDriver driver, string filePath, bool testFailedCase, string message)
        {
            BrowseFileToUpload(driver, filePath);
            driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.BULK_UPLOAD_BTN), 5000).Click();
            Utils.Pause(10000);

            if (testFailedCase)
            {
                bool bulkFailedMessage = driver.FindElement(By.ClassName(CygnusPages.GlobalToolsPages.BULK_FAILED_EXCEL_VALIDATION), 5000)
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
                driver.FindElement(By.ClassName(CygnusPages.GlobalToolsPages.BULK_PASSED_EXCEL_VALIDATION), 5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.BULK_APPLY_BTN)).Click();

                //some bulky has confirmation popup (like add delegate)
                BulkConfirmationPopUp(driver);
                Utils.Pause(10000);
            }
        }

        public static List<DnrModel> CheckDnr(CygnusAutomationModel loadConfig, string dnis)
        {
            List<DnrModel> dnrList = new List<DnrModel>();

            SqlConnection cnn = new SqlConnection(loadConfig.DnrDbConnectionString);
            cnn.Open();

            String sql = Queries.GetDnr(dnis);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();

            while (dtReader.Read())
            {
                DnrModel dm = new DnrModel();

                dm.UfkDnr = dtReader.GetValue(0).ToString();
                dm.DnrE164 = dtReader.GetValue(1).ToString();
                dm.DisplayNumber = dtReader.GetValue(2).ToString();
                dm.DisplayName = dtReader.GetValue(3).ToString();
                dm.DiscUrn = dtReader.GetValue(4).ToString();
                dm.CustomAllocation = dtReader.GetValue(5).ToString();
                dm.IsActive = Convert.ToInt32(dtReader.GetValue(6));
                dm.Notes = dtReader.GetValue(7).ToString();
                dm.MediaServer = dtReader.GetValue(8).ToString();
                dm.Carrier = dtReader.GetValue(9).ToString();
                dm.Dnis = dtReader.GetValue(10).ToString();
                dm.PartnerUri = dtReader.GetValue(11).ToString();
                dnrList.Add(dm);
            }
            cnn.Close();

            return dnrList;
        }

        public static List<DialInNumberSetsModel> CheckDialInNumberSetsByNameOrByUfkDnrg(CygnusAutomationModel loadConfig, string dialInNumberSetNameOrUfkDnrg)
        {
            List<DialInNumberSetsModel> dnrgList = new List<DialInNumberSetsModel>();
            List<DnrModel> dnrsList = new List<DnrModel>();

            SqlConnection cnn = new SqlConnection(loadConfig.DnrDbConnectionString);
            cnn.Open();

            String sql = Queries.GetDialInNumberSetsAndItsDnrs(dialInNumberSetNameOrUfkDnrg);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();

            DialInNumberSetsModel dnsm = new DialInNumberSetsModel();

            while (dtReader.Read())
            {
                DnrModel dm = new DnrModel();

                dnsm.DialInNumberSetName = dtReader.GetValue(0).ToString();
                dm.UfkDnr = dtReader.GetValue(1).ToString();
                dnrsList.Add(dm);
                dnsm.UfkDnrg = dtReader.GetValue(2).ToString();
            }

            if (!string.IsNullOrWhiteSpace(dnsm.DialInNumberSetName))
            {
                dnrgList.Add(new DialInNumberSetsModel() { DialInNumberSetName = dnsm.DialInNumberSetName, DialInNumbers = dnrsList, UfkDnrg = dnsm.UfkDnrg });
            }

            cnn.Close();

            return dnrgList;
        }

        public static List<string> CheckUfkDnrsOfDnrSet(CygnusAutomationModel loadConfig, string ufkDnrg)
        {
            List<string> ufkDnrsList = new List<string>();

            var connection = new SqlConnection(loadConfig.DbConnectionString);
            var parameters = new SqlParameter[]
            {
            new SqlParameter("@PARTNER_URI", loadConfig.PartnerUri),
            new SqlParameter("@UFK_DNRG", ufkDnrg),
            new SqlParameter("@INCLUDE_DNR", 1)
            };
            var dataSet = GetDataSet(connection, "antares_dnr.dbo.[stp_DNRG_Find]", parameters);

            var firstTable = dataSet?.Tables?[1];

            foreach (DataRow dr in firstTable.Rows)
            {
                string ufkDnr = dr["UFK_DNR"].ToString();
                ufkDnrsList.Add(ufkDnr);
            }

            return ufkDnrsList;

        }

        public static DataSet GetDataSet(SqlConnection connection, string storedProcName, params SqlParameter[] parameters)
        {
            var command = new SqlCommand(storedProcName, connection) { CommandType = CommandType.StoredProcedure };
            command.Parameters.AddRange(parameters);

            var result = new DataSet();
            var dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(result);

            return result;
        }

        public static List<AccountDialInTemplateModel> GetExistingEmailAddress(CygnusAutomationModel loadConfig)
        {
            List<AccountDialInTemplateModel> email = new List<AccountDialInTemplateModel>();

            SqlConnection cnn = new SqlConnection(loadConfig.DbConnectionString);
            cnn.Open();

            String sql = Queries.GetExistingEmailAndUfkUser(loadConfig.AccountName);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();

            while (dtReader.Read())
            {
                AccountDialInTemplateModel tm = new AccountDialInTemplateModel();

                tm.EmailAddress = dtReader.GetValue(0).ToString();
                tm.UfkUser = dtReader.GetValue(1).ToString();
                email.Add(tm);

            }
            cnn.Close();

            return email;
        }

        public static List<AccountDialInTemplateModel> CheckAccountDialInTemplates(CygnusAutomationModel loadConfig, string accountTemplateName)
        {
            List<AccountDialInTemplateModel> tagList = new List<AccountDialInTemplateModel>();

            SqlConnection cnn = new SqlConnection(loadConfig.DnrDbConnectionString);
            cnn.Open();

            String sql = Queries.GetAccountDialInTemplates(accountTemplateName);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader();

            while (dtReader.Read())
            {
                AccountDialInTemplateModel tm = new AccountDialInTemplateModel();

                tm.UfkTag = dtReader.GetValue(0).ToString();
                tm.GroupDisplayName = dtReader.GetValue(1).ToString();
                tm.DiscUrn = dtReader.GetValue(2).ToString();
                tm.Type = dtReader.GetValue(3).ToString();
                tm.UfkUser = dtReader.GetValue(4).ToString();
                tagList.Add(tm);

            }
            cnn.Close();

            return tagList;
        }

        public static void DeleteDnr(CygnusAutomationModel loadConfig, string ufkDnr, string partnerUri)
        {

            SqlConnection cnn = new SqlConnection(loadConfig.DnrDbConnectionString);
            cnn.Open();

            String sql = Queries.DeleteDnr(ufkDnr, partnerUri);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            cmd.ExecuteReader();

            cnn.Close();
        }

        public static void DeleteDialInNumberSets(CygnusAutomationModel loadConfig, string ufkDnrg)
        {
            SqlConnection cnn = new SqlConnection(loadConfig.DnrDbConnectionString);
            cnn.Open();

            String sql = Queries.DeleteDialInNumberSet(ufkDnrg, loadConfig.PartnerUri);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            cmd.ExecuteReader();

            cnn.Close();
        }

        public static void DeleteDialInTemplate(CygnusAutomationModel loadConfig, string ufkTemplate)
        {
            SqlConnection cnn = new SqlConnection(loadConfig.DnrDbConnectionString);
            cnn.Open();

            String sql = Queries.DeleteTemplate(loadConfig.PartnerUri, ufkTemplate);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            cmd.ExecuteReader();

            cnn.Close();
        }

        public static void DeleteAccountDialInTemplate(CygnusAutomationModel loadConfig, string ufkTag)
        {
            SqlConnection cnn = new SqlConnection(loadConfig.DnrDbConnectionString);
            cnn.Open();

            String sql = Queries.DeleteTag(loadConfig.PartnerUri, ufkTag);

            SqlCommand cmd = new SqlCommand(sql, cnn);
            cmd.ExecuteReader();

            cnn.Close();
        }
    }
}
