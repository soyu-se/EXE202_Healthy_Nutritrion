using HealthyNutritionApp.Domain.Exceptions;
using HealthyNutritionApp.Domain.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;

namespace HealthyNutritionApp.Filters
{
    public class BaseExceptionFilter : IExceptionFilter
    {
        #region Mẫu ExceptionFilter
        //public void OnException(ExceptionContext context)
        //{
        //    // Handle NotFoundCustomException
        //    if (context.Exception is Application.Exceptions.NotFoundCustomException notFoundException)
        //    {
        //        context.Result = new Microsoft.AspNetCore.Mvc.NotFoundObjectResult(new { Message = notFoundException.Message });
        //        context.HttpContext.Response.StatusCode = notFoundException.StatusCode;
        //        context.ExceptionHandled = true;
        //    }
        //    // Handle UnauthorizedCustomException
        //    else if (context.Exception is Application.Exceptions.UnauthorizedCustomException unauthorizedException)
        //    {
        //        context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult(new { Message = unauthorizedException.Message });
        //        context.HttpContext.Response.StatusCode = unauthorizedException.StatusCode;
        //        context.ExceptionHandled = true;
        //    }
        //    // Handle ValidationCustomException
        //    else if (context.Exception is Application.Exceptions.ValidationCustomException validationException)
        //    {
        //        context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { Message = validationException.Message });
        //        context.HttpContext.Response.StatusCode = validationException.StatusCode;
        //        context.ExceptionHandled = true;
        //    }
        //    // Handle ExternalServiceCustomException
        //    else if (context.Exception is Infrastructure.Exceptions.ExternalServiceCustomException externalServiceException)
        //    {
        //        context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(new { Message = externalServiceException.Message })
        //        {
        //            StatusCode = externalServiceException.StatusCode
        //        };
        //        context.HttpContext.Response.StatusCode = externalServiceException.StatusCode;
        //        context.ExceptionHandled = true;
        //    }
        //}
        #endregion

        public BaseExceptionFilter()
        {
        }

        public void OnException(ExceptionContext context)
        {
            ProblemDetails problem;
            Exception exception = context.Exception;

            if (context.Exception is BaseException appEx)
            {
                //_logger.LogWarning(appEx, "=============================================================\nSystem error occurred at UTC+7 time: {Time}", TimeControl.GetUtcPlus7Time());
                
                Log.Error(exception, exception.Message);
                problem = new ProblemDetails
                {
                    Title = ReasonPhrases.GetReasonPhrase(appEx.StatusCode),
                    Status = appEx.StatusCode,
                    Detail = appEx.Message,
                    Type = appEx.ErrorType,
                    Instance = context.HttpContext.Request.Path
                };

                context.HttpContext.Response.StatusCode = appEx.StatusCode;
            }
            else
            {
                //_logger.LogError(context.Exception, "=============================================================\nSystem error occurred at UTC+7 time: {Time}", TimeControl.GetUtcPlus7Time());

                Log.Fatal(exception, exception.Message);
                problem = new ProblemDetails
                {
                    Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError), // Default to 500 Internal Server Error
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = exception.Message,
                    Type = "Lỗi này do chưa config hoặc chưa xác định được"
                };

                context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }

            context.Result = new ObjectResult(problem) { StatusCode = problem.Status };
            context.ExceptionHandled = true;
        }
    }
}
