using System.Diagnostics;
using System.IO;
using AymanMVCProject.Models;
using Microsoft.Extensions.Options;

namespace DataCoin.Operations
{
    public class PythonExec
    {
        private readonly string path;
        private readonly string rootFolder;
        private readonly int period;
        
        public PythonExec(IOptions<ApplicationSettings> services, int period)
        {
            path = services.Value.ForecastDir;
            rootFolder = services.Value.RootFolder;
            this.period = period;
        }

        public void RunPython()
        {
            var cmd = Directory.GetCurrentDirectory().ToString();
            
            var start = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                FileName = "python",
                Arguments = $"forecast.py {path} {rootFolder} {period}",
                RedirectStandardError = true
            };

            using (var process = Process.Start(start))
            {
                using (var reader = process.StandardOutput)
                {   
                    var errors = process.StandardError.ReadToEnd();
                    var result = reader.ReadToEnd(); 
                    process.WaitForExit();
                }
            }
        }
    }
}