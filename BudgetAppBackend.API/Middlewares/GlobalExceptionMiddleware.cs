using BudgetAppBackend.Domain.Exceptions.BudgetExceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text.Json;

namespace BudgetAppBackend.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            var response = context.Response;
            response.ContentType = "application/json";

            var problemDetails = new ProblemDetails
            {
                Instance = context.Request.Path,
                Title = "An error occurred while processing your request."
            };

            switch (ex)
            {
                case ValidationException validationEx:
                    _logger.LogWarning("Validation error at {Path}: {Errors}", context.Request.Path, validationEx.Errors);
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Validation Error";
                    problemDetails.Extensions["success"] = false;
                    problemDetails.Extensions["errors"] = validationEx.Errors.Select(e => e.ErrorMessage).ToArray();
                    break;


                case UnauthorizedAccessException unauthorizedEx:
                    _logger.LogWarning("Unauthorized access attempt at {Path}: {Message}", context.Request.Path, unauthorizedEx.Message);
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Unauthorized";
                    problemDetails.Detail = unauthorizedEx.Message;
                    problemDetails.Extensions["success"] = false;
                    break;

                case SecurityTokenExpiredException tokenExpiredEx:
                    _logger.LogWarning("Session expired at {Path}: {Message}", context.Request.Path, tokenExpiredEx.Message);
                    response.StatusCode = 440;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Session Expired";
                    problemDetails.Detail = tokenExpiredEx.Message;
                    problemDetails.Extensions["success"] = false;
                    break;


                case KeyNotFoundException notFoundEx:
                    _logger.LogWarning("Resource not found at {Path}: {Message}", context.Request.Path, notFoundEx.Message);
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Not Found";
                    problemDetails.Detail = notFoundEx.Message;
                    problemDetails.Extensions["success"] = false;
                    break;


                case OperationCanceledException canceledEx:
                    _logger.LogWarning("Request was canceled at {Path}. Reason: {Message}", context.Request.Path, canceledEx.Message);
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Request Canceled";
                    problemDetails.Detail = "The request was canceled by the client or server.";
                    problemDetails.Extensions["success"] = false;
                    break;

                // ---------------------------------------
                // Domain-Specific Budget Exceptions
                // ---------------------------------------
                case BudgetNotFoundException budgetNotFoundEx:
                    _logger.LogWarning("Budget not found at {Path}: {Message}",
                    context.Request.Path, budgetNotFoundEx.Message);
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Budget Not Found";
                    problemDetails.Detail = budgetNotFoundEx.Message;
                    problemDetails.Extensions["success"] = false;
                    break;

                case BudgetAlreadyExistsException budgetExistsEx:
                    _logger.LogWarning("Budget already exists at {Path}: {Message}",
                    context.Request.Path, budgetExistsEx.Message);
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Budget Already Exists";
                    problemDetails.Detail = budgetExistsEx.Message;
                    problemDetails.Extensions["success"] = false;
                    break;

                case BudgetInvalidTitleException invalidTitleEx:
                    _logger.LogWarning("Invalid budget title at {Path}: {Message}",
                    context.Request.Path, invalidTitleEx.Message);
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Invalid Budget Title";
                    problemDetails.Detail = invalidTitleEx.Message;
                    problemDetails.Extensions["success"] = false;
                    break;

                case BudgetInvalidAmountException invalidAmountEx:
                    _logger.LogWarning("Invalid budget amount at {Path}: {Message}",
                    context.Request.Path, invalidAmountEx.Message);
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Invalid Budget Amount";
                    problemDetails.Detail = invalidAmountEx.Message;
                    problemDetails.Extensions["success"] = false;
                    break;

                case BudgetRollbackException rollbackEx:
                    _logger.LogWarning("Budget rollback error at {Path}: {Message}",
                    context.Request.Path, rollbackEx.Message);
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Budget Rollback Error";
                    problemDetails.Detail = rollbackEx.Message;
                    problemDetails.Extensions["success"] = false;
                    break;

                case BudgetDecreaseAmountException decreaseEx:
                    _logger.LogWarning("Budget decrease error at {Path}: {Message}",
                        context.Request.Path, decreaseEx.Message);
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Budget Decrease Error";
                    problemDetails.Detail = decreaseEx.Message;
                    problemDetails.Extensions["success"] = false;
                    break;

                default:
                    _logger.LogError("An unexpected error occurred: {Message}", ex.Message);
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Status = response.StatusCode;
                    problemDetails.Title = "Internal Server Error";
                    problemDetails.Detail = "An unexpected error occurred. Please try again later.";
                    problemDetails.Extensions["success"] = false;
                    break;
            }

            await response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}
