# KekeDataStore.Binary

KekeDataStore.Binary is a simple in-memory binary file, data storage (NuGet Package) .NET Standard 2.1.
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



## Usage

You can register it in a Depenency Injection framework.

```C#
using KekeDataStore.Binary;

var serviceProvider = new ServiceCollection()
                    .AddScoped(typeof(IBinaryDataStore<>), typeof(BinaryDataStore<>))
                    .BuildServiceProvider();

var dataStore = serviceProvider.GetService<IBinaryDataStore<Contact>>();
```

Or you can use it like : 

```C#
var dataStore = new BinaryDataStore<Contact>();
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

Licensed under the [MIT](LICENSE) License.