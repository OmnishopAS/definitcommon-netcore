using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Omnishop.Common.Server
{
    public class BodyAsStringModelBinder : IModelBinder
    {
        public BodyAsStringModelBinder()
        {

        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;


            if (bindingContext.ModelMetadata.ModelType == typeof(string))
            {
                using (var reader = new StreamReader(bindingContext.HttpContext.Request.Body))
                {
                    var body = reader.ReadToEnd();
                    bindingContext.Result = ModelBindingResult.Success(body);
                    return Task.CompletedTask;
                }
            }

            bindingContext.ModelState.TryAddModelError(modelName, "Unknown target type. This should not happen.");
            return Task.CompletedTask;
        }
    }
}
