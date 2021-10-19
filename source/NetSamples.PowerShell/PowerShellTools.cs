using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace NetSamples.PowerShell
{
    /// <summary>
    /// Powershell tools
    /// </summary>
    public class PowerShellTools
    {
        /// <summary>
        /// Uploads <paramref name="file"/> to <paramref name="destinationComputer"/> using Powershell remote
        /// </summary>
        /// <param name="destinationComputer">NetBIOS name, an IP address, or a fully qualified domain name of the remote computer to upload the file to</param>
        /// <param name="destinationPath">Destination path on <paramref name="destinationComputer"/> where the file will be saved</param>
        /// <param name="credential"><paramref name="destinationComputer"/> credentials</param>
        /// <param name="file">File to upload</param>
        /// <param name="cancelToken">Canellation token</param>
        /// <returns></returns>
        public static async Task UploadFileToAsync(string destinationComputer, PSCredential credential, FileInfo file, string destinationPath, CancellationToken cancelToken=default)
        {
            if (!file.Exists)
                throw new FileNotFoundException($"File {file.FullName} does not exist");

            using var powerShell = System.Management.Automation.PowerShell.Create();
            powerShell.AddCommand("New-PSSession")
                .AddParameter("ComputerName", destinationComputer)
                .AddParameter("Credential", credential)
                .AddParameter("UseSSL")
                .AddParameter("ErrorAction", "Stop");

            var psSession = (await powerShell.InvokeAsync()).SingleOrDefault();
            try
            {
                cancelToken.ThrowIfCancellationRequested();
                powerShell.Commands.Clear();
                powerShell.AddCommand("Copy-Item")
                    .AddParameter("Path",file.FullName)
                    .AddParameter("Destination", destinationPath)
                    .AddParameter("ToSession", psSession)
                    .AddParameter("ErrorAction", "Stop");
                await powerShell.InvokeAsync();
            }
            finally
            {
                if (psSession != null)
                {
                    powerShell.Commands.Clear();
                    powerShell.AddCommand("Remove-PSSession")
                        .AddArgument(psSession);
                    await powerShell.InvokeAsync();
                }
            }
        }
    }
}
