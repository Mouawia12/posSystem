using System.Globalization;
using System.Windows.Data;

namespace Presentation.Converters
{
    public sealed class CurrencyDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0 || values[0] is null)
            {
                return string.Empty;
            }

            if (!TryGetDecimal(values[0], out var amount))
            {
                return string.Empty;
            }

            var currencyCode = values.Length > 1 && values[1] is string code && !string.IsNullOrWhiteSpace(code)
                ? code.Trim().ToUpperInvariant()
                : "USD";

            var formatCulture = (CultureInfo)culture.Clone();
            formatCulture.NumberFormat.CurrencySymbol = currencyCode == "SAR" ? "ر.س" : "$";

            return amount.ToString("C2", formatCulture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static bool TryGetDecimal(object value, out decimal amount)
        {
            switch (value)
            {
                case decimal d:
                    amount = d;
                    return true;
                case double d:
                    amount = (decimal)d;
                    return true;
                case float f:
                    amount = (decimal)f;
                    return true;
                case int i:
                    amount = i;
                    return true;
                case long l:
                    amount = l;
                    return true;
                case string s when decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                    amount = parsed;
                    return true;
                default:
                    amount = 0m;
                    return false;
            }
        }
    }
}
