using System.Data.Common;
using System.Dynamic;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Xml;

var config = new LoggingConfiguration();
var target = new FileTarget { FileName = @"C:\training\SupportBank-CSharp\log\SupportBank.log", Layout = @"${longdate} ${level} - ${logger}: ${message}" };
config.AddTarget("File Logger", target);
config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
LogManager.Configuration = config;

List<Person> people = new List<Person>();
//List<Transaction> transactions = new List<Transaction>();
List<NewTransaction> newTransactions = new List<NewTransaction>();
List<Account> accounts = new List<Account>();
List<Transaction> dataList = new List<Transaction>();
try
{
    // Open the text file using a stream reader.
    string filePath = "";
    // get user to enter the filename
    filePath = GetFileName();
    int index = filePath.IndexOf(".") + 1;
    string fileExtension = filePath.Substring(index);
    Console.WriteLine($"fileExtension: {fileExtension}");
    switch(fileExtension)
    {
        case "json":
        ReadJsonFile(filePath, dataList);
        break;
        case "csv":
        ReadCSVTxnFile(filePath, dataList);
        break;
   case "xml":
        readXMLFile(filePath, dataList);
        break;   
    default:
        Console.WriteLine("File name not provided - exiting ");
        // file name not provided so exit
        return;
    };

    // create list of people from the transaction table (for both the fromPerson and toPerson)
    CreateNewPerson("From", dataList);
    CreateNewPerson("To", dataList);

    // create a new transation class replacing person names with person IDs (that were generated above)
    CreateTxnWithPersonID(dataList);

    // call the function to list the accounts
    CreateAccountSummary();

    // Get the user to select which report to produce (on screen or to file)
    int option = GetReportOutputOption();

    string ExportFileName = "";
    switch (option)
    {
        case 1:
        // output report data to screen for summary
        PrintReport("ALL", 0, dataList, people, accounts);
        break;
    case 2:
        int personId = GetPersonName();
        PrintReport("Person", personId, dataList, people, accounts);
        break;

    case 3:
        // export the transactions for a person to a file
        // get file name from the user
        ExportFileName = GetExportFileName();
        ExportAccountSummaryToFile(ExportFileName, accounts);
        break;
    case 4:
        // export the account summary to a file
        ExportFileName = GetExportFileName();
        ExportPersonTxnToFile(ExportFileName, dataList);
        break;
    default:
        break;
    }

}
catch (IOException e)
{
    Console.WriteLine("The file could not be read");
    Console.WriteLine(e.Message);
}

// Functions
int GetReportOutputOption()
{
    Console.WriteLine("Select option:");
    Console.WriteLine("1. List Account Summary");
    Console.WriteLine("2. List Detail Account for a Person");
    Console.WriteLine("3. Extract Account Summary to a CSV file");
    Console.WriteLine("4. Extract Detail Account for a Person to a CSV file");
    Console.Write("Option:");
    int option = int.Parse(Console.ReadLine());
    if (option < 1 || option >4)
    {
        // valid option not selected 
        option = 0;  // default option to 0
    }
    return option;
}

string GetFileName()
{
    var log = LogManager.GetCurrentClassLogger();
    Console.WriteLine("Enter transaction filename to export data from (including path and extension)");
    string fileName = Console.ReadLine();
    // check if this file exist
    if (!File.Exists(fileName))
    {
        // we have some bad data in the file
        log.Warn($"Bad file name provided : {fileName} ");
        Console.WriteLine("File name not valid/provided - see .\\log\\SupportBank.log");
        fileName = "";
    }
    else
    {
        log.Info($"Valid file - Reading file name : {fileName} ");
    }
    return fileName;
}

