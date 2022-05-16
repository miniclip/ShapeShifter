using System.Diagnostics;
using System.Text;

namespace Miniclip.ShapeShifter.Extensions
{
    //From https://gist.github.com/edwardrowe/ee34ad5eea62516dd79dbf64f13b6506
    internal static class ProcessExtensions
    {
        internal static int Run(this Process process, string application,
            string arguments, string workingDirectory, out string output,
            out string errors)
        {
            process.StartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = application,
                Arguments = arguments,
                WorkingDirectory = workingDirectory
            };

            StringBuilder outputBuilder = new StringBuilder();
            StringBuilder errorsBuilder = new StringBuilder();
            process.OutputDataReceived += (_, args) => outputBuilder.AppendLine(args.Data);
            process.ErrorDataReceived += (_, args) => errorsBuilder.AppendLine(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            output = outputBuilder.ToString().TrimEnd();
            errors = errorsBuilder.ToString().TrimEnd();
            return process.ExitCode;
        }
    }
}