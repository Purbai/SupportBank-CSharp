using NLog;

public class Transaction
{

    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public DateOnly TxnDate { get; set; }
    public string FromPerson { get; set; } = "";
    public string ToPerson { get; set; } = "";
    public string Narrative { get; set; } = "";
    public decimal Amount { get; set; }
}
