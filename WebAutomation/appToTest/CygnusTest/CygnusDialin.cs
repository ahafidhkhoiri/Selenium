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
    public class CygnusDialin
    {
        IWebDriver driver;
        private CygnusAutomationModel loadConfig;
        string chromeDriverPath;

        List<string> dnises = new List<string>();
        List<string> ufkDnrg = new List<string>();
        List<string> ufkTemplates = new List<string>();
        List<string> ufkTags = new List<string>();

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

            if (environment == "PROD")
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
        public void AddEditandSetInactiveDNR()
        {
            try
            {
                string number = DateTime.UtcNow.Ticks.ToString().Substring(8);

                DnrModel dialInNumber = new DnrModel()
                {
                    Carrier = loadConfig.Carrier,
                    CustomAllocation = "custom allocation test",
                    DialInCategory = "Indonesia (Toll-Free)",
                    DisplayName = "Test Indonesia (Local no)",
                    DnrE164 = "628" + number,
                    Dnis = "dnis" + number,
                    MediaServer = loadConfig.MediaServer,
                    MobileDnis = "mobileDnis" + number,
                    Notes = "This is dial-in number test"
                };

                dnises.Add(dialInNumber.Dnis);

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.GLOBAL_DIAL_IN_NUMBERS_BUTTON), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                //Add new DNR
                GlobalToolsUtils.AddDnr(driver, dialInNumber);

                //Search newly created dnr
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dialInNumber.DnrE164);
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_BUTTON)).Click();
                Utils.Pause(3000);

                Assert.That(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DNIS_COLUMN)).Text,
                    Does.Contain(dialInNumber.Dnis));

                //Edit new dnr
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DNIS_COLUMN)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DISPLAY_NUMBER_FIELD)).Clear();
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DISPLAY_NUMBER_FIELD)).SendKeys(dialInNumber.DnrE164 +"updated");
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DISPLAY_LABEL_FIELD)).Clear();
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DISPLAY_LABEL_FIELD)).SendKeys(dialInNumber.DisplayName +" updated"); 
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(10000);

                //Search updated dnr
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dialInNumber.DnrE164);
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_BUTTON)).Click();
                Utils.Pause(3000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DISPLAY_NUMBER_COLUMN)).Text,
                    dialInNumber.DnrE164 + "updated");

                //Make the dnr as inactive
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DNIS_COLUMN)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SWITCH_DNR_STATUS_BUTTON)).Click();
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(10000);

                //Search inactive dnr
                var filter = new SelectElement(driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.FILTER_DROPDOWN)));
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.FILTER_DROPDOWN)).Click();
                filter.SelectByText("Inactive");
                Utils.Pause(2000);
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dialInNumber.DnrE164);
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_BUTTON)).Click();
                Utils.Pause(3000);

                Assert.That(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DNIS_COLUMN)).Text,
                    Does.Contain(dialInNumber.Dnis));

                //Check on database for newly dnr, updated dnr, and inactive dnr
                List<DnrModel> compareDb = GlobalToolsUtils.CheckDnr(loadConfig, dialInNumber.Dnis);

                if (compareDb.Count > 0)
                {
                    if (dialInNumber.Dnis.Equals(compareDb[0].Dnis) && 0.Equals(compareDb[0].IsActive)
                        && compareDb[0].DisplayNumber.Contains("updated"))
                    {
                        Console.WriteLine($"All match for {dialInNumber.DnrE164}");
                    }
                    else
                    {
                        throw new Exception($"There's a mismatch data for this dnr {dialInNumber.DnrE164}");
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
        public void AddEditDNRFailedValidation()
        {
            try
            {
                string number = DateTime.UtcNow.Ticks.ToString().Substring(8);

                string dnre164 = "628" + number;
                DnrModel dialInNumber = new DnrModel()
                {
                    DialInCategory = "Indonesia (Toll-Free)",
                    MediaServer = loadConfig.MediaServer
                };
                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.GLOBAL_DIAL_IN_NUMBERS_BUTTON), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                //Add new DNR
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_BUTTON)).Click();
                Utils.Pause(5000);

                //Error dial-in category is required
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(3000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ERROR_MESSAGE)).Text,
                    "Dial-in Category is required.");

                //Error display number is required 
                var dialInCategory = new SelectElement(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_CATEGORY_DROPDOWN)));
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_CATEGORY_DROPDOWN)).Click();
                dialInCategory.SelectByText(dialInNumber.DialInCategory);

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(3000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ERROR_MESSAGE)).Text,
                    ("Display Number is required."));

                //Error dnre164 must be at least 10 characters
                var mediaServer = new SelectElement(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.MEDIA_SERVER_DROPDOWN)));
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.MEDIA_SERVER_DROPDOWN)).Click();
                mediaServer.SelectByText(dialInNumber.MediaServer);

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DNRE164_FIELD)).SendKeys("test");
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(3000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ERROR_MESSAGE)).Text,
                    ("Fully Qualified Phone Number must be at least 10 characters."));

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
        public void AddBulkDialinNumber()
        {
            try
            {
                DataBulkDialinNumber.BulkDialinNumberData(loadConfig, ConfigurationManager.AppSettings["BulkDialinNumberExcelPath"], false);

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.GLOBAL_DIAL_IN_NUMBERS_BUTTON), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                //Add new DNR
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.BULK_UPLOAD_DNR_BUTTON)).Click();
                Utils.Pause(5000);

                GlobalToolsUtils.DnrBulkyProcess(driver, ConfigurationManager.AppSettings["BulkDialinNumberExcelPath"], false,
                        "dnr(s) has been successfully added.");

                //check on database.
                //now we'll check All dnrs data in database and compare it from the excel file.
                var excelData = new ExcelMapper(ConfigurationManager.AppSettings["BulkDialinNumberExcelPath"]).Fetch<DnrModel>();

                int i = 0;

                foreach (var dnr in excelData)
                {
                    string dnis = dnr.Dnis;
                    string dnre164 = dnr.DnrE164;
                    string displayName = dnr.DisplayName;
                    string customAllocation = dnr.CustomAllocation;
                    dnises.Add(dnis);

                    List<DnrModel> compareDb = GlobalToolsUtils.CheckDnr(loadConfig, dnis);

                    if (compareDb.Count > 0)
                    {
                        if (dnis.Equals(compareDb[i].Dnis) && dnre164.Equals(compareDb[i].DnrE164) &&
                            displayName.Equals(compareDb[i].DisplayName) && customAllocation.Equals(compareDb[i].CustomAllocation))
                        {
                            Console.WriteLine($"All match for {dnre164}");
                        }
                        else
                        {
                            throw new Exception($"There's a mismatch data for this dnr {dnre164}");
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
        public void AddBulkDialinNumberFailedValidation()
        {
            try
            {
                DataBulkDialinNumber.BulkDialinNumberData(loadConfig, ConfigurationManager.AppSettings["BulkDialinNumberExcelPath"], true);

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.GLOBAL_DIAL_IN_NUMBERS_BUTTON), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                //Go to Bulk Upload DNR
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.BULK_UPLOAD_DNR_BUTTON)).Click();
                Utils.Pause(5000);

                GlobalToolsUtils.DnrBulkyProcess(driver, ConfigurationManager.AppSettings["BulkDialinNumberExcelPath"], true,
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
        public void AddDialInNumberSetsWithoutChoosingAnyDnr()
        {
            try
            {
                string dialInNumberSetName = $"DNRG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";

                DialInNumberSetsModel dialInNumberSets = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = dialInNumberSetName,
                    ApplicationServer = loadConfig.ApplicationServer
                };

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Click on the three-dots-menu-button
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION), 5000).Click();
                Utils.Pause(5000);

                //Add new DNRG (Dial-in Number Sets)
                dialInNumberSets.DialInNumbers = GlobalToolsUtils.AddEditDialInNumberSets(driver, dialInNumberSets, loadConfig, false, false, false, false);

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
        public void AddDialInNumberSetsWhileChoosingInactiveDnr()
        {
            try
            {
                string dialInNumberSetName = $"DNRG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";
                List<DnrModel> dnrList = new List<DnrModel>();
                List<DialInNumberSetsModel> compareDb;

                DialInNumberSetsModel dialInNumberSets = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = dialInNumberSetName,
                    ApplicationServer = loadConfig.ApplicationServer
                };

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Click on the three-dots-menu-button
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION), 5000).Click();
                Utils.Pause(5000);

                //Add new DNRG (Dial-in Number Sets) but will choose inactive dnrs.
                dnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dialInNumberSets, loadConfig, true, false, false, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.INACTIVE_DIAL_IN_NUMBER_POPUP))
                                       .Text.Contains("Some dial-in numbers are inactive."));

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
        public void AddEditDialInNumberSets()
        {
            try
            {
                string dialInNumberSetName = $"DNRG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";
                List<DnrModel> dnrList = new List<DnrModel>();
                List<DialInNumberSetsModel> compareDb;

                DialInNumberSetsModel dialInNumberSets = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = dialInNumberSetName,
                    ApplicationServer = loadConfig.ApplicationServer
                };

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Click on the three-dots-menu-button
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION), 5000).Click();
                Utils.Pause(5000);

                //Add new DNRG (Dial-in Number Sets)
                dnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dialInNumberSets, loadConfig, true, false, true, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                       .Text.Contains("has been successfully added."));
                dialInNumberSets.DialInNumbers = dnrList;

                //Check on database for newly created dnrg and set the ufkDnrg into dialinNumberSets.UfkDnrg so we can later delete the dnrg.
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dialInNumberSets.DialInNumberSetName);
                dialInNumberSets.UfkDnrg = compareDb[0].UfkDnrg;
                ufkDnrg.Add(dialInNumberSets.UfkDnrg);

                //Search newly created dnrg
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dialInNumberSetName);
                Utils.Pause(3000);

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);

                dialInNumberSets.DialInNumberSetName = $"{dialInNumberSetName} - updated";

                //Edit DNRG (Dial-in Number Sets) and add another dnr to the dnrg
                dnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dialInNumberSets, loadConfig, true, true, true, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                        .Text.Contains("has been successfully updated."));

                dialInNumberSets.DialInNumbers.AddRange(dnrList);

                dialInNumberSets.DialInNumbers = dialInNumberSets.DialInNumbers.OrderBy(a => a.DisplayName).ToList();

                //Check on database for updated dnrg name and its selection of dnr
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dialInNumberSets.UfkDnrg);

                if (compareDb.Count > 0)
                {
                    int i = 0;
                    foreach (var dnrDnrg in compareDb[0].DialInNumbers)
                    {
                        if (compareDb[0].DialInNumberSetName.Equals($"{dialInNumberSetName} - updated") && dnrDnrg.UfkDnr.Equals(dialInNumberSets.DialInNumbers[i].UfkDnr))
                        {
                            Console.WriteLine($"All match for {dnrDnrg.UfkDnr}");
                            i++;
                        }
                        else
                        {
                            throw new Exception($"There's a mismatch data for this dnr {dnrDnrg.UfkDnr}");
                        }
                    }
                }
                else
                {
                    throw new Exception("Can't compare the data from Db. return 0 row.");
                }

                //Search updated dnrg so we can delete the dnrg.
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys($"{dialInNumberSetName} - updated");
                Utils.Pause(3000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DISPLAY_NUMBER_COLUMN)).Text,
                    $"{dialInNumberSetName} - updated");

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);

                //Click on the three-dots-menu-button and choose to remove dnrg.
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.REMOVE_BUTTON), 5000).Click();
                Utils.Pause(5000);

                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.REMOVE_DIAL_IN_NUMBER_SET_POPUP))
                    .Text.Contains("You are about to remove"));

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PROCEED_REMOVING_DIAL_IN_NUMBER_SET), 5000).Click();
                Utils.Pause(5000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                        .Text.Contains("has been successfully removed."));

                //try to search again
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys($"{dialInNumberSetName} - updated");
                Utils.Pause(3000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.NO_MATCHING_DIAL_IN_NUMBER_SET_RECORD)).Displayed);

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
        public void AddEditDeleteDialInTemplate()
        {
            try
            {
                string templateName = $"TEMPLATE_{DateTime.UtcNow.Ticks.ToString().Substring(9)}";

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATES_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Add new dial-in template
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(2000);
                GlobalToolsUtils.AddEditTemplate(driver, templateName, false);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SEARCH_DIAL_IN_TEMPLATES_FIELD)).SendKeys(templateName + Keys.Enter);
                Utils.Pause(5000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE)).Text,
                    templateName + " has been successfully added.");

                //Check on database for newly dial-in template
                List<DialInTemplateModel> compareDb = GlobalToolsUtils.CheckTemplate(loadConfig, templateName, false);

                string ufkTemplate = compareDb[0].UfkTemplate;
                string name = compareDb[0].DialInTemplateName;
                string discUrn = compareDb[0].DiscUrn;
                string type = compareDb[0].Type;
                ufkTemplates.Add(ufkTemplate);

                //Edit newly dial-in template
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_COLUMN)).Click();
                Utils.Pause(5000);
                GlobalToolsUtils.AddEditTemplate(driver, templateName, true);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SEARCH_DIAL_IN_TEMPLATES_FIELD)).SendKeys(templateName + Keys.Enter);
                Utils.Pause(5000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE)).Text,
                    templateName + "updated " + "has been successfully updated.");

                //Check on database for updated dial-in template
                compareDb = GlobalToolsUtils.CheckTemplate(loadConfig, templateName, false);

                // In here, we check the primary number only
                if (compareDb.Count > 0)
                {
                    if ((templateName + "updated").Equals(compareDb[0].DialInTemplateName) && compareDb[0].Type == "111"
                            && compareDb[0].DiscUrn.Contains("IDN"))
                    {
                        Console.WriteLine($"All match for {templateName}");
                    }
                    else
                    {
                        throw new Exception($"There's a mismatch data for this template {templateName}");
                    }
                }
                else
                {
                    throw new Exception("Can't compare the data from Db. return 0 row.");
                }

                //Delete newly dial-in template
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_COLUMN)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.REMOVE_BUTTON)).Click();
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PROCEED_REMOVE_BUTTON)).Click();
                Utils.Pause(7000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE)).Text,
                    templateName + "updated " + "has been successfully removed.");
                Console.WriteLine($"Successfully deleted this template: {templateName}");
                ufkTemplates.Remove(ufkTemplate);

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
        public void AddEditDialInTemplateFailedValidation()
        {
            try
            {
                string templateName = $"TEMPLATE_{DateTime.UtcNow.Ticks.ToString().Substring(9)}";

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATES_BUTTON), 3000).Click();

                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                //Add new dial-in template
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION)).Click();
                Utils.Pause(5000);

                //Dial-in template name is required
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(5000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.TEMPLATE_NAME_REQUIRED)).Displayed);

                //Primary number is required
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_FIELD)).SendKeys(templateName);
                Utils.Pause(5000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PRIMARY_NUMBER_REQUIRED)).Displayed);

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
        public void AddEditDialInNumberSetsException()
        {
            try
            {
                string dialInNumberSetName = $"DNRG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";
                List<DialInNumberSetsModel> dnrgException = new List<DialInNumberSetsModel>();
                List<DialInNumberSetsModel> parentDnrg = new List<DialInNumberSetsModel>();
                List<DialInNumberSetsModel> compareDb;

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Add Dnr Set
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION), 5000).Click();
                Utils.Pause(5000);

                DialInNumberSetsModel dnrSetParent = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = dialInNumberSetName,
                    ApplicationServer = loadConfig.ApplicationServer
                };

                List<DnrModel> ParentdnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dnrSetParent, loadConfig, true, false, true, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                       .Text.Contains("has been successfully added."));
                dnrSetParent.DialInNumbers = ParentdnrList;

                //Check on database for newly created dnrg and set the ufkDnrg into dialinNumberSets.UfkDnrg so we can later delete the dnrg.
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dnrSetParent.DialInNumberSetName);
                dnrSetParent.UfkDnrg = compareDb[0].UfkDnrg;
                ufkDnrg.Add(dnrSetParent.UfkDnrg);

                //Add Dnr Set Exception
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_DIAL_IN_NUMBER_SETS_EXCEPTION_BUTTON), 5000).Click();
                Utils.Pause(5000);
                DialInNumberSetsModel dnrSetException = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = $"ExceptionOf:{dnrSetParent.DialInNumberSetName}",
                    ApplicationServer = loadConfig.ApplicationServer
                };

                List <DnrModel> ExceptionDnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dnrSetException, loadConfig, true, false, true, true);
                dnrSetException.DialInNumbers = ExceptionDnrList;
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                       .Text.Contains("has been successfully added."));

                //Check on database for newly created dnrg exception and set the ufkDnrg into dialinNumberSets.UfkDnrg so we can later delete the dnrg.
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dnrSetException.DialInNumberSetName);
                dnrSetException.UfkDnrg = compareDb[0].UfkDnrg;
                ufkDnrg.Add(dnrSetException.UfkDnrg);

                //In case exception we have to make sure this dnrg exception is deleted on teardown & deleted before it parent
                int lastIndex = ufkDnrg.Count() - 1;

                //Get value of last item of ufkDnrg List, which we suspected as dnrg exception
                string lastItemValue = ufkDnrg.ElementAt(lastIndex);
                ufkDnrg.RemoveAt(lastIndex);
                ufkDnrg.Insert(0, lastItemValue);

                // Check if DNRG Exception Had Correct Parents
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dnrSetException.DialInNumberSetName);
                Utils.Pause(3000);
                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SET_PARENT_COLUMN)).Text,
                    dnrSetParent.DialInNumberSetName);

                // Check If Dnrs From Parent Exists on Its Exception
                List<string> result = new List<string>();
                List<DnrModel> parentDnrs = dnrSetParent.DialInNumbers;
                List<string> parentDnrsList = new List<string>();
                foreach (var tmp in parentDnrs)
                {
                    parentDnrsList.Add(tmp.UfkDnr);
                }

                List<string> exceptionsDnrsList = GlobalToolsUtils.CheckUfkDnrsOfDnrSet(loadConfig, dnrSetException.UfkDnrg);
                foreach (string prnt in parentDnrsList)
                {
                    foreach (string excptn in exceptionsDnrsList)
                    {
                        if (excptn.Equals(prnt))
                        {
                            result.Add(excptn);
                            break;
                        }
                    }
                }

                Assert.IsTrue((result.Count) > 0);

                //Edit DNRG Exceptions (Dial-in Number Sets Exceptions) and add another dnr to the dnrg
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);
                dnrSetException.DialInNumberSetName = $"{dnrSetException.DialInNumberSetName} - updated";
                List <DnrModel> dnrsToUpdate = GlobalToolsUtils.AddEditDialInNumberSets(driver, dnrSetException, loadConfig, true, true, true, false);
                Utils.Pause(5000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                        .Text.Contains("has been successfully updated."));
                dnrSetException.DialInNumbers.AddRange(dnrsToUpdate);

                //Get the latest dnrs of dnr set exception after updated
                List<string> exceptionsDnrsListUpdated = GlobalToolsUtils.CheckUfkDnrsOfDnrSet(loadConfig, dnrSetException.UfkDnrg);

                // We expect once the dnrg expception updated, it has more dnrs, so we are going to compare dnrs list before and after it's updated
                var dnrsResult = exceptionsDnrsListUpdated.Except(exceptionsDnrsList, StringComparer.OrdinalIgnoreCase).ToList();
                Assert.IsTrue((dnrsResult.Count) > 0);

                //Confirm that we could not remove dnr set parent unless we remove it's exception first
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dnrSetParent.DialInNumberSetName);
                Utils.Pause(5000);
                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DISPLAY_NUMBER_COLUMN)).Text,
                    dnrSetParent.DialInNumberSetName);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);

                //Click on the three-dots-menu-button and choose to remove dnrg.
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.REMOVE_BUTTON), 5000).Click();
                Utils.Pause(5000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.REMOVE_DIAL_IN_NUMBER_SET_POPUP))
                    .Text.Contains("You are about to remove"));
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PROCEED_REMOVING_DIAL_IN_NUMBER_SET), 5000).Click();
                Utils.Pause(5000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DNR_SET_ERROR_MESSAGE))
                                        .Text.Contains($"You cannot remove {dnrSetParent.DialInNumberSetName}"));

                Utils.Pause(5000);
                driver.SwitchTo().DefaultContent();
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_BUTTON), 10000).Click();
                Utils.Pause(5000);

                Utils.SwitchToCygnusIframe(driver);

                // We have to remove dnrg exception first, before we can remove the parents
                // Parents dnrg will be removed on tear down
                //Search updated dnrg so we can delete the dnrg.
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dnrSetException.DialInNumberSetName);
                Utils.Pause(5000);
                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DISPLAY_NUMBER_COLUMN)).Text,
                    dnrSetException.DialInNumberSetName);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);

                //Click on the three-dots-menu-button and choose to remove dnrg.
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.REMOVE_BUTTON), 5000).Click();
                Utils.Pause(5000);

                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.REMOVE_DIAL_IN_NUMBER_SET_POPUP))
                    .Text.Contains("You are about to remove"));

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PROCEED_REMOVING_DIAL_IN_NUMBER_SET), 5000).Click();
                Utils.Pause(5000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                        .Text.Contains("has been successfully removed."));
                //try to search again
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dnrSetException.DialInNumberSetName);
                Utils.Pause(5000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.NO_MATCHING_DIAL_IN_NUMBER_SET_RECORD)).Displayed);

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
        public void AddEditDialinNumberSetsExceptionWhileChoosingInactiveDnr()
        {
            try
            {
                string dialInNumberSetName = $"DNRG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";
                List<DialInNumberSetsModel> dnrgException = new List<DialInNumberSetsModel>();
                List<DialInNumberSetsModel> parentDnrg = new List<DialInNumberSetsModel>();
                List<DialInNumberSetsModel> compareDb;

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Add Dnr Set
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION), 5000).Click();
                Utils.Pause(5000);

                DialInNumberSetsModel dnrSetParent = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = dialInNumberSetName,
                    ApplicationServer = loadConfig.ApplicationServer
                };

                List<DnrModel> ParentdnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dnrSetParent, loadConfig, true, false, true, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                       .Text.Contains("has been successfully added."));
                dnrSetParent.DialInNumbers = ParentdnrList;

                //Check on database for newly created dnrg and set the ufkDnrg into dialinNumberSets.UfkDnrg so we can later delete the dnrg.
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dnrSetParent.DialInNumberSetName);
                dnrSetParent.UfkDnrg = compareDb[0].UfkDnrg;
                ufkDnrg.Add(dnrSetParent.UfkDnrg);

                //Add Dnr Set Exception and add with inactive dnr
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_DIAL_IN_NUMBER_SETS_EXCEPTION_BUTTON), 5000).Click();
                Utils.Pause(5000);
                DialInNumberSetsModel dnrSetException = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = $"ExceptionOf:{dnrSetParent.DialInNumberSetName}",
                    ApplicationServer = loadConfig.ApplicationServer
                };

                List<DnrModel> ExceptionDnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dnrSetException, loadConfig, true, false, false, true);

                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.INACTIVE_DIAL_IN_NUMBER_POPUP))
                       .Text.Contains("Some dial-in numbers are inactive."));

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
        public void AddEditDeleteCloneDialInTemplate()
        {
            try
            {
                string templateName = $"TEMPLATE_{DateTime.UtcNow.Ticks.ToString().Substring(9)}";

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATES_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Add new dial-in template
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(2000);
                GlobalToolsUtils.AddEditTemplate(driver, templateName, false);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SEARCH_DIAL_IN_TEMPLATES_FIELD)).SendKeys(templateName + Keys.Enter);
                Utils.Pause(5000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE)).Text,
                    templateName + " has been successfully added.");

                //Check on database for newly dial-in template
                List<DialInTemplateModel> compareDb = GlobalToolsUtils.CheckTemplate(loadConfig, templateName, false);

                string ufkTemplate = compareDb[0].UfkTemplate;
                string name = compareDb[0].DialInTemplateName;
                string discUrn = compareDb[0].DiscUrn;
                string type = compareDb[0].Type;
                ufkTemplates.Add(ufkTemplate);

                //Clone newly dial-in template
                string cloneTemplateName = "CLONE_" + templateName;
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_COLUMN)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CLONE_BUTTON)).Click();
                Utils.Pause(8000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_FIELD)).SendKeys(cloneTemplateName);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PROCEED_WITHOUT_SELECTION_BUTTON)).Click();
                Utils.Pause(10000);

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SEARCH_DIAL_IN_TEMPLATES_FIELD)).SendKeys(cloneTemplateName + Keys.Enter);
                Utils.Pause(5000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE)).Text,
                    cloneTemplateName + " has been successfully added.");

                //Check on database for all selections between clone and master template
                List<DialInTemplateModel> compareDbClone = GlobalToolsUtils.CheckTemplate(loadConfig, cloneTemplateName, true);
                compareDb = GlobalToolsUtils.CheckTemplate(loadConfig, templateName, true);
                ufkTemplate = compareDbClone[0].UfkTemplate;
                ufkTemplates.Add(ufkTemplate);

                int i = 0;

                foreach (var dt in compareDbClone)
                {
                    if (compareDbClone.Count > 0)
                    {
                        if (!(compareDbClone[i].DiscUrn == compareDb[i].DiscUrn && compareDbClone[i].Type == compareDb[i].Type))
                        {
                            // If the selection is not match, it will throw exception directly
                            throw new Exception($"There's mismatch data between {templateName} and {cloneTemplateName}");
                        }
                        i++;
                    }
                    else
                    {
                        throw new Exception("Can't compare the data from Db. return 0 row.");
                    }
                }
                Console.WriteLine($"All selections are match between {templateName} and {cloneTemplateName}");

                //Edit clone dial-in template
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_COLUMN)).Click();
                Utils.Pause(5000);

                GlobalToolsUtils.AddEditTemplate(driver, cloneTemplateName, true);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE)).Text,
                    cloneTemplateName + "updated " + "has been successfully updated.");

                compareDbClone = GlobalToolsUtils.CheckTemplate(loadConfig, cloneTemplateName + "updated", false);

                // In here, we check the primary number only in clone dial-in template
                if (compareDbClone.Count > 0)
                {
                    if ((cloneTemplateName + "updated").Equals(compareDbClone[0].DialInTemplateName) && compareDbClone[0].Type == "111"
                            && compareDbClone[0].DiscUrn.Contains("IDN"))
                    {
                        Console.WriteLine($"All match for {cloneTemplateName}");
                    }
                    else
                    {
                        throw new Exception($"There's a mismatch data for this template {cloneTemplateName}");
                    }
                }
                else
                {
                    throw new Exception("Can't compare the data from Db. return 0 row.");
                }

                //Delete clone dial-in template ( delete master template will be handled by teardown )
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SEARCH_DIAL_IN_TEMPLATES_FIELD)).SendKeys(cloneTemplateName + Keys.Enter);
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_COLUMN)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.REMOVE_BUTTON)).Click();
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PROCEED_REMOVE_BUTTON)).Click();
                Utils.Pause(7000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE)).Text,
                    cloneTemplateName + "updated " + "has been successfully removed.");

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
        public void AddCloneDialInTemplateFailedValidation()
        {
            try
            {
                string templateName = $"TEMPLATE_{DateTime.UtcNow.Ticks.ToString().Substring(9)}";

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATES_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Add new dial-in template
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(2000);
                GlobalToolsUtils.AddEditTemplate(driver, templateName, false);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SEARCH_DIAL_IN_TEMPLATES_FIELD)).SendKeys(templateName + Keys.Enter);
                Utils.Pause(5000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE)).Text,
                    templateName + " has been successfully added.");

                //Check on database for newly dial-in template
                List<DialInTemplateModel> compareDb = GlobalToolsUtils.CheckTemplate(loadConfig, templateName, false);

                string ufkTemplate = compareDb[0].UfkTemplate;
                ufkTemplates.Add(ufkTemplate);

                //Clone newly dial-in template
                string cloneTemplateName = "CLONE_" + templateName;
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_COLUMN)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CLONE_BUTTON)).Click();
                Utils.Pause(8000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(3000);

                // Invalid case where template name is required
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.TEMPLATE_NAME_REQUIRED)).Displayed);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_FIELD)).SendKeys(cloneTemplateName);

                // Invalid case where primary number is required
                var primaryNumber = new SelectElement(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PRIMARY_NUMBER_DROPDOWN)));
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PRIMARY_NUMBER_DROPDOWN)).Click();
                primaryNumber.SelectByText("-");
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(5000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PRIMARY_NUMBER_REQUIRED)).Displayed);

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
        public void AddEditCloneDialInNumberSets()
        {
            try
            {
                string dialInNumberSetName = $"DNRG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";
                List<DnrModel> dnrList = new List<DnrModel>();
                List<DialInNumberSetsModel> compareDb;
                List<DialInNumberSetsModel> compareDbClone;

                DialInNumberSetsModel dialInNumberSets = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = dialInNumberSetName,
                    ApplicationServer = loadConfig.ApplicationServer
                };

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Click on the three-dots-menu-button
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION), 5000).Click();
                Utils.Pause(5000);

                //Add new DNRG (Dial-in Number Sets)
                dnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dialInNumberSets, loadConfig, true, false, true, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                       .Text.Contains("has been successfully added."));
                dialInNumberSets.DialInNumbers = dnrList;

                //Check on database for newly created dnrg and set the ufkDnrg into dialinNumberSets.UfkDnrg so we can later delete the dnrg.
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dialInNumberSets.DialInNumberSetName);
                dialInNumberSets.UfkDnrg = compareDb[0].UfkDnrg;
                ufkDnrg.Add(dialInNumberSets.UfkDnrg);

                //Clone newly created dnrg
                dialInNumberSets.DialInNumberSetName = $"CLONE_{dialInNumberSetName}";
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dialInNumberSetName);
                Utils.Pause(3000);

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CLONE_BUTTON)).Click();
                Utils.Pause(7000);
                driver.FindElement(By.Name(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_NAME)).SendKeys(dialInNumberSets.DialInNumberSetName);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(7000);

                //Check on database for newly clone dnrg
                compareDbClone = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dialInNumberSets.DialInNumberSetName);
                dialInNumberSets.UfkDnrg = compareDbClone[0].UfkDnrg;
                ufkDnrg.Add(dialInNumberSets.UfkDnrg);

                //Compare the value of clone dnrg and master dnrg
                int i = 0;

                foreach (var dnr in compareDbClone)
                {
                    if (compareDbClone.Count > 0)
                    {
                        if (!(compareDbClone[i].DialInNumbers[i].UfkDnr == compareDb[i].DialInNumbers[i].UfkDnr))
                        {
                            // If the selection is not match, it will throw exception directly
                            throw new Exception($"There's mismatch data between clone and master of {dialInNumberSets.DialInNumberSetName}");
                        }
                        i++;
                    }
                    else
                    {
                        throw new Exception("Can't compare the data from Db. return 0 row.");
                    }
                }
                Console.WriteLine($"All dnrs between clone and master are match :{dialInNumberSets.DialInNumberSetName}");

                //Search newly clone dnrg
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dialInNumberSets.DialInNumberSetName);
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);

                dialInNumberSets.DialInNumberSetName = $"CLONE_{dialInNumberSetName} - updated";

                //Edit clone dnrg (Dial-in Number Sets) and add another dnr to the clone dnrg
                dnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dialInNumberSets, loadConfig, true, true, true, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                        .Text.Contains("has been successfully updated."));

                dialInNumberSets.DialInNumbers.AddRange(dnrList);

                dialInNumberSets.DialInNumbers = dialInNumberSets.DialInNumbers.OrderBy(a => a.DisplayName).ToList();

                //Check on database for updated dnrg name and its selection of dnr
                compareDbClone = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dialInNumberSets.UfkDnrg);

                if (compareDbClone.Count > 0)
                {
                    i = 0;
                    foreach (var dnrDnrg in compareDbClone[0].DialInNumbers)
                    {
                        if (compareDbClone[0].DialInNumberSetName.Equals($"{dialInNumberSets.DialInNumberSetName}") 
                            && dnrDnrg.UfkDnr.Equals(dialInNumberSets.DialInNumbers[i].UfkDnr))
                        {
                            Console.WriteLine($"All match for {dnrDnrg.UfkDnr}");
                            i++;
                        }
                        else
                        {
                            throw new Exception($"There's a mismatch data for this dnr {dnrDnrg.UfkDnr}");
                        }
                    }
                }
                else
                {
                    throw new Exception("Can't compare the data from Db. return 0 row.");
                }

                //Delete the clone dnrg
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys($"{dialInNumberSets.DialInNumberSetName}");
                Utils.Pause(3000);

                Assert.AreEqual(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DISPLAY_NUMBER_COLUMN)).Text,
                    $"{dialInNumberSets.DialInNumberSetName}");

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);

                //Click on the three-dots-menu-button and choose to remove dnrg.
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.REMOVE_BUTTON), 5000).Click();
                Utils.Pause(5000);

                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.REMOVE_DIAL_IN_NUMBER_SET_POPUP))
                    .Text.Contains("You are about to remove"));

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.PROCEED_REMOVING_DIAL_IN_NUMBER_SET), 5000).Click();
                Utils.Pause(5000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                        .Text.Contains("has been successfully removed."));

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
        public void AddCloneDialInNumberSetsWhileChoosingInactiveDnr()
        {
            try
            {
                string dialInNumberSetName = $"DNRG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";
                List<DnrModel> dnrList = new List<DnrModel>();
                List<DialInNumberSetsModel> compareDb;

                DialInNumberSetsModel dialInNumberSets = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = dialInNumberSetName,
                    ApplicationServer = loadConfig.ApplicationServer
                };

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Click on the three-dots-menu-button
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION), 5000).Click();
                Utils.Pause(5000);

                //Add new DNRG (Dial-in Number Sets)
                dnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dialInNumberSets, loadConfig, true, false, true, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                       .Text.Contains("has been successfully added."));
                dialInNumberSets.DialInNumbers = dnrList;

                //Check on database for newly created dnrg and set the ufkDnrg into dialinNumberSets.UfkDnrg so we can later delete the dnrg.
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dialInNumberSets.DialInNumberSetName);
                dialInNumberSets.UfkDnrg = compareDb[0].UfkDnrg;
                ufkDnrg.Add(dialInNumberSets.UfkDnrg);

                //Set inactive DNR in clone dnrg page
                dialInNumberSets.DialInNumberSetName = $"CLONE_{dialInNumberSetName}";
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dialInNumberSetName);
                Utils.Pause(3000);

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CLONE_BUTTON)).Click();
                Utils.Pause(7000);

                dnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dialInNumberSets, loadConfig, true, false, false, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.INACTIVE_DIAL_IN_NUMBER_POPUP))
                                       .Text.Contains("Some dial-in numbers are inactive."));

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
        public void AddCloneDialInNumberSetsWithoutChoosingAnyDnr()
        {
            try
            {
                string dialInNumberSetName = $"DNRG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";
                List<DnrModel> dnrList = new List<DnrModel>();
                List<DialInNumberSetsModel> compareDb;

                DialInNumberSetsModel dialInNumberSets = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = dialInNumberSetName,
                    ApplicationServer = loadConfig.ApplicationServer
                };

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Click on the three-dots-menu-button
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION), 5000).Click();
                Utils.Pause(5000);

                //Add new DNRG (Dial-in Number Sets)
                dnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dialInNumberSets, loadConfig, true, false, true, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                       .Text.Contains("has been successfully added."));
                dialInNumberSets.DialInNumbers = dnrList;

                //Check on database for newly created dnrg and set the ufkDnrg into dialinNumberSets.UfkDnrg so we can later delete the dnrg.
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dialInNumberSets.DialInNumberSetName);
                dialInNumberSets.UfkDnrg = compareDb[0].UfkDnrg;
                ufkDnrg.Add(dialInNumberSets.UfkDnrg);

                //Set empty option with specific ufkdnr that we set before
                dialInNumberSets.DialInNumberSetName = $"CLONE_{dialInNumberSetName}";
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dialInNumberSetName);
                Utils.Pause(3000);

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CLONE_BUTTON)).Click();
                Utils.Pause(7000);

                driver.FindElement(By.Name(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_NAME)).SendKeys(dialInNumberSets.DialInNumberSetName);
                dnrList = GlobalToolsUtils.GetUfkDnrByAppServer(loadConfig, false, true, false, "");

                foreach (var ufk in dnrList)
                {
                    driver.FindElement(By.XPath($"//option[@value='{ufk.UfkDnr}']//parent::select//option[contains(text(),'<empty>')]")).Click();
                    Utils.Pause(1000);
                }
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(3000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CONFIRMATION_MESSAGE))
                    .Text.Contains("Please select at least one dial-in number"));

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
        public void AddEditCloneDialinNumberSetExceptions()
        {
            try
            {
                string dialInNumberSetName = $"DNRG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";
                List<DialInNumberSetsModel> dnrgException = new List<DialInNumberSetsModel>();
                List<DialInNumberSetsModel> parentDnrg = new List<DialInNumberSetsModel>();
                List<DialInNumberSetsModel> compareDb;
                List<DialInNumberSetsModel> compareDbClone;

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Add Dnr Set
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION), 5000).Click();
                Utils.Pause(5000);

                DialInNumberSetsModel dnrSetParent = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = dialInNumberSetName,
                    ApplicationServer = loadConfig.ApplicationServer
                };

                List<DnrModel> parentDnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dnrSetParent, loadConfig, true, false, true, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                       .Text.Contains("has been successfully added."));
                dnrSetParent.DialInNumbers = parentDnrList;

                //Check on database for newly created dnrg and set the ufkDnrg into dialinNumberSets.UfkDnrg so we can later delete the dnrg.
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dnrSetParent.DialInNumberSetName);
                dnrSetParent.UfkDnrg = compareDb[0].UfkDnrg;
                ufkDnrg.Add(dnrSetParent.UfkDnrg);

                //Add Dnr Set Exception
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_DIAL_IN_NUMBER_SETS_EXCEPTION_BUTTON), 5000).Click();
                Utils.Pause(5000);
                DialInNumberSetsModel dnrSetException = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = $"ExceptionOf:{dnrSetParent.DialInNumberSetName}",
                    ApplicationServer = loadConfig.ApplicationServer
                };

                List<DnrModel> exceptionDnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dnrSetException, loadConfig, true, false, true, true);
                dnrSetException.DialInNumbers = exceptionDnrList;
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                       .Text.Contains("has been successfully added."));

                //Check on database for newly created dnrg exception and set the ufkDnrg into dialinNumberSets.UfkDnrg so we can later delete the dnrg.
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dnrSetException.DialInNumberSetName);
                dnrSetException.UfkDnrg = compareDb[0].UfkDnrg;
                ufkDnrg.Add(dnrSetException.UfkDnrg);

                //In case exception we have to make sure this dnrg exception is deleted on teardown & deleted before it parent
                int lastIndex = ufkDnrg.Count() - 1;

                //Get value of last item of ufkDnrg List, which we suspected as dnrg exception
                string lastItemValue = ufkDnrg.ElementAt(lastIndex);
                ufkDnrg.RemoveAt(lastIndex);
                ufkDnrg.Insert(0, lastItemValue);

                //Lets start to clone dnr set exception
                //Search dnrSetException
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dnrSetException.DialInNumberSetName);
                Utils.Pause(3000);

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CLONE_BUTTON)).Click();
                Utils.Pause(7000);
                string cloneDnrSetExceptionName = $"CLONE_{dnrSetException.DialInNumberSetName}";
                driver.FindElement(By.Name(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_NAME)).SendKeys(cloneDnrSetExceptionName);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(7000);

                //Check on database for newly clone dnrg
                compareDbClone = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, cloneDnrSetExceptionName);
                string clonedUfkDnrg = compareDbClone[0].UfkDnrg;
                ufkDnrg.Add(clonedUfkDnrg);

                //In case exception we have to make sure this dnrg exception is deleted on teardown & deleted before it parent
                lastIndex = 0;
                lastIndex = ufkDnrg.Count() - 1;

                //Get value of last item of ufkDnrg List, which we suspected as dnrg exception
                lastItemValue = "";
                lastItemValue = ufkDnrg.ElementAt(lastIndex);
                ufkDnrg.RemoveAt(lastIndex);
                ufkDnrg.Insert(0, lastItemValue);

                //Comparing all dnrs from dnrsetException and its clone
                List<string> exceptionsDnrsList = GlobalToolsUtils.CheckUfkDnrsOfDnrSet(loadConfig, dnrSetException.UfkDnrg);
                List<string> dnrsFromClonedDnrsException = GlobalToolsUtils.CheckUfkDnrsOfDnrSet(loadConfig, clonedUfkDnrg);
                Assert.AreEqual((exceptionsDnrsList.OrderBy(q => q).ToList()), (dnrsFromClonedDnrsException.OrderBy(q => q).ToList()));

                //Edit newly cloned exception dnr set
                //Search newly clone dnrg
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(cloneDnrSetExceptionName);
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);
                dnrSetException.DialInNumberSetName = $"{cloneDnrSetExceptionName} - updated";
                List<DnrModel> dnrsToUpdate = GlobalToolsUtils.AddEditDialInNumberSets(driver, dnrSetException, loadConfig, true, true, true, false);
                Utils.Pause(5000);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                        .Text.Contains("has been successfully updated."));
                dnrSetException.DialInNumbers.AddRange(dnrsToUpdate);

                //Get the latest dnrs of dnr set exception after updated
                List<string> exceptionsDnrsListUpdated = GlobalToolsUtils.CheckUfkDnrsOfDnrSet(loadConfig, clonedUfkDnrg);

                // We expect once the dnrg expception updated, it has more dnrs, so we are going to compare dnrs list before and after it's updated
                var dnrsResult = exceptionsDnrsListUpdated.Except(dnrsFromClonedDnrsException, StringComparer.OrdinalIgnoreCase).ToList();
                Assert.IsTrue((dnrsResult.Count) > 0);

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
        public void AddEditCloneDialinNumberSetExceptionsWithInActiveDnrs()
        {
            try
            {
                string dialInNumberSetName = $"DNRG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";
                List<DialInNumberSetsModel> dnrgException = new List<DialInNumberSetsModel>();
                List<DialInNumberSetsModel> parentDnrg = new List<DialInNumberSetsModel>();
                List<DialInNumberSetsModel> compareDb;
                List<DialInNumberSetsModel> compareDbClone;

                Utils.GotoGlobalTools(driver, loadConfig);

                //Go to Dial-in Management Pages
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_MANAGEMENT_BUTTON), 3000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_BUTTON), 3000).Click();

                Utils.Pause(20000);
                Utils.SwitchToCygnusIframe(driver);

                //Add Dnr Set
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_OPTION), 5000).Click();
                Utils.Pause(5000);

                DialInNumberSetsModel dnrSetParent = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = dialInNumberSetName,
                    ApplicationServer = loadConfig.ApplicationServer
                };

                List<DnrModel> parentDnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dnrSetParent, loadConfig, true, false, true, false);
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                       .Text.Contains("has been successfully added."));
                dnrSetParent.DialInNumbers = parentDnrList;

                //Check on database for newly created dnrg and set the ufkDnrg into dialinNumberSets.UfkDnrg so we can later delete the dnrg.
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dnrSetParent.DialInNumberSetName);
                dnrSetParent.UfkDnrg = compareDb[0].UfkDnrg;
                ufkDnrg.Add(dnrSetParent.UfkDnrg);

                //Add Dnr Set Exception
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU), 10000).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_DIAL_IN_NUMBER_SETS_EXCEPTION_BUTTON), 5000).Click();
                Utils.Pause(5000);
                DialInNumberSetsModel dnrSetException = new DialInNumberSetsModel()
                {
                    DialInNumberSetName = $"ExceptionOf:{dnrSetParent.DialInNumberSetName}",
                    ApplicationServer = loadConfig.ApplicationServer
                };

                List<DnrModel> ExceptionDnrList = GlobalToolsUtils.AddEditDialInNumberSets(driver, dnrSetException, loadConfig, true, false, true, true);
                dnrSetException.DialInNumbers = ExceptionDnrList;
                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                       .Text.Contains("has been successfully added."));

                //Check on database for newly created dnrg exception and set the ufkDnrg into dialinNumberSets.UfkDnrg so we can later delete the dnrg.
                compareDb = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dnrSetException.DialInNumberSetName);
                dnrSetException.UfkDnrg = compareDb[0].UfkDnrg;
                ufkDnrg.Add(dnrSetException.UfkDnrg);

                //In case exception we have to make sure this dnrg exception is deleted on teardown & deleted before it parent
                int lastIndex = ufkDnrg.Count() - 1;

                //Get value of last item of ufkDnrg List, which we suspected as dnrg exception
                string lastItemValue = ufkDnrg.ElementAt(lastIndex);
                ufkDnrg.RemoveAt(lastIndex);
                ufkDnrg.Insert(0, lastItemValue);

                //Lets start to clone dnr set exception
                //Search dnrSetException
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dnrSetException.DialInNumberSetName);
                Utils.Pause(3000);

                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.THREE_DOTS_MENU)).Click();
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.CLONE_BUTTON)).Click();
                Utils.Pause(7000);
                string cloneDnrSetExceptionName = $"CLONE_{dnrSetException.DialInNumberSetName}";
                driver.FindElement(By.Name(CygnusPages.GlobalToolsPages.DIAL_IN_NUMBER_SETS_NAME)).SendKeys(cloneDnrSetExceptionName);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(7000);

                //Check on database for newly clone dnrg
                compareDbClone = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, cloneDnrSetExceptionName);
                string clonedUfkDnrg = compareDbClone[0].UfkDnrg;
                ufkDnrg.Add(clonedUfkDnrg);

                //In case exception we have to make sure this dnrg exception is deleted on teardown & deleted before it parent
                lastIndex = 0;
                lastIndex = ufkDnrg.Count() - 1;

                //Get value of last item of ufkDnrg List, which we suspected as dnrg exception
                lastItemValue = "";
                lastItemValue = ufkDnrg.ElementAt(lastIndex);
                ufkDnrg.RemoveAt(lastIndex);
                ufkDnrg.Insert(0, lastItemValue);

                //Comparing all dnrs from dnrsetException and its clone
                List<string> exceptionsDnrsList = GlobalToolsUtils.CheckUfkDnrsOfDnrSet(loadConfig, dnrSetException.UfkDnrg);
                List<string> dnrsFromClonedDnrsException = GlobalToolsUtils.CheckUfkDnrsOfDnrSet(loadConfig, clonedUfkDnrg);
                Assert.AreEqual((exceptionsDnrsList.OrderBy(q => q).ToList()), (dnrsFromClonedDnrsException.OrderBy(q => q).ToList()));

                // Try to edit that cloned exceptions and Add them with inActive DNR
                // Search newly edited clone dnrg
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(dnrSetException.DialInNumberSetName);
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SELECTED_DIAL_IN_NUMBER_SET)).Click();
                Utils.Pause(5000);

                List<DnrModel> inActiveDnrsSelected = GlobalToolsUtils.AddEditDialInNumberSets(driver, dnrSetException, loadConfig, true, true, false, true);

                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.INACTIVE_DIAL_IN_NUMBER_POPUP))
                       .Text.Contains("Some dial-in numbers are inactive."));

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
        public void AddEditAccountDialInTemplates()
        {
            try
            {
                string accountTemplateName = $"TAG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Account dial-in template / TAG menu
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ACCOUNT_NUMBERS_MENU)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ACCOUNT_DIAL_IN_TEMPLATE_MENU)).Click();
                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                //Add new TAG
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_BUTTON)).Click();
                Utils.Pause(8000);

                GlobalToolsUtils.AddEditAccountTemplate(driver, accountTemplateName, false);

                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                       .Text.Contains($"{accountTemplateName} has been successfully added."));

                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(accountTemplateName);

                //Check on database for newly TAG
                List<AccountDialInTemplateModel> compareDb = GlobalToolsUtils.CheckAccountDialInTemplates(loadConfig, accountTemplateName);
                string ufkTag = compareDb[0].UfkTag;
                ufkTags.Add(ufkTag);

                if (compareDb.Count > 0)
                {
                    if (accountTemplateName.Equals(compareDb[0].GroupDisplayName) && compareDb[0].Type == "111"
                            && compareDb[0].DiscUrn.Contains("IDN"))
                    {
                        Console.WriteLine($"All match for {accountTemplateName}");
                    }
                    else
                    {
                        throw new Exception($"There's a mismatch data for this TAG {accountTemplateName}");
                    }
                }
                else
                {
                    throw new Exception("Can't compare the data from Db. return 0 row.");
                }

                //Edit newly created TAG
                accountTemplateName = $"{accountTemplateName}_UPDATED";

                Utils.Pause(5000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.DIAL_IN_TEMPLATE_NAME_COLUMN)).Click();
                Utils.Pause(8000);

                GlobalToolsUtils.AddEditAccountTemplate(driver, accountTemplateName, true);

                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_MESSAGE))
                                       .Text.Contains($"{accountTemplateName} has been successfully updated."));

                //Check on database for updated TAG
                compareDb = GlobalToolsUtils.CheckAccountDialInTemplates(loadConfig, accountTemplateName);

                if (compareDb.Count > 0)
                {
                    if (accountTemplateName.Equals(compareDb[0].GroupDisplayName) && compareDb[0].Type == "111"
                            && compareDb[0].DiscUrn.Contains("GBR"))
                    {
                        Console.WriteLine($"All match for {accountTemplateName}");
                    }
                    else
                    {
                        throw new Exception($"There's a mismatch data for this TAG {accountTemplateName}");
                    }
                }
                else
                {
                    throw new Exception("Can't compare the data from Db. return 0 row.");
                }

                //Get existing email address
                List<AccountDialInTemplateModel> email = GlobalToolsUtils.GetExistingEmailAddress(loadConfig);

                //Switch into main frame of CMP
                driver.SwitchTo().ParentFrame();
                Utils.Pause(3000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ASSIGN_DIAL_IN_TEMPLATES_MENU)).Click();

                //Switch into cygnus frame again and assign TAG to the user
                Utils.SwitchToCygnusIframe(driver);
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.SEARCH_FIELD)).SendKeys(email[0].EmailAddress);
                Utils.Pause(5000);
                var tagDropdown = new SelectElement(driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.ACCOUNT_DIAL_IN_TEMPLATES_DROPDOWN)));
                driver.FindElement(By.Id(CygnusPages.GlobalToolsPages.ACCOUNT_DIAL_IN_TEMPLATES_DROPDOWN)).Click();
                tagDropdown.SelectByText(accountTemplateName);
                Utils.Pause(35000); // The reason put this long pause because the account that we use need some times to make save button become enable, not directly enabled.
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(5000);

                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SUCCESS_UPDATED_MESSAGE))
                                       .Text.Contains($"has been assigned to the {accountTemplateName} dial-in template."));

                //Check on database for assigned user to TAG
                compareDb = GlobalToolsUtils.CheckAccountDialInTemplates(loadConfig, accountTemplateName);

                if (compareDb.Count > 0)
                {
                    if (accountTemplateName.Equals(compareDb[0].GroupDisplayName) && compareDb[0].UfkUser == email[0].UfkUser)
                    {
                        Console.WriteLine($"{email[0].EmailAddress} is successfully assigned to {accountTemplateName}");
                    }
                    else
                    {
                        throw new Exception($"There's a mismatch data between TAG {accountTemplateName} and {email[0].EmailAddress}");
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
        public void AddEditAccountDialInTemplatesFailedValidation()
        {
            try
            {
                string accountTemplateName = $"TAG_{DateTime.UtcNow.Ticks.ToString().Substring(8)}";

                Utils.SearchAccount(driver, loadConfig, false);

                //Go to Account dial-in template / TAG menu
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ACCOUNT_NUMBERS_MENU)).Click();
                Utils.Pause(2000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ACCOUNT_DIAL_IN_TEMPLATE_MENU)).Click();
                Utils.Pause(30000);
                Utils.SwitchToCygnusIframe(driver);

                //Add new TAG with empty name
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ADD_NEW_BUTTON)).Click();
                Utils.Pause(8000);
                driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.SAVE_BUTTON)).Click();
                Utils.Pause(5000);

                Assert.IsTrue(driver.FindElement(By.XPath(CygnusPages.GlobalToolsPages.ERROR_MESSAGE))
                                       .Text.Contains("You need to enter a dial-in template name to save this template."));

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
            int count = dnises.Count();
            while (count > 0)
            {
                List<DnrModel> compareDb = GlobalToolsUtils.CheckDnr(loadConfig, dnises[0]);

                if (compareDb.Count > 0)
                {  
                    //Delete dnr
                    string ufkDnr = compareDb[0].UfkDnr;
                    string partnerUri = compareDb[0].PartnerUri;
                    string dnre164 = compareDb[0].DnrE164;
                    GlobalToolsUtils.DeleteDnr(loadConfig, ufkDnr, partnerUri);

                    //Re-check whether dnr is deleted or not
                    List<DnrModel> compareDbCheck = GlobalToolsUtils.CheckDnr(loadConfig, dnises[0]);
                    if (compareDbCheck.Count > 0)
                    {
                        Console.WriteLine($"Dnr for {dnre164} is not deleted");
                    }
                    else
                    {
                        Console.WriteLine($"Dnr for {dnre164} is deleted");
                        dnises.Remove(dnises[0]);
                    }
                    count--;
                }
            }

            if (ufkDnrg.Count > 0)
            {
                foreach(var dnrg in ufkDnrg)
                {
                    GlobalToolsUtils.DeleteDialInNumberSets(loadConfig, dnrg);
                    List<DialInNumberSetsModel> getDialInNumberSet = GlobalToolsUtils.CheckDialInNumberSetsByNameOrByUfkDnrg(loadConfig, dnrg);
                    if (getDialInNumberSet.Count > 0)
                    {
                        Console.WriteLine($"Failed to delete this ufkDnrg: {dnrg}");
                    }
                    else
                    {
                        Console.WriteLine($"Successfully deleted this ufkDnrg: {dnrg}");
                    }
                }
            }

            if (ufkTemplates.Count > 0)
            {
                foreach (var template in ufkTemplates)
                {
                    GlobalToolsUtils.DeleteDialInTemplate(loadConfig, template);
                    List<DialInTemplateModel> getDialInTemplate = GlobalToolsUtils.CheckTemplate(loadConfig, template, false);
                    if (getDialInTemplate.Count > 0)
                    {
                        Console.WriteLine($"Failed to delete this ufkTemplate: {template}");
                    }
                    else
                    {
                        Console.WriteLine($"Successfully deleted this ufkTemplate: {template}");
                    }
                }
            }

            if (ufkTags.Count > 0)
            {
                foreach (var tag in ufkTags)
                {
                    GlobalToolsUtils.DeleteAccountDialInTemplate(loadConfig, tag);
                    List<AccountDialInTemplateModel> getAccountDialInTemplate = GlobalToolsUtils.CheckAccountDialInTemplates(loadConfig, tag);
                    if (getAccountDialInTemplate.Count > 0)
                    {
                        Console.WriteLine($"Failed to delete this ufkTag: {tag}");
                    }
                    else
                    {
                        Console.WriteLine($"Successfully deleted this ufkTag: {tag}");
                    }
                }
            }
            driver.Close();
            driver.Quit();
        }

    }
}
