using Ganss.Excel;
using System;
namespace CygnusAutomation.Cygnus.Models
{
    public class BillingCodeModel
    {
        [Column("E-MAIL ADDRESS")]
        public String EmailAddress { get; set; }

        [Column("LEADER_CODE")]
        public String LeaderCode { get; set; }

        [Column("BILLING_CODE")]
        public String BillingCode { get; set; }
        
    }
}
