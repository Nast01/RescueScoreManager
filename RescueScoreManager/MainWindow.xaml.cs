using System.Windows.Input;

using RescueScoreManager.Modules.Home;

namespace RescueScoreManager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow(HomeViewModel homeViewModel)
    {
        InitializeComponent();
        
        //DataContext = viewModel;
        //HomeView.DataContext = homeViewModel;

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}
