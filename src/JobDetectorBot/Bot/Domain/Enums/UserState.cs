namespace Bot.Domain.Enums
{
    public enum UserState
    {
        None,                // Нет активного сценария
        AwaitingCriteria,
        AwaitingCriteriaEdit // Ожидание редактирования критериев
    }
}
