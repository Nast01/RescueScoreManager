using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Services;

namespace RescueScoreManager.Helpers;
public abstract class BaseViewModel : ObservableObject, IDisposable
{
    protected readonly ILogger Logger;
    protected readonly IExceptionHandlingService ExceptionHandler;
    private bool _disposed = false;

    protected BaseViewModel(ILogger logger, IExceptionHandlingService exceptionHandler)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ExceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
    }

    protected virtual void HandleException(Exception exception, string? context = null)
    {
        ExceptionHandler.HandleException(exception, context ?? GetType().Name);
    }

    protected virtual void HandleExceptionWithUserMessage(Exception exception, string userMessage, string? context = null)
    {
        ExceptionHandler.HandleExceptionWithUserNotification(exception, userMessage, context ?? GetType().Name);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Override in derived classes to dispose managed resources
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}