string GetExportFileName()
{
    var log = LogManager.GetCurrentClassLogger();
    Console.WriteLine("Enter filename to export to (including extension)");
    string fileName = Console.ReadLine();
    Console.WriteLine("Enter path to write the export file to ");
    string filePath = Console.ReadLine();
    string ExportFileName = string.Concat(filePath, fileName);
    // check if this file exist
    if (File.Exists(ExportFileName))
    {
        // we have some bad data in the file
        log.Warn($"Export file name provided : {ExportFileName} ");
        Console.WriteLine("File name already exists - see .\\log\\SupportBank.log");
        ExportFileName = "";
    }
    else
    {
        log.Info($"Exporting data to file name : {ExportFileName} ");
    }
    return ExportFileName;
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
        if (person != null)
        {
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


void ReadCSVTxnFile(string filePath, List<Transaction> dataList)
{
    var log = LogManager.GetCurrentClassLogger();
    using (var reader = new StreamReader(filePath))
    {
        // want to ignore the first line since it is the header
        reader.ReadLine();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(',');
            // check if value is decimal
            try
            {
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
            catch
            {
                // we have some bad data in the file
                log.Warn($"Invalid record -  date: {values[0]}, loan from person {values[1]} loaned to {values[2]} for {values[3]} amount: {values[4]} ");
                Console.WriteLine("Found a bad row - Pls check the log\\SupportBank.log");
            };
        }
    }
}

void ReadJsonFile(string filePath, List<Transaction> dataList)
{
    var log = LogManager.GetCurrentClassLogger();
    using (StreamReader r = new StreamReader(filePath))
    {
        string json = r.ReadToEnd();
        // Console.WriteLine($"{json}");
        List<TransactionJson> items = JsonConvert.DeserializeObject<List<TransactionJson>>(json);
        foreach (var item in items)
        {
            try
            {
                //Console.WriteLine($"{item.FromAccount}");
                var data = new Transaction
                {
                    TxnDate = DateOnly.Parse(item.Date),
                    FromPerson = item.FromAccount,
                    ToPerson = item.ToAccount,
                    Narrative = item.Narrative,
                    Amount = item.Amount
                };
                dataList.Add(data);
            }
            catch
            {
                // we have some bad data in the file
                log.Warn($"Invalid record -  date: {item.Date}, loan from person {item.FromAccount} loaned to {item.ToAccount} for {item.Narrative} amount: {item.Amount} ");
                Console.WriteLine("Found a bad row - Pls check the log\\SupportBank.log");
            };
        }
    }
}

void readXMLFile(string filePath, List<Transaction> dataList)
{
    DateTime convertedDate = new DateTime();
    string description = "";
    decimal value = 0;
    string fromPerson = "";
    string toPerson = "";

    using (XmlReader reader = XmlReader.Create(filePath))
    {
        while (reader.Read())
        {
            if (reader.IsStartElement())
            {
                switch (reader.Name.ToString())
                {
                    case "SupportTransaction":
                        int excelDate = int.Parse(reader.GetAttribute("Date"));
                        DateTime baseDate = new DateTime(1900, 1, 1);
                        convertedDate = baseDate.AddDays(excelDate - 2); // Subtract 2 to account for Excel's date system quirks
                        break;
                    case "Description":
                        description = reader.ReadElementContentAsString();
                        break;
                    case "Value":
                        value = reader.ReadElementContentAsDecimal();
                        break;
                    case "From":
                        fromPerson = reader.ReadElementContentAsString();
                        break;
                    case "To":
                        toPerson = reader.ReadElementContentAsString();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (reader.Name.ToString() == "SupportTransaction")
                {
                    dataList.Add(
                        new Transaction()
                        {
                            TxnDate = DateOnly.FromDateTime(convertedDate),
                            Narrative = description,
                            Amount = value,
                            FromPerson = fromPerson,
                            ToPerson = toPerson
                        }
                    );
                }
            }
        }
    }
}

void ExportPersonTxnToFile(string fileName, List<Transaction> dataList)
{
    using (StreamWriter exportedFile = new StreamWriter(fileName))
    {
        // write out a header row...
        exportedFile.WriteLine("Transaction Date, From Person, To Person, Narrative, Amount");
        foreach (Transaction transaction in dataList)
        {
            exportedFile.WriteLine($"{transaction.TxnDate.ToShortDateString()},{transaction.FromPerson},{transaction.ToPerson},{transaction.Narrative},{transaction.Amount}");
        }
    }
}

void ExportAccountSummaryToFile(string fileName, List<Account> accounts)
{
    using (StreamWriter exportedFile = new StreamWriter(fileName))
    {
        // write out a header row...
        exportedFile.WriteLine("Person ID, Person Name, Amount Owed, Amount Owes");
        foreach (Account account in accounts)
        {
            Person? accountPerson = people.Find(p => p.Id == account.PersonId);
            exportedFile.WriteLine($"{account.PersonId},{accountPerson?.Name},{account.AmountOwed},{account.AmountOwes}");
        }
    }
}

void CreateTxnWithPersonID(List<Transaction> dataList)
{
    foreach (var transaction in dataList)
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

    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public DateOnly TxnDate { get; set; }
    public string FromPerson { get; set; } = "";
    public string ToPerson { get; set; } = "";
    public string Narrative { get; set; } = "";
    public decimal Amount { get; set; }
}
public class TransactionJson
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    public string Date { get; set; }
    public string FromAccount { get; set; } = "";
    public string ToAccount { get; set; } = "";
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