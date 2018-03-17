using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using AymanMVCProject.Models;
using Microsoft.Extensions.Options;

namespace DataCoin.Operations
{
    public class DirectoryManager
    {
        private readonly string todayDate;
        private readonly IOptions<ApplicationSettings> _services;
        private readonly string location;
        private readonly string env;
        private readonly string countInfo;
        private int currentCounts;
        public string LastFolder => GetLastFolder();
        public string FolderForImg => DirForImages();
        public string Location => location;
        public int CurrentCounts => GetRequestCount();
        public string OutFile => "out.csv";
        private object locker;
        public string OutComponents => "components.png";

        public string OutForecast => "forecast.png";

        public DirectoryManager(IOptions<ApplicationSettings> services, string env)
        {
            todayDate = DateTime.Today.Date.ToString("dd-MM-yy");          
            _services = services;
            this.env = env;
            location = Dir();
            countInfo = "counter.txt";
            currentCounts = 0;
            locker = new ReaderWriterLock();
        }

        private string Dir()
        {
            
            var rootLocation = Path.Combine(Directory.GetCurrentDirectory(), _services.Value.ForecastDir);
            var newLocation =  string.Format(@"{0}/{1}", rootLocation, todayDate);
            var exist = Directory.Exists(newLocation);
            if (!exist)
            {
                Directory.CreateDirectory(newLocation);
            }

            return newLocation;
        }

        public string GenerateForecastFolder(string assetId, int period)
        {
            var timeNow = DateTime.Now.ToString("hh:mm:ss").Replace(':', '.');
            var newFolder = $"{assetId}_{period}_{timeNow}";
            var location = string.Format("{0}/{1}", this.location, newFolder);
            
            var exist = Directory.Exists(location);
            if (!exist)
            {
                Directory.CreateDirectory(location);
            }

            return location;
        }

        private string GetLastFolder()
        {
            return LastDir(location);
        }

        private string DirForImages()
        {            
            var tmpTodayFolder = location.Replace("//", "/").Split('/').Last();
            var tmpCurrent = LastDir(location).Replace("//", "/").Split('/').Last();
            
            return Path.Combine(tmpTodayFolder, tmpCurrent);     
        }

        private static string LastDir(string dir)
        {
            var lastHigh = new DateTime(1900,1,1);
            var highDir = string.Empty;
            foreach (var subdir in Directory.GetDirectories(dir)){
                var fi1 = new DirectoryInfo(subdir);
                var created = fi1.LastWriteTime;

                if (created <= lastHigh) continue;
                highDir = subdir;
                lastHigh = created;
            }
            
            return highDir;
        }

        public void UpdateRequests(int count)
        {
            
            var counter = Path.Combine(location, countInfo);
            var exist = File.Exists(counter);
            if (!exist)
            {
                File.WriteAllText(counter, 0.ToString());
            }

            try
            {
                using(var reader = new StreamReader(System.IO.File.OpenRead(counter)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var requestsPerDay = line.Split(',').FirstOrDefault() ?? default(int).ToString();
                        currentCounts = Convert.ToInt32(requestsPerDay) + count;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            lock (locker)
            {
                File.WriteAllText(counter, currentCounts.ToString());
            }
        }

        private int GetRequestCount()
        {
            var counter = Path.Combine(location, countInfo);
            var exist = File.Exists(counter);
            if (!exist)
            {
                File.WriteAllText(counter, 0.ToString());
            }

            try
            {
                using(var reader = new StreamReader(System.IO.File.OpenRead(counter)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        return Convert.ToInt32(line.Split(',').FirstOrDefault());

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return default(int);
        }
    }
}