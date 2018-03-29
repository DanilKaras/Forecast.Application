using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using AymanMVCProject.Models;
using DataCoin.Models;
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
        private readonly object locker;
        private readonly string manual;
        private readonly string automatic;
        private readonly string fixedAssets;
        private readonly string dirNegative;
        private readonly string dirNeutral;
        private readonly string dirPositive;
        private readonly string subFolderForAuto;
        public string Location => location;
        public string AsstesLocation => Path.Combine(env, fixedAssets);
        public int CurrentCounts => GetRequestCount();
        
        public string OutFile => "out.csv";
        
        public string OutComponents => "components.png";

        public string OutForecast => "forecast.png";

        public DirectoryManager(IOptions<ApplicationSettings> services, string env)
        {
            todayDate = DateTime.Today.Date.ToString("dd-MM-yy");          
            _services = services;
            this.env = env;
            manual = _services.Value.ManualFolder;
            automatic = _services.Value.AutoFolder;
            location = Dir();
            countInfo = _services.Value.CounterFile;
            currentCounts = 0;
            locker = new ReaderWriterLock();
            fixedAssets = _services.Value.AssetFile;
            dirNegative = "Negative";
            dirNeutral = "Neutral";
            dirPositive = "Positive";
            subFolderForAuto = DateTime.Now.ToString("T").Replace(':', '-');
        }

        private string Dir()
        {
            var rootLocation = Path.Combine(Directory.GetCurrentDirectory(), _services.Value.ForecastDir);
            var newLocation = Path.Combine(rootLocation, todayDate);
            var exist = Directory.Exists(newLocation);
            if (!exist)
            {
                Directory.CreateDirectory(newLocation);
            }
            return newLocation;
        }
        
        public string GenerateForecastFolder(string assetId, int period, DirSwitcher switcher)
        {
            var timeNow = DateTime.Now.ToString("hh:mm:ss").Replace(':', '.');
            var newFolder = $"{assetId}_{period}_{timeNow}";
            string newLocation;
            switch (switcher)
            {
                case DirSwitcher.Auto:
                    newLocation = Path.Combine(this.location, automatic, subFolderForAuto, newFolder);
                    break;
                case DirSwitcher.Manual:
                    newLocation = Path.Combine(this.location, manual ,newFolder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
            }
            var exist = Directory.Exists(newLocation);
            if (!exist)
            {
                Directory.CreateDirectory(newLocation);
            }

            return newLocation;
        }

        public string GetLastFolder(DirSwitcher switcher)
        {
            string loc;
            switch (switcher)
            {
                case DirSwitcher.Auto:
                    loc = Path.Combine(location, automatic);
                    break;
                case DirSwitcher.Manual:
                    loc = Path.Combine(location, manual);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
            }
            return LastDir(loc);
        }

        public string DirForImages(DirSwitcher switcher)
        {
            string tmpCurrent;
            string path;
            
            var tmpTodayFolder= location.Replace("//", "/").Split('/').Last();
            switch (switcher)
            {
                case DirSwitcher.Auto:
                    tmpCurrent = LastDir(Path.Combine(location, automatic)).Replace("//", "/").Split('/').Last();
                    path = Path.Combine(automatic, tmpCurrent);
                    break;
                case DirSwitcher.Manual:
                    tmpCurrent = LastDir(Path.Combine(location, manual)).Replace("//", "/").Split('/').Last();
                    path = Path.Combine(manual, tmpCurrent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
            }
            
            return Path.Combine(tmpTodayFolder, path);     
        }

        private static string LastDir(string dir)
        {
            var lastHigh = new DateTime(1900,1,1);
            var highDir = string.Empty;
            foreach (var subdir in Directory.GetDirectories(dir))
            {
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

        public void RemoveFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public bool SpecifyDirByTrend(Indicator switcher, string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return false;
                var getUpperFolder = path.Remove(0,path.LastIndexOf(Path.DirectorySeparatorChar)+1);
                var getRootPath = path.Remove(path.LastIndexOf(Path.DirectorySeparatorChar), path.Length - path.LastIndexOf(Path.DirectorySeparatorChar));
                string moveTo;
                switch(switcher)
                {
                    case Indicator.Positive:
                        moveTo = CreateSubDir(getRootPath, dirPositive);
                        break;
                    case Indicator.Neutral:
                        moveTo = CreateSubDir(getRootPath, dirNeutral);
                        break;
                    case Indicator.Negative:
                        moveTo = CreateSubDir(getRootPath, dirNegative);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
                }

                if (!string.IsNullOrEmpty(moveTo))
                {
                    MoveFolderToDir(path, moveTo, getUpperFolder);
                    return true;
                }                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           

            return false;
        }

        private static string CreateSubDir(string path, string folderName)
        {
            var newPath = Path.Combine(path, folderName);
            var exist = Directory.Exists(newPath);
            
            if (exist) return path;
            
            Directory.CreateDirectory(newPath);
            return newPath;

        }

        private static void MoveFolderToDir(string moveFrom, string MoveTo, string oldFolderName)
        {
            var folderWithOldName = CreateSubDir(MoveTo, oldFolderName);
            var files = System.IO.Directory.GetFiles(moveFrom);
            //Directory.Move(moveFrom, MoveTo);
            foreach (var s in files)
            {
                
                var fileName = System.IO.Path.GetFileName(s);
                var destFile = System.IO.Path.Combine(folderWithOldName, fileName);
                File.Copy(s, destFile, true);
            }
            
            if(Directory.Exists(moveFrom))
            {
                try
                {
                    Directory.Delete(moveFrom, true);
                }

                catch (IOException e)
                {
                    throw new Exception(e.Message);
                }
            }
        }
    }
}