using CommunityToolkit.Mvvm.ComponentModel;

namespace Presentation.ViewModels
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = "Ready";
    }
}
