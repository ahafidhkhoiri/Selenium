using Ganss.Excel;
using System;
namespace CygnusAutomation.Cygnus.Models
{
    public class TerminateUserModel
    {
        [Column("USER_EMAIL_ADDRESS")]
        public String EmailAddress { get; set; }

        [Ignore]
        public int UserStatus { get; set; }
    }
}
