using PX.Common;

namespace PLNSC
{
    [PXLocalizable]
    public static class CustomMessage
    {
        public const string NotMSO230 = "Could not execute MSO230";
        public const string NotMSM23BA = "Not MSM23BA Screen";
        public const string NotMSM220A = "Not MSM220A Screen";
        public const string NotMSM221A = "Not MSM221A Screen";
        public const string NotMSM232A = "Not MSM232A Screen";
        public const string NotMSM23EA = "Not MSM23EA Screen";
        public const string NotMSM500A = "Not MSM500A Screen";
        public const string NotMSM200A = "Not MSM200A Screen";
        public const string NotMSM905A = "Not MSM905A Screen";
        public const string NotMSO155 = "Not MSO155 Screen";
        public const string NotMSM156A = "Not MSM156A Screen";
        public const string EmptyVendor = "Vendor is empty";
        public const string RecordExists =  "Record Already Exist";
        public const string MoreThanFourChar = "Cannot Be  More Than 4 Characters";
        public const string BUMoreThanFourChar = "Base Unit Cannot Be  More Than 4 Characters";
        public const string SUMoreThanFourChar = "Sales Unit Cannot Be  More Than 4 Characters";
        public const string PUMoreThanFourChar = "Purchase Unit Cannot Be  More Than 4 Characters";
        public const string MoreThanFourChar5 = "Cannot Be  More Than 4 Characters";
        public const string LTMustNotZero = "Lead Time Must Be Greater Than Zero";
        public const string COGSAccountEmpty = "COGS Account Is Empty";
        public const string priceCodeRequired = "Payment Terms Can Not Be Empty";

        public const string FutureQuoteDate = "Quotation Order Date Cannot Be In The Future";
        public const string UsdoWaitApproval = "Unable to create Delivery Order, USDO still waiting for approval";
        public const string CustOrderEmpty = "Unable to create Delivery Order, Customer Order Number required on QT";

        public const string CustomerIDInvalid = "Cannot Be Less Or More Than 6 Characters";
        public const string BAccountCharInvalid = "is NOT an alphanumeric string. Please enter only letters or numbers.";

        public const string DateGreaterToday = "DATE MUST BE GREATER THAN OR EQUAL TO TODAY.";
        public const string DateLessToday = "DATE MUST BE LESS THAN OR EQUAL TO TODAY.";

        public const string PredoDateCantBeEmpty = "Pre-DO Date Cannot Be Empty";
        public const string PredoNbrCantBeEmpty = "Pre-DO Number Cannot Be Empty";
        public const string DoNbrCantBeEmpty = "DO Number Cannot Be Empty";
        public const string QtyMustGreaterZero = "Quantity Must Be Greater Than Zero";
    }
}
