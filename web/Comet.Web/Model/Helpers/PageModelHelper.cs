using Comet.Web.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace Comet.Web.Model.Helpers
{
    public static class PageModelHelper
    {

        public static void FillErrorModel(this PageModel model, LanguageService language)
        {
            if (model.ModelState.ErrorCount == 0)
                return;

            foreach (var modelState in model.ModelState.Values.Where(x => x.Errors.Count > 0))
            {
                for (int i = modelState.Errors.Count - 1; i >= 0; i--)
                {
                    var error = modelState.Errors[i];
                    ModelError modelError;
                    if (error.Exception != null)
                    {
                        modelError = new ModelError(error.Exception, language.GetString(error.ErrorMessage));
                    }
                    else
                    {
                        modelError = new ModelError(language.GetString(error.ErrorMessage));
                    }

                    modelState.Errors.RemoveAt(i);

                    if (i == modelState.Errors.Count)
                    {
                        modelState.Errors.Add(modelError);
                    }
                    else
                    {
                        modelState.Errors.Insert(i, modelError);
                    }
                }
            }
        }

    }
}