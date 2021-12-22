namespace CheckinQrWeb.Core.Models
{
    public class DccQrValidationResult
    {
        public DccQrValidationResult()
        {
            //   Errors = new List<string>();
        }
        //public List<string> Errors { get; set; }
        //public bool IsValid => !Errors.Any();

        public string Content { get; internal set; }
    }
}
