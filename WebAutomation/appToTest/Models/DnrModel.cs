using System;
using Ganss.Excel;

namespace CygnusAutomation.Cygnus.Models
{
    public class DnrModel
    {
        [Column("COUNTRY_ISO")]
        public string CountryIso { get; set; }

        [Column("IVR_LOCALE")]
        public string IvrLocale { get; set; }

        [Column("COUNTRY_LABEL")]
        public string CountryLabel { get; set; }

        [Ignore]
        public string UfkDnr { get; set; }

        [Column("DNR_E164")]
        public string DnrE164 { get; set; }

        [Ignore]
        public string DialInCategory { get; set; }

        [Column("DISPLAY_NUMBER")]
        public string DisplayNumber { get; set; }

        [Column("DNR_E164_DISPLAY_NAME")]
        public string DisplayName { get; set; }

        [Column("TYPE")]
        public string DiscUrn { get; set; }

        [Column("CUSTOM_ALLOCATION")]
        public string CustomAllocation { get; set; }

        [Ignore]
        public int IsActive { get; set; }

        [Column("COMMENTS")]
        public string Notes { get; set; }

        [Column("MEDIA_SERVER")]
        public string MediaServer { get; set; }

        [Column("CARRIER")]
        public string Carrier { get; set; }

        [Column("DNIS")]
        public string Dnis { get; set; }

        [Column("MOBILE_DNIS")]
        public string MobileDnis { get; set; }

        [Ignore]
        public string PartnerUri { get; set; }
    }
}