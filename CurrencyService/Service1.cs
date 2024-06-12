using CsvHelper;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;


namespace CurrencyService
{
    public partial class Service1 : ServiceBase
    {

        private bool _canceled = false;

        public Service1()
        {
            InitializeComponent();
        }



        protected override void OnStart(string[] args)
        {
            Debugger.Launch();


            GetConfiguration(out string format,
                             out string fileName,
                             out string baseUrl,
                             out string directoryName,
                             out TimeSpan interval);

            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryName);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            

  
            Task.Run(async () =>
            {
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                //using (var writer = new StreamWriter(
                //    Path.Combine(directoryCreate, fileName + "." + format), false, Encoding.UTF8))
                //{
                //    writer.Write(baseUrl + format);
                //}
                DateTime date;
                using (HttpClient client = new HttpClient())
                {
                    while (!_canceled)
                    {
                        var body = await GetCurrencies(client, baseUrl, "json");

                        date = DateTime.Now.Date;
                        if (!Directory.Exists(Path.Combine(directoryPath, $"{fileName}_{date.ToString("dd_MM_yyyy")}")))

                        await Task.Delay(interval);
                    }
                }
            });
        }

        private async Task<string> GetCurrencies(HttpClient httpClient, string baseUrl, string format)
        {
            try
            {
                var url = baseUrl + format == "csv" ? "json" : format;
                HttpResponseMessage message = await httpClient.GetAsync(url);
                message.EnsureSuccessStatusCode();

                string responseBody = await message.Content.ReadAsStringAsync();

                return responseBody;
            }
            catch (HttpRequestException e)
            {

                Debug.WriteLine(e.Message);
                return null;
            }


        }


        public void WriteToFile(string jsonContent, string path, string fileName, string format)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (format == "csv")
                WriteToCsvFile(jsonContent, path, fileName, path);
            else
            {
                using (var writer = new StreamWriter(Path.Combine(path, fileName + "." + format), false))
                {
                    writer.Write(jsonContent);
                }
            }

        }

        public void WriteToCsvFile(string jsonContent, string path, string fileName, string format)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var dt = JsonConvert.DeserializeObject<DataTable>(jsonContent);

            using (var writer = new StreamWriter(Path.Combine(path, fileName + "." + format), false, Encoding.UTF8))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
                {

                    foreach (DataColumn column in dt.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    foreach (DataRow row in dt.Rows)
                    {
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
            }
        }
        private void GetConfiguration(out string format,
                                      out string fileName,
                                      out string baseUrl,
                                      out string directoryName,
                                      out TimeSpan interval)
        {
            format = ConfigurationManager.AppSettings["format"];
            fileName = ConfigurationManager.AppSettings["filename"];
            directoryName = ConfigurationManager.AppSettings["dirname"];
            baseUrl = ConfigurationManager.AppSettings["url"];
            interval = new TimeSpan(Convert.ToInt64(ConfigurationManager.AppSettings["interval"]));
        }

        protected override void OnStop()
        {
            _canceled = true;
        }
    }
}
