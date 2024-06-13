using CsvHelper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;



namespace CurrencyService
{
    public class Service
    {
        private string _directoryPath;
        private readonly ILogger<Service> _logger;
        private readonly HttpClient _httpClient;
        public Service(ILogger<Service> logger, string path)
        {

            _logger = logger;
            _httpClient = new HttpClient();
            _directoryPath = path;

        }

        public async Task Run(string baseUrl, string fileName, string format, int interval)
        {
            CheckDirectory(_directoryPath);

            DateTime date;
            string directoryPathNow = string.Empty;

            await Task.Run(async () =>
            {
                while (true)
                {
                    date = DateTime.Now;

                    if (string.IsNullOrWhiteSpace(directoryPathNow) || !directoryPathNow.Contains(date.ToString("dd_MM_yyyy")))
                        directoryPathNow = Path.Combine(_directoryPath, $"{fileName}_{date.ToString("dd_MM_yyyy")}");

                    CheckDirectory(directoryPathNow);

                    var currencyRate = await GetCurrencies(baseUrl, "json");

                    WriteToFile(
                        currencyRate,
                    directoryPathNow,
                        string.Join("_", fileName, date.ToString("dd_MM_yyyy_HH-mm")),
                        format);

                    await Task.Delay(interval);
                }
            });
        }

        private async Task<string> GetCurrencies(string baseUrl, string format)
        {
            try
            {
                var url = baseUrl + (format == "csv" ? "json" : format);
                HttpResponseMessage message = await _httpClient.GetAsync(url);
                message.EnsureSuccessStatusCode();

                string responseBody = await message.Content.ReadAsStringAsync();

                _logger.LogInformation($"Received currency rate");
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }


        private void CheckDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                _logger.LogInformation($"Created directory: {directoryPath}");
            }
        }

        public void WriteToFile(string jsonContent, string directory, string fileName, string format)
        {
            CheckDirectory(directory);
            var file = fileName + "." + format;
            if (format == "csv")
                WriteToCsvFile(jsonContent, directory, file, directory);
            else
            {

                var path = Path.Combine(directory, file);
                try
                {
                    using (var writer = new StreamWriter(path, false))
                    {
                        writer.Write(jsonContent);
                        _logger.LogInformation($"The exchange rate has been successfully saved to the file {file}");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    throw;
                }


            }

        }

        public void WriteToCsvFile(string jsonContent, string directory, string file, string format)
        {
            CheckDirectory(directory);

            var dt = JsonConvert.DeserializeObject<DataTable>(jsonContent);
            try
            {

                using (var writer = new StreamWriter(Path.Combine(directory, file), false, Encoding.UTF8))
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
            catch (Exception e)
            {


            }


        }
        private static void GetConfiguration(out string format,
                                      out string fileName,
                                      out string baseUrl,
                                      out string directoryName,
                                      out string directoryPath,
                                      out TimeSpan interval)
        {
            format = "json";
            fileName = "curRate";
            directoryName = "CurrencyRates";
            directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), directoryName);
            baseUrl = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?";
            interval = new TimeSpan(61000);
        }

    }

}
