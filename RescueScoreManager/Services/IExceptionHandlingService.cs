using System;
using System.Windows;

namespace RescueScoreManager.Services;

public interface IExceptionHandlingService
{
    void HandleException(Exception exception, string? context = null);
    void HandleExceptionWithUserNotification(Exception exception, string userMessage, string? context = null);
}