using RescueScoreManager.Data;

namespace RescueScoreManager.Messages;

public record class LoginMessage(bool IsConnected = false);
public record class SelectNewCompetitionMessage(Competition NewCompetition = null);
