using Application.DTOs;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Enums;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Printing;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Win32;

namespace Presentation.ViewModels
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        private const decimal LowStockThreshold = 5m;

        private readonly IProductSearchService _productSearchService;
        private readonly IProductManagementService _productManagementService;
        private readonly ICustomerManagementService _customerManagementService;
        private readonly IWarrantyManagementService _warrantyManagementService;
        private readonly IMaintenanceManagementService _maintenanceManagementService;
        private readonly IReportingService _reportingService;
        private readonly IDashboardService _dashboardService;
        private readonly ISettingsService _settingsService;
        private readonly IUserManagementService _userManagementService;
        private readonly IBackupRestoreService _backupRestoreService;
        private readonly IPrintingService _printingService;
        private readonly IInvoiceService _invoiceService;
        private readonly IUserContextService _userContextService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;

        [ObservableProperty] private string _windowTitle = "Offline POS + Inventory";
        [ObservableProperty] private string _loggedUserName = "owner";
        [ObservableProperty] private UserRole _loggedUserRole = UserRole.Owner;
        [ObservableProperty] private string _companyName = "My Retail Store";
        [ObservableProperty] private string _activeModule = "DASHBOARD";

        [ObservableProperty] private int _dashboardTotalProducts;
        [ObservableProperty] private int _dashboardActiveCustomers;
        [ObservableProperty] private int _dashboardInvoicesToday;
        [ObservableProperty] private decimal _dashboardSalesToday;
        [ObservableProperty] private int _dashboardDueMaintenance;
        [ObservableProperty] private int _dashboardActiveWarranties;

        [ObservableProperty] private string _searchTerm = string.Empty;
        [ObservableProperty] private string _barcodeInput = string.Empty;
        [ObservableProperty] private decimal _discount;
        [ObservableProperty] private decimal _tax;
        [ObservableProperty] private decimal _paymentAmount;
        [ObservableProperty] private long? _lastSavedInvoiceId;
        [ObservableProperty] private ProductSearchDto? _selectedProduct;
        [ObservableProperty] private PosCartItemViewModel? _selectedCartItem;

        [ObservableProperty] private string _productsSearchTerm = string.Empty;
        [ObservableProperty] private ProductManagementDto? _selectedManagedProduct;
        [ObservableProperty] private long? _editingProductId;
        [ObservableProperty] private string _editingProductSku = string.Empty;
        [ObservableProperty] private string _editingProductName = string.Empty;
        [ObservableProperty] private decimal _editingProductCostPrice;
        [ObservableProperty] private decimal _editingProductSalePrice;
        [ObservableProperty] private bool _editingProductIsActive = true;
        [ObservableProperty] private decimal _stockMovementQuantity = 1m;
        [ObservableProperty] private string _stockMovementReason = string.Empty;

        [ObservableProperty] private string _customersSearchTerm = string.Empty;
        [ObservableProperty] private CustomerManagementDto? _selectedManagedCustomer;
        [ObservableProperty] private long? _editingCustomerId;
        [ObservableProperty] private string _editingCustomerFullName = string.Empty;
        [ObservableProperty] private string _editingCustomerPhone = string.Empty;
        [ObservableProperty] private string _editingCustomerLocation = string.Empty;
        [ObservableProperty] private string _editingCustomerNotes = string.Empty;

        [ObservableProperty] private string _warrantySearchTerm = string.Empty;
        [ObservableProperty] private WarrantyManagementDto? _selectedWarranty;
        [ObservableProperty] private long _warrantyCustomerId;
        [ObservableProperty] private long _warrantyInvoiceId;
        [ObservableProperty] private string _warrantyDeviceType = string.Empty;
        [ObservableProperty] private string _warrantyModel = string.Empty;
        [ObservableProperty] private string _warrantySerialNumber = string.Empty;
        [ObservableProperty] private DateTime _warrantySoldAt = DateTime.UtcNow;

        [ObservableProperty] private string _maintenanceSearchTerm = string.Empty;
        [ObservableProperty] private MaintenanceScheduleManagementDto? _selectedMaintenanceSchedule;
        [ObservableProperty] private string _maintenanceWorkType = string.Empty;
        [ObservableProperty] private string _maintenanceVisitNotes = string.Empty;
        [ObservableProperty] private decimal _maintenanceVisitCost;
        [ObservableProperty] private bool _maintenanceWarrantyCovered = true;

        [ObservableProperty] private DateTime _reportFromDate = DateTime.UtcNow.Date.AddDays(-30);
        [ObservableProperty] private DateTime _reportToDate = DateTime.UtcNow.Date;
        [ObservableProperty] private int _reportTotalInvoices;
        [ObservableProperty] private decimal _reportGrossSales;
        [ObservableProperty] private decimal _reportNetSales;
        [ObservableProperty] private decimal _reportProfit;
        [ObservableProperty] private decimal _reportTotalPayments;

        [ObservableProperty] private string _settingsCompanyName = string.Empty;
        [ObservableProperty] private string _settingsAddress = string.Empty;
        [ObservableProperty] private string _settingsPhone = string.Empty;
        [ObservableProperty] private string _settingsInvoicePrefix = "INV";
        [ObservableProperty] private int _settingsNextInvoiceNumber = 1;
        [ObservableProperty] private string _settingsCurrency = "USD";
        [ObservableProperty] private string _settingsPrinterName = string.Empty;
        [ObservableProperty] private string _backupFilePath = string.Empty;
        [ObservableProperty] private string _lastBackupFilePath = string.Empty;

        [ObservableProperty] private string _usersSearchTerm = string.Empty;
        [ObservableProperty] private UserManagementDto? _selectedManagedUser;
        [ObservableProperty] private long? _editingUserId;
        [ObservableProperty] private string _editingUsername = string.Empty;
        [ObservableProperty] private string _editingUserPassword = string.Empty;
        [ObservableProperty] private UserRole _editingUserRole = UserRole.Cashier;
        [ObservableProperty] private bool _editingUserIsActive = true;

        [ObservableProperty] private object? _currentContent;
        [ObservableProperty] private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        [ObservableProperty] private string _currentLanguageCode = "EN";
        [ObservableProperty] private bool _canAccessUsersModule;
        [ObservableProperty] private bool _canProcessSales;

        public ObservableCollection<ProductSearchDto> Products { get; } = [];
        public ObservableCollection<PosCartItemViewModel> CartItems { get; } = [];
        public ObservableCollection<ProductManagementDto> ManagedProducts { get; } = [];
        public ObservableCollection<CustomerManagementDto> ManagedCustomers { get; } = [];
        public ObservableCollection<WarrantyManagementDto> Warranties { get; } = [];
        public ObservableCollection<MaintenanceScheduleManagementDto> MaintenanceSchedules { get; } = [];
        public ObservableCollection<ReportDailySalesDto> DailySalesReport { get; } = [];
        public ObservableCollection<ReportTopProductDto> TopProductsReport { get; } = [];
        public ObservableCollection<UserManagementDto> ManagedUsers { get; } = [];
        public ObservableCollection<UserRole> AvailableUserRoles { get; } = [UserRole.Owner, UserRole.Manager, UserRole.Cashier];
        public ObservableCollection<string> AvailableCurrencies { get; } = ["USD", "SAR"];
        public ObservableCollection<string> LowStockAlerts { get; } = [];

        public bool HasLowStockAlerts => LowStockAlerts.Count > 0;
        public string LowStockAlertsSummary => Lf("MsgLowStockSummary", $"{LowStockAlerts.Count} items are near depletion.", LowStockAlerts.Count);

        public bool IsDashboardModule => ActiveModule == "DASHBOARD";
        public bool IsPosModule => ActiveModule == "POS";
        public bool IsProductsModule => ActiveModule == "PRODUCTS";
        public bool IsCustomersModule => ActiveModule == "CUSTOMERS";
        public bool IsWarrantyModule => ActiveModule == "WARRANTY";
        public bool IsMaintenanceModule => ActiveModule == "MAINTENANCE";
        public bool IsReportsModule => ActiveModule == "REPORTS";
        public bool IsSettingsModule => ActiveModule == "SETTINGS";
        public bool IsUsersModule => ActiveModule == "USERS";

        public IAsyncRelayCommand ShowDashboardModuleCommand { get; }
        public IRelayCommand ShowPosModuleCommand { get; }
        public IAsyncRelayCommand ShowProductsModuleCommand { get; }
        public IAsyncRelayCommand ShowCustomersModuleCommand { get; }
        public IAsyncRelayCommand ShowWarrantyModuleCommand { get; }
        public IAsyncRelayCommand ShowMaintenanceModuleCommand { get; }
        public IAsyncRelayCommand ShowReportsModuleCommand { get; }
        public IAsyncRelayCommand ShowSettingsModuleCommand { get; }
        public IAsyncRelayCommand ShowUsersModuleCommand { get; }

        public IAsyncRelayCommand SearchCommand { get; }
        public IRelayCommand AddSelectedToCartCommand { get; }
        public IAsyncRelayCommand ScanBarcodeCommand { get; }
        public IRelayCommand IncreaseQuantityCommand { get; }
        public IRelayCommand DecreaseQuantityCommand { get; }
        public IRelayCommand RemoveSelectedCartItemCommand { get; }
        public IAsyncRelayCommand SaveInvoiceCommand { get; }
        public IAsyncRelayCommand PayCashCommand { get; }
        public IAsyncRelayCommand PayPartialCommand { get; }
        public IAsyncRelayCommand PrintInvoiceCommand { get; }
        public IAsyncRelayCommand PrintThermalCommand { get; }
        public IRelayCommand CancelInvoiceCommand { get; }
        public IRelayCommand FocusSearchCommand { get; }
        public IRelayCommand SwitchToEnglishCommand { get; }
        public IRelayCommand SwitchToArabicCommand { get; }

        public IAsyncRelayCommand LoadManagedProductsCommand { get; }
        public IAsyncRelayCommand SaveManagedProductCommand { get; }
        public IRelayCommand NewManagedProductCommand { get; }
        public IAsyncRelayCommand DeactivateManagedProductCommand { get; }
        public IAsyncRelayCommand ReactivateManagedProductCommand { get; }
        public IAsyncRelayCommand DeleteManagedProductCommand { get; }
        public IAsyncRelayCommand StockInCommand { get; }
        public IAsyncRelayCommand StockOutCommand { get; }

        public IAsyncRelayCommand LoadManagedCustomersCommand { get; }
        public IAsyncRelayCommand SaveManagedCustomerCommand { get; }
        public IRelayCommand NewManagedCustomerCommand { get; }
        public IAsyncRelayCommand DeactivateManagedCustomerCommand { get; }
        public IAsyncRelayCommand ReactivateManagedCustomerCommand { get; }
        public IAsyncRelayCommand DeleteManagedCustomerCommand { get; }

        public IAsyncRelayCommand LoadWarrantiesCommand { get; }
        public IAsyncRelayCommand RegisterWarrantyCommand { get; }
        public IAsyncRelayCommand CancelWarrantyCommand { get; }
        public IAsyncRelayCommand ReactivateWarrantyCommand { get; }
        public IAsyncRelayCommand DeleteWarrantyCommand { get; }

        public IAsyncRelayCommand LoadMaintenanceSchedulesCommand { get; }
        public IAsyncRelayCommand MarkMaintenanceDoneCommand { get; }
        public IAsyncRelayCommand MarkMaintenanceSkippedCommand { get; }
        public IAsyncRelayCommand AddMaintenanceVisitCommand { get; }
        public IAsyncRelayCommand LoadReportsCommand { get; }
        public IRelayCommand PrintReportCommand { get; }
        public IRelayCommand ExportReportCommand { get; }
        public IAsyncRelayCommand LoadSettingsCommand { get; }
        public IAsyncRelayCommand SaveSettingsCommand { get; }
        public IAsyncRelayCommand CreateBackupCommand { get; }
        public IAsyncRelayCommand RestoreBackupCommand { get; }
        public IAsyncRelayCommand VerifyBackupCommand { get; }
        public IAsyncRelayCommand LoadManagedUsersCommand { get; }
        public IAsyncRelayCommand SaveManagedUserCommand { get; }
        public IRelayCommand NewManagedUserCommand { get; }
        public IAsyncRelayCommand DeactivateManagedUserCommand { get; }

        public MainWindowViewModel(
            IProductSearchService productSearchService,
            IProductManagementService productManagementService,
            ICustomerManagementService customerManagementService,
            IWarrantyManagementService warrantyManagementService,
            IMaintenanceManagementService maintenanceManagementService,
            IReportingService reportingService,
            IDashboardService dashboardService,
            ISettingsService settingsService,
            IUserManagementService userManagementService,
            IBackupRestoreService backupRestoreService,
            IPrintingService printingService,
            IInvoiceService invoiceService,
            IUserContextService userContextService,
            IPermissionService permissionService,
            ILocalizationService localizationService)
        {
            _productSearchService = productSearchService;
            _productManagementService = productManagementService;
            _customerManagementService = customerManagementService;
            _warrantyManagementService = warrantyManagementService;
            _maintenanceManagementService = maintenanceManagementService;
            _reportingService = reportingService;
            _dashboardService = dashboardService;
            _settingsService = settingsService;
            _userManagementService = userManagementService;
            _backupRestoreService = backupRestoreService;
            _printingService = printingService;
            _invoiceService = invoiceService;
            _userContextService = userContextService;
            _permissionService = permissionService;
            _localizationService = localizationService;

            ShowDashboardModuleCommand = new AsyncRelayCommand(ShowDashboardModuleAsync);
            ShowPosModuleCommand = new RelayCommand(() => ActiveModule = "POS");
            ShowProductsModuleCommand = new AsyncRelayCommand(ShowProductsModuleAsync);
            ShowCustomersModuleCommand = new AsyncRelayCommand(ShowCustomersModuleAsync);
            ShowWarrantyModuleCommand = new AsyncRelayCommand(ShowWarrantyModuleAsync);
            ShowMaintenanceModuleCommand = new AsyncRelayCommand(ShowMaintenanceModuleAsync);
            ShowReportsModuleCommand = new AsyncRelayCommand(ShowReportsModuleAsync);
            ShowSettingsModuleCommand = new AsyncRelayCommand(ShowSettingsModuleAsync);
            ShowUsersModuleCommand = new AsyncRelayCommand(ShowUsersModuleAsync);

            SearchCommand = new AsyncRelayCommand(SearchAsync);
            AddSelectedToCartCommand = new RelayCommand(AddSelectedToCart);
            ScanBarcodeCommand = new AsyncRelayCommand(ScanBarcodeAsync);
            IncreaseQuantityCommand = new RelayCommand(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand(DecreaseQuantity);
            RemoveSelectedCartItemCommand = new RelayCommand(RemoveSelectedCartItem);
            SaveInvoiceCommand = new AsyncRelayCommand(() => SaveInvoiceAsync(Total), CanSaveOrPay);
            PayCashCommand = new AsyncRelayCommand(() => SaveInvoiceAsync(Total), CanSaveOrPay);
            PayPartialCommand = new AsyncRelayCommand(() => SaveInvoiceAsync(PaymentAmount), CanSaveOrPay);
            PrintInvoiceCommand = new AsyncRelayCommand(() => PrintLastInvoiceAsync(PrintTemplateType.A4));
            PrintThermalCommand = new AsyncRelayCommand(() => PrintLastInvoiceAsync(PrintTemplateType.Thermal80mm));
            CancelInvoiceCommand = new RelayCommand(CancelSale);
            FocusSearchCommand = new RelayCommand(() => StatusMessage = L("MsgSearchFocusRequested", "Search focus requested (F3)."));
            SwitchToEnglishCommand = new RelayCommand(() => _localizationService.SetLanguage("en-US"));
            SwitchToArabicCommand = new RelayCommand(() => _localizationService.SetLanguage("ar-SA"));

            LoadManagedProductsCommand = new AsyncRelayCommand(LoadManagedProductsAsync);
            SaveManagedProductCommand = new AsyncRelayCommand(SaveManagedProductAsync);
            NewManagedProductCommand = new RelayCommand(ResetManagedProductForm);
            DeactivateManagedProductCommand = new AsyncRelayCommand(DeactivateManagedProductAsync);
            ReactivateManagedProductCommand = new AsyncRelayCommand(ReactivateManagedProductAsync);
            DeleteManagedProductCommand = new AsyncRelayCommand(DeleteManagedProductAsync);
            StockInCommand = new AsyncRelayCommand(() => AdjustStockAsync(InventoryMovementType.In));
            StockOutCommand = new AsyncRelayCommand(() => AdjustStockAsync(InventoryMovementType.Out));

            LoadManagedCustomersCommand = new AsyncRelayCommand(LoadManagedCustomersAsync);
            SaveManagedCustomerCommand = new AsyncRelayCommand(SaveManagedCustomerAsync);
            NewManagedCustomerCommand = new RelayCommand(ResetManagedCustomerForm);
            DeactivateManagedCustomerCommand = new AsyncRelayCommand(DeactivateManagedCustomerAsync);
            ReactivateManagedCustomerCommand = new AsyncRelayCommand(ReactivateManagedCustomerAsync);
            DeleteManagedCustomerCommand = new AsyncRelayCommand(DeleteManagedCustomerAsync);

            LoadWarrantiesCommand = new AsyncRelayCommand(LoadWarrantiesAsync);
            RegisterWarrantyCommand = new AsyncRelayCommand(RegisterWarrantyAsync);
            CancelWarrantyCommand = new AsyncRelayCommand(CancelWarrantyAsync);
            ReactivateWarrantyCommand = new AsyncRelayCommand(ReactivateWarrantyAsync);
            DeleteWarrantyCommand = new AsyncRelayCommand(DeleteWarrantyAsync);

            LoadMaintenanceSchedulesCommand = new AsyncRelayCommand(LoadMaintenanceSchedulesAsync);
            MarkMaintenanceDoneCommand = new AsyncRelayCommand(() => SetMaintenanceStatusAsync(MaintenanceStatus.Done));
            MarkMaintenanceSkippedCommand = new AsyncRelayCommand(() => SetMaintenanceStatusAsync(MaintenanceStatus.Skipped));
            AddMaintenanceVisitCommand = new AsyncRelayCommand(AddMaintenanceVisitAsync);
            LoadReportsCommand = new AsyncRelayCommand(LoadReportsAsync);
            PrintReportCommand = new RelayCommand(PrintReport);
            ExportReportCommand = new RelayCommand(ExportReportHtml);
            LoadSettingsCommand = new AsyncRelayCommand(LoadSettingsAsync);
            SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
            CreateBackupCommand = new AsyncRelayCommand(CreateBackupAsync);
            RestoreBackupCommand = new AsyncRelayCommand(RestoreBackupAsync);
            VerifyBackupCommand = new AsyncRelayCommand(VerifyBackupAsync);
            LoadManagedUsersCommand = new AsyncRelayCommand(LoadManagedUsersAsync);
            SaveManagedUserCommand = new AsyncRelayCommand(SaveManagedUserAsync);
            NewManagedUserCommand = new RelayCommand(ResetManagedUserForm);
            DeactivateManagedUserCommand = new AsyncRelayCommand(DeactivateManagedUserAsync);
            LowStockAlerts.CollectionChanged += OnLowStockAlertsChanged;

            InitializeSecurityContext();
            _localizationService.LanguageChanged += OnLanguageChanged;
            ApplyLanguageState();
            ApplyCurrencyState(SettingsCurrency);
            _ = InitializeCurrencyFromSettingsAsync();
            _ = ShowDashboardModuleAsync();
            StatusMessage = L("MsgSystemInitialized", "System initialized.");
        }

        public decimal Subtotal => CartItems.Sum(x => x.LineTotal);
        public decimal Total => Subtotal - Discount + Tax;

        private async Task ShowDashboardModuleAsync() { ActiveModule = "DASHBOARD"; await LoadDashboardAsync(); }
        private async Task ShowProductsModuleAsync() { ActiveModule = "PRODUCTS"; await LoadManagedProductsAsync(); await RefreshLowStockAlertsAsync(); }
        private async Task ShowCustomersModuleAsync() { ActiveModule = "CUSTOMERS"; await LoadManagedCustomersAsync(); }
        private async Task ShowWarrantyModuleAsync() { ActiveModule = "WARRANTY"; await LoadWarrantiesAsync(); }
        private async Task ShowMaintenanceModuleAsync() { ActiveModule = "MAINTENANCE"; await LoadMaintenanceSchedulesAsync(); }
        private async Task ShowReportsModuleAsync() { ActiveModule = "REPORTS"; await LoadReportsAsync(); }
        private async Task ShowSettingsModuleAsync() { ActiveModule = "SETTINGS"; await LoadSettingsAsync(); }
        private async Task ShowUsersModuleAsync()
        {
            if (!CanAccessUsersModule)
            {
                StatusMessage = L("MsgAccessDeniedUsersModule", "You are not allowed to access Users module.");
                return;
            }

            ActiveModule = "USERS";
            await LoadManagedUsersAsync();
        }

        private async Task LoadDashboardAsync()
        {
            var summary = await _dashboardService.GetSummaryAsync();
            DashboardTotalProducts = summary.TotalProducts;
            DashboardActiveCustomers = summary.ActiveCustomers;
            DashboardInvoicesToday = summary.InvoicesToday;
            DashboardSalesToday = summary.SalesToday;
            DashboardDueMaintenance = summary.DueMaintenanceCount;
            DashboardActiveWarranties = summary.ActiveWarrantyCount;
            StatusMessage = L("MsgDashboardLoaded", "Dashboard loaded.");
        }

        private async Task InitializeCurrencyFromSettingsAsync()
        {
            var settings = await _settingsService.GetAsync();
            SettingsCurrency = NormalizeCurrencyCode(settings.Currency);
            ApplyCurrencyState(SettingsCurrency);
        }

        private async Task SearchAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var results = await _productSearchService.SearchAsync(SearchTerm, 200);
                Products.Clear();
                foreach (var item in results) Products.Add(item);
                StatusMessage = Lf("MsgRecordsFetched", $"{Products.Count} records fetched.", Products.Count);
                await RefreshLowStockAlertsAsync();
            }
            finally { IsBusy = false; }
        }

        private void AddSelectedToCart()
        {
            if (SelectedProduct is null) return;
            AddOrUpdateCartItem(SelectedProduct);
            SelectedProduct = null;
        }

        private async Task ScanBarcodeAsync()
        {
            if (string.IsNullOrWhiteSpace(BarcodeInput)) return;
            var barcode = BarcodeInput.Trim();
            var product = Products.FirstOrDefault(x => string.Equals(x.SKU, barcode, StringComparison.OrdinalIgnoreCase));
            if (product is null)
            {
                var result = await _productSearchService.SearchAsync(barcode, 20);
                product = result.FirstOrDefault(x => string.Equals(x.SKU, barcode, StringComparison.OrdinalIgnoreCase));
            }
            if (product is null) { StatusMessage = L("MsgBarcodeNotFound", "Barcode not found in current search result."); return; }
            AddOrUpdateCartItem(product);
            BarcodeInput = string.Empty;
        }

        private void AddOrUpdateCartItem(ProductSearchDto product)
        {
            PlayCartBeep();
            var existing = CartItems.FirstOrDefault(x => x.ProductId == product.Id);
            if (existing is not null) existing.Quantity += 1;
            else CartItems.Add(new PosCartItemViewModel { ProductId = product.Id, Sku = product.SKU, Name = product.Name, UnitPrice = product.SalePrice, UnitCost = product.SalePrice, Quantity = 1m });
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
            NotifyPaymentCommandsState();

            if (product.QuantityOnHand <= LowStockThreshold)
            {
                StatusMessage = Lf(
                    "MsgAddedToCartLowStock",
                    $"Added {product.Name}. Warning: low stock ({product.QuantityOnHand:0.###}).",
                    product.Name,
                    product.QuantityOnHand);
            }
            else
            {
                StatusMessage = Lf("MsgAddedToCart", $"{product.Name} added to cart.", product.Name);
            }
        }

        private void IncreaseQuantity()
        {
            if (SelectedCartItem is null) return;
            SelectedCartItem.Quantity += 1;
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
            NotifyPaymentCommandsState();
        }

        private void DecreaseQuantity()
        {
            if (SelectedCartItem is null) return;
            SelectedCartItem.Quantity -= 1;
            if (SelectedCartItem.Quantity <= 0) { CartItems.Remove(SelectedCartItem); SelectedCartItem = null; }
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
            NotifyPaymentCommandsState();
        }

        private void RemoveSelectedCartItem()
        {
            if (SelectedCartItem is null) return;
            CartItems.Remove(SelectedCartItem);
            SelectedCartItem = null;
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
            NotifyPaymentCommandsState();
        }

        private async Task SaveInvoiceAsync(decimal paymentAmount)
        {
            if (IsBusy || CartItems.Count == 0) { if (CartItems.Count == 0) StatusMessage = L("MsgCartEmpty", "Cart is empty."); return; }
            IsBusy = true;
            NotifyPaymentCommandsState();
            try
            {
                var request = new CreateInvoiceRequestDto
                {
                    UserId = _userContextService.UserId,
                    CustomerId = null,
                    Discount = Discount,
                    Tax = Tax,
                    PaymentAmount = Math.Max(0m, paymentAmount),
                    PaymentMethod = Core.Enums.PaymentMethod.Cash,
                    Items = CartItems.Select(x => new CreateInvoiceItemDto { ProductId = x.ProductId, Quantity = x.Quantity, UnitPrice = x.UnitPrice }).ToList()
                };
                var invoiceId = await _invoiceService.CreateInvoiceAsync(request);
                LastSavedInvoiceId = invoiceId;
                StatusMessage = Lf("MsgInvoiceSaved", $"Invoice #{invoiceId} saved successfully.", invoiceId);
                CancelSale();
                await RefreshLowStockAlertsAsync();
            }
            catch (Exception ex) { StatusMessage = Lf("MsgSaveFailed", $"Save failed: {ex.Message}", ex.Message); }
            finally { IsBusy = false; NotifyPaymentCommandsState(); }
        }

        private void CancelSale()
        {
            CartItems.Clear(); Discount = 0; Tax = 0; PaymentAmount = 0; SelectedCartItem = null;
            OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(Total)); NotifyPaymentCommandsState();
            StatusMessage = L("MsgSaleCanceled", "Sale canceled and cart cleared.");
        }

        private async Task LoadManagedProductsAsync()
        {
            var result = await _productManagementService.GetProductsAsync(ProductsSearchTerm);
            ManagedProducts.Clear();
            foreach (var item in result) ManagedProducts.Add(item);
            StatusMessage = Lf("MsgProductsLoaded", $"{ManagedProducts.Count} products loaded for management.", ManagedProducts.Count);
        }

        private async Task SaveManagedProductAsync()
        {
            if (string.IsNullOrWhiteSpace(EditingProductSku) || string.IsNullOrWhiteSpace(EditingProductName)) { StatusMessage = L("MsgProductSkuNameRequired", "SKU and Name are required."); return; }
            await _productManagementService.UpsertAsync(new UpsertProductRequestDto { Id = EditingProductId, SKU = EditingProductSku, Name = EditingProductName, CostPrice = EditingProductCostPrice, SalePrice = EditingProductSalePrice, IsActive = EditingProductIsActive });
            await LoadManagedProductsAsync();
            ResetManagedProductForm();
            StatusMessage = L("MsgProductSaved", "Product saved successfully.");
        }

        private async Task DeactivateManagedProductAsync()
        {
            if (SelectedManagedProduct is null) return;
            await _productManagementService.DeactivateAsync(SelectedManagedProduct.Id);
            await LoadManagedProductsAsync();
            StatusMessage = L("MsgProductDeactivated", "Product deactivated.");
        }

        private async Task ReactivateManagedProductAsync()
        {
            if (SelectedManagedProduct is null) return;
            try
            {
                await _productManagementService.ReactivateAsync(SelectedManagedProduct.Id);
                await LoadManagedProductsAsync();
                StatusMessage = L("MsgProductReactivated", "Product reactivated.");
            }
            catch (Exception ex)
            {
                StatusMessage = Lf("MsgActionFailed", $"Action failed: {ex.Message}", ex.Message);
            }
        }

        private async Task DeleteManagedProductAsync()
        {
            if (SelectedManagedProduct is null) return;
            try
            {
                await _productManagementService.DeleteAsync(SelectedManagedProduct.Id);
                await LoadManagedProductsAsync();
                ResetManagedProductForm();
                StatusMessage = L("MsgProductDeleted", "Product deleted.");
            }
            catch (Exception ex)
            {
                StatusMessage = Lf("MsgActionFailed", $"Action failed: {ex.Message}", ex.Message);
            }
        }

        private async Task AdjustStockAsync(InventoryMovementType movementType)
        {
            if (SelectedManagedProduct is null) { StatusMessage = L("MsgSelectProductFirst", "Select a product first."); return; }
            if (StockMovementQuantity <= 0) { StatusMessage = L("MsgQuantityGreaterThanZero", "Quantity must be greater than zero."); return; }
            await _productManagementService.AdjustStockAsync(new AdjustStockRequestDto { ProductId = SelectedManagedProduct.Id, MovementType = movementType, Quantity = StockMovementQuantity, Reason = StockMovementReason });
            await LoadManagedProductsAsync();
            await RefreshLowStockAlertsAsync();
            StatusMessage = L("MsgStockUpdated", "Stock updated.");
        }

        private void ResetManagedProductForm()
        {
            EditingProductId = null; EditingProductSku = string.Empty; EditingProductName = string.Empty; EditingProductCostPrice = 0; EditingProductSalePrice = 0; EditingProductIsActive = true;
        }

        private async Task LoadManagedCustomersAsync()
        {
            var result = await _customerManagementService.GetCustomersAsync(CustomersSearchTerm);
            ManagedCustomers.Clear();
            foreach (var item in result) ManagedCustomers.Add(item);
            StatusMessage = Lf("MsgCustomersLoaded", $"{ManagedCustomers.Count} customers loaded.", ManagedCustomers.Count);
        }

        private async Task SaveManagedCustomerAsync()
        {
            if (string.IsNullOrWhiteSpace(EditingCustomerFullName) || string.IsNullOrWhiteSpace(EditingCustomerPhone)) { StatusMessage = L("MsgCustomerNamePhoneRequired", "Customer name and phone are required."); return; }
            await _customerManagementService.UpsertAsync(new UpsertCustomerRequestDto { Id = EditingCustomerId, FullName = EditingCustomerFullName, Phone = EditingCustomerPhone, Location = string.IsNullOrWhiteSpace(EditingCustomerLocation) ? null : EditingCustomerLocation, Notes = string.IsNullOrWhiteSpace(EditingCustomerNotes) ? null : EditingCustomerNotes, IsActive = true });
            await LoadManagedCustomersAsync();
            ResetManagedCustomerForm();
            StatusMessage = L("MsgCustomerSaved", "Customer saved successfully.");
        }

        private async Task DeactivateManagedCustomerAsync()
        {
            if (SelectedManagedCustomer is null) return;
            await _customerManagementService.DeactivateAsync(SelectedManagedCustomer.Id);
            await LoadManagedCustomersAsync();
            StatusMessage = L("MsgCustomerDeactivated", "Customer deactivated.");
        }

        private async Task ReactivateManagedCustomerAsync()
        {
            if (SelectedManagedCustomer is null) return;
            try
            {
                await _customerManagementService.ReactivateAsync(SelectedManagedCustomer.Id);
                await LoadManagedCustomersAsync();
                StatusMessage = L("MsgCustomerReactivated", "Customer reactivated.");
            }
            catch (Exception ex)
            {
                StatusMessage = Lf("MsgActionFailed", $"Action failed: {ex.Message}", ex.Message);
            }
        }

        private async Task DeleteManagedCustomerAsync()
        {
            if (SelectedManagedCustomer is null) return;
            try
            {
                await _customerManagementService.DeleteAsync(SelectedManagedCustomer.Id);
                await LoadManagedCustomersAsync();
                ResetManagedCustomerForm();
                StatusMessage = L("MsgCustomerDeleted", "Customer deleted.");
            }
            catch (Exception ex)
            {
                StatusMessage = Lf("MsgActionFailed", $"Action failed: {ex.Message}", ex.Message);
            }
        }

        private void ResetManagedCustomerForm()
        {
            EditingCustomerId = null; EditingCustomerFullName = string.Empty; EditingCustomerPhone = string.Empty; EditingCustomerLocation = string.Empty; EditingCustomerNotes = string.Empty;
        }

        private async Task LoadManagedUsersAsync()
        {
            if (!CanAccessUsersModule)
            {
                StatusMessage = L("MsgAccessDeniedUsersModule", "You are not allowed to access Users module.");
                return;
            }

            var result = await _userManagementService.GetUsersAsync(UsersSearchTerm);
            ManagedUsers.Clear();
            foreach (var item in result) ManagedUsers.Add(item);
            StatusMessage = Lf("MsgUsersLoaded", $"{ManagedUsers.Count} users loaded.", ManagedUsers.Count);
        }

        private async Task SaveManagedUserAsync()
        {
            if (!CanAccessUsersModule)
            {
                StatusMessage = L("MsgAccessDeniedManageUsers", "You are not allowed to manage users.");
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingUsername))
            {
                StatusMessage = L("MsgUsernameRequired", "Username is required.");
                return;
            }

            if (EditingUserId is null && string.IsNullOrWhiteSpace(EditingUserPassword))
            {
                StatusMessage = L("MsgPasswordRequiredForNewUser", "Password is required for new user.");
                return;
            }

            await _userManagementService.UpsertAsync(new UpsertUserRequestDto
            {
                Id = EditingUserId,
                Username = EditingUsername,
                Password = string.IsNullOrWhiteSpace(EditingUserPassword) ? null : EditingUserPassword,
                Role = EditingUserRole,
                IsActive = EditingUserIsActive
            });

            await LoadManagedUsersAsync();
            ResetManagedUserForm();
            StatusMessage = L("MsgUserSaved", "User saved successfully.");
        }

        private async Task DeactivateManagedUserAsync()
        {
            if (!CanAccessUsersModule || SelectedManagedUser is null)
            {
                return;
            }

            await _userManagementService.DeactivateAsync(SelectedManagedUser.Id);
            await LoadManagedUsersAsync();
            StatusMessage = L("MsgUserDeactivated", "User deactivated.");
        }

        private void ResetManagedUserForm()
        {
            EditingUserId = null;
            EditingUsername = string.Empty;
            EditingUserPassword = string.Empty;
            EditingUserRole = UserRole.Cashier;
            EditingUserIsActive = true;
        }

        private async Task LoadWarrantiesAsync()
        {
            var result = await _warrantyManagementService.GetWarrantiesAsync(WarrantySearchTerm);
            Warranties.Clear();
            foreach (var item in result) Warranties.Add(item);
            StatusMessage = Lf("MsgWarrantiesLoaded", $"{Warranties.Count} warranties loaded.", Warranties.Count);
        }

        private async Task RegisterWarrantyAsync()
        {
            if (WarrantyCustomerId <= 0 || WarrantyInvoiceId <= 0 || string.IsNullOrWhiteSpace(WarrantySerialNumber))
            {
                StatusMessage = L("MsgWarrantyRequiredFields", "CustomerId, InvoiceId and Serial Number are required.");
                return;
            }

            var id = await _warrantyManagementService.RegisterDeviceWarrantyAsync(new RegisterDeviceWarrantyRequestDto
            {
                CustomerId = WarrantyCustomerId,
                InvoiceId = WarrantyInvoiceId,
                DeviceType = WarrantyDeviceType,
                Model = WarrantyModel,
                SerialNumber = WarrantySerialNumber,
                SoldAt = WarrantySoldAt
            });

            await LoadWarrantiesAsync();
            StatusMessage = Lf("MsgWarrantyRegistered", $"Warranty #{id} registered.", id);
        }

        private async Task CancelWarrantyAsync()
        {
            if (SelectedWarranty is null) return;
            await _warrantyManagementService.CancelWarrantyAsync(SelectedWarranty.WarrantyId);
            await LoadWarrantiesAsync();
            StatusMessage = L("MsgWarrantyCanceled", "Warranty canceled.");
        }

        private async Task ReactivateWarrantyAsync()
        {
            if (SelectedWarranty is null) return;
            try
            {
                await _warrantyManagementService.ReactivateWarrantyAsync(SelectedWarranty.WarrantyId);
                await LoadWarrantiesAsync();
                StatusMessage = L("MsgWarrantyReactivated", "Warranty reactivated.");
            }
            catch (Exception ex)
            {
                StatusMessage = Lf("MsgActionFailed", $"Action failed: {ex.Message}", ex.Message);
            }
        }

        private async Task DeleteWarrantyAsync()
        {
            if (SelectedWarranty is null) return;
            try
            {
                await _warrantyManagementService.DeleteWarrantyAsync(SelectedWarranty.WarrantyId);
                await LoadWarrantiesAsync();
                StatusMessage = L("MsgWarrantyDeleted", "Warranty deleted.");
            }
            catch (Exception ex)
            {
                StatusMessage = Lf("MsgActionFailed", $"Action failed: {ex.Message}", ex.Message);
            }
        }

        private async Task LoadMaintenanceSchedulesAsync()
        {
            var result = await _maintenanceManagementService.GetSchedulesAsync(MaintenanceSearchTerm);
            MaintenanceSchedules.Clear();
            foreach (var item in result) MaintenanceSchedules.Add(item);
            StatusMessage = Lf("MsgMaintenanceSchedulesLoaded", $"{MaintenanceSchedules.Count} maintenance schedules loaded.", MaintenanceSchedules.Count);
        }

        private async Task SetMaintenanceStatusAsync(MaintenanceStatus status)
        {
            if (SelectedMaintenanceSchedule is null) return;
            await _maintenanceManagementService.SetScheduleStatusAsync(SelectedMaintenanceSchedule.ScheduleId, status);
            await LoadMaintenanceSchedulesAsync();
            StatusMessage = Lf("MsgMaintenanceMarked", $"Maintenance marked as {status}.", status);
        }

        private async Task AddMaintenanceVisitAsync()
        {
            if (SelectedMaintenanceSchedule is null || string.IsNullOrWhiteSpace(MaintenanceWorkType))
            {
                StatusMessage = L("MsgSelectScheduleProvideWorkType", "Select schedule and provide work type.");
                return;
            }

            await _maintenanceManagementService.AddVisitAsync(new AddMaintenanceVisitRequestDto
            {
                ScheduleId = SelectedMaintenanceSchedule.ScheduleId,
                VisitDate = DateTime.UtcNow,
                WorkType = MaintenanceWorkType,
                Notes = MaintenanceVisitNotes,
                WarrantyCovered = MaintenanceWarrantyCovered,
                CostAmount = MaintenanceVisitCost
            });

            await LoadMaintenanceSchedulesAsync();
            StatusMessage = L("MsgMaintenanceVisitAdded", "Maintenance visit added.");
        }

        private async Task LoadReportsAsync()
        {
            try
            {
                var from = ReportFromDate.Date;
                var to = ReportToDate.Date.AddDays(1).AddTicks(-1);
                if (to < from)
                {
                    StatusMessage = L("MsgReportDateRangeInvalid", "Report date range is invalid.");
                    return;
                }

                var summary = await _reportingService.GetSummaryAsync(from, to);
                ReportTotalInvoices = summary.TotalInvoices;
                ReportGrossSales = summary.GrossSales;
                ReportNetSales = summary.NetSales;
                ReportProfit = summary.Profit;
                ReportTotalPayments = summary.TotalPayments;

                DailySalesReport.Clear();
                foreach (var item in summary.DailySales)
                {
                    DailySalesReport.Add(item);
                }

                TopProductsReport.Clear();
                foreach (var item in summary.TopProducts)
                {
                    TopProductsReport.Add(item);
                }

                StatusMessage = L("MsgReportsGenerated", "Reports generated successfully.");
            }
            catch (Exception ex)
            {
                StatusMessage = Lf("MsgActionFailed", $"Action failed: {ex.Message}", ex.Message);
            }
        }

        private void PrintReport()
        {
            try
            {
                var document = BuildReportDocument();
                var queue = ResolveReportPrintQueue();
                if (queue is null)
                {
                    StatusMessage = L("MsgReportPrinterNotFound", "No available printer was found.");
                    return;
                }

                var printDialog = new PrintDialog
                {
                    PrintQueue = queue,
                    PrintTicket = queue.DefaultPrintTicket
                };

                document.ColumnWidth = printDialog.PrintableAreaWidth;
                document.PageWidth = printDialog.PrintableAreaWidth;
                document.PageHeight = printDialog.PrintableAreaHeight;
                document.PagePadding = new Thickness(24);

                printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, L("PrintReport", "Print Report"));
                StatusMessage = L("MsgReportPrinted", "Report sent to printer.");
            }
            catch (Exception ex)
            {
                StatusMessage = Lf("MsgActionFailed", $"Action failed: {ex.Message}", ex.Message);
            }
        }

        private PrintQueue? ResolveReportPrintQueue()
        {
            try
            {
                var server = new LocalPrintServer();
                if (!string.IsNullOrWhiteSpace(SettingsPrinterName))
                {
                    var namedQueue = server
                        .GetPrintQueues()
                        .FirstOrDefault(q => string.Equals(q.Name, SettingsPrinterName, StringComparison.OrdinalIgnoreCase));
                    if (namedQueue is not null)
                    {
                        return namedQueue;
                    }
                }

                return LocalPrintServer.GetDefaultPrintQueue();
            }
            catch
            {
                return null;
            }
        }

        private void ExportReportHtml()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = L("DownloadReport", "Download Report"),
                    Filter = "HTML (*.html)|*.html",
                    FileName = $"report-{DateTime.Now:yyyyMMdd-HHmmss}.html"
                };

                if (dialog.ShowDialog() != true)
                {
                    StatusMessage = L("MsgReportSaveCanceled", "Report export canceled.");
                    return;
                }

                var html = BuildReportHtml();
                File.WriteAllText(dialog.FileName, html, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                StatusMessage = Lf("MsgReportSaved", "Report exported to: {0}", dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = Lf("MsgActionFailed", $"Action failed: {ex.Message}", ex.Message);
            }
        }

        private FlowDocument BuildReportDocument()
        {
            var isArabic = string.Equals(_localizationService.CurrentCultureCode, "ar-SA", StringComparison.OrdinalIgnoreCase);
            var from = ReportFromDate.Date;
            var to = ReportToDate.Date;
            var generatedAt = DateTime.Now;

            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 13,
                FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                PagePadding = new Thickness(24)
            };

            document.Blocks.Add(new Paragraph(new Run(L("NavReports", "Reports")))
            {
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 8)
            });

            document.Blocks.Add(new Paragraph(new Run($"{L("CompanyName", "Company Name")}: {CompanyName}")) { Margin = new Thickness(0, 0, 0, 2) });
            document.Blocks.Add(new Paragraph(new Run($"{L("ReportPeriod", "Period")}: {from:yyyy-MM-dd} - {to:yyyy-MM-dd}")) { Margin = new Thickness(0, 0, 0, 2) });
            document.Blocks.Add(new Paragraph(new Run($"{L("ReportGeneratedAt", "Generated At")}: {generatedAt:yyyy-MM-dd HH:mm}")) { Margin = new Thickness(0, 0, 0, 14) });

            document.Blocks.Add(new Paragraph(new Run(L("ReportSummarySection", "Summary"))) { FontSize = 16, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 8) });
            var summary = new Table { CellSpacing = 0 };
            summary.Columns.Add(new TableColumn { Width = new GridLength(260) });
            summary.Columns.Add(new TableColumn { Width = new GridLength(220) });
            var summaryGroup = new TableRowGroup();
            summaryGroup.Rows.Add(MakeRow(L("TotalInvoices", "Total Invoices"), ReportTotalInvoices.ToString(CultureInfo.CurrentCulture)));
            summaryGroup.Rows.Add(MakeRow(L("GrossSales", "Gross Sales"), FormatCurrency(ReportGrossSales)));
            summaryGroup.Rows.Add(MakeRow(L("NetSales", "Net Sales"), FormatCurrency(ReportNetSales)));
            summaryGroup.Rows.Add(MakeRow(L("Profit", "Profit"), FormatCurrency(ReportProfit)));
            summaryGroup.Rows.Add(MakeRow(L("Payments", "Payments"), FormatCurrency(ReportTotalPayments)));
            summary.RowGroups.Add(summaryGroup);
            document.Blocks.Add(summary);

            document.Blocks.Add(new Paragraph(new Run(L("DailySales", "Daily Sales"))) { FontSize = 16, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 16, 0, 8) });
            var dailyTable = BuildStyledTable(
                [L("Date", "Date"), L("TotalInvoices", "Total Invoices"), L("NetSales", "Net Sales"), L("Profit", "Profit")],
                DailySalesReport
                    .Select(x => (IReadOnlyList<string>)
                    [
                        x.Date.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture),
                        x.InvoiceCount.ToString(CultureInfo.CurrentCulture),
                        FormatCurrency(x.NetSales),
                        FormatCurrency(x.Profit)
                    ])
                    .ToList());
            document.Blocks.Add(dailyTable);

            document.Blocks.Add(new Paragraph(new Run(L("TopProducts", "Top Products"))) { FontSize = 16, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 16, 0, 8) });
            var productsTable = BuildStyledTable(
                [L("ProductName", "Product Name"), L("Qty", "Qty"), L("NetSales", "Net Sales"), L("Profit", "Profit")],
                TopProductsReport
                    .Select(x => (IReadOnlyList<string>)
                    [
                        x.ProductName,
                        x.QuantitySold.ToString("0.###", CultureInfo.CurrentCulture),
                        FormatCurrency(x.SalesAmount),
                        FormatCurrency(x.ProfitAmount)
                    ])
                    .ToList());
            document.Blocks.Add(productsTable);

            return document;
        }

        private static TableRow MakeRow(string label, string value)
        {
            var labelCell = new TableCell(new Paragraph(new Run(label)) { Margin = new Thickness(0) })
            {
                Padding = new Thickness(8, 6, 8, 6),
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 235, 242)),
                BorderThickness = new Thickness(0.6)
            };

            var valueCell = new TableCell(new Paragraph(new Run(value)) { Margin = new Thickness(0) })
            {
                Padding = new Thickness(8, 6, 8, 6),
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 235, 242)),
                BorderThickness = new Thickness(0.6)
            };

            var row = new TableRow();
            row.Cells.Add(labelCell);
            row.Cells.Add(valueCell);
            return row;
        }

        private static Table BuildStyledTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            var borderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 235, 242));
            var headerBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(243, 247, 252));
            var stripeBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 252, 255));

            var table = new Table { CellSpacing = 0 };
            foreach (var _ in headers)
            {
                table.Columns.Add(new TableColumn());
            }

            var group = new TableRowGroup();
            var headerRow = new TableRow();
            foreach (var header in headers)
            {
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run(header))
                {
                    Margin = new Thickness(0),
                    FontWeight = FontWeights.SemiBold
                })
                {
                    Padding = new Thickness(8, 6, 8, 6),
                    BorderBrush = borderBrush,
                    BorderThickness = new Thickness(0.7),
                    Background = headerBrush
                });
            }
            group.Rows.Add(headerRow);

            for (var i = 0; i < rows.Count; i++)
            {
                var dataRow = new TableRow();
                var cells = rows[i];
                foreach (var cell in cells)
                {
                    dataRow.Cells.Add(new TableCell(new Paragraph(new Run(cell)) { Margin = new Thickness(0) })
                    {
                        Padding = new Thickness(8, 6, 8, 6),
                        BorderBrush = borderBrush,
                        BorderThickness = new Thickness(0.7),
                        Background = i % 2 == 1 ? stripeBrush : null
                    });
                }
                group.Rows.Add(dataRow);
            }

            table.RowGroups.Add(group);
            return table;
        }

        private string BuildReportHtml()
        {
            var isArabic = string.Equals(_localizationService.CurrentCultureCode, "ar-SA", StringComparison.OrdinalIgnoreCase);
            var dir = isArabic ? "rtl" : "ltr";
            var align = isArabic ? "right" : "left";
            var from = ReportFromDate.Date;
            var to = ReportToDate.Date;
            var generatedAt = DateTime.Now;

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine($"<html lang=\"{(isArabic ? "ar" : "en")}\" dir=\"{dir}\">");
            sb.AppendLine("<head><meta charset=\"utf-8\" /><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
            sb.AppendLine($"<title>{EscapeHtml(L("NavReports", "Reports"))}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:'Segoe UI',Tahoma,Arial,sans-serif;background:#f4f7fb;color:#17212f;margin:0;padding:24px;}");
            sb.AppendLine(".wrap{max-width:980px;margin:0 auto;background:#fff;border:1px solid #e7edf5;border-radius:14px;padding:22px;}");
            sb.AppendLine("h1{margin:0 0 10px 0;font-size:28px;} .meta{color:#5a6d82;font-size:13px;margin:3px 0;}");
            sb.AppendLine(".sec{margin-top:20px;} .sec h2{font-size:18px;margin:0 0 8px 0;}");
            sb.AppendLine("table{width:100%;border-collapse:collapse;margin-top:8px;} th,td{border:1px solid #e7edf5;padding:8px 10px;text-align:" + align + ";} th{background:#f3f7fc;}");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<div class=\"wrap\">");
            sb.AppendLine($"<h1>{EscapeHtml(L("NavReports", "Reports"))}</h1>");
            sb.AppendLine($"<div class=\"meta\">{EscapeHtml(L("CompanyName", "Company Name"))}: {EscapeHtml(CompanyName)}</div>");
            sb.AppendLine($"<div class=\"meta\">{EscapeHtml(L("ReportPeriod", "Period"))}: {from:yyyy-MM-dd} - {to:yyyy-MM-dd}</div>");
            sb.AppendLine($"<div class=\"meta\">{EscapeHtml(L("ReportGeneratedAt", "Generated At"))}: {generatedAt:yyyy-MM-dd HH:mm}</div>");

            sb.AppendLine($"<div class=\"sec\"><h2>{EscapeHtml(L("ReportSummarySection", "Summary"))}</h2>");
            sb.AppendLine("<table><tbody>");
            AppendSummaryRow(sb, L("TotalInvoices", "Total Invoices"), ReportTotalInvoices.ToString(CultureInfo.CurrentCulture));
            AppendSummaryRow(sb, L("GrossSales", "Gross Sales"), FormatCurrency(ReportGrossSales));
            AppendSummaryRow(sb, L("NetSales", "Net Sales"), FormatCurrency(ReportNetSales));
            AppendSummaryRow(sb, L("Profit", "Profit"), FormatCurrency(ReportProfit));
            AppendSummaryRow(sb, L("Payments", "Payments"), FormatCurrency(ReportTotalPayments));
            sb.AppendLine("</tbody></table></div>");

            sb.AppendLine($"<div class=\"sec\"><h2>{EscapeHtml(L("DailySales", "Daily Sales"))}</h2>");
            sb.AppendLine("<table><thead><tr>");
            sb.AppendLine($"<th>{EscapeHtml(L("Date", "Date"))}</th><th>{EscapeHtml(L("TotalInvoices", "Total Invoices"))}</th><th>{EscapeHtml(L("NetSales", "Net Sales"))}</th><th>{EscapeHtml(L("Profit", "Profit"))}</th>");
            sb.AppendLine("</tr></thead><tbody>");
            foreach (var row in DailySalesReport)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{row.Date:yyyy-MM-dd}</td><td>{row.InvoiceCount}</td><td>{EscapeHtml(FormatCurrency(row.NetSales))}</td><td>{EscapeHtml(FormatCurrency(row.Profit))}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody></table></div>");

            sb.AppendLine($"<div class=\"sec\"><h2>{EscapeHtml(L("TopProducts", "Top Products"))}</h2>");
            sb.AppendLine("<table><thead><tr>");
            sb.AppendLine($"<th>{EscapeHtml(L("ProductName", "Product Name"))}</th><th>{EscapeHtml(L("Qty", "Qty"))}</th><th>{EscapeHtml(L("NetSales", "Net Sales"))}</th><th>{EscapeHtml(L("Profit", "Profit"))}</th>");
            sb.AppendLine("</tr></thead><tbody>");
            foreach (var row in TopProductsReport)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{EscapeHtml(row.ProductName)}</td><td>{row.QuantitySold:0.###}</td><td>{EscapeHtml(FormatCurrency(row.SalesAmount))}</td><td>{EscapeHtml(FormatCurrency(row.ProfitAmount))}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody></table></div>");
            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private static void AppendSummaryRow(StringBuilder sb, string label, string value)
            => sb.AppendLine($"<tr><td><strong>{EscapeHtml(label)}</strong></td><td>{EscapeHtml(value)}</td></tr>");

        private static string EscapeHtml(string? value)
            => System.Net.WebUtility.HtmlEncode(value ?? string.Empty);

        private static string FormatCurrency(decimal value)
            => value.ToString("C2", CultureInfo.CurrentCulture);

        private async Task LoadSettingsAsync()
        {
            var settings = await _settingsService.GetAsync();
            SettingsCompanyName = settings.CompanyName;
            SettingsAddress = settings.Address ?? string.Empty;
            SettingsPhone = settings.Phone ?? string.Empty;
            SettingsInvoicePrefix = settings.InvoicePrefix;
            SettingsNextInvoiceNumber = settings.NextInvoiceNumber;
            SettingsCurrency = NormalizeCurrencyCode(settings.Currency);
            ApplyCurrencyState(SettingsCurrency);
            SettingsPrinterName = settings.PrinterName ?? string.Empty;
            StatusMessage = L("MsgSettingsLoaded", "Settings loaded.");
        }

        private async Task SaveSettingsAsync()
        {
            if (string.IsNullOrWhiteSpace(SettingsCompanyName) || string.IsNullOrWhiteSpace(SettingsInvoicePrefix))
            {
                StatusMessage = L("MsgSettingsCompanyAndPrefixRequired", "Company Name and Invoice Prefix are required.");
                return;
            }

            var normalizedCurrency = NormalizeCurrencyCode(SettingsCurrency);
            await _settingsService.SaveAsync(new SettingsDto
            {
                CompanyName = SettingsCompanyName,
                Address = string.IsNullOrWhiteSpace(SettingsAddress) ? null : SettingsAddress,
                Phone = string.IsNullOrWhiteSpace(SettingsPhone) ? null : SettingsPhone,
                InvoicePrefix = SettingsInvoicePrefix,
                NextInvoiceNumber = SettingsNextInvoiceNumber <= 0 ? 1 : SettingsNextInvoiceNumber,
                Currency = normalizedCurrency,
                PrinterName = string.IsNullOrWhiteSpace(SettingsPrinterName) ? null : SettingsPrinterName
            });

            SettingsCurrency = normalizedCurrency;
            ApplyCurrencyState(normalizedCurrency);
            StatusMessage = L("MsgSettingsSaved", "Settings saved successfully.");
        }

        private async Task CreateBackupAsync()
        {
            var path = await _backupRestoreService.CreateBackupAsync(string.IsNullOrWhiteSpace(BackupFilePath) ? null : BackupFilePath);
            LastBackupFilePath = path;
            BackupFilePath = path;
            StatusMessage = Lf("MsgBackupCreated", $"Backup created: {path}", path);
        }

        private async Task RestoreBackupAsync()
        {
            if (string.IsNullOrWhiteSpace(BackupFilePath))
            {
                StatusMessage = L("MsgBackupPathRequired", "Backup file path is required.");
                return;
            }

            await _backupRestoreService.RestoreBackupAsync(BackupFilePath);
            StatusMessage = L("MsgBackupRestored", "Database restored from backup successfully.");
        }

        private async Task VerifyBackupAsync()
        {
            if (string.IsNullOrWhiteSpace(BackupFilePath))
            {
                StatusMessage = L("MsgBackupPathRequired", "Backup file path is required.");
                return;
            }

            var valid = await _backupRestoreService.VerifyDatabaseAsync(BackupFilePath);
            StatusMessage = valid
                ? L("MsgBackupIntegrityValid", "Backup integrity is valid.")
                : L("MsgBackupIntegrityFailed", "Backup integrity check failed.");
        }

        private async Task PrintLastInvoiceAsync(PrintTemplateType templateType)
        {
            if (LastSavedInvoiceId is null || LastSavedInvoiceId <= 0)
            {
                StatusMessage = L("MsgNoSavedInvoiceForPrinting", "No saved invoice available for printing.");
                return;
            }

            var printed = await _printingService.PrintInvoiceAsync(LastSavedInvoiceId.Value, templateType, SettingsPrinterName);
            StatusMessage = printed
                ? L("MsgInvoiceSentToPrinter", "Invoice sent to printer.")
                : L("MsgPrintingCanceledOrFailed", "Printing was canceled or failed.");
        }

        private void OnLanguageChanged()
        {
            ApplyLanguageState();
            ApplyCurrencyState(SettingsCurrency);
            OnPropertyChanged(nameof(LowStockAlertsSummary));
            StatusMessage = _localizationService.CurrentCultureCode == "ar-SA"
                ? L("MsgLanguageArabic", "Switched to Arabic.")
                : L("MsgLanguageEnglish", "Switched to English.");
        }

        private void ApplyLanguageState()
        {
            FlowDirection = _localizationService.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            CurrentLanguageCode = string.Equals(_localizationService.CurrentCultureCode, "ar-SA", StringComparison.OrdinalIgnoreCase) ? "AR" : "EN";
        }
        private static string NormalizeCurrencyCode(string? currencyCode)
            => string.Equals(currencyCode, "SAR", StringComparison.OrdinalIgnoreCase) ? "SAR" : "USD";

        private void ApplyCurrencyState(string currencyCode)
        {
            var normalized = NormalizeCurrencyCode(currencyCode);
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NativeDigits = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];
            culture.NumberFormat.DigitSubstitution = DigitShapes.None;
            culture.NumberFormat.CurrencySymbol = normalized == "SAR" ? "ر.س" : "$";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            culture.NumberFormat.CurrencyNegativePattern = 1;

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(ReportGrossSales));
            OnPropertyChanged(nameof(ReportNetSales));
            OnPropertyChanged(nameof(ReportProfit));
            OnPropertyChanged(nameof(ReportTotalPayments));
            OnPropertyChanged(nameof(DashboardSalesToday));
        }

        private async Task RefreshLowStockAlertsAsync()
        {
            var products = await _productManagementService.GetProductsAsync();
            var alerts = products
                .Where(x => x.IsActive && x.QuantityOnHand <= LowStockThreshold)
                .OrderBy(x => x.QuantityOnHand)
                .ThenBy(x => x.Name)
                .Take(12)
                .Select(x => $"{x.Name} ({x.SKU}) - {x.QuantityOnHand:0.###}")
                .ToList();

            LowStockAlerts.Clear();
            foreach (var alert in alerts)
            {
                LowStockAlerts.Add(alert);
            }

            if (HasLowStockAlerts && IsPosModule)
            {
                StatusMessage = Lf(
                    "MsgLowStockAlert",
                    $"Low stock alert: {LowStockAlerts.Count} products are near depletion.",
                    LowStockAlerts.Count);
            }
        }

        private void PlayCartBeep()
        {
            try { SystemSounds.Beep.Play(); }
            catch { }
        }

        private void InitializeSecurityContext()
        {
            LoggedUserName = _userContextService.Username;
            LoggedUserRole = _userContextService.Role;
            CanAccessUsersModule = _permissionService.CanAccessUsersModule(LoggedUserRole);
            CanProcessSales = _permissionService.CanProcessSales(LoggedUserRole);
            NotifyPaymentCommandsState();
        }

        private bool CanSaveOrPay() => CanProcessSales && CartItems.Count > 0 && !IsBusy && Total >= 0m;
        private void NotifyPaymentCommandsState() { SaveInvoiceCommand.NotifyCanExecuteChanged(); PayCashCommand.NotifyCanExecuteChanged(); PayPartialCommand.NotifyCanExecuteChanged(); }

        partial void OnDiscountChanged(decimal value) { OnPropertyChanged(nameof(Total)); NotifyPaymentCommandsState(); }
        partial void OnTaxChanged(decimal value) { OnPropertyChanged(nameof(Total)); NotifyPaymentCommandsState(); }
        partial void OnPaymentAmountChanged(decimal value) { NotifyPaymentCommandsState(); }

        partial void OnActiveModuleChanged(string value)
        {
            OnPropertyChanged(nameof(IsDashboardModule));
            OnPropertyChanged(nameof(IsPosModule));
            OnPropertyChanged(nameof(IsProductsModule));
            OnPropertyChanged(nameof(IsCustomersModule));
            OnPropertyChanged(nameof(IsWarrantyModule));
            OnPropertyChanged(nameof(IsMaintenanceModule));
            OnPropertyChanged(nameof(IsReportsModule));
            OnPropertyChanged(nameof(IsSettingsModule));
            OnPropertyChanged(nameof(IsUsersModule));

            if (string.Equals(value, "POS", StringComparison.Ordinal))
            {
                _ = RefreshLowStockAlertsAsync();
            }
        }

        partial void OnSelectedManagedProductChanged(ProductManagementDto? value)
        {
            if (value is null) return;
            EditingProductId = value.Id;
            EditingProductSku = value.SKU;
            EditingProductName = value.Name;
            EditingProductCostPrice = value.CostPrice;
            EditingProductSalePrice = value.SalePrice;
            EditingProductIsActive = value.IsActive;
        }

        partial void OnSelectedManagedCustomerChanged(CustomerManagementDto? value)
        {
            if (value is null) return;
            EditingCustomerId = value.Id;
            EditingCustomerFullName = value.FullName;
            EditingCustomerPhone = value.Phone;
            EditingCustomerLocation = value.Location ?? string.Empty;
            EditingCustomerNotes = value.Notes ?? string.Empty;
        }

        partial void OnSelectedProductChanged(ProductSearchDto? value)
        {
            if (!IsPosModule || value is null) return;
            AddOrUpdateCartItem(value);
            SelectedProduct = null;
        }

        partial void OnSelectedManagedUserChanged(UserManagementDto? value)
        {
            if (value is null) return;
            EditingUserId = value.Id;
            EditingUsername = value.Username;
            EditingUserPassword = string.Empty;
            EditingUserRole = value.Role;
            EditingUserIsActive = value.IsActive;
        }

        private void OnLowStockAlertsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasLowStockAlerts));
            OnPropertyChanged(nameof(LowStockAlertsSummary));
        }

        private static string L(string key, string fallback)
        {
            var value = System.Windows.Application.Current.TryFindResource(key) as string;
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string Lf(string key, string fallbackFormat, params object[] args)
        {
            var format = L(key, fallbackFormat);
            return string.Format(format, args);
        }
    }
}
