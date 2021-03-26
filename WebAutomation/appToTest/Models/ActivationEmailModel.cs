using Ganss.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CygnusAutomation.Cygnus.Models
{
    public class ActivationEmailModel
    {
        [Column("USER_EMAIL_ADDRESS")]
        public String EmailAddress { get; set; }

        [Ignore]
        public String LatestActivationEmail { get; set; }
    }
}