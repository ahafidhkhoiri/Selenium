using Ganss.Excel;
using System;
namespace CygnusAutomation.Cygnus.Models
{
    public class UserModel
    {
       	[Column("FIRST NAME")]
        public String FirstName { get; set; }

        [Column("LAST NAME")]
		public String LastName { get; set; }

        [Column("E-MAIL ADDRESS")]
		public String EmailAddress { get; set; }

        [Column("BUSINESS PHONE")]
		public String BusinessPhone { get; set; }

        [Column("MOBILE PHONE")]
		public String MobilePhone { get; set; }

        [Column("BILLING GROUP")]
		public String BillingGroup { get; set; }

        [Column("OFFICE")]
		public String Office { get; set; }

        [Column("DIAL-IN TEMPLATE")]
        public String DialInTemplate { get; set; }

        [Column("BILLING_CODE")]
		public String BillingCode{ get; set; }

        [Column("LEADER_STARTS_CONFERENCE")]
		public String LeaderStartsConference { get; set; }

        [Column("LEADER_ENDS_CONFERENCE")]
		public String LeaderEndsConference { get; set; }

        [Column("ROLL_CALL")]
		public String RollCall { get; set; }

        [Column("LECTURE_MODE")]
		public String LectureMode { get; set; }

        [Column("ALLOW PARTICIPANT UN MUTE")]
		public String AllowParticipantUnMute { get; set; }

        [Column("JOIN_TONES")]
		public String JoinTones { get; set; }

        [Column("AUTO_RECORD")]
		public String AutoRecord { get; set; }

        [Column("UFK_USER")]
		public String UfkUser { get; set; }
    }
}
