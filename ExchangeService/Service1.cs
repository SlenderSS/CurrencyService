using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace CurrencyService
{
    public partial class Service1 : ServiceBase
    {

        private bool _canceled = false;
        private CancellationTokenSource _tokenSource;
        public Service1()
        {
            InitializeComponent();
        }



        protected override async void OnStart(string[] args)
        {

            //Debugger.Launch();

            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;

            GetConfiguration(out string format,
                             out string fileName,
                             out string baseUrl,
                             out string directoryName,
                             out string directoryPath,
                             out int interval);


            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(directoryPath, "logs.txt"), rollingInterval: RollingInterval.Day);
                builder.AddSerilog(loggerConfiguration.CreateLogger());
            });

            Service service = new Service(
                loggerFactory.CreateLogger<Service>(),
                directoryPath
                );

            await service.Run(baseUrl, fileName, format, interval, token);
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
            directoryName = Path.GetFileName(Path.GetDirectoryName(directoryPath));
            baseUrl = ConfigurationManager.AppSettings["baseurl"];
            interval = int.Parse(ConfigurationManager.AppSettings["interval"]);
        }


        
        protected override void OnStop()
        {
            _tokenSource.Cancel();
            _canceled = true;
        }
    }

}
