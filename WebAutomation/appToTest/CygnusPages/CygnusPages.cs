using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CygnusAutomation.Cygnus.CygnusPages
{
    public class CygnusPages
    {
        /*
          PLEASE MAKE SURE TO SORT IT ASCENDING PER EACH OF THE CATEGORY.
        */

        //CMP Login
        public static string LOGIN_SSO_BUTTON = "//*[@data-test-id='saml-login-button']";
        public static string USERNAME = "//*[@id='okta-signin-username']";
        public static string PASSWORD = "//*[@id='okta-signin-password']";
        public static string SIGNOKTA = "//*[@id='okta-signin-submit']";

        //Account
        public static string ACCOUNT_SETTINGS_MENU = "//*[@data-test-id='Account Settings']";
        public static string ACCOUNT_SSO_CERTIFICATE = "account-sso-certificate";
        public static string ACCOUNT_SSO_EMAIL_DOMAIN = "//*[@id='account-sso-settings']/loopup-sso-settings/article/form/div[2]/div[1]/div[3]/div/div/input";
        public static string ACCOUNT_SSO_DEFAULT_PHONE_NUMBER = "accountDefaultPhoneNumber";
        public static string ACCOUNT_SSO_IDP_URL = "account-sso-idp-url";
        public static string ACCOUNT_SSO_ISSUER_URL = "account-sso-issuer-url";
        public static string ACCOUNT_SSO_LOGOUT_URL = "account-sso-logout-url";
        public static string ACCOUNT_SSO_SETTINGS_MENU = "//a[@data-test-id='account-sso-settings']";
        public static string AUTO_PROVISIONING_SSO_BUTTON = "//*[@id='auto-provisioning']//input";
        public static string ENABLE_SSO_BUTTON = "//*[@id='enable-sso']//input";
        public static string GET_FIRST_ROW = "//*[@class='clickable']/td[1]";
        public static string GLOBAL_TOOLS_BUTTON = "//*[@data-test-id='global-tools-btn']";
        public static string SEARCH_FIELD = "//*[@data-test-id='search-field']";
        public static string SEARCH_BUTTON = "//*[@data-test-id='search']";

        //Billing and Usage
        public static string BILLING_AND_USAGE_MENU = "//*[@data-test-id='Billing and Usage']";
        public static string BILLING_SETTINGS_MENU = "//a[@data-test-id='billing-settings']";
        public static string VALUE_BILLING_DROPDOWN = "//select[@value.bind='accountSelectedValue']";

        //Bulky
        public static string BULK_APPLY_BTN = "//loopup-button[@is-clickable.bind='readyToProvision']";
        public static string BULK_BROWSE_FILE = "custom-file";
        public static string BULK_CONFIRM_BUTTON = "//*[contains(text(),' Proceed')]";
        public static string BULK_DELEGATE_MENU = "//a[@data-test-id='manage-csv-button']";
        public static string BULK_UPLOAD_BTN = "//loopup-button[contains(@action,'upload')]";
        public static string BULK_FAILED_EXCEL_VALIDATION = "error-message";
        public static string BULK_PASSED_EXCEL_VALIDATION = "info-message";
        public static string BULK_PRIORITY_PROVISIONING = ("//select[@value.bind='selectedBulkPriority']");
        public static string BULK_REDIRECT_TO_QUEUE_MONITOR = "this link";

        //Common
        public static string APPLY_BUTTON = "apply-action";
        public static string CYGNUS_IFRAME = "//*[@data-test-id='cygnus-frame']";
        public static string ENVIRONMENT_SWITCH_BUTTON = "//*[@data-test-id='environment-switch-button']";
        public static string ERROR_MESSAGE = "div[class = 'error-message']";
        public static string FEATURE_DROPDOWN = "//select[@value.bind='accountSelectedFeature']";
        public static string SAVE_BUTTON = "//loopup-button[@text='Save']";
        public static string SUCCESS_UPDATED_MESSAGE = "//*[@class='success-message']";

        //Email Settings
        public static string ACTIVATION_EMAILS_MENU = "//a[@data-test-id='account-activation-emails']";
        public static string BULK_ACTIVATION_EMAILS_BUTTON = "//a[@data-test-id='bulk-activation-emails']";
        public static string EMAIL_SETTINGS_MENU = "//*[@data-test-id='Email Settings']";

        //Queue Monitoring
        public static string COMPLETED_TAB = "//span[text()='Completed']"; 
        public static string FAILED_TAB = "//span[text()='Failed']";
        public static string GET_ACCESS_CODE = "//tr[1]//td[contains(text(),'@')][1]/following-sibling::td[1]";
        public static string GET_CODE = "//tr[1]//td[contains(text(),'@')][1]/following-sibling::td[2]";
        public static string GET_DELEGATEE = "//tr[1]//td[contains(text(),'@')][2]";
        public static string GET_MAIL = "//tr[1]//td[contains(text(),'@')][1]";
        public static string REFRESH_BUTTON = "//i[@click.delegate]";
        public static string SEARCH_BAR_QUEUE = "//input[@placeholder]";
        public static string SEARCH_BTN = "//button[@click.delegate]";
        public static string QUEUED_BY_DROP_DOWN = "filter-queued-by";

        //User        
        public static string BULK_ACTIONS_DROPDOWN = "bulk-actions";
        public static string BULK_ADD_USERS_BUTTON = ("//a[@data-test-id='bulk-add-users']");
        public static string BULK_TERMINATE_USER_PAGE = "bulkTerminateUser";
        public static string BULK_UPDATE_USERS = "//a[@data-test-id='bulk-update-users']"; 
        public static string CATEGORY_DROPDOWN = "//select[@value.bind='accountSelectedCategory']";
        public static string EXPIRY_TIME_FIELD = "//*[@name='expiryTime']";
        public static string FIRST_USER_CHECKLIST = "//*[@id='tbl-users']//tr[1]/td[1]";
        public static string GET_MAIL_ACCESS_CODE = "//*[@id='tbl-users']//tr[1]/td[2]";
        public static string RECYCLE_ACCESS_CODE_BUTTON = "recycle-access-code";
        public static string RECYCLE_ACCESS_CODES_MENU = "//a[@data-test-id='recycle-access-codes']"; 
        public static string REMOVE_USER_EXCEPTION = "auto-remove-exception";
        public static string SHOW_ADVANCED_SETTINGS = "//*[text()='Show Advanced']";
        public static string TODAY_OPTION = "//*[@data-range-key='Today']";
        public static string UPDATE_BILLING_CODE_MENU = "//a[@data-test-id='update-billing-codes']";
        public static string USER_BULKY_THIRD_COLUMN = "//*[@id='tbl-users']//tr[1]/td[4]";
        public static string USER_DELEGATE_MENU = "//a[@data-test-id='delegate-management']";
        public static string USER_MANAGEMENT = "//*[text()='User Management']";
        public static string USER_PAGE = "//*[text()='Users']";
        public static string TERMINATE_USER_BUTTON = "execute";
        public static string TERMINATE_USER_PAGE = "//*[text()='Terminate Users']";

    }
}
