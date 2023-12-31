using RescueScoreManager.Data;

namespace RescueScoreManager.Messages;

public record class LoginMessage(bool IsConnected = false);
public record class SelectNewCompetitionMessage(Competition NewCompetition = null);
public record class OpenCompetitionMessage();
public record class IsBusyMessage(bool IsBusy = false, string Text = "");
public record class SnackMessage(string Text);