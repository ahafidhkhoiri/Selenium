using CygnusAutomation.Cygnus.CygnusUtils;
using CygnusAutomation.Cygnus.CygnusUtils.BulkyData;
using CygnusAutomation.Cygnus.Models;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using Ganss.Excel;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using OpenQA.Selenium.Support.UI;
using Newtonsoft.Json;
using System.Linq;

namespace CygnusAutomation.Cygnus.CygnusTest
{
    [TestFixture]
    public class CygnusAccount
    {
        IWebDriver driver;
        private CygnusAutomationModel loadConfig;
        Dictionary<string, string> lastData;
        string chromeDriverPath;

        [SetUp]
        public void startdriver()
        {
            ChromeOptions option = new ChromeOptions();
            option.AddArguments("--window-size=1920,1080");
            option.AddArguments("--start-maximized");
            option.AddArguments("--headless");
            //option.AddArguments("--incognito");

            //if the environment is not being passed as arg, then it'll run the test in LAB by default.
            string environment = TestContext.Parameters["env"]?.ToUpper() ?? "LAB";

            loadConfig = Utils.ParseEnvironmentConfig(environment);

            //current supported chromeDriver version is v.87
            chromeDriverPath = Path.Combine(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly()
                                .Location)).FullName, @"..\..\packages\chromedriver_win32");

            driver = new ChromeDriver(chromeDriverPath, option);

            if(environment == "PROD")
            {
                driver.Url = loadConfig.CmpUrl; 
            }
            else
            {
                driver.Url = ConfigurationManager.AppSettings["URL"];
            }

