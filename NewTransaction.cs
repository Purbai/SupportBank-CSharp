public class NewTransaction
{
    public DateOnly TxnDate { get; set; }
    public int FromPersonID { get; set; }
    public int ToPersonID { get; set; }
    public string Narrative { get; set; } = "";
    public decimal Amount { get; set; }
}
