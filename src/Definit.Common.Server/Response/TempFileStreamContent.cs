using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Definit.Common.Server.Response
{
    /// <summary>
    /// Will delete file after content has been read (downloaded)
    /// </summary>
    public class TempFileResult : PhysicalFileResult
    {
        readonly string _fileName;

        public TempFileResult(string fileName)
            : base(fileName, "application/octet-stream")
        {
            _fileName = fileName;

        }

        public override void ExecuteResult(ActionContext context)
        {
            BeforeExecute(context);
            base.ExecuteResult(context);
            AfterExecute(context);
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            BeforeExecute(context);
            await base.ExecuteResultAsync(context);
            AfterExecute(context);
        }

        private void BeforeExecute(ActionContext context)
        {
            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = Path.GetFileName(_fileName),
                Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
            };
            context.HttpContext.Response.Headers["Content-Disposition"]= cd.ToString();
        }

        private void AfterExecute(ActionContext context)
        {
            File.Delete(_fileName);
        }

    }
}
