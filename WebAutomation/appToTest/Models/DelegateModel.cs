using Ganss.Excel;
using System;
namespace CygnusAutomation.Cygnus.Models
{
    public class DelegateModel
    {
        [Column("E-MAIL ADDRESS (Manager)")]
        public String EmailAddress { get; set; }

        [Column("DELEGATE EMAIL ADDRESS (PA)")]
        public String DelegateEmailAddress { get; set; }
    }
}
