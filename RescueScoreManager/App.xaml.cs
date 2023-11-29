using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RescueScoreManager.Data;
using RescueScoreManager.Home;
using RescueScoreManager.Login;
using RescueScoreManager.Services;

using System;
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
        app.MainWindow.Visibility = Visibility.Visible;
        app.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder)
            => configurationBuilder.AddUserSecrets(typeof(App).Assembly))
        .ConfigureServices((hostContext, services) =>
        {
            services.AddTransient<IDialogService, DialogService>();
            services.AddSingleton<IWSIRestService, WSIRestService>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();

            services.AddSingleton<LoginView>();
            services.AddSingleton<LoginViewModel>();
            //provider =>
            //{
            //    return new LoginViewModel(provider.GetRequiredService<IMessenger>());
            //}

            services.AddSingleton<HomeView>();
            services.AddSingleton<HomeViewModel>();
            //provider =>
            //{
            //    return new HomeViewModel(provider.GetRequiredService<RescueScoreManagerContext>(),
            //                             provider.GetRequiredService<LoginViewModel>(),
            //                             provider.GetRequiredService<IDialogService>(),
            //                             provider.GetRequiredService<IMessenger>()
            //        );
            //}

            services.AddSingleton<WeakReferenceMessenger>();
            services.AddSingleton<IMessenger, WeakReferenceMessenger>(provider => provider.GetRequiredService<WeakReferenceMessenger>());

            services.AddSingleton(_ => Current.Dispatcher);

            services.AddDbContext<RescueScoreManagerContext>();

            services.AddTransient<ISnackbarMessageQueue>(provider =>
            {
                Dispatcher dispatcher = provider.GetRequiredService<Dispatcher>();
                return new SnackbarMessageQueue(TimeSpan.FromSeconds(3.0), dispatcher);
            });
        });
}
