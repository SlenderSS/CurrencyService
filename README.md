# Exchange Service

<p>This project is a Windows service designed to get 
the exchange rate using data from the National
Bank of Ukraine (NBU) API </p>

## Used libraries

 <p>**CsvHeplers** - A library for reading and writing CSV files. Extremely fast, flexible, and easy to use. Supports reading and writing of custom class objects. CsvHelper provides a simple and intuitive API for working with CSV files. It makes it easy to read and write data in CSV format without unnecessary complexity.</p>
 <p>**Serilog** - diagnostic logging library for .NET applications. It is easy to set up, has a clean API, and runs on all recent .NET platforms. While it's useful even in the simplest applications, Serilog's support for structured logging shines when instrumenting complex, distributed, and asynchronous applications and systems.</p>
    
## How it work`s

<p>The service receives the exchange rate and saves it in a file with the format specified in the configuration file (json, xml, or csv). This is done at the specified interval. Files are saved to the specified directory and for each new day a new directory is created with the date of creation, and during the day all files will be saved to this directory. The files will have the names specified in the configuration file + date and time of data acquisition. When changing the day, a new directory is created and the following files will be saved to the new one.</p>

![](img/exchangerates.png)

<p>Actions performed by the service, as well as exceptions that may occur during operation, are saved to a log file</p>

![](img/logs.png)

## How to register service

To use the service, you need:

1. Download and unzip the archive

2. Run Commmand Prompt as admin
3. Navigate to "C:\Windows\Microsoft.NET\Framework\v4.0.30319"
4. Than input to cmd
   `InstallUtil.exe "<path to folder>\Release\ExchangeService.exe"`
5. Press Enter

![](img/register_service.png)

2. Press 'Start'

## How to delete service

1. Run Commmand Prompt as admin
2. Than input to cmd
   `InstallUtil.exe -u "<path to folder>\Release\ExchangeService.exe"`
3. Press Enter

![](img/delete_service.png)

## Configure Service

Open `ExchangeService.exe.config` file and change parameters on `AppSettings` tag.

![](img/config.png)

If you want to change the configuration parameters, you need to stop the service if it is running.

## How to start service

1. Open Services and find nessesary one

![](img/start_service.png)