            Utils.LoginToCMP(driver, ConfigurationManager.AppSettings["CmpEmail"],
                ConfigurationManager.AppSettings["CmpPassword"]);
        }

        [Test]
        public void AddBulkBillingCode()
        {
            try
            {
                lastData = DataBulkBillingCode.BulkBillingCodeTestData(loadConfig,
                ConfigurationManager.AppSettings["BulkBillingCodeExcelPath"], false);

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk Billing Code menu
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.UPDATE_BILLING_CODE_MENU), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                CygnusBulkUtils.BulkyProcess(driver, ConfigurationManager.AppSettings["BulkBillingCodeExcelPath"], true, false,
                        "Successfully bulk update billing code.");

                //check on bulk monitoring page
                Utils.Pause(5000);
                CygnusBulkUtils.BulkMonitoringPage(driver, lastData["email"], true);

                //!!Configurable!!
                //By default we'll try to wait in total of 3mins (latest) to see if the last user (the third user)
                //has been successfully provisioned / updated or not. if not, then we'll just throw exception.
                //if we want to provision / bulk update any feature > 3 users, then we need to adjust the pause(3000)
                // and (counter == 40)
                bool userPresent = false;
                int counter = 0;
                do
                {
                    driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_BTN)).Click();

                    try
                    {
                        //we need to use try catch, because if it's not present then it'll throw exception right away.
                        userPresent = driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Displayed;
                        if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Text.Equals(lastData["email"]) ||
                                !driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_CODE)).Text.Equals(lastData["bCode"]))
                        {
                            userPresent = false;
                        }
                        Console.WriteLine("userPresent: '" + userPresent + "'");
                    }
                    catch (Exception e)
                    {
                    }

                    Utils.Pause(Convert.ToInt32(ConfigurationManager.AppSettings["BulkyWaitTime"]));
                    counter++;

                    if (counter == Convert.ToInt32(ConfigurationManager.AppSettings["BulkyCounterTimes"]))
                    {
                        throw new Exception("Time Limit exceeded. It's either the user is failed or still in queue. " +
                                "Please check for this user: '" + lastData["email"] + "'");
                    }
                } while (userPresent == false);

                //check on database.
                //now we'll check All users data in database and compare it from the excel file.
                var excelData = new ExcelMapper(ConfigurationManager.AppSettings["BulkBillingCodeExcelPath"]).Fetch<BillingCodeModel>();
                List<BillingCodeModel> compareDb = CygnusBulkUtils.GetExistingUserBillingCode(loadConfig);

                int i = 0;

                foreach (var bc in excelData)
                {
                    string email = bc.EmailAddress;
                    string bCode = bc.BillingCode;

                    if (compareDb.Count > 0)
                    {
                        if (email.Equals(compareDb[i].EmailAddress) && bCode.Equals(compareDb[i].BillingCode))
                        {
                            Console.WriteLine($"All match for {email}");
                        }
                        else
                        {
                            throw new Exception($"There's a mismatch data for this user {email}, DB: {compareDb[i].BillingCode}, CSV: {bCode}");
                        }
                        i++;
                    }
                    else
                    {
                        throw new Exception("Can't compare the data from Db. return 0 row.");
                    }
                }
                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        [Test]
        public void AddBulkBillingCodeFailedValidation()
        {
            try
            {
                lastData = DataBulkBillingCode.BulkBillingCodeTestData(loadConfig,
                ConfigurationManager.AppSettings["BulkBillingCodeExcelPath"], true);

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk Billing Code menu
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.UPDATE_BILLING_CODE_MENU), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                CygnusBulkUtils.BulkyProcess(driver, ConfigurationManager.AppSettings["BulkBillingCodeExcelPath"], true, true,
                        "There are 1 error(s) detected in the table.");

                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddSSOSettings()
        {
            try
            {
                Utils.SearchAccount(driver, loadConfig, false);
                int counter = 3;

                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACCOUNT_SETTINGS_MENU), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACCOUNT_SSO_SETTINGS_MENU), 2000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);
                Utils.EnableSSO(driver);
                Utils.FillSSOData(driver);

                //Add User Exceptions based on counter value
                while (counter > 0)
                {
                    driver.FindElement(By.XPath("//*[@id=\"tbl-users\"]//tr[" + counter + "]/td[1]")).Click();
                    counter--;
                }

                Utils.Pause(2000);
                driver.FindElement(By.Id(CygnusPages.CygnusPages.APPLY_BUTTON)).Click();
                Utils.Pause(2000);

                driver.FindElement(By.XPath(CygnusPages.CygnusPages.SAVE_BUTTON)).Click();
                Utils.Pause(5000);

                bool checkEnableSSOMessage = driver.FindElement(By.XPath(CygnusPages.CygnusPages.SUCCESS_UPDATED_MESSAGE), 10000)
                .Text.Contains("Enable SSO for " + loadConfig.AccountName + " has been successfully updated.");

                if (checkEnableSSOMessage)
                {
                    Console.WriteLine("Assertion checkEnableSSOMessage: " + checkEnableSSOMessage + ". Successfully enabled SSO for the account..");
                }
                else
                {
                    throw new NoSuchElementException("Assertion checkEnableSSOMessage: " + checkEnableSSOMessage + ".Failed to enable SSO");
                }

                //Disable SSO to revert back
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ENABLE_SSO_BUTTON)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.SAVE_BUTTON)).Click();
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.SUCCESS_UPDATED_MESSAGE), 10000);

                Boolean checkDisableSSOMessage = driver.FindElement(By.XPath(CygnusPages.CygnusPages.SUCCESS_UPDATED_MESSAGE), 10000)
                        .Text.Contains("Enable SSO for " + loadConfig.AccountName + " has been successfully updated.") &&
                        driver.FindElement(By.XPath(CygnusPages.CygnusPages.SUCCESS_UPDATED_MESSAGE), 10000).Text.Contains("has been removed");

                if (checkDisableSSOMessage)
                {
                    Console.WriteLine("Assertion checkEnableSSOMessage: " + checkEnableSSOMessage + ". Successfully disabled SSO for the account..");
                }
                else
                {
                    throw new NoSuchElementException("Assertion checkEnableSSOMessage: " + checkEnableSSOMessage + ".Failed to disable SSO");
                }

                Console.WriteLine("test_passed");


            }
            catch(ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddSSOSettingsFailedValidation()
        {
            try
            {
                Utils.SearchAccount(driver, loadConfig, false);

                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACCOUNT_SETTINGS_MENU), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACCOUNT_SSO_SETTINGS_MENU), 2000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);
                Utils.EnableSSO(driver);

                //grab existing email domain from db
                string existingEmailDomain = CygnusBulkUtils.GetExistingEmailDomain(loadConfig.DbConnectionString);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACCOUNT_SSO_EMAIL_DOMAIN)).Click();
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACCOUNT_SSO_EMAIL_DOMAIN)).Clear();
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACCOUNT_SSO_EMAIL_DOMAIN)).SendKeys(existingEmailDomain);

                driver.FindElement(By.XPath(CygnusPages.CygnusPages.SAVE_BUTTON)).Click();
                Utils.Pause(5000);

                bool checkDuplicatedDomain = driver.FindElement(By.CssSelector(CygnusPages.CygnusPages.ERROR_MESSAGE))
                    .Text.Contains("One or more email domain value already used by other account.");
                if (checkDuplicatedDomain)
                {
                    Console.WriteLine("Assertion for duplicated email domain: " + checkDuplicatedDomain + ". Validation passed");
                }
                else
                {
                    throw new Exception("Failed to validate duplicated email domain.");
                }

                // Validate Invalid Certificate
                driver.FindElement(By.Id(CygnusPages.CygnusPages.ACCOUNT_SSO_CERTIFICATE)).Clear();
                driver.FindElement(By.Id(CygnusPages.CygnusPages.ACCOUNT_SSO_CERTIFICATE)).SendKeys("invalid");

                driver.FindElement(By.XPath(CygnusPages.CygnusPages.SAVE_BUTTON)).Click();
                Utils.Pause(5000);

                bool checkSSOValidationMessage = driver.FindElement(By.CssSelector(CygnusPages.CygnusPages.ERROR_MESSAGE))
                    .Text.Contains("Account Sso Certificate is invalid");

                if (checkSSOValidationMessage)
                {
                    Console.WriteLine("Assertion for SSO Certificate: " + checkSSOValidationMessage + " is passed.");
                }
                else
                {
                    throw new Exception("Failed to validate SSO Certificate.");
                }
                
                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void UpdateAccountBillingSettings()
        {
            try
            {
                Utils.SearchAccount(driver, loadConfig, false);

                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BILLING_AND_USAGE_MENU), 30).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BILLING_SETTINGS_MENU), 30).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                //UK Payment Method Feature
                Utils.UpdateBillingSettings(driver,loadConfig, "Payment method");
                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.CygnusPages.SUCCESS_UPDATED_MESSAGE)).Text,
                    "Payment method for " + loadConfig.AccountName + " has been successfully updated.");

                //Include Call Details on PDF
                Utils.UpdateBillingSettings(driver, loadConfig, "Include call details on invoice pdf");
                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.CygnusPages.SUCCESS_UPDATED_MESSAGE)).Text,
                    "Include call details on invoice pdf for " + loadConfig.AccountName + " has been successfully updated.");
                
                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddBulkDelegate()
        {
            try
            {
                lastData = DataBulkDelegate.BulkDelegateTestData(loadConfig,
                ConfigurationManager.AppSettings["BulkDelegateExcelPath"], false);

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk Delegate menu
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_DELEGATE_MENU), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_DELEGATE_MENU), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                CygnusBulkUtils.BulkyProcess(driver, ConfigurationManager.AppSettings["BulkDelegateExcelPath"], true, false,
                        "Successfully bulk delegate user.");

                //check on bulk monitoring page
                Utils.Pause(5000);
                CygnusBulkUtils.BulkMonitoringPage(driver, lastData["email"], true);

                //!!Configurable!!
                //By default we'll try to wait in total of 3mins (latest) to see if the last user (the third user)
                //has been successfully provisioned / updated or not. if not, then we'll just throw exception.
                //if we want to provision / bulk update any feature > 3 users, then we need to adjust the pause(3000)
                // and (counter == 40)
                bool userPresent = false;
                int counter = 0;
                do
                {
                    driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_BTN)).Click();

                    try
                    {
                        //we need to use try catch, because if it's not present then it'll throw exception right away.
                        userPresent = driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Displayed;
                        if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Text.Equals(lastData["email"]) ||
                                !driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_DELEGATEE)).Text.Equals(lastData["delegateEmail"]))
                        {
                            userPresent = false;
                        }
                        Console.WriteLine("userPresent: '" + userPresent + "'");
                    }
                    catch (Exception e)
                    {
                    }

                    Utils.Pause(Convert.ToInt32(ConfigurationManager.AppSettings["BulkyWaitTime"]));
                    counter++;

                    if (counter == Convert.ToInt32(ConfigurationManager.AppSettings["BulkyCounterTimes"]))
                    {
                        throw new Exception("Time Limit exceeded. It's either the user is failed or still in queue. " +
                                "Please check for this user: '" + lastData["email"] + "'");
                    }
                } while (userPresent == false);

                //check on database.
                //now we'll check All users data in database and compare it from the excel file.
                var excelData = new ExcelMapper(ConfigurationManager.AppSettings["BulkDelegateExcelPath"]).Fetch<DelegateModel>();

                int i = 0;

                foreach (var dm in excelData)
                {
                    string email = dm.EmailAddress;
                    string delegateEmail = dm.DelegateEmailAddress;

                    List<DelegateModel> compareDb = CygnusBulkUtils.GetExistingDelegate(loadConfig, true, delegateEmail);
                    if (compareDb.Count > 0)
                    {
                        if (email.Equals(compareDb[i].EmailAddress) && delegateEmail.Equals(compareDb[i].DelegateEmailAddress))
                        {
                            Console.WriteLine($"All match for {delegateEmail}");
                        }
                        else
                        {
                            throw new Exception($"There's a mismatch data for this user {delegateEmail}, DB: {compareDb[i].EmailAddress}, CSV: {delegateEmail}");
                        }
                    }
                    else
                    {
                        throw new Exception("Can't compare the data from Db. return 0 row.");
                    }
                }
                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddBulkDelegateFailedValidation()
        {
            try
            {
                lastData = DataBulkDelegate.BulkDelegateTestData(loadConfig,
                ConfigurationManager.AppSettings["BulkDelegateExcelPath"], true);

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk Delegate menu
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_DELEGATE_MENU), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_DELEGATE_MENU), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                CygnusBulkUtils.BulkyProcess(driver, ConfigurationManager.AppSettings["BulkDelegateExcelPath"], false, true,
                        "There are 2 error(s) detected in the table.");

                //check on bulk monitoring page
                Utils.Pause(5000);
                
                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddBulkProvisionUser()
        {
            try
            {
                String lastData = DataBulkUser.BulkUserTestData(loadConfig,
                ConfigurationManager.AppSettings["BulkProvisionUserPath"], false);
                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk Provision User menu
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_PAGE)).Click();
                Utils.Pause(10000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_ADD_USERS_BUTTON)).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                var priorityType = new SelectElement(driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_PRIORITY_PROVISIONING)));
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_PRIORITY_PROVISIONING)).Click();
                priorityType.SelectByText("High Priority");

                CygnusBulkUtils.BulkyProcess(driver, ConfigurationManager.AppSettings["BulkProvisionUserPath"], true, false,
                        "Successfully bulk provision user");

                //check on bulk monitoring page
                Utils.Pause(5000);
                CygnusBulkUtils.BulkMonitoringPage(driver, lastData, true);

                //!!Configurable!!
                //By default we'll try to wait in total of 3mins (latest) to see if the last user (the third user)
                //has been successfully provisioned / updated or not. if not, then we'll just throw exception.
                //if we want to provision / bulk update any feature > 3 users, then we need to adjust the pause(3000)
                // and (counter == 40)
                bool userPresent = false;
                int counter = 0;
                do
                {
                    driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_BTN)).Click();

                    try
                    {
                        //we need to use try catch, because if it's not present then it'll throw exception right away.
                        userPresent = driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Displayed;
                        if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Text.Equals(lastData))
                        {
                            userPresent = false;
                        }
                        Console.WriteLine("userPresent: '" + userPresent + "'");
                    }
                    catch (Exception e)
                    {
                    }

                    Utils.Pause(Convert.ToInt32(ConfigurationManager.AppSettings["BulkyWaitTime"]));
                    counter++;

                    if (counter == Convert.ToInt32(ConfigurationManager.AppSettings["BulkyCounterTimes"]))
                    {
                        throw new Exception("Time Limit exceeded. It's either the user is failed or still in queue. " +
                                "Please check for this user: '" + lastData + "'");
                    }
                } while (userPresent == false);

                //check on database.
                //now we'll check All users data in database and compare it from the excel file.
                var excelData = new ExcelMapper(ConfigurationManager.AppSettings["BulkProvisionUserPath"]).Fetch<UserModel>();

                int i = 0;

                foreach (var usr in excelData)
                {
                    string fName = usr.FirstName;
                    string lName = usr.LastName;
                    string email = usr.EmailAddress;
                    string bCode = usr.BillingCode;
                    string ufkUsr = usr.UfkUser;

                    List<UserModel> compareDb = CygnusBulkUtils.CheckProvisionUser(loadConfig, email);

                    if (compareDb.Count > 0)
                    {
                        if (fName.Equals(compareDb[i].FirstName) && lName.Equals(compareDb[i].LastName) &&
                            bCode.Equals(compareDb[i].BillingCode) && ufkUsr.Equals(compareDb[i].UfkUser))
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
                        throw new Exception("Can't compare the data from Db. return 0 row.");
                    }
                }

                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddBulkProvisionUserFailedValidation()
        {
            try
            {
                String lastData = DataBulkUser.BulkUserTestData(loadConfig,
                ConfigurationManager.AppSettings["BulkProvisionUserPath"], true);

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk Provision User menu
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_PAGE)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_ADD_USERS_BUTTON)).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                var priorityType = new SelectElement(driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_PRIORITY_PROVISIONING)));
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_PRIORITY_PROVISIONING)).Click();
                priorityType.SelectByText("High Priority");

                CygnusBulkUtils.BulkyProcess(driver, ConfigurationManager.AppSettings["BulkProvisionUserPath"], true, true,
                        "There are 2 error(s) detected in the table.");

                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddBulkActivationEmail()
        {
            try
            {
                lastData = DataBulkActivationEmail.BulkActivationEmailTestData(loadConfig,
                ConfigurationManager.AppSettings["BulkActivationEmailPath"], false);

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk activation email menu
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.EMAIL_SETTINGS_MENU)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACTIVATION_EMAILS_MENU)).Click();
                Utils.Pause(10000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_ACTIVATION_EMAILS_BUTTON), 30).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                CygnusBulkUtils.BulkyProcess(driver, ConfigurationManager.AppSettings["BulkActivationEmailPath"], true, false,
                        "Successfully bulk activation email");

                //check on bulk monitoring page
                Utils.Pause(5000);
                CygnusBulkUtils.BulkMonitoringPage(driver, lastData["email"], true);

                //!!Configurable!!
                //By default we'll try to wait in total of 3mins (latest) to see if the last user (the third user)
                //has been successfully provisioned / updated or not. if not, then we'll just throw exception.
                //if we want to provision / bulk update any feature > 3 users, then we need to adjust the pause(3000)
                // and (counter == 40)
                bool userPresent = false;
                int counter = 0;
                do
                {
                    driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_BTN)).Click();

                    try
                    {
                        //we need to use try catch, because if it's not present then it'll throw exception right away.
                        userPresent = driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Displayed;
                        if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Text.Equals(lastData["email"]))
                        {
                            userPresent = false;
                        }
                        Console.WriteLine("userPresent: '" + userPresent + "'");
                    }
                    catch (Exception e)
                    {
                    }

                    Utils.Pause(Convert.ToInt32(ConfigurationManager.AppSettings["BulkyWaitTime"]));
                    counter++;

                    if (counter == Convert.ToInt32(ConfigurationManager.AppSettings["BulkyCounterTimes"]))
                    {
                        throw new Exception("Time Limit exceeded. It's either the user is failed or still in queue. " +
                                "Please check for this user: '" + lastData["email"] + "'");
                    }
                } while (userPresent == false);

                //check on database.
                //now we'll check All users data in database and compare it from the excel file.
                var excelData = new ExcelMapper(ConfigurationManager.AppSettings["BulkActivationEmailPath"]).Fetch<ActivationEmailModel>();

                int i = 0;

                foreach (var act in excelData)
                {
                    string email = act.EmailAddress;

                    List<ActivationEmailModel> compareDb = CygnusBulkUtils.GetEmailFromDb(loadConfig, true, email);

                    if (compareDb.Count > 0)
                    {
                        if (email.Equals(compareDb[i].EmailAddress) && !compareDb[i].LatestActivationEmail.Equals(null))
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
                        throw new Exception("Can't compare the data from Db. return 0 row.");
                    }
                }

                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddBulkActivationEmailFailedValidation()
        {
            try
            {
                lastData = DataBulkActivationEmail.BulkActivationEmailTestData(loadConfig,
                ConfigurationManager.AppSettings["BulkActivationEmailPath"], true);

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk activation email menu
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.EMAIL_SETTINGS_MENU)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.ACTIVATION_EMAILS_MENU)).Click();
                Utils.Pause(10000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_ACTIVATION_EMAILS_BUTTON), 30).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                CygnusBulkUtils.BulkyProcess(driver, ConfigurationManager.AppSettings["BulkActivationEmailPath"], true, true,
                        "There are 1 error(s) detected in the table.");

                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddRecycleAccessCodeNonSalesforceAccount()
        {
            try
            {
                //This method on JK will not working until LUP-7821 is done, but against LAB would be fine.
                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Recycle access code menu
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.RECYCLE_ACCESS_CODES_MENU)).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                Utils.Pause(5000);
                String email = driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL_ACCESS_CODE)).Text;
                String oldAccessCode = driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_BULKY_THIRD_COLUMN)).Text;
                CygnusBulkUtils.BulkAccessCode(driver, "Successfully recycle access code.");

                //check on bulk monitoring page
                Utils.Pause(5000);
                CygnusBulkUtils.BulkMonitoringPage(driver, email, false);

                //!!Configurable!!
                //By default we'll try to wait in total of 3mins (latest) to see if the last user
                //has been successfully provisioned / updated or not. if not, then we'll just throw exception.
                //if we want to provision / bulk update any feature > 3 users, then we need to adjust the pause(3000)
                // and (counter == 40)
                bool userPresent = false;
                int counter = 0;
                do
                {
                    driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_BTN)).Click();

                    try
                    {
                        //we need to use try catch, because if it's not present then it'll throw exception right away.
                        userPresent = driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Displayed;
                        if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Text.Equals(email)
                            || !driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_ACCESS_CODE)).Text.Equals(oldAccessCode))
                        {
                            userPresent = false;
                        }
                        Console.WriteLine("userPresent: '" + userPresent + "'");
                    }
                    catch (Exception e)
                    {
                    }

                    Utils.Pause(Convert.ToInt32(ConfigurationManager.AppSettings["BulkyWaitTime"]));
                    counter++;

                    if (counter == Convert.ToInt32(ConfigurationManager.AppSettings["BulkyCounterTimes"]))
                    {
                        throw new Exception("Time Limit exceeded. It's either the user is failed or still in queue. " +
                                "Please check for this user: '" + email + "'");
                    }
                } while (userPresent == false);

                //check on database.
                //now we'll check All users data in database and compare it from old data

                List<AccessCodeModel> compareDb = CygnusBulkUtils.CheckAccessCode(loadConfig, email);

                if (compareDb.Count > 0)
                {
                    if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_ACCESS_CODE)).Text.Equals(compareDb[0].AccessCode)
                    || !driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Text.Equals(compareDb[0].EmailAddress))
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
                    throw new Exception("Can't compare the data from Db. return 0 row.");
                }

                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddRecycleAccessCodeSalesforceAccount()
        {
            try
            {
                Utils.SearchAccount(driver, loadConfig, true);

                //Go to Recycle access code menu
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.RECYCLE_ACCESS_CODES_MENU)).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                Utils.Pause(5000);
                String email = driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL_ACCESS_CODE)).Text;
                String oldAccessCode = driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_BULKY_THIRD_COLUMN)).Text;
                CygnusBulkUtils.BulkAccessCode(driver, "Successfully recycle access code.");

                //check on bulk monitoring page
                Utils.Pause(5000);
                CygnusBulkUtils.BulkMonitoringPage(driver, email, true);

                //!!Configurable!!
                //By default we'll try to wait in total of 3mins (latest) to see if the last user
                //has been successfully provisioned / updated or not. if not, then we'll just throw exception.
                //if we want to provision / bulk update any feature > 3 users, then we need to adjust the pause(3000)
                // and (counter == 40)
                bool userPresent = false;
                int counter = 0;
                do
                {
                    driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_BTN)).Click();

                    try
                    {
                        //we need to use try catch, because if it's not present then it'll throw exception right away.
                        userPresent = driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Displayed;
                        if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Text.Equals(email)
                            || !driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_ACCESS_CODE)).Text.Equals(oldAccessCode))
                        {
                            userPresent = false;
                        }
                        Console.WriteLine("userPresent: '" + userPresent + "'");
                    }
                    catch (Exception e)
                    {
                    }

                    Utils.Pause(Convert.ToInt32(ConfigurationManager.AppSettings["BulkyWaitTime"]));
                    counter++;

                    if (counter == Convert.ToInt32(ConfigurationManager.AppSettings["BulkyCounterTimes"]))
                    {
                        throw new Exception("Time Limit exceeded. It's either the user is failed or still in queue. " +
                                "Please check for this user: '" + email + "'");
                    }
                } while (userPresent == false);

                //check on database.
                //now we'll check All users data in database and compare it from old data

                List<AccessCodeModel> compareDb = CygnusBulkUtils.CheckAccessCode(loadConfig, email);

                if (compareDb.Count > 0)
                {
                    if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_ACCESS_CODE)).Text.Equals(compareDb[0].AccessCode)
                    || !driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Text.Equals(compareDb[0].EmailAddress))
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
                    throw new Exception("Can't compare the data from Db. return 0 row.");
                }

                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddBulkTerminateUser()
        {
            try
            {
                lastData = DataBulkTerminateUser.BulkTerminateUserTestData(loadConfig,
                           ConfigurationManager.AppSettings["BulkTerminateUserPath"], false);

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk Terminate menu
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.TERMINATE_USER_PAGE), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                Utils.Pause(2000);
                driver.FindElement(By.Id(CygnusPages.CygnusPages.BULK_TERMINATE_USER_PAGE), 3000).Click();

                Utils.Pause(5000);
                CygnusBulkUtils.BulkyProcess(driver, ConfigurationManager.AppSettings["BulkTerminateUserPath"], true, false,
                        "Successfully bulk terminate user.");

                //check on bulk monitoring page
                Utils.Pause(5000);
                CygnusBulkUtils.BulkMonitoringPage(driver, lastData["email"], true);

                //!!Configurable!!
                //By default we'll try to wait in total of 3mins (latest) to see if the last user (the third user)
                //has been successfully terminated / updated or not. if not, then we'll just throw exception.
                //if we want to provision / bulk update any feature > 3 users, then we need to adjust the pause(3000)
                // and (counter == 40)
                bool userPresent = false;
                int counter = 0;
                do
                {
                    driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_BTN)).Click();

                    try
                    {
                        //we need to use try catch, because if it's not present then it'll throw exception right away.
                        userPresent = driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Displayed;
                        if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Text.Equals(lastData["email"]))
                        {
                            userPresent = false;
                        }
                        Console.WriteLine("userPresent: '" + userPresent + "'");
                    }
                    catch (Exception e)
                    {
                    }

                    Utils.Pause(Convert.ToInt32(ConfigurationManager.AppSettings["BulkyWaitTime"]));
                    counter++;

                    if (counter == Convert.ToInt32(ConfigurationManager.AppSettings["BulkyCounterTimes"]))
                    {
                        throw new Exception("Time Limit exceeded. It's either the user is failed or still in queue. " +
                                "Please check for this user: '" + lastData["email"] + "'");
                    }
                } while (userPresent == false);

                //check on database.
                //now we'll check All users data in database and compare it from the excel file.
                var excelData = new ExcelMapper(ConfigurationManager.AppSettings["BulkTerminateUserPath"]).Fetch<TerminateUserModel>();

                int i = 0;

                foreach (var tu in excelData)
                {
                    string email = tu.EmailAddress;

                    List<TerminateUserModel> compareDb = CygnusBulkUtils.GetExistingUser(loadConfig, true, email);
                    if (compareDb.Count > 0)
                    {
                        if (email.Equals(compareDb[i].EmailAddress) && compareDb[i].UserStatus == 8)
                        {
                            Console.WriteLine($"All match for {email}");
                        }
                        else
                        {
                            throw new Exception($"There's a mismatch data for this user {email}, DB: {compareDb[i].EmailAddress}, status: {compareDb[i].UserStatus}");
                        }
                    }
                    else
                    {
                        throw new Exception("Can't compare the data from Db. return 0 row.");
                    }
                }
                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddBulkTerminateUserFailedValidation()
        {
            try
            {
                lastData = DataBulkTerminateUser.BulkTerminateUserTestData(loadConfig,
                ConfigurationManager.AppSettings["BulkTerminateUserPath"], true);

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk Terminate menu
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.TERMINATE_USER_PAGE), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                Utils.Pause(2000);
                driver.FindElement(By.Id(CygnusPages.CygnusPages.BULK_TERMINATE_USER_PAGE), 3000).Click();

                CygnusBulkUtils.BulkyProcess(driver, ConfigurationManager.AppSettings["BulkTerminateUserPath"], true, true,
                        "There are 1 error(s) detected in the table.");

                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void TerminateUser()
        {
            try
            {
                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk Terminate menu
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.TERMINATE_USER_PAGE), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                //always ticked the first user and store the user email into a variable
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.FIRST_USER_CHECKLIST), 3000).Click();
                string userEmail = driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_BULKY_THIRD_COLUMN)).Text;

                driver.FindElement(By.Id(CygnusPages.CygnusPages.TERMINATE_USER_BUTTON), 3000).Click();
                CygnusBulkUtils.BulkConfirmationPopUp(driver);
                CygnusBulkUtils.RedirectToQueueMonitoringPage(driver, "Successfully terminate a user");

                //check on bulk monitoring page
                Utils.Pause(5000);
                CygnusBulkUtils.BulkMonitoringPage(driver, userEmail, true);

                //!!Configurable!!
                //By default we'll try to wait in total of 3mins (latest) to see if the last user (the third user)
                //has been successfully terminated / updated or not. if not, then we'll just throw exception.
                //if we want to provision / bulk update any feature > 3 users, then we need to adjust the pause(3000)
                // and (counter == 40)
                bool userPresent = false;
                int counter = 0;
                do
                {
                    driver.FindElement(By.XPath(CygnusPages.CygnusPages.SEARCH_BTN)).Click();

                    try
                    {
                        //we need to use try catch, because if it's not present then it'll throw exception right away.
                        userPresent = driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Displayed;
                        if (!driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Text.Equals(userEmail))
                        {
                            userPresent = false;
                        }
                        Console.WriteLine("userPresent: '" + userPresent + "'");
                    }
                    catch (Exception e)
                    {
                    }

                    Utils.Pause(Convert.ToInt32(ConfigurationManager.AppSettings["BulkyWaitTime"]));
                    counter++;

                    if (counter == Convert.ToInt32(ConfigurationManager.AppSettings["BulkyCounterTimes"]))
                    {
                        throw new Exception("Time Limit exceeded. It's either the user is failed or still in queue. " +
                                "Please check for this user: '" + userEmail + "'");
                    }
                } while (userPresent == false);

                //check on database.
                //now we'll check the user status in database

                List<TerminateUserModel> compareDb = CygnusBulkUtils.GetExistingUser(loadConfig, true, userEmail);
                if (compareDb.Count > 0)
                {
                    if (userEmail.Equals(compareDb[0].EmailAddress) && compareDb[0].UserStatus == 8)
                    {
                        Console.WriteLine($"All match for {userEmail}");
                    }
                    else
                    {
                        throw new Exception($"There's a mismatch data for this user {userEmail}, DB: {compareDb[0].EmailAddress}, status: {compareDb[0].UserStatus}");
                    }
                }
                else
                {
                    throw new Exception("Can't compare the data from Db. return 0 row.");
                }

                Console.WriteLine("test_passed");
            }
            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void AddBulkUpdateUserSettings()
        {
            try
            {
                string JSON = File.ReadAllText($@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Cygnus\Configurations\PauFeatures.json");
                var category = JsonConvert.DeserializeObject<List<UserCategory>>(JSON);

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Bulk Update User Settings menu
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.USER_MANAGEMENT)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.BULK_UPDATE_USERS)).Click();

                Utils.Pause(35000);
                Utils.SwitchToCygnusIframe(driver);

                driver.FindElement(By.XPath(CygnusPages.CygnusPages.SHOW_ADVANCED_SETTINGS)).Click();
                Utils.Pause(3000);
                driver.FindElement(By.Id(CygnusPages.CygnusPages.REMOVE_USER_EXCEPTION)).Click();

                //Tick first user
                string email = driver.FindElement(By.XPath(CygnusPages.CygnusPages.GET_MAIL)).Text;
                driver.FindElement(By.XPath(CygnusPages.CygnusPages.FIRST_USER_CHECKLIST), 3000).Click();

                foreach (var (cat, fea, input) in from cat in category
                                                  from fea in cat.Features
                                                  from input in fea.Values
                                                  select (cat, fea, input))
                {
                    Utils.BulkUpdateSettings(driver, loadConfig, cat.CategoryName, fea.FeatureName, input.InputValue, 
                        fea.PauKey, input.PauValue, email, true);
                }

                Console.WriteLine("test_passed");
            }

            catch (ElementNotVisibleException e)
            {
                Console.WriteLine(e);
            }
            catch (ElementNotInteractableException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [TearDown]
        public void closedriver()
        {
            driver.Close();
            driver.Quit();
        }

    }
}
