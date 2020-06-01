# KekeDataStore.Binary

KekeDataStore.Binary is a simple in-memory binary file, data storage (NuGet Package) .NET Standard 2.0.
By default its in-memory data store but it can be used 


[![NuGet](https://img.shields.io/nuget/v/KekeDataStore.Binary.svg)](https://www.nuget.org/packages/KekeDataStore.Binary/)
[![NuGetCount](https://img.shields.io/nuget/dt/KekeDataStore.Binary.svg
)](https://www.nuget.org/packages/KekeDataStore.Binary/)


Simple data store that saves the data in JSON format to a single file.

* Small API with basic functionality that is needed for handling CRUD operations.
* Thread-safe.
* Works with strongly typed data
* Synchronous and asynchronous methods
* Data is stored in separated Binary files
  * Easy to initialize
  * Very fast and can be used in-memory
  * Easy to edit
  * Perfect for small apps and prototyping
* [.NET Standard 2.0](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md)
  * .NET Core 3.1 & .NET Framework 4.8

## Installation

You can install the latest version via [NuGet](https://www.nuget.org/packages/KekeDataStore.Binary/).

```sh
# Package Manager
PM> Install-Package KekeDataStore.Binary -Version 1.0.2

# .NET CLI
> dotnet add package KekeDataStore.Binary --version 1.0.2

# PackageReference
<PackageReference Include="KekeDataStore.Binary" Version="1.0.2" />

# Paket CLI
paket add KekeDataStore.Binary --version 1.0.2
```

## Usage

### Declaring Entities
Data is stored in a Key-Value form so each class Entity that you want to be saved in the store, must implement ``IBaseEntity.cs`` interface from ``KekeDataStore.Binary`` project and must be marked with ``SerializableAttribute``.
#### Example
```C#
// IBaseEntity.cs 
namespace KekeDataStore.Binary
{
    public interface IBaseEntity
    {
        Guid Id { get; set; }
    }
}


[Serializable]
public class Contact : IBaseEntity
{
    public Guid Id { get; set; }
    public Person Person { get; set; }
    public Phone Phone { get; set; }
}

[Serializable]
public class Person
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

[Serializable]
public class Phone
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; }
    public PhoneType Type { get; set; }
}

[Serializable]
public enum PhoneType : byte
{
    WORK,
    CELLPHONE,
    HOME
}
```
You can initialize an instance in your method like: 

```csharp
var dataStore = new BinaryDataStore<Contact>();
```
And a file with the filename ``Contacts.bin`` will be generated on the client's ``/bin/Debug`` project path.


### Example
```csharp
using KekeDataStore.Binary;

// Open database (create new file in the bin/Debug directory called "Contacts.bin")
var dataStore = new BinaryDataStore<Contact>();

// Create new contact instance
var contact = new Contact { Person = new Person { FirstName = "Skerdi", LastName = "Berberi" },
                            Phone = new Phone { PhoneNumber = "0684555555", Type = PhoneType.HOME } }

// Insert new contact
// Id is generated automatically when it's not specified
dataStore.Create(contact);

// Update contact
contact.Person.FirstName = "TestFirstName";

// Updates the element and then returns the updated element
var updatedContact = dataStore.Update(contact.Id.ToString(), contact);

// Save data to binary file. If you never call SaveChanges() you can use the api like an in-memory database
dataStore.SaveChanges();

// Get all items in the store
var allcontacts = dataStore.GetAll();

// Get all items with strategy design pattern
// the example below shows that you can use it for search functionality
var contactsWithPredicate = dataStore.Get(x => x.Person.FirstName.Contains("Sk")); 

// Get element by Id (Key)
var contactById = dataStore.GetById(contact.Id.ToString());

// Gets single item, with predicate. If the predicate returns more than a single value, it throws an error.
var singleContact = dataStore.GetSingle(x => x.Id == new Guid("bba9dcb0-563e-42b6-a2ff-6554ebba87f2"));

// Orders contacts by person's first name in ascending order, then by lastname ascending
var orderedContacts = dataStore.AsQueryable().OrderBy("Person.FirstName").ThenBy("Person.LastName");

// Orders contacts by person's last name in descending order, then it sorts by firstname descending 
var orderedContacts = dataStore.AsQueryable().OrderByDescending("Person.LastName").ThenBy("Person.FirstName");

// Use LINQ to query items, also you can use LINQ with GetAll, Get ...
var queryContacts = dataStore.AsQueryable().Where(x => x.Person.FirstName == "Skerdi");

// Removes the item from the store
var deleteContact = dataStore.Delete(contact.Id.ToString());

// Removes all elements 
dataStore.Truncate();
```

### Visual Data
For testing purposes you can also use a visualizer like ``DrawContactTable.cs``. (I created this class only to test data for 'Contact' model.) 
````csharp
// Generates automatically visuals of the contacts collection that you pass.
// Path parameter determines the path of the '.txt' file.It writes the lines to a '.txt' file asynchronously 
await DrawContactTable.VisualizeToFileAsync(contacts, @"C:\..Path\filename.txt");
````

```
                                                                        Contacts Table

+-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
|  Row_Count   |                   Id                   |         First Name          |          Last Name          |        Phone Number         |         Phone Type          |
+-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
|      1       |  772e1e23-56be-4871-a429-7fb761317421  |        TestFirstName        |        TestLastName         |         0684594960          |          CELLPHONE          |
|      2       |  4d4e1d6e-29c8-404e-8f07-e54bf5ad75d5  |            Jane             |            Doe              |         0684594960          |          CELLPHONE          |
|      3       |  ab37f289-1b0e-48dd-b8d4-fba92192080f  |           Skerdi            |          Berberi            |         0684594960          |          CELLPHONE          |
|      4       |  bba9dcb0-563e-42b6-a2ff-6554ebba87f2  |            Test1            |           Test1             |         0684594960          |          CELLPHONE          |
|      6       |  5582fbf0-a6b2-474f-89a6-3edd96133b21  |            First            |            Last             |         0684594960          |          CELLPHONE          |
|      7       |  3e8b0fea-220c-45c9-b731-383bca815345  |            John             |            Doe              |         0684594960          |          CELLPHONE          |
+-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
 Count: 7                                                                                                                                             Load time: 3 ms
```
## Best Practices
#### Depenency Injection

Best practice when you are not going to save data into file is to use ``AddScoped``. You can use ``AddTransient`` too but it's not recommended.
You can register it in a Depenency Injection framework like this:

```csharp
using KekeDataStore.Binary;

// Set up DI
var serviceProvider = new ServiceCollection()
                    .AddScoped(typeof(IBinaryDataStore<>), typeof(BinaryDataStore<>)).BuildServiceProvider();

//Consume it like:
var contactsDataStore = serviceProvider.GetService<IBinaryDataStore<Contact>>();
var testsDataStore = serviceProvider.GetService<IBinaryDataStore<TestModel>>();
```

Best practice when you are going to save data into binary file is to use ``AddSingleton`` for injection.
```csharp
using KekeDataStore.Binary;

// Set up DI
var serviceProvider = new ServiceCollection()
                    .AddSingleton(typeof(IBinaryDataStore<>), typeof(BinaryDataStore<>)).BuildServiceProvider();

//Consume it like:
var contactsDataStore = serviceProvider.GetService<IBinaryDataStore<Contact>>();
var testsDataStore = serviceProvider.GetService<IBinaryDataStore<TestModel>>();
```


## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

Licensed under the [MIT](LICENSE) License.