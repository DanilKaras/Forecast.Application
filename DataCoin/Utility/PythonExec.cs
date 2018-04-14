using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataCoin.Models;
using DataCoin.Utility;

namespace DataCoin.Operations
{
    public class PythonExec
    {
        private readonly string path;
        private readonly int periods;
        private bool seasonalityHourly;
        private bool seasonalityDaily;
        private string pythonLocation;
        private object locker;
        
        public PythonExec(string path, int periods,  bool seasonalityHourly, bool seasonalityDaily, string pythonLocation)
        {
            this.path = path;
            this.periods = periods;
            this.seasonalityHourly = seasonalityHourly;
            this.seasonalityDaily = seasonalityDaily;
            this.pythonLocation = pythonLocation; 
            locker = new object();
        }

        public void RunPython()
        {
            var cmd = Directory.GetCurrentDirectory().ToString();
            var arguments = $"{path} {periods} {seasonalityHourly} " +
                            $"{seasonalityDaily}";

            string[] output = null;
            ;
            var start = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                FileName = pythonLocation,
                Arguments = $"forecast.py {arguments}",
                RedirectStandardError = true
            };
            try
            {
                using (var process = Process.Start(start))
                { 
                    //using (var reader = process.StandardOutput)
                    //{   
                        //var errors = process.StandardError.ReadToEnd();
                        //var result = reader.ReadToEnd();
                        //output = result.Substring(result.IndexOf(StaticUtility.HiddenVal) + StaticUtility.HiddenVal.Length,  result.Length - result.IndexOf(StaticUtility.HiddenVal) - StaticUtility.HiddenVal.Length).Trim().Split(',');
                        
                        //if (errors != null)
                        //{
                        //    var saveTo = Path.Combine(path, "errors.txt");
                        //    File.WriteAllText(saveTo, errors);
                        //}
                        
                        //process.WaitForExit();
                    //}
                    process.WaitForExit();                  
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}