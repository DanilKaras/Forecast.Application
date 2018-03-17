using System;
using System.Diagnostics;
using System.IO;
using AymanMVCProject.Models;
using Microsoft.Extensions.Options;

namespace DataCoin.Operations
{
    public class PythonExec
    {
        private readonly string path;
        private readonly int periods;
        private bool seasonalityHourly;
        private bool seasonalityDaily;
        private string pythonLocation;
        public PythonExec(string path, int periods,  bool seasonalityHourly, bool seasonalityDaily, string pythonLocation)
        {
            this.path = path;
            this.periods = periods;
            this.seasonalityHourly = seasonalityHourly;
            this.seasonalityDaily = seasonalityDaily;
            this.pythonLocation = pythonLocation;
        }

        public void RunPython()
        {
            var cmd = Directory.GetCurrentDirectory().ToString();
            var arguments = $"{path} {periods} {seasonalityHourly} " +
                            $"{seasonalityDaily}";
            
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
                    using (var reader = process.StandardOutput)
                    {   
                        var errors = process.StandardError.ReadToEnd();
                        var result = reader.ReadToEnd();
                        if (errors != null)
                        {
                            var saveTo = Path.Combine(path, "errors.txt");
                            File.WriteAllText(saveTo, errors);
                        }
                    
                        process.WaitForExit();
                    }
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