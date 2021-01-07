using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Definit.Common.Server.ApiDoc
{
    public abstract class TypeScriptControllerBase : ControllerBase
    {
        private readonly TypescriptWriter _writer;

        protected TypeScriptControllerBase(TypescriptWriter writer)
        {
            _writer = writer;
        }

        [HttpGet("metadata")]
        public IActionResult GetMetaData()
        {
            var sb = new StringBuilder();
            _writer.WriteMetadata(sb);
            return new ContentResult()
            {
                 ContentType ="text/plain",
                 Content = sb.ToString(),
                 StatusCode=200
            };
        }

        [HttpGet("entities")]
        public IActionResult GetEntities()
        {
            var sb = new StringBuilder();
            _writer.WriteEntities(sb);
            return new ContentResult()
            {
                ContentType = "text/plain",
                Content = sb.ToString(),
                StatusCode = 200
            };
        }

        [HttpGet("enums")]
        public IActionResult GetEnums()
        {
            var sb = new StringBuilder();
            _writer.WriteEnums(sb);
            return new ContentResult()
            {
                ContentType = "text/plain",
                Content = sb.ToString(),
                StatusCode = 200
            };
        }

        [HttpGet("datatransferobjects")]
        public IActionResult GetDataTransferObjects()
        {
            var sb = new StringBuilder();
            _writer.WriteDataTransferObjects(sb);
            return new ContentResult()
            {
                ContentType = "text/plain",
                Content = sb.ToString(),
                StatusCode = 200
            };
        }

        [HttpGet("customtypes")]
        public IActionResult GetCustomTypes()
        {
            var sb = new StringBuilder();
            _writer.WriteCustomTypes(sb);
            return new ContentResult()
            {
                ContentType = "text/plain",
                Content = sb.ToString(),
                StatusCode = 200
            };
        }

    }
}
