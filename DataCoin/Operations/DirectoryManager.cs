using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AymanMVCProject.Models;
using DataCoin.Models;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

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
        private static object locker;
        private readonly string manual;
        private readonly string automatic;
        private readonly string fixedAssets;
        private readonly string dirNegative;
        private readonly string dirNeutral;
        private readonly string dirPositive;
        private readonly string dirStrongPositive;
        private readonly string subFolderForAuto;
        private readonly string instant;
        private static int timeName;
        public string Location => location;
        public string AsstesLocation => Path.Combine(env, fixedAssets);
        public int CurrentCounts => GetRequestCount();
        
        public string OutFile => "out.csv";
        
        public string OutComponents => "components.png";

        public string OutForecast => "forecast.png";

        public string DirNegative => dirNegative;

        public string DirPositive => dirPositive;

        public string DirNeutral => dirNeutral;

        public string DirStrongPositive => dirStrongPositive;

        public DirectoryManager(IOptions<ApplicationSettings> services, string env)
        {
            todayDate = DateTime.Today.Date.ToString("dd-MM-yy");          
            _services = services;
            this.env = env;
            manual = _services.Value.ManualFolder;
            automatic = _services.Value.AutoFolder;
            instant = _services.Value.InstantFolder;
            locker = new object();
            location = Dir();
            countInfo = _services.Value.CounterFile;
            currentCounts = 0;
            fixedAssets = _services.Value.AssetFile;
            dirNegative = Indicator.Negative.ToString();
            dirNeutral = Indicator.Neutral.ToString();
            dirPositive = Indicator.Positive.ToString();
            dirStrongPositive = Indicator.StrongPositive.ToString(); 
            
            subFolderForAuto = DateTime.Now.ToString("HH:mm:ss").Replace(':', '-');
            timeName = 12;
        }
        public DirectoryManager()
        {
            
        }
        private string Dir()
        {
            
            var rootLocation = Path.Combine(Directory.GetCurrentDirectory(), _services.Value.ForecastDir);
            var newLocation = Path.Combine(rootLocation, todayDate);
            var exist = Directory.Exists(newLocation);
            if (exist) return newLocation;
            lock (locker)
            {
                Directory.CreateDirectory(newLocation);
            }
            return newLocation;
        }
        
        public string GenerateForecastFolder(string assetId, int period, DirSwitcher switcher)
        {
            var timeNow = DateTime.Now.ToString("HH:mm:ss").Replace(':', '.');
            var newFolder = $"{assetId}_{period}_{timeNow}";
            string newLocation;
            switch (switcher)
            {
                case DirSwitcher.Auto:
                    newLocation = Path.Combine(this.location, automatic, subFolderForAuto, newFolder);
                    break;
                case DirSwitcher.Manual:
                    newLocation = Path.Combine(this.location, manual, newFolder);
                    break;
                case DirSwitcher.Instant:
                    newLocation = Path.Combine(this.location, instant, newFolder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
            }

            try
            {
                var exist = Directory.Exists(newLocation);
                if (exist) return newLocation;
                lock (locker)
                {
                    Directory.CreateDirectory(newLocation);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't generate the Forecast Folder");
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
                case DirSwitcher.Instant:
                    loc = Path.Combine(location, instant);
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
            //TODO make location crossplatform
            var tmpTodayFolder = location.Replace("//", "/").Split('/').Last();
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
                case DirSwitcher.Instant:
                    tmpCurrent = LastDir(Path.Combine(location, instant)).Replace("//", "/").Split('/').Last();
                    path = Path.Combine(instant, tmpCurrent);
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
                var created = fi1.CreationTime;

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

            lock (locker)
            {
                File.WriteAllText(counter, count.ToString());
            }
        }

        public int GetRequestCount()
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

            return 0;
        }

        public static void RemoveFolder(string path)
        {
            if (!Directory.Exists(path)) return;
            lock (locker)
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
                    case Indicator.StrongPositive:
                        moveTo = CreateSubDir(getRootPath, dirStrongPositive);
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
            bool exist;
            lock (locker)
            {
                exist = Directory.Exists(newPath);
            }

            if (exist) return newPath;
            lock (locker)
            {
                Directory.CreateDirectory(newPath);
            }
            return newPath;

        }

        private static void MoveFolderToDir(string moveFrom, string moveTo, string oldFolderName)
        {
            var folderWithOldName = CreateSubDir(moveTo, oldFolderName);
            string[] files;
            lock (locker)
            {
                files = Directory.GetFiles(moveFrom);
            }
            //Directory.Move(moveFrom, MoveTo);
            foreach (var s in files)
            {
                lock (locker)
                {
                    var fileName = Path.GetFileName(s);
                    var destFile = Path.Combine(folderWithOldName, fileName);
                    File.Copy(s, destFile, true);
                }
            }

            lock (locker)
            {
                if (!Directory.Exists(moveFrom)) return;
            }

            try
            {
                lock (locker)
                {
                    Directory.Delete(moveFrom, true);
                }   
            }
            catch (IOException e)
            {
                throw new Exception(e.Message);
            }
        }

        public static List<string> GetFolderNames(string dir)
        {
            var names = new List<string>();
            if (!Directory.Exists(dir)) return names;
            var files = Directory.GetDirectories(dir);
            foreach (var folder in files)
            {
                var lastFolder = folder.Split(Path.DirectorySeparatorChar).Last();
                var name = lastFolder.Substring(0, lastFolder.Length - timeName);
                names.Add(name);
            }

            return names;
        }
        
        public static string GetForecastFolderByName(string dir, string assetName)
        {
            try
            {
                string name;
                var files = Directory.GetDirectories(dir);
                return files.FirstOrDefault(x => x.Contains(assetName)).Split(Path.DirectorySeparatorChar).Last();
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't Get Forecast Folder in {dir} by Name {assetName}");
            }           
        }


        public static bool IsFolderExist(string path)
        {
            return Directory.Exists(path);
        }

        public static void WriteLogToExcel(FileInfo file, IEnumerable<ExcelLog> log)
        {
            using (var package = new ExcelPackage(file))
            {
                // add a new worksheet to the empty workbook
                
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                var rowNumber = 1;
                foreach (var subLog in log)
                {
                    worksheet.Cells[rowNumber, 1].Value = subLog.AssetName;
                    worksheet.Cells[rowNumber, 2].Value = subLog.Log;   
                    worksheet.Cells[rowNumber, 3].Value = subLog.Rate;
                    rowNumber++;
                }
               
                package.Save();
            }
        }
        
        public void WriteAssetsToExcel(string path, IEnumerable<string> asstes)
        {
            var file = new FileInfo(path);
            using (var package = new ExcelPackage(file))
            {
                // add a new worksheet to the empty workbook
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                var rowNumber = 1;
                foreach (var asset in asstes)
                {
                    worksheet.Cells[rowNumber, 1].Value = asset;
                               
                    rowNumber++;
                }
               
                package.Save();
            }
        }
        
        public static List<string> ReadAssetsFromExcel(string path)
        {
            try
            {
                var file = new FileInfo(path);
                var rawText = new List<string>();
                using (var package = new ExcelPackage(file))
                {       
                    var worksheet = package.Workbook.Worksheets[1];
                    var rowCount = worksheet.Dimension.Rows;          
                    for (var row = 1; row <= rowCount; row++)
                    {
                        rawText.Add(worksheet.Cells[row, 1].Value.ToString());                    
                    }
                }
                return rawText;
            }
            catch (Exception)
            {
              
                throw new Exception("Symbols file is empty");
            }
            
        }

        public List<ExcelLog> ReadLog(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var excelLog = new List<ExcelLog>();
                var file = new FileInfo(path);
                using (var package = new ExcelPackage(file))
                {       
                    var worksheet = package.Workbook.Worksheets[1];
                    var rowCount = worksheet.Dimension.Rows;          
                    for (var row = 1; row <= rowCount; row++)
                    {
                        excelLog.Add(new ExcelLog()
                        {
                            AssetName = worksheet.Cells[row, 1].Value.ToString(),
                            Log =  worksheet.Cells[row, 2].Value.ToString(),
                            Rate = worksheet.Cells[row,3].Value.ToString()
                        });                 
                    }
                }
            
            return excelLog;
            }
            catch (Exception)
            {
                throw new Exception("Log file is empty");
            }
        }
    }
}