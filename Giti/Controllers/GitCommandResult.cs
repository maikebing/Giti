using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Giti.Controllers
{
    public class GitCommandResult : IActionResult
    {
        public GitCommandOptions Options { get; set; }

        public GitCommandResult(GitCommandOptions options)
        {           
            Options = options;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            HttpResponse response = context.HttpContext.Response;
            Stream responseStream = GetOutputStream(context.HttpContext);

            string contentType = $"application/x-{Options.Service}";
            if (Options.AdvertiseRefs)
                contentType += "-advertisement";

            response.ContentType = contentType;

            ProcessStartInfo info = new ProcessStartInfo("git", Options.ToString())
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process process = Process.Start(info))
            {
                GetInputStream(context.HttpContext).CopyTo(process.StandardInput.BaseStream);

                if (Options.EndStreamWithNull)
                    process.StandardInput.Write('\0');
                process.StandardInput.Dispose();

                using (StreamWriter writer = new StreamWriter(responseStream))
                {
                    if (Options.AdvertiseRefs)
                    {
                        string service = $"# service={Options.Service}\n";
                        writer.Write($"{service.Length + 4:x4}{service}0000");
                        writer.Flush();
                    }

                    process.StandardOutput.BaseStream.CopyTo(responseStream);
                }

                process.WaitForExit();
            }
        }

        private Stream GetInputStream(HttpContext context)
        {
            return string.Equals(context.Request.Headers["Content-Encoding"], "gzip")
                ? new GZipStream(context.Request.Body, CompressionMode.Decompress)
                : context.Request.Body;
        }

        private Stream GetOutputStream(HttpContext context)
        {
            string acceptEncoding;
            if ((acceptEncoding = context.Request.Headers["Accept-Encoding"]) != null && acceptEncoding.Contains("gzip"))
            {
                context.Response.Headers.Add("Content-Encoding", "gzip");
                return new GZipStream(context.Response.Body, CompressionMode.Compress);
            }
            return context.Response.Body;
        }
    }
}
