using System;
using System.Collections.Generic;

namespace CygnusAutomation.Cygnus.Models
{
    public class DialInNumberSetsModel
    {
        public string ApplicationServer { get; set; }

        public string DialInNumberSetName { get; set; }

        public List<DnrModel> DialInNumbers { get; set; }

        public string UfkDnrg { get; set; }
    }
}
