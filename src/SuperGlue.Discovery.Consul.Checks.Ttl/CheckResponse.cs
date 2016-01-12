namespace SuperGlue.Discovery.Consul.Checks.Ttl
{
    public class CheckResponse
    {
        public CheckResponse(CheckStatus status, string note = "")
        {
            Status = status;
            Note = note;
        }

        public CheckStatus Status { get; }
        public string Note { get; } 
    }
}