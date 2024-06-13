using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;



namespace CurrencyService
{
    public partial class Service1 : ServiceBase
    {

        private bool _canceled = false;

        public Service1()
        {
            InitializeComponent();
        }



        protected override async void OnStart(string[] args)
        {
            Debugger.Launch();

            GetConfiguration(out string format,
                             out string fileName,
                             out string baseUrl,
                             out string directoryName,
                             out string directoryPath,
                             out int interval);


            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .WriteTo.File(Path.Combine("", "logs.txt"), rollingInterval: RollingInterval.Day);
                builder.AddSerilog(loggerConfiguration.CreateLogger());
            });

            Service service = new Service(
                loggerFactory.CreateLogger<Service>(),
                @"C:\Users\Alex\Desktop\ExchangeRates"
                );

            await service.Run(baseUrl, fileName, format, interval);
        }


        private void GetConfiguration(out string format,
                                      out string fileName,
                                      out string baseUrl,
                                      out string directoryName,
                                      out string directoryPath,
                                      out int interval)
        {

            format = ConfigurationManager.AppSettings["format"];
            fileName = ConfigurationManager.AppSettings["filename"];
            directoryPath = ConfigurationManager.AppSettings["dirpath"];
            directoryName = Path.GetDirectoryName(directoryPath);
            baseUrl = ConfigurationManager.AppSettings["baseurl"];
            interval = int.Parse(ConfigurationManager.AppSettings["format"]);
        }


        //private async Task<string> GetCurrencies(HttpClient httpClient, string baseUrl, string format)
        //{
        //GetConfiguration(out string format,
        //                 out string fileName,
        //                 out string baseUrl,
        //                 out string directoryName,
        //                 out TimeSpan interval);

        //Task.Run(async () =>
        //{
        //    var baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryName);

        //    CheckDirectory(baseDirectory, string.Empty);

        //    DateTime date;
        //    string directoryPathNow = string.Empty;
        //    using (HttpClient client = new HttpClient())
        //    {
        //        while (!_canceled)
        //        {
        //            date = DateTime.Now.Date;

        //            if (string.IsNullOrWhiteSpace(directoryPathNow) || !directoryPathNow.Contains(date.ToString("dd_MM_yyyy")))
        //                directoryPathNow = Path.Combine(baseDirectory, $"{fileName}_{date.ToString("dd_MM_yyyy")}");

        //            CheckDirectory(directoryPathNow, baseDirectory);

        //            var currencyRate = await GetCurrencies(client, baseUrl, "json");

        //            WriteToFile(
        //                currencyRate, 
        //                directoryPathNow,
        //                baseDirectory, 
        //                string.Join("_",fileName, date.ToString("dd_MM_yyyy_HH:mm")), 
        //                format);

        //            await Task.Delay(interval);
        //        }
        //    }
        //});



        //    try
        //    {
        //        var url = baseUrl + format == "csv" ? "json" : format;
        //        HttpResponseMessage message = await httpClient.GetAsync(url);
        //        message.EnsureSuccessStatusCode();

        //        string responseBody = await message.Content.ReadAsStringAsync();

        //        return responseBody;
        //    }
        //    catch (HttpRequestException e)
        //    {

        //        Debug.WriteLine(e.Message);
        //        return null;
        //    }


        //}


        //private void CheckDirectory(string directoryPath, string baseDirectory)
        //{
        //    if (!Directory.Exists(directoryPath))
        //    {
        //        Directory.CreateDirectory(directoryPath);
        //    }
        //}

        //public void WriteToFile(string jsonContent, string directory, string baseDirectory, string fileName, string format)
        //{
        //    CheckDirectory(directory, baseDirectory);

        //    if (format == "csv")
        //        WriteToCsvFile(jsonContent, directory, baseDirectory, fileName, directory);
        //    else
        //    {
        //        using (var writer = new StreamWriter(Path.Combine(directory, fileName + "." + format), false))
        //        {
        //            writer.Write(jsonContent);
        //        }
        //    }

        //}

        //public void WriteToCsvFile(string jsonContent, string directory, string baseDirectory, string fileName, string format)
        //{
        //    CheckDirectory(directory, baseDirectory);

        //    var dt = JsonConvert.DeserializeObject<DataTable>(jsonContent);

        //    using (var writer = new StreamWriter(Path.Combine(directory, fileName + "." + format), false, Encoding.UTF8))
        //    {
        //        using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
        //        {

        //            foreach (DataColumn column in dt.Columns)
        //            {
        //                csv.WriteField(column.ColumnName);
        //            }
        //            csv.NextRecord();

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                for (var i = 0; i < dt.Columns.Count; i++)
        //                {
        //                    csv.WriteField(row[i]);
        //                }
        //                csv.NextRecord();
        //            }
        //        }
        //    }
        //}
        //private void GetConfiguration(out string format,
        //                              out string fileName,
        //                              out string baseUrl,
        //                              out string directoryName,
        //                              out TimeSpan interval)
        //{
        //    format = ConfigurationManager.AppSettings["format"];
        //    fileName = ConfigurationManager.AppSettings["filename"];
        //    directoryName = ConfigurationManager.AppSettings["dirname"];
        //    baseUrl = ConfigurationManager.AppSettings["url"];
        //    interval = new TimeSpan(Convert.ToInt64(ConfigurationManager.AppSettings["interval"]));
        //}

        protected override void OnStop()
        {
            _canceled = true;
        }
    }

}
