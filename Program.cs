using System.Data.Common;
using System.Dynamic;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Xml;
using System.ComponentModel;
using System.Runtime.InteropServices;

var config = new LoggingConfiguration();
// output to .\log\SupportBank.log file - each log will have a long datetime and type of log msg, name of the projectfile and the msg
var target = new FileTarget { FileName = @"C:\training\SupportBank-CSharp\log\SupportBank.log", Layout = @"${longdate} ${level} - ${logger}: ${message}" };
config.AddTarget("File Logger", target);
config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
LogManager.Configuration = config;

List<Person> people = new List<Person>();
List<NewTransaction> newTransactions = new List<NewTransaction>();
List<Account> accounts = new List<Account>();
List<Transaction> transactionList = new List<Transaction>();
try
{
    // Open the text file using a stream reader.
    string filePath = "";
    // get user to enter the filename and find the extension since processing will be different for different types of extensions
    // filePath = GetFileName();
    filePath = GetFileNameFromDir();
    int index = filePath.IndexOf(".") + 1;
    string fileExtension = filePath.Substring(index);
    switch (fileExtension)
    {
        case "json":
            ReadJsonFile(filePath, transactionList);
            break;
        case "csv":
            ReadCSVTxnFile(filePath, transactionList);
            break;
        case "xml":
            readXMLFile(filePath, transactionList);
            break;
        default:
            Console.WriteLine("File name not provided - exiting ");
            // file name not provided so exit
            return;
    };

    // create list of people from the transaction table (for both the fromPerson and toPerson)
    CreateNewPerson("From", transactionList);
    CreateNewPerson("To", transactionList);

    // create a new transation class replacing person names with person IDs (that were generated above)
    CreateTxnWithPersonID(transactionList);

    // call the function to list the accounts
    CreateAccountSummary();

    // Get the user to select which report to produce (on screen or to file)
    int option = GetReportOption();

    string exportFileName = "";
    switch (option)
    {
        case 0:
            // output report data to screen for summary
            PrintReport("Account", 0, transactionList, people, accounts);
            break;
        case 1:
            int personId = GetPersonName();
            PrintReport("Person", personId, transactionList, people, accounts);
            break;
        case 2:

            // get file name from the user
            exportFileName = GetExportFileName();
            if (exportFileName != "")
            {
                // export the account summary to a file
                ExportAccountSummaryToFile(exportFileName, accounts);
            }
            break;
        case 3:
            // get file name from the user
            exportFileName = GetExportFileName();
            if (exportFileName != "")
            {
                // export the transactions for a person to a file
                ExportPersonTxnToFile(exportFileName, transactionList);
            }
            break;
        default:
            break;
    }
}
catch (IOException e)
{
    var log = LogManager.GetCurrentClassLogger();
    Console.WriteLine("Exception found - exiting");
    log.Info($" Exception found - exiting {e.Message} ");
}

// Functions
int GetReportOption()
{
    // list the report types
    string[] reportOptions = [];
    Array.Resize(ref reportOptions, 4);
    reportOptions[0] = "List Account Summary to screen";
    reportOptions[1] = "List Detail Account for a Person to screen";
    reportOptions[2] = "Extract Account Summary to a CSV fil";
    reportOptions[3] = "Extract Detail Account for a Person to a CSV file";

    // call the downdrop function to allow user to select report that they want to produce
    string selectedOption = DropdownList(reportOptions);
    int option = Array.IndexOf(reportOptions,selectedOption);

    return option; 
}

void InputNewTxns()
{
// Create a loop to read the input from user - need someway for the user to exit the loop after they have finished entering new transactions
// read the input (txn date, from Person, to Person, Narrative, Ammount)
// none of the columns can be null, 
// if to/from person is not in people list, then ask user if they want to correct the input or it is a new person (if new, add to people list)
// we can ask the user to read all fields from same line (fields seperated by ,)

// if the txn data is the year that in any of the filenames from GetFileListFromDir(), then add the new transaction that file
// else create a new txn file (csv format) - filename = data\Transactions<year>.csv + header record 
// if year is before 2012, or year is 2014 or later, then format of file is CSV
// if year is 2013, then format is json
// if year is 2012, then format is xml

// run reports for impact years that the user has entered new txns

}

string GetFileNameFromDir()
{
    string directoryPath = "";
    var log = LogManager.GetCurrentClassLogger();
    // get directory path
    // get list of data files from the path
    // allow user to select on of the file name from the directory list or exit
    while (true)
    {
        Console.WriteLine("Enter the directory name (enter 'Exit' to quit): ");
        directoryPath = Console.ReadLine();

        if (directoryPath == "Exit")
            break;

        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine($"This directory, {directoryPath} does not exist");
            continue;
        }
        break;
    }

    string[] listOfFiles = Directory.GetFiles(directoryPath);

    return DropdownList(listOfFiles);
}


string DropdownList(string[] listOfOptions)
{
    Array.Resize(ref listOfOptions, listOfOptions.Length + 1);
     listOfOptions[listOfOptions.Length - 1] = "Exit";

    int selectedIndex = 0;
    Console.WriteLine($"Options List: {listOfOptions[listOfOptions.Length - 1]}");

  ConsoleKey key;
    do
    {
        Console.Clear();
        Console.WriteLine("Use the arrow keys to navigate and Enter to select:");

        for (int i = 0; i < listOfOptions.Length; i++)
        {
            if (i == selectedIndex)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.WriteLine(listOfOptions[i]);
            Console.ResetColor();
        }

        key = Console.ReadKey().Key;

        if (key == ConsoleKey.UpArrow)
        {
            selectedIndex = (selectedIndex == 0) ? listOfOptions.Length - 1 : selectedIndex - 1;
        }
        else if (key == ConsoleKey.DownArrow)
        {
            selectedIndex = (selectedIndex == listOfOptions.Length - 1) ? 0 : selectedIndex + 1;
        }

    } while (key != ConsoleKey.Enter);

    Console.WriteLine($"You selected: {listOfOptions[selectedIndex]}");  

    return listOfOptions[selectedIndex];
}

/* string GetFileName()
{

// get directory path
// get list of data files from the path
// allow user to select on of the file name from the directory list or exit

    var log = LogManager.GetCurrentClassLogger();
    Console.WriteLine("Enter transaction filename to import data from (including path and extension)");
    string fileName = Console.ReadLine();
    Console.WriteLine($"file name entered: {fileName}");
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
        log.Info($"Valid file - Reading data from file name : {fileName} ");
    }
    return fileName;
} */

string GetExportFileName()
{
    var log = LogManager.GetCurrentClassLogger();
    Console.WriteLine("Enter filename to export to (will generate CSV file)");
    string fileName = Console.ReadLine();
    Console.WriteLine("Enter path to write the export file to ");
    string filePath = Console.ReadLine();
    int slashFound = filePath.LastIndexOf("\\");
    string exportFileName = "";
    if (slashFound != -1)
        exportFileName = string.Concat(filePath, fileName, ".csv");
    else
        exportFileName = string.Concat(filePath, "\\", fileName, ".csv");
    // check if this file exist
    if (File.Exists(exportFileName))
    {
        // file already exist - hence warn user and exit
        log.Warn($"Export file name provided already exist : {exportFileName} ");
        Console.WriteLine("File name already exists - see .\\log\\SupportBank.log");
        exportFileName = "";
    }
    else
    {
        log.Info($"Exporting data to file name : {exportFileName} ");
    }
    return exportFileName;
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
        // Exit entered hence quit
        if (personName == "Exit") break;
        // check if the person is valid by checking against people list
        person = people.Find(p => p.Name == personName);
        if (person != null)
        {
            personId = person.Id;
            break;
        }
    }
    return personId;  // return the person ID
}

void PrintReport(string listType, int personId, List<Transaction> txnList, List<Person> people, List<Account> accounts)
{

    if (listType == "Account")
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
        List<Transaction> filteredTxn = txnList.FindAll(txn => txn.FromPerson == person.Name || txn.ToPerson == person.Name);
        foreach (var transaction in filteredTxn)
        {
            Console.WriteLine($"Transaction Date: {transaction.TxnDate} From Name: {transaction.FromPerson} , To Person: {transaction.ToPerson} , Narrative: {transaction.Narrative}, Amount Owed: {transaction.Amount}");
        }
    }
}


void ReadCSVTxnFile(string filePath, List<Transaction> txnList)
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
                txnList.Add(data);
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

void ReadJsonFile(string filePath, List<Transaction> txnList)
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
                txnList.Add(data);
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

void readXMLFile(string filePath, List<Transaction> txnList)
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
                    txnList.Add(
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

void ExportPersonTxnToFile(string fileName, List<Transaction> txnList)
{
    using (StreamWriter exportedFile = new StreamWriter(fileName))
    {
        // write out a header row...
        exportedFile.WriteLine("Transaction Date, From Person, To Person, Narrative, Amount");
        foreach (Transaction transaction in txnList)
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

void CreateTxnWithPersonID(List<Transaction> txnList)
{
    foreach (var transaction in txnList)
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

void CreateNewPerson(string personType, List<Transaction> txnList)
{
    List<Transaction> sortedTxn;
    // sort the transaction data by From Person and add the person to Person class if not already there
    if (personType == "To")
    {
        sortedTxn = txnList.OrderBy(o => o.ToPerson).ToList();
    }
    else
    {
        sortedTxn = txnList.OrderBy(o => o.FromPerson).ToList();
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