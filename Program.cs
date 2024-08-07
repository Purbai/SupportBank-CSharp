using System.Data.Common;
using System.Dynamic;
//using Newtonsoft.Json;

List<Person> people = new List<Person>();
List<Transaction> transactions = new List<Transaction>();
List<NewTransaction> newTransactions = new List<NewTransaction>();
List<Account> accounts = new List<Account>();

try
{
    // Open the text file using a stream reader.
    string filePath = "Transactions2014.csv";
    //string filePath = "Transactions2013.json";
    List<Transaction> dataList = new List<Transaction>();
    ReadCSVTxnFile (filePath, dataList);

    // create list of person from the transaction table (for both the fromPerson and toPerson)
    CreateNewPerson("From", dataList);
    CreateNewPerson("To", dataList);

    // create a new transation class replacing person names with person IDs (that were generated above)
    TxnWithPersonID (dataList);

    // call the function to list the accounts
    CreateAccountSummary();

    // Get the user input
    int option = GetUserSelection();
    Console.WriteLine($"Option selected: {option}");

    if (option == 2)
    {
        int personId = GetPersonName();
        Console.WriteLine(personId);
        PrintReport("Person", personId, dataList, people, accounts);    
    }
    else
    {
        PrintReport("ALL", 0 , dataList, people, accounts);  
    }
}
catch (IOException e)
{
    Console.WriteLine("The file could not be read");
    Console.WriteLine(e.Message);
}

// Functions
int GetUserSelection()
{
    Console.WriteLine("Select option:");
    Console.WriteLine("1. List Account Summary");
    Console.WriteLine("2. List Detail Account for a Person");
    Console.Write("Option:");
    int option = int.Parse(Console.ReadLine());

    return option;
}

int GetPersonName()
{
    int personId = 0;
    Person person;

    while (true)
    {
        // should try to do a dropdown list that user can select from - we already have the list of names in Person class
        Console.Write("Enter Person Name (enter Exit to quit): ");
        string personName = Console.ReadLine();

        if (personName == "Exit") break;

        person = people.Find(p => p.Name == personName);
        if (person != null) {
            personId = person.Id;
            break;
        }
    }

    return personId;
}

void PrintReport(string listType, int personId, List<Transaction> dataList, List<Person> people, List<Account> accounts)
{

 
    if (listType == "ALL")
    {
        foreach (var account in accounts)
        {
            Person? person = people.Find(p => p.Id == account.PersonId);
            Console.WriteLine($"Person Id: {account.PersonId} Name: {person.Name} , Amount Owes: {account.AmountOwes} , Amount Owed: {account.AmountOwed}");
        }
    }
    else
    {
        // list all transactions for a person
        Person? person = people.Find(p => p.Id == personId);
        List<Transaction> filteredTxn = dataList.FindAll(txn => txn.FromPerson == person.Name || txn.ToPerson == person.Name);
        foreach (var transaction in filteredTxn)
        {
            Console.WriteLine($"Transaction Date: {transaction.TxnDate} From Name: {transaction.FromPerson} , To Person: {transaction.ToPerson} , Narrative: {transaction.Narrative}, Amount Owed: {transaction.Amount}");    
        }
    }
}


void ReadCSVTxnFile (string filePath, List<Transaction> dataList)
{
    using (var reader = new StreamReader(filePath))
    {
        // want to ignore the first line since it is the header
        reader.ReadLine();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(',');
            // check if value is decimal
            // if (decimal.TryParse(values[4], out _))
            // {
            // it's decimal
            var data = new Transaction
            {
                TxnDate = DateOnly.Parse(values[0]),
                FromPerson = values[1],
                ToPerson = values[2],
                Narrative = values[3],

                    Amount = Decimal.Parse(values[4])


            };
            dataList.Add(data);
            // }
            // else
            // {
            //     Console.WriteLine($"Invalid record date: {values[0]}, loan from person {values[1]} loaned to {values[2]} for {values[3]} amount: {values[4]} ");
            // };

        }
    }
}

/* void ReadJsonFile(string filePath, List<Transaction> dataList)
{
    using (StreamReader r = new StreamReader(filePath))
    {
        string json = r.ReadToEnd();
        List<Transaction> items = JsonConvert.DeserializeObject<List<Transaction>>(json);
        foreach (var item in items)
        {
            //Console.WriteLine($"{item.Id} {item.Name}");
            var data = new Transaction
            {
                TxnDate = DateOnly.Parse(item.date),
                FromPerson = item.FromAccount,
                ToPerson = item.toAccount,
                Narrative = item.Narrative,
                Amount = Decimal.Parse(item.Amount)
            };

            dataList.Add(data);
        }
    }
} */

void TxnWithPersonID(List<Transaction> dataList)
{        foreach (var transaction in dataList)
        {
            // find the from & to person name in the Person class
            // if name found, get Person ID

            Person fromPerson = people.Find(p => p.Name == transaction.FromPerson);
            Person toPerson = people.Find(p => p.Name == transaction.ToPerson);

            // add new record to NewTransaction class (TxnDate, fromPersonID,toPersonID, Narrative, Amount)
            NewTransaction newTxn = new()
            {
                TxnDate = transaction.TxnDate,
                FromPersonID = fromPerson.Id,
                ToPersonID = toPerson.Id,
                Narrative = transaction.Narrative,
                Amount = transaction.Amount
            };
            newTransactions.Add(newTxn);
        }
}

void CreateNewPerson(string personType, List<Transaction> dataList)
{
    List<Transaction> sortedTxn;
        // sort the transaction data by From Person and add the person to Person class if not already there
        if (personType == "To")
        {
            sortedTxn = dataList.OrderBy(o => o.ToPerson).ToList();
        }
        else
        {
           sortedTxn = dataList.OrderBy(o => o.FromPerson).ToList(); 
        };

        // List<Person> people = new List<Person>;
        foreach (var transaction in sortedTxn)
        {
            string nameToCheck = transaction.FromPerson;
            bool nameExists = people.Any(p => p.Name == nameToCheck);
            // add the person if not exist (don't need to do anything with ID since we are autogenerating)
            if (!nameExists)
            {
                people.Add(new Person(nameToCheck));
                // Console.WriteLine($"From Person with name {nameToCheck} added.");
            };
        }
}

void CreateAccountSummary()
{

        // loop round each person, and get total amount owed and owes from the transaction file (total amount)
        foreach (Person person in people)
        {
            Account account = new Account()
            {
                PersonId = person.Id,
                AmountOwes = newTransactions.FindAll(txn => txn.ToPersonID == person.Id).Sum(txn => txn.Amount),
                AmountOwed = newTransactions.FindAll(txn => txn.FromPersonID == person.Id).Sum(txn => txn.Amount),
            };
            accounts.Add(account);
            //Console.WriteLine($"Person Id: {account.PersonId} Name: {person.Name} , Amount Owes: {account.AmountOwes} , Amount Owed: {account.AmountOwed}");
        }

}

// Data Structures
public class Person
{
    private static int lastId = 0;
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Person(string name)
    {
        Id = ++lastId;
        Name = name;
    }
}

public class Transaction
{
    public DateOnly TxnDate { get; set; }
    public string FromPerson { get; set; } = "";
    public string ToPerson { get; set; } = "";
    public string Narrative { get; set; } = "";
    public decimal Amount { get; set; }
}

public class NewTransaction
{
    public DateOnly TxnDate { get; set; }
    public int FromPersonID { get; set; }
    public int ToPersonID { get; set; }
    public string Narrative { get; set; } = "";
    public decimal Amount { get; set; }
}

public class Account
{
    public int PersonId { get; set; }
    public decimal AmountOwed { get; set; }
    public decimal AmountOwes { get; set; }
}