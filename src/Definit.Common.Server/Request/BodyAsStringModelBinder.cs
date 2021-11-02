using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Definit.Common.Server.Request
{
    public class BodyAsStringModelBinder : IModelBinder
    {
        public BodyAsStringModelBinder()
        {

        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
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
                    var body = await reader.ReadToEndAsync();
                    bindingContext.Result = ModelBindingResult.Success(body);
                    return;
                }
            }

            bindingContext.ModelState.TryAddModelError(modelName, "Unknown target type. This should not happen.");
            return;
        }
    }
}
