using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using CygnusAutomation.Cygnus.Models;
using OpenQA.Selenium.Interactions;
using System.Reflection;
using System.Data.SqlClient;

namespace CygnusAutomation.Cygnus.CygnusUtils
{
    public static class Utils
    {
        public static CygnusAutomationModel ParseEnvironmentConfig(string env)
        {
            using (StreamReader readfile = new StreamReader($@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Cygnus\Configurations\{env}.json"))
            {
                string jsonFile = readfile.ReadToEnd();
                var configFile = JsonConvert.DeserializeObject<CygnusAutomationModel>(jsonFile);
                
                return configFile;
            }
        }

        public static void LoginToCMP(IWebDriver driver, string username, string password)
        {
            Pause(20000);

            //if there's new update features from CMP, it will usually have an overlay banner that we need to close first).
            try
            {
                var ssoLoginButtonLocation = driver.FindElement(By.XPath(CygnusPages.CygnusPages.LOGIN_SSO_BUTTON)).Location;

                Actions builder = new Actions(driver);

                var body = driver.FindElement(By.XPath(".//body"));

                builder
                    .MoveToElement(body, ssoLoginButtonLocation.X+1, ssoLoginButtonLocation.Y)
                    .Click()
                    .Build()
                    .Perform();
            }
            catch(Exception e)
            {
                Console.WriteLine("No notification found");
            }

            driver.FindElement(By.XPath(CygnusPages.CygnusPages.LOGIN_SSO_BUTTON), 3000).Click();

            Pause(3000);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.USERNAME)).SendKeys(username);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.PASSWORD)).SendKeys(password);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.SIGNOKTA)).Click();
        }

        public static void SearchAccount(IWebDriver driver, CygnusAutomationModel loadConfig, bool salesforceAccount)
        {
            // Switch environment
            if (loadConfig.TestEnvironment.ToLower() != "prod")
            {
                Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ENVIRONMENT_SWITCH_BUTTON), 30).Click();
                Pause(5000);
                driver.FindElement(By.XPath("//*[@data-test-id='" + loadConfig.CmpEnvironment + "']"))
                    .Click();
            }

            // try to search an account
            Pause(5000);
            if (salesforceAccount)
            {
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_FIELD)).SendKeys(loadConfig.SalesforceAccountName);
            }
            else
            {
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_FIELD)).SendKeys(loadConfig.AccountName);
            }

            driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_BUTTON)).Click();
            Pause(5000);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_FIRST_ROW)).Click();
            Pause(10000);
        }

        public static void GotoGlobalTools(IWebDriver driver, CygnusAutomationModel loadConfig)
        {
            // Switch environment
            if (loadConfig.TestEnvironment.ToLower() != "prod")
            {
                Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ENVIRONMENT_SWITCH_BUTTON), 30).Click();
                Pause(5000);
                driver.FindElement(By.XPath("//*[@data-test-id='" + loadConfig.CmpEnvironment + "']"))
                    .Click();
            }

            Pause(5000);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.GLOBAL_TOOLS_BUTTON)).Click();
            Pause(5000);
        }

        public static void Pause(int seconds)
        {
            Task.Delay(seconds).Wait();
        }

        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }
            return driver.FindElement(by);
        }

        public static void SwitchToCygnusIframe(IWebDriver driver)
        {
            Pause(10000);
            IWebElement cygnusIframe = driver.FindElement(By.XPath(CygnusPages.CygnusPages.CYGNUS_IFRAME), 25000);
            driver.SwitchTo().Frame(cygnusIframe);
            Utils.Pause(5000);
        }

        public static void EnableSSO(IWebDriver driver)
        {
            Pause(5000);

            if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.ENABLE_SSO_BUTTON)).Selected)
            {
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ENABLE_SSO_BUTTON)).Click();
            }
            if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.AUTO_PROVISIONING_SSO_BUTTON)).Selected)
            {
                Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.AUTO_PROVISIONING_SSO_BUTTON)).Click();
            }
        }

        public static void FillSSOData(IWebDriver driver)
        {
            Random number = new Random();
            string domain = "domain" + number.Next(10000) + ".co";

            Pause(5000);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACCOUNT_SSO_EMAIL_DOMAIN)).Click();
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACCOUNT_SSO_EMAIL_DOMAIN)).Clear();
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACCOUNT_SSO_EMAIL_DOMAIN)).SendKeys(domain);
            Pause(2000);

            driver.FindElement(By.Name(CygnusPages.CygnusPages.ACCOUNT_SSO_DEFAULT_PHONE_NUMBER)).Clear();
            driver.FindElement(By.Name(CygnusPages.CygnusPages.ACCOUNT_SSO_DEFAULT_PHONE_NUMBER)).SendKeys("14152511666");

            driver.FindElement(By.Id(CygnusPages.CygnusPages.ACCOUNT_SSO_IDP_URL)).Clear();
            driver.FindElement(By.Id(CygnusPages.CygnusPages.ACCOUNT_SSO_IDP_URL)).SendKeys(ConfigurationManager.AppSettings["SSOIdpUrl"]);

            driver.FindElement(By.Id(CygnusPages.CygnusPages.ACCOUNT_SSO_ISSUER_URL)).Clear();
            driver.FindElement(By.Id(CygnusPages.CygnusPages.ACCOUNT_SSO_ISSUER_URL)).SendKeys(ConfigurationManager.AppSettings["SSOIssuerUrl"]);

            driver.FindElement(By.Id(CygnusPages.CygnusPages.ACCOUNT_SSO_LOGOUT_URL)).Clear();
            driver.FindElement(By.Id(CygnusPages.CygnusPages.ACCOUNT_SSO_LOGOUT_URL)).SendKeys(ConfigurationManager.AppSettings["SSOLogoutUrl"]);

            driver.FindElement(By.Id(CygnusPages.CygnusPages.ACCOUNT_SSO_CERTIFICATE)).Clear();
            driver.FindElement(By.Id(CygnusPages.CygnusPages.ACCOUNT_SSO_CERTIFICATE)).SendKeys(ConfigurationManager.AppSettings["SSOCertificate"]);
            Pause(5000);
        }

        public static void UpdateBillingSettings(IWebDriver driver, CygnusAutomationModel loadConfig, String feature) 
        {

            String value = Utils.GetBillingSettings(loadConfig.DbConnectionString, loadConfig.AccountName, feature);
            var billingSettingsFeature = new SelectElement(driver.FindElement(By.XPath(CygnusPages.CygnusPages.FEATURE_DROPDOWN)));
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.FEATURE_DROPDOWN)).Click();
            billingSettingsFeature.SelectByText(feature);

            var billingSettingsValue = new SelectElement(driver.FindElement(By.XPath(CygnusPages.CygnusPages.VALUE_BILLING_DROPDOWN)));
            Pause(5000);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.VALUE_BILLING_DROPDOWN)).Click();
            Pause(5000);
            if (value.Equals("CONVENTIONAL"))
            {
                billingSettingsValue.SelectByText("Credit card");
            }
            else if(value.Equals("DIRECT_DEBIT"))
            {
                billingSettingsValue.SelectByText("Manual");
            }
            else if (value.Equals("CREDIT_CARD"))
            {
                billingSettingsValue.SelectByText("Direct debit");
            }
            else if(value.Equals("1"))
            {
                billingSettingsValue.SelectByText("Disable");
            }
            else if(value.Equals("0"))
            {
                billingSettingsValue.SelectByText("Enable");
            }
            Pause(3000); 
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.SAVE_BUTTON)).Click();
            Pause(5000);
        }

        public static string GetBillingSettings(string dbConnectionString, string accountName ,string feature ) 
        {
            String sql = "";
            SqlConnection cnn = new SqlConnection(dbConnectionString);
            cnn.Open();

            if (feature.Equals("Payment method")) {
                 sql = Queries.GetBillingSettingsPaymentMethod(accountName);
            }
            else{
                 sql = Queries.GetBillingSettingsCallDetails(accountName);
            }

            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataReader dtReader = cmd.ExecuteReader(); 
            while (dtReader.Read())
            { 
                sql = dtReader.GetValue(0).ToString();
            }
            cnn.Close();

            return sql;
        }

        public static void BulkUpdateSettings(IWebDriver driver, CygnusAutomationModel loadConfig, string category, string feature, string action
            , string pauKey, string pauValue, string email, bool checkDb)
        {
            var categoryDropdown = new SelectElement(driver.FindElement(By.XPath(CygnusPages.CygnusPages.CATEGORY_DROPDOWN)));
            var featureDropdown = new SelectElement(driver.FindElement(By.XPath(CygnusPages.CygnusPages.FEATURE_DROPDOWN)));

            driver.FindElement(By.XPath(CygnusPages.CygnusPages.CATEGORY_DROPDOWN)).Click();
            categoryDropdown.SelectByText(category);
            Pause(3000);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.FEATURE_DROPDOWN)).Click();
            featureDropdown.SelectByText(feature);
            ChangeActionDropdown(driver, loadConfig, category, feature, action, pauKey, pauValue, email, checkDb);
        }

        public static void ChangeActionDropdown(IWebDriver driver, CygnusAutomationModel loadConfig, string category, string feature, string action
            , string pauKey, string pauValue, string email, bool checkDb)
        {
            var actionDropdown = new SelectElement(driver.FindElement(By.Id(CygnusPages.CygnusPages.BULK_ACTIONS_DROPDOWN)));

            driver.FindElement(By.Id(CygnusPages.CygnusPages.BULK_ACTIONS_DROPDOWN)).Click();
            Pause(10000);
            actionDropdown.SelectByText(action);
            driver.FindElement(By.Id(CygnusPages.CygnusPages.APPLY_BUTTON)).Click();
            Pause(3000);
            driver.FindElement(By.XPath(CygnusPages.CygnusPages.SAVE_BUTTON)).Click();
            Pause(5000);

            bool checkFeatureMessage = driver.FindElement(By.XPath(CygnusPages.CygnusPages.SUCCESS_UPDATED_MESSAGE), 10000)
               .Text.Contains(feature + " for " + loadConfig.AccountName + " has been successfully updated.");

            if (checkFeatureMessage)
            {
                Console.WriteLine("Category : " + category + ", Feature : " + feature + ", Actions : " + action + ", is working");
            }
            else
            {
                throw new NoSuchElementException("Failed to update " + category + ", Feature: " + feature + ", Action: " + action);
            }

            if (checkDb)
            {
                Utils.CompareDBUserSettings(loadConfig, pauKey, pauValue, email);
            }
        }

        public static void CompareDBUserSettings(CygnusAutomationModel loadConfig, string pauKey, string pauValue, string email)
            {
                List<ExtendedProperty> compareDb = CygnusBulkUtils.CheckUserSettings(loadConfig, email, pauKey);

                if (compareDb.Count > 0)
                {
                    if (compareDb[0].IntegerValue.Equals(pauValue))
                    {
                        Console.WriteLine($"All match for {email}");
                    }
                    else if (compareDb[0].StringValue.Equals(pauValue))
                    {
                        Console.WriteLine($"All match for {email}");
                    }
                    else
                    {
                        throw new Exception($"There's a mismatch data for this user {email}");
                    }
                }
                else
                {
                    //This condition is for Remove Exceptions which will return 0 row
                    Console.WriteLine($"All match for {email}");
                }
            }
        }
    }
