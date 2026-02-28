using Presentation.ViewModels;
using System.Text;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace Presentation.Views
{
    public partial class MainWindow : FluentWindow
    {
        private static readonly TimeSpan ScannerInterKeyThreshold = TimeSpan.FromMilliseconds(60);
        private const int MinimumScannerLength = 6;

        private readonly StringBuilder _scannerBuffer = new();
        private DateTime _lastScannerInputUtc = DateTime.MinValue;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            PreviewTextInput += OnPreviewTextInput;
            PreviewKeyDown += OnPreviewKeyDown;
            PreviewMouseDown += (_, _) => ResetScannerBuffer();
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm || !vm.IsPosModule)
            {
                ResetScannerBuffer();
                return;
            }

            if (string.IsNullOrEmpty(e.Text) || e.Text.Length != 1 || char.IsControl(e.Text[0]))
            {
                return;
            }

            var now = DateTime.UtcNow;
            if (_lastScannerInputUtc != DateTime.MinValue && now - _lastScannerInputUtc > ScannerInterKeyThreshold)
            {
                _scannerBuffer.Clear();
            }

            _scannerBuffer.Append(e.Text);
            _lastScannerInputUtc = now;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm || !vm.IsPosModule)
            {
                ResetScannerBuffer();
                return;
            }

            if (e.Key != Key.Enter && e.Key != Key.Return)
            {
                return;
            }

            if (_scannerBuffer.Length < MinimumScannerLength)
            {
                ResetScannerBuffer();
                return;
            }

            var scannedValue = _scannerBuffer.ToString();
            ResetScannerBuffer();

            vm.BarcodeInput = scannedValue;
            _ = vm.ScanBarcodeCommand.ExecuteAsync(null);
            e.Handled = true;
        }

        private void ResetScannerBuffer()
        {
            _scannerBuffer.Clear();
            _lastScannerInputUtc = DateTime.MinValue;
        }
    }
}
