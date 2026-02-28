using Application.Services;
using System.Globalization;
using System.Windows;

namespace Presentation.Resources.Localization
{
    public sealed class LocalizationService : ILocalizationService
    {
        private const string EnUsDictionary = "/Presentation/Resources/Localization/Strings.en-US.xaml";
        private const string ArSaDictionary = "/Presentation/Resources/Localization/Strings.ar-SA.xaml";

        public event Action? LanguageChanged;

        public string CurrentCultureCode { get; private set; } = "en-US";

        public bool IsRightToLeft => CultureInfo.GetCultureInfo(CurrentCultureCode).TextInfo.IsRightToLeft;

        public LocalizationService()
        {
            SetLanguage(CurrentCultureCode);
        }

        public void SetLanguage(string cultureCode)
        {
            var next = string.Equals(cultureCode, "ar-SA", StringComparison.OrdinalIgnoreCase)
                ? "ar-SA"
                : "en-US";

            var culture = CultureInfo.GetCultureInfo(next);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            var source = next == "ar-SA" ? ArSaDictionary : EnUsDictionary;
            ReplaceLocalizationDictionary(source);
            CurrentCultureCode = next;
            LanguageChanged?.Invoke();
        }

        private static void ReplaceLocalizationDictionary(string source)
        {
            var resources = System.Windows.Application.Current.Resources.MergedDictionaries;
            var current = resources.FirstOrDefault(x => x.Source is not null && x.Source.OriginalString.Contains("Strings.", StringComparison.OrdinalIgnoreCase));
            if (current is not null)
            {
                resources.Remove(current);
            }

            resources.Add(new ResourceDictionary { Source = new Uri(source, UriKind.Relative) });
        }
    }
}
