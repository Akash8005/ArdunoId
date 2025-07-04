using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace ArduinoWebIDE.Pages
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        public class CodeModel
        {
            public string? Code { get; set; }
        }

        public void OnGet() { }

        public IActionResult OnPostUpload([FromBody] CodeModel codeModel)
        {
            if (string.IsNullOrWhiteSpace(codeModel?.Code))
                return BadRequest("❌ No code received from frontend.");

            string fqbn = "arduino:avr:nano:cpu=atmega328";
            string folder = @"C:\Users\AKASH\Blink";
            string filePath = Path.Combine(folder, "Blink.ino");
            string logFile = Path.Combine(folder, "last-upload-log.txt");

            try
            {
                Directory.CreateDirectory(folder);
                System.IO.File.WriteAllText(filePath, codeModel.Code);

                string compileCmd = $"arduino-cli compile --fqbn {fqbn} \"{folder}\"";
                string compileOutput = RunShellCommand(compileCmd);

                string uploadCmd = $"arduino-cli upload -p COM6 --fqbn {fqbn} \"{folder}\"";
                string uploadOutput = RunShellCommand(uploadCmd);

                System.IO.File.WriteAllText(logFile, compileOutput + "\n\n" + uploadOutput);

                return Content($"✅ Upload successful!\n\n--- Compilation Log ---\n{compileOutput}\n\n--- Upload Log ---\n{uploadOutput}");
            }
            catch (Exception ex)
            {
                return Content($"❌ Error: {ex.Message}");
            }
        }

        private string RunShellCommand(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return output + (string.IsNullOrWhiteSpace(error) ? "" : "\nERRORS:\n" + error);
        }
    }
}
