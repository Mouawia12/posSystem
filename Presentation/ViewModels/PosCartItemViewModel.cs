using CommunityToolkit.Mvvm.ComponentModel;

namespace Presentation.ViewModels
{
    public partial class PosCartItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private long _productId;

        [ObservableProperty]
        private string _sku = string.Empty;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private decimal _unitPrice;

        [ObservableProperty]
        private decimal _unitCost;

        [ObservableProperty]
        private decimal _quantity = 1m;

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
