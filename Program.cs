List<Person> people = new List<Person>();
List<Transaction> transactions = new List<Transaction>();
int personId = 0;

try {
    // Open the text file using a stream reader.
        string filePath = "Transactions2014.csv";
        List<Transaction> dataList = new List<Transaction>();
        using (var reader = new StreamReader(filePath))
        {
        // want to ignore the first line since it is the header
        reader.ReadLine();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                var data = new Transaction
                {
                    TxnDate = DateOnly.Parse(values[0]),
                    FromPerson = values[1],
                    ToPerson = values[2],
                    Narrative = values[3],
                    Amount = Decimal.Parse(values[4])
  
                };

                dataList.Add(data);
            }
foreach (var transaction in dataList) 
{
  Console.WriteLine(transaction.FromPerson);
  Console.WriteLine(transaction.ToPerson);
}


}
}
catch (IOException e)
{
    Console.WriteLine("The file could not be read");
    Console.WriteLine(e.Message);
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class Transaction
{
    public DateOnly TxnDate { get; set; }
    public string FromPerson { get; set; } ="";
    public string ToPerson { get; set; } ="";
    public string Narrative { get; set; } ="";
    public decimal Amount { get; set; }
}