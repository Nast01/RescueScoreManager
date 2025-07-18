using System;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace RescueScoreManager.Services;
public class ExceptionHandlingService : IExceptionHandlingService
{
    private readonly ILogger<ExceptionHandlingService> _logger;

    public ExceptionHandlingService(ILogger<ExceptionHandlingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void HandleException(Exception exception, string? context = null)
    {
        var contextInfo = string.IsNullOrEmpty(context) ? "Unknown context" : context;
        _logger.LogError(exception, "Unhandled exception in {Context}", contextInfo);

        // Log additional details
        LogExceptionDetails(exception, contextInfo);
    }

    public void HandleExceptionWithUserNotification(Exception exception, string userMessage, string? context = null)
    {
        HandleException(exception, context);

        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            MessageBox.Show(
                userMessage,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        });
    }

    private void LogExceptionDetails(Exception exception, string context)
    {
        _logger.LogError("Exception Details:");
        _logger.LogError("- Context: {Context}", context);
        _logger.LogError("- Type: {ExceptionType}", exception.GetType().Name);
        _logger.LogError("- Message: {ExceptionMessage}", exception.Message);
        _logger.LogError("- Stack Trace: {StackTrace}", exception.StackTrace);

        if (exception.InnerException != null)
        {
            _logger.LogError("- Inner Exception: {InnerException}", exception.InnerException.Message);
        }
    }
}