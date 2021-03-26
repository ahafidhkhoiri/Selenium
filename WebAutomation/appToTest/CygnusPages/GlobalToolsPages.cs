using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CygnusAutomation.Cygnus.CygnusPages
{
    public class GlobalToolsPages
    {
        /*
          PLEASE MAKE SURE TO SORT IT ASCENDING PER EACH OF THE CATEGORY.
        */

        //Account Dial-in Templates
        public static string ACCOUNT_TEMPLATE_NAME_FIELD = "//input[@name='groupDisplayName']";
        public static string ACCOUNT_DIAL_IN_TEMPLATES_DROPDOWN = "user-ufk-tag-select";
        public static string AVAILABLE_CHECKBOX_UK_TOLL_FREE = "//td[3]//*[@class='checkbox cb-avaiable form-check-input peers ai-c']//*[@value='United Kingdom']";
        public static string AVAILABLE_CHECKBOX_IDN_TOLL_FREE = "//td[3]//*[@class='checkbox cb-avaiable form-check-input peers ai-c']//*[@value='Indonesia']";
        public static string THREE_DOTS_UK_TOLL_FREE = "//td[(text()='United Kingdom')]/following-sibling::td[2]//*/loopup-card-menu/div";
        public static string THREE_DOTS_IDN_TOLL_FREE = "//td[(text()='Indonesia')]/following-sibling::td[2]//*/loopup-card-menu/div";
        public static string SET_PRIMARY_NUMBER_UK_TOLL_FREE = "//td[(text()='United Kingdom')]/following-sibling::td[2]//*[(text()=' Set as Primary Number')]";
        public static string SET_PRIMARY_NUMBER_IDN_TOLL_FREE = "//td[(text()='Indonesia')]/following-sibling::td[2]//*[(text()=' Set as Primary Number')]";
        public static string SET_SECONDARY_NUMBER_UK_TOLL_FREE = "//td[(text()='United Kingdom')]/following-sibling::td[2]//*[(text()=' Set as Secondary Number')]";
        public static string SET_SECONDARY_NUMBER_IDN_TOLL_FREE = "//td[(text()='Indonesia')]/following-sibling::td[2]//*[(text()=' Set as Secondary Number')]";

        //Account Numbers
        public static string ACCOUNT_DIAL_IN_TEMPLATE_MENU = "//*[@data-test-id='account-dial-in-templates']";
        public static string ACCOUNT_NUMBERS_MENU = "//*[@data-test-id='Account Numbers']";
        public static string ASSIGN_DIAL_IN_TEMPLATES_MENU = "//*[@data-test-id='assign-dial-in-templates']";

        //Bulky
        public static string BULK_APPLY_BTN = "//loopup-button[@is-clickable.bind='readyToProvision']";
        public static string BULK_BROWSE_FILE = "custom-file";
        public static string BULK_CONFIRM_BUTTON = "//*[contains(text(),' Proceed')]";
        public static string BULK_UPLOAD_BTN = "//loopup-button[contains(@action,'upload')]";
        public static string BULK_FAILED_EXCEL_VALIDATION = "error-message";
        public static string BULK_PASSED_EXCEL_VALIDATION = "info-message";

        //Dial-in Management
        public static string ADD_NEW_OPTION = "//*[@id='mainContent']/div/div/div/div/div[3]/loopup-card-menu/div/ul/li[1]"; //
        public static string DIAL_IN_MANAGEMENT_BUTTON = "//*[@data-test-id='Dial-in Management']";
        public static string DIAL_IN_NUMBER_SETS_BUTTON = "//*[@data-test-id='dial-in-number-sets']";
        public static string DIAL_IN_TEMPLATES_BUTTON = "//*[@data-test-id='dial-in-templates']";
        public static string GLOBAL_DIAL_IN_NUMBERS_BUTTON = "//*[@data-test-id='global-dial-in-numbers']";
        public static string REMOVE_BUTTON = "*//button[contains(text(), 'Remove')]";
        public static string SAVE_BUTTON = "//loopup-button[@text='Save']";
        public static string SEARCH_BUTTON = "search-pattern-submit";
        public static string SEARCH_FIELD = "search-pattern";
        public static string SUCCESS_MESSAGE = "//*[@id='statusMessage']/span";
        public static string THREE_DOTS_MENU = "//*/loopup-card-menu/div";

        //Dial-in Template
        public static string AVAILABLE_TOLL_CHECKBOX = "//input[@checked.two-way='availableTollChecked']";
        public static string AVAILABLE_TOLL_FREE_CHECKBOX = "//input[@checked.two-way='availableTollFreeChecked']";
        public static string CLEAR_SELECTION = "//*[@id='mainContent']/div/div/div/div/div/loopup-card-menu/div/ul/li[1]/loopup-button";
        public static string CLONE_BUTTON = "//button[contains(text(), 'Clone')]";
        public static string DEFAULT_TOLL_CHECKBOX = "//input[@checked.two-way='defaultTollChecked']";
        public static string DEFAULT_TOLL_FREE_CHECKBOX = "//input[@checked.two-way='defaultTollFreeChecked']";
        public static string DIAL_IN_TEMPLATE_NAME_COLUMN = "//*[@id='dataTable']/tbody/tr/td[1]";
        public static string DIAL_IN_TEMPLATE_NAME_FIELD = "//input[@name='templateDisplayname']";
        public static string PRIMARY_NUMBER_DROPDOWN = "//select[@name='primaryNumber']";
        public static string PRIMARY_NUMBER_REQUIRED = "//li[contains(text(),'Primary Number is required.')]";
        public static string PROCEED_REMOVE_BUTTON = "//loopup-button[contains(@action,'proceedRemovingDialinSelection')]";
        public static string PROCEED_WITHOUT_SELECTION_BUTTON = "//*[@id='selection-template-clone-popup']//loopup-button[contains(@action,'proceedWithoutSelection')]"; 
        public static string SEARCH_DIAL_IN_TEMPLATES_FIELD = "//input[contains(@placeholder,'search')]";
        public static string SECONDARY_NUMBER_DROPDOWN = "//select[@name='secondary']";
        public static string TEMPLATE_NAME_REQUIRED = "//li[contains(text(),'Dial-in template name is required.')]";

        //Global Dial-in Numbers
        public static string ADD_NEW_BUTTON = "//loopup-button[@text='Add New']";
        public static string APPLY_MOBILE_DNIS = "applyMobile";
        public static string BULK_UPLOAD_DNR_BUTTON = "//loopup-button[@action='bulkUpload']";
        public static string CANCEL_BUTTON = "//loopup-button[@action='cancel']";
        public static string CARRIER_DROPDOWN = "//select[@name='carrier']";
        public static string CUSTOM_ALLOCATION_FIELD = "//input[@name='customAllocation']";
        public static string DIAL_IN_CATEGORY_DROPDOWN = "//select[@name='discUrn']";
        public static string DISPLAY_LABEL_FIELD = "//input[@name='dnrE164DisplayName']";
        public static string DISPLAY_NUMBER_COLUMN = "//*[@id='dataTable']/tbody/tr[1]/td[1]";
        public static string DISPLAY_NUMBER_FIELD = "//input[@name='dnrE164DisplayNumber']";
        public static string ERROR_MESSAGE = "//*[@class='error-message']//li";
        public static string DNIS_COLUMN = "//*[@id='dataTable']/tbody/tr/td[5]";
        public static string DNIS_FIELD = "//*[@id='dialin-number']/div[2]/div/div[7]/div/div/input";
        public static string DNRE164_FIELD = "//input[@name='dnrE164']";
        public static string FILTER_DROPDOWN = "is-active-number";
        public static string MEDIA_SERVER_DROPDOWN = "//select[@name='mediaServer']";
        public static string MOBILE_DNIS_FIELD = "//*[@id='dialin-number']/div[2]/div/div[8]/div/div/input";
        public static string NOTES_FIELD = "//textarea[@name='notes']";
        public static string SUCCESS_UPDATED_MESSAGE = "//*[@class='success-message']//span";
        public static string SWITCH_DNR_STATUS_BUTTON = "//loopup-switch-button[@action='dnrStatus']";

        //Dial-in Number Sets
        public static string ADD_NEW_DIAL_IN_NUMBER_SETS_EXCEPTION_BUTTON = "//*[@id='mainContent']/div/div/div/div/div[3]/loopup-card-menu/div/ul/li[2]";
        public static string APPLICATION_SERVER_DROPDOWN = "//*/loopup-appserver-field/div/select";
        public static string CONFIRMATION_MESSAGE = "//*[@id='dialin-number-popup']/div/div/div[2]/div";
        public static string CREATE_NUMBER_SET_EXCEPTION_BUTTON = "//*[text()=' Create Number Set Exception']";
        public static string DNR_SET_ERROR_MESSAGE = "//*[@class='error-message']";
        public static string DIAL_IN_NUMBER_SET_PARENT_COLUMN = "//*[@id=\"dataTable\"]/tbody/tr[1]/td[2]";
        public static string DIAL_IN_NUMBER_SETS_EXCEPTION_NAME = "//input[@name='dnrgDisplayName']";
        public static string DIAL_IN_NUMBER_SETS_NAME = "dnrgDisplayName";
        public static string INACTIVE_DIAL_IN_NUMBER_POPUP = "//*[@id='inactive-dnrs-popup']/div/div/div[2]/div";
        public static string MASTER_DIAL_IN_NUMBER_SET_DROPDOWN = "//*[@id=\"master-dialin-group\"]/div[3]/div[1]/div/div[2]/select";
        public static string NO_MATCHING_DIAL_IN_NUMBER_SET_RECORD = "//td[contains(text(), 'No matching records found')]";
        public static string PROCEED_REMOVING_DIAL_IN_NUMBER_SET = "//*[@action='proceedRemovingDnrg']";
        public static string REMOVE_DIAL_IN_NUMBER_SET_POPUP = "//*[@id='remove-dnrg-popup']/div/div/div[2]/div";
        public static string SELECTED_DIAL_IN_NUMBER_SET = "//*[@id='dataTable']/tbody/tr/td[1]/a";
        public static string SELECTED_DIAL_NUMBER_SET_EXCEPTION = "//*[@id=\"dataTable\"]/tbody/tr[1]/td[2]";
        public static string SET_APP_SERVERS_BUTTON = "//loopup-button[@action='setAppServers']";
    }
}
