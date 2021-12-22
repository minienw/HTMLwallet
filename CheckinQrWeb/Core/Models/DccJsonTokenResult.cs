namespace CheckinQrWeb.Core.Models
{
    public class DccJsonTokenResult
    {
        public DccJsonTokenResult()
        {
            Errors = new List<string>();
        }
        public List<string> Errors { get; }
        public bool IsValid
        {
            get
            {
                if (Errors == null || Errors.Count == 0) return true;

                return false;
            }
        }

    }
}