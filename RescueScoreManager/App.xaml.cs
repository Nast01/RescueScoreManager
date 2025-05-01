using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RescueScoreManager.Data;
using RescueScoreManager.Modules.Home;
using RescueScoreManager.Modules.Login;
using RescueScoreManager.Properties;
using RescueScoreManager.Modules.SelectNewCompetition;
using RescueScoreManager.Services;

using System;
using System.Globalization;
using System.Transactions;
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

        //using (RescueScoreManagerContext context = host.Services.GetRequiredService<RescueScoreManagerContext>())
        //{
        //    context.Database.Migrate();
        //}

        App app = new();
        app.InitializeComponent();
        app.MainWindow = host.Services.GetRequiredService<MainWindow>();
        app.MainWindow.DataContext = host.Services.GetRequiredService<MainWindowViewModel>();
        app.MainWindow.Visibility = Visibility.Visible;
        app.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder)
            => configurationBuilder.AddUserSecrets(typeof(App).Assembly))
        .ConfigureServices((hostContext, services) =>
        {
            //Services
            services.AddTransient<IDialogService, DialogService>();
            services.AddSingleton<IApiService, ApiService>();
            services.AddSingleton<IXMLService, XMLService>();
            services.AddSingleton<IExcelService, ExcelService>();
            services.AddSingleton<ILocalizationService, ResourceManagerLocalizationService>();
            services.AddSingleton<IStorageService, LocalStorageService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();

            //Pages
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<HomeView>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<HomeInformationsView>();
            services.AddSingleton<HomeInformationsViewModel>();


            //Dialogs
            services.AddSingleton<LoginView>();
            services.AddSingleton<LoginViewModel>();
            services.AddSingleton<SelectNewCompetitionView>();
            services.AddSingleton<SelectNewCompetitionViewModel>();


            services.AddSingleton<WeakReferenceMessenger>();
            services.AddSingleton<IMessenger, WeakReferenceMessenger>(provider => provider.GetRequiredService<WeakReferenceMessenger>());

            services.AddSingleton(_ => Current.Dispatcher);

            //services.AddDbContext<RescueScoreManagerContext>();

            services.AddTransient<ISnackbarMessageQueue>(provider =>
            {
                Dispatcher dispatcher = provider.GetRequiredService<Dispatcher>();
                return new SnackbarMessageQueue(TimeSpan.FromSeconds(3.0), dispatcher);
            });
        });
}
