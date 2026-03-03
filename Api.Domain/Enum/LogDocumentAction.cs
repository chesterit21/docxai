using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Enum
{
    public static class LogDocumentAction
    {
        public const string InsertNewDocument = "Insert New Document";
		public const string IntitUploadForNewDocument = "Initial upload For New Document";
		public const string ViewDocument = "View Document";

		public const string EditDocument = "Update";
		public const string AddReminder = "Add Reminder";
		public const string AddShared = "Share";
		public const string DeleteShared = "Delete Share";
		public const string AddWorkflow = "Add Workflow";
		public const string DeleteWorkflow = "Delete Workflow";
		public const string DeleteWorkflowUser = "Delete Workflow User";
		public const string SendMail = "Send Mail";
		public const string UplaodNewVersion = "Uplaod New Version";
		public const string AddAttribute = "Add Attribute";
		public const string UpdateAttribute = "Update Attribute";
		public const string ViewFile = "Fiew File";
		public const string DownloadFile = "Download File";
		public const string Approve = "Approve";
		public const string Reject = "Reject";

		public const string ViewDocumentLog = "View Document Log";
	}
}
