namespace Api.Domain.Constants
{
    public static class LangCodes
    {
        public const string LangBack = "lang-back";
        public const string LangApprove = "lang-approve";
        public const string LangSend = "lang-send";
        public const string LangCancel = "lang-cancel";

        public const string PassInvalid = "pass-invalid";
        public const string PassLength = "pass-length";
        public const string PassNumber = "pass-number";
        public const string PassUpperCase = "pass-upper-case";
        public const string PassRepetitive = "pass-repetitive";
		public const string PassNotMatch = "password-not-match";

		public const string InputEmpty = "input-empty";
        public const string InputInvalid = "input-invalid";
        public const string InputInvalidType = "input-invalid-type";
        public const string InputInvalidFormat = "input-invalid-format";
        public const string InputInvalidCode = "input-invalid-code";
        public const string InputInvalidDateRange = "input-invalid-daterange";
        public const string InvalidCodeVerification = "invalid-code-verification";

		public const string MessageSent = "message-sent";
        public const string MessageUnsent = "message-unsent";

        public const string EmailInvalid = "email-invalid";
        public const string EmailVerified = "email-verified";
        public const string EmailUnverified = "email-unverified";
        public const string EmailExpired = "email-expired";

        public const string ProcessSuccess = "process-success";
        public const string ProcessFailed = "process-failed";

        public const string Duplicate = "duplicate";
		public const string DuplicateUsername = "duplicate-username";
		public const string DuplicateEmailAdress = "duplicate-email-address";
		public const string DuplicatePhoneNumber = "duplicate-phone-number";
		public const string UnknownException = "unknown-exception";
        public const string NotFound = "not-found";

        public const string LoginExceeded = "login-exeeded";
        public const string LoginInvalid = "login-invalid";
        public const string LoginAlready = "login-already";

        public const string DateExpired = "date-expired";

		public const string UploadExtInvalid = "upload-ext-invalid";

		public const string CategoryTitleExist = "category-title-exist";
		public const string DocumentTitleExist = "document-title-exist";
		public const string DocumentFileNameExist = "document-file-name-exist";
		public const string PasswordChanged = "password-changed";
		public const string AttachmentFilesItemlistEmpty = "itemlist-files-empty";

        public const string PendingDocument = "pending-approval";
		public const string GroupEveryone = "group-everyone";

		public const string DocumentSameReference = "document-same-references";
		public const string NotTheOwner = "not-the-owner";
		public const string NotTheOwnerOrHaveNoPriv = "not-the-owner-no-priv";

        public const string AttributeNameExist = "attribute-name-exist";
		public const string RejectReasonIsEmpty = "reject-reason-empty";

        public const string UserMaxReached = "user-max-reached";
	}
}