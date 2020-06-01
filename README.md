# KekeDataStore.Binary

KekeDataStore.Binary is a simple in-memory binary file, data storage (NuGet Package) .NET Standard 2.0.
By default its in-memory data store but it can be used 


[![NuGet](https://img.shields.io/nuget/v/KekeDataStore.Binary.svg)](https://www.nuget.org/packages/KekeDataStore.Binary/)
[![NuGetCount](https://img.shields.io/nuget/dt/KekeDataStore.Binary.svg
)](https://www.nuget.org/packages/KekeDataStore.Binary/)


Simple data store that saves the data in JSON format to a single file.

* Small API with basic functionality that is needed for handling CRUD operations.
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
Each class Entity that you want to be saved in the store, must implement ``IBaseEntity.cs`` interface and must be marked with ``SerializableAttribute``.
#### Example
```C#
public interface IBaseEntity
{
    Guid Id { get; set; }
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
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

[Serializable]
public class Phone
{
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

### Depenency Injection

You can register it in a Depenency Injection framework like this:

```csharp
using KekeDataStore.Binary;

// Set up DI
var serviceProvider = new ServiceCollection()
                    .AddScoped(typeof(IBinaryDataStore<>), typeof(BinaryDataStore<>)).BuildServiceProvider();

//Consume it like:
var contactsDataStore = serviceProvider.GetService<IBinaryDataStore<Contact>>();
var peopleDataStore = serviceProvider.GetService<IBinaryDataStore<Person>>();
var phoneDataStore = serviceProvider.GetService<IBinaryDataStore<Phone>>();
```

or, declare it like this and consume it the same way:
```csharp
using KekeDataStore.Binary;

// Set up DI
var serviceProvider = new ServiceCollection()
                    .AddScoped<IBinaryDataStore<Contact>, BinaryDataStore<Contact>>())
                    .AddScoped<IBinaryDataStore<Contact>, BinaryDataStore<Person>>())
                    .AddScoped<IBinaryDataStore<Contact>, BinaryDataStore<Phone>>()).BuildServiceProvider();

//Consume it like:
var contactsDataStore = serviceProvider.GetService<IBinaryDataStore<Contact>>();
var peopleDataStore = serviceProvider.GetService<IBinaryDataStore<Person>>();
var phoneDataStore = serviceProvider.GetService<IBinaryDataStore<Phone>>();
```

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

// get all items in the store
var allcontacts = dataStore.GetAll();

// Get all items with strategy design pattern
// the example below shows that you can use it for search functionality
var contactsWithPredicate = dataStore.Get(x => x.Person.FirstName.Contains("Sk")); 

// Get element by Id (Key)
var contactById = dataStore.GetById(contact.Id.ToString());

// Gets single item, with predicate
var singleContact = dataStore.GetSingle(x => x.Id == new Guid("bba9dcb0-563e-42b6-a2ff-6554ebba87f2"));

// Orders contacts by person's first name in ascending order, then by lastname ascending
var orderedContacts = dataStore.AsQueryable().OrderBy("Person.FirstName").ThenBy("Person.LastName");

// Orders contacts by person's last name in descending order, then it sorts by firstname descending 
var orderedContacts = dataStore.AsQueryable().OrderByDescending("Person.LastName").ThenBy("Person.FirstName");

// Use LINQ to query items, also you can use LINQ with GetAll, Get ...
var queryContacts = dataStore.AsQueryable().Where(x => x.Person.FirstName == "Skerdi");

// Removes the element from the store
var deleteContact = dataStore.Delete(contact.Id.ToString());

// Removes all elements 
dataStore.Truncate();

```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

Licensed under the [MIT](LICENSE) License.