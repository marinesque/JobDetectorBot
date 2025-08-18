namespace Bot.Domain.Enums
{
    public enum UserState
    {
        None,                       // Нет активного сценария
        AwaitingCriteria,           // Режим сценария
        AwaitingCriteriaEdit,       // Ожидание редактирования критериев
        AwaitingCustomValue,        // Ожидание кастомного значения
        SearchingVacancies          // Просмотр подходящих вакансий
    }
}
