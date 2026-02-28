namespace Application.Services
{
    public interface ILocalizationService
    {
        event Action? LanguageChanged;
        string CurrentCultureCode { get; }
        bool IsRightToLeft { get; }
        void SetLanguage(string cultureCode);
    }
}
