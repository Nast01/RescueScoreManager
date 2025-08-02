using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Data;
using RescueScoreManager.Modules.Home;
using RescueScoreManager.Modules.Login;
using RescueScoreManager.Properties;
using RescueScoreManager.Modules.SelectNewCompetition;
using RescueScoreManager.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;

namespace RescueScoreManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    [STAThread]
    public static void Main(string[] args)
    {
        using IHost host = CreateHostBuilder(args).Build();
        host.Start();

        App app = new();
        app.InitializeComponent();
        app.MainWindow = host.Services.GetRequiredService<MainWindow>();
        app.MainWindow.DataContext = host.Services.GetRequiredService<MainWindowViewModel>();
        app.MainWindow.Visibility = Visibility.Visible;
        app.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
            logging.SetMinimumLevel(LogLevel.Information);
        })
        .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder)
            => configurationBuilder.AddUserSecrets(typeof(App).Assembly))
        .ConfigureServices((hostContext, services) =>
        {
            // ApiService registration - using constructor that creates its own HttpClient
            services.AddSingleton<IApiService, ApiService>();

            services.AddSingleton<IXMLService, XMLService>();
            services.AddSingleton<IExcelService, ExcelService>();
            services.AddSingleton<ILocalizationService, ResourceManagerLocalizationService>();
            services.AddSingleton<IStorageService, LocalStorageService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IExceptionHandlingService, ExceptionHandlingService>();
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddSingleton<IImageService, ImageService>();
            services.AddSingleton<INetworkConnectivityService, NetworkConnectivityService>();

            // Main Window (Singleton)
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();


            // Views and ViewModels (Transient for proper MVVM)
            services.AddTransient<HomeView>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<HomeInformationsView>();
            services.AddTransient<HomeInformationsViewModel>();

            // Login components - Transient so each login gets fresh instances
            services.AddTransient<LoginView>();
            services.AddTransient<LoginViewModel>();

            // SelectNewCompetition components
            services.AddTransient<SelectNewCompetitionView>();
            services.AddTransient<SelectNewCompetitionViewModel>();

            // Messaging
            services.AddSingleton<WeakReferenceMessenger>();
            services.AddSingleton<IMessenger, WeakReferenceMessenger>(provider =>
                provider.GetRequiredService<WeakReferenceMessenger>());

            // UI Services
            services.AddSingleton(_ => Current.Dispatcher);

            services.AddTransient<ISnackbarMessageQueue>(provider =>
            {
                Dispatcher dispatcher = provider.GetRequiredService<Dispatcher>();
                return new SnackbarMessageQueue(TimeSpan.FromSeconds(4.0), dispatcher);
            });

            // Add IServiceProvider to be injected (for scenarios where we need to resolve services dynamically)
            services.AddTransient<IServiceProvider>(provider => provider);
        });
}
