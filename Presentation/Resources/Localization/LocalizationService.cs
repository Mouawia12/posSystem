using Application.Services;
using System.Globalization;
using System.Threading;
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

            var culture = CreateCultureWithLatinDigits(next);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            var source = next == "ar-SA" ? ArSaDictionary : EnUsDictionary;
            ReplaceLocalizationDictionary(source);
            CurrentCultureCode = next;
            LanguageChanged?.Invoke();
        }

        private static CultureInfo CreateCultureWithLatinDigits(string cultureCode)
        {
            var culture = (CultureInfo)CultureInfo.GetCultureInfo(cultureCode).Clone();
            culture.NumberFormat.NativeDigits = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];
            culture.NumberFormat.DigitSubstitution = DigitShapes.None;
            return culture;
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
