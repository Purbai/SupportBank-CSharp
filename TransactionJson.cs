using NLog;

public class TransactionJson
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    public string Date { get; set; }
    public string FromAccount { get; set; } = "";
    public string ToAccount { get; set; } = "";
    public string Narrative { get; set; } = "";
    public decimal Amount { get; set; }
}
