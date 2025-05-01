using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using RescueScoreManager.Data;
using RescueScoreManager.Messages;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.SelectNewCompetition;

public partial class SelectNewCompetitionViewModel : ObservableObject
{
    #region Properties
    [ObservableProperty]
    private string _title;
    [ObservableProperty]
    private DateTime _beginDate;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ValidateCommand))]
    public Competition _selectedCompetition;

    [ObservableProperty]
    private List<Competition> _competitions = new();
    #endregion Properties

    #region Attributes
    private IApiService _wsiService { get; }
    private IMessenger _messenger { get; }
    #endregion Attributes

    public event EventHandler RequestClose;

    #region Command
    [RelayCommand]
    private async Task Refresh()
    {
        await UpdateCompetitionList();
    }

    [RelayCommand(CanExecute = nameof(CanValidate))]
    private Task Validate()
    {
        _messenger.Send(new SelectNewCompetitionMessage(SelectedCompetition));
        return Task.CompletedTask;
    }

    private bool CanValidate() => SelectedCompetition != null;

    [RelayCommand]
    private void Cancel()
    {
        _messenger.Send(new SelectNewCompetitionMessage());
    }

    public void OnRequestClose()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
    #endregion Command

    public SelectNewCompetitionViewModel(
                            IApiService wsiService,
                            IMessenger messenger)
    {
        _title = "Nouvelle Compétition";
        _beginDate = DateTime.Now;
        _wsiService = wsiService;
        _messenger = messenger;
    }



    public async Task UpdateCompetitionList()
    {
        Competitions.Clear();
        Competitions.AddRange(await _wsiService.GetCompetitions(BeginDate));
        Competitions = Competitions.OrderBy(c => c.BeginDate).ToList();
    }

    //public void OnRequestClose()
    //{
    //    if (RequestClose != null)
    //        RequestClose(this, EventArgs.Empty);
    //}
}
