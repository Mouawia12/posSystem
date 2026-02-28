using Application.DTOs;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Enums;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Presentation.ViewModels
{
    public partial class MainWindowViewModel : BaseViewModel
    {
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
        public IAsyncRelayCommand StockInCommand { get; }
        public IAsyncRelayCommand StockOutCommand { get; }

        public IAsyncRelayCommand LoadManagedCustomersCommand { get; }
        public IAsyncRelayCommand SaveManagedCustomerCommand { get; }
        public IRelayCommand NewManagedCustomerCommand { get; }
        public IAsyncRelayCommand DeactivateManagedCustomerCommand { get; }

        public IAsyncRelayCommand LoadWarrantiesCommand { get; }
        public IAsyncRelayCommand RegisterWarrantyCommand { get; }
        public IAsyncRelayCommand CancelWarrantyCommand { get; }

        public IAsyncRelayCommand LoadMaintenanceSchedulesCommand { get; }
        public IAsyncRelayCommand MarkMaintenanceDoneCommand { get; }
        public IAsyncRelayCommand MarkMaintenanceSkippedCommand { get; }
        public IAsyncRelayCommand AddMaintenanceVisitCommand { get; }
        public IAsyncRelayCommand LoadReportsCommand { get; }
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
            FocusSearchCommand = new RelayCommand(() => StatusMessage = "Search focus requested (F3).");
            SwitchToEnglishCommand = new RelayCommand(() => _localizationService.SetLanguage("en-US"));
            SwitchToArabicCommand = new RelayCommand(() => _localizationService.SetLanguage("ar-SA"));

            LoadManagedProductsCommand = new AsyncRelayCommand(LoadManagedProductsAsync);
            SaveManagedProductCommand = new AsyncRelayCommand(SaveManagedProductAsync);
            NewManagedProductCommand = new RelayCommand(ResetManagedProductForm);
            DeactivateManagedProductCommand = new AsyncRelayCommand(DeactivateManagedProductAsync);
            StockInCommand = new AsyncRelayCommand(() => AdjustStockAsync(InventoryMovementType.In));
            StockOutCommand = new AsyncRelayCommand(() => AdjustStockAsync(InventoryMovementType.Out));

            LoadManagedCustomersCommand = new AsyncRelayCommand(LoadManagedCustomersAsync);
            SaveManagedCustomerCommand = new AsyncRelayCommand(SaveManagedCustomerAsync);
            NewManagedCustomerCommand = new RelayCommand(ResetManagedCustomerForm);
            DeactivateManagedCustomerCommand = new AsyncRelayCommand(DeactivateManagedCustomerAsync);

            LoadWarrantiesCommand = new AsyncRelayCommand(LoadWarrantiesAsync);
            RegisterWarrantyCommand = new AsyncRelayCommand(RegisterWarrantyAsync);
            CancelWarrantyCommand = new AsyncRelayCommand(CancelWarrantyAsync);

            LoadMaintenanceSchedulesCommand = new AsyncRelayCommand(LoadMaintenanceSchedulesAsync);
            MarkMaintenanceDoneCommand = new AsyncRelayCommand(() => SetMaintenanceStatusAsync(MaintenanceStatus.Done));
            MarkMaintenanceSkippedCommand = new AsyncRelayCommand(() => SetMaintenanceStatusAsync(MaintenanceStatus.Skipped));
            AddMaintenanceVisitCommand = new AsyncRelayCommand(AddMaintenanceVisitAsync);
            LoadReportsCommand = new AsyncRelayCommand(LoadReportsAsync);
            LoadSettingsCommand = new AsyncRelayCommand(LoadSettingsAsync);
            SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
            CreateBackupCommand = new AsyncRelayCommand(CreateBackupAsync);
            RestoreBackupCommand = new AsyncRelayCommand(RestoreBackupAsync);
            VerifyBackupCommand = new AsyncRelayCommand(VerifyBackupAsync);
            LoadManagedUsersCommand = new AsyncRelayCommand(LoadManagedUsersAsync);
            SaveManagedUserCommand = new AsyncRelayCommand(SaveManagedUserAsync);
            NewManagedUserCommand = new RelayCommand(ResetManagedUserForm);
            DeactivateManagedUserCommand = new AsyncRelayCommand(DeactivateManagedUserAsync);

            InitializeSecurityContext();
            _localizationService.LanguageChanged += OnLanguageChanged;
            ApplyLanguageState();
            ApplyCurrencyState(SettingsCurrency);
            _ = InitializeCurrencyFromSettingsAsync();
            _ = ShowDashboardModuleAsync();
            StatusMessage = "System initialized.";
        }

        public decimal Subtotal => CartItems.Sum(x => x.LineTotal);
        public decimal Total => Subtotal - Discount + Tax;

        private async Task ShowDashboardModuleAsync() { ActiveModule = "DASHBOARD"; await LoadDashboardAsync(); }
        private async Task ShowProductsModuleAsync() { ActiveModule = "PRODUCTS"; await LoadManagedProductsAsync(); }
        private async Task ShowCustomersModuleAsync() { ActiveModule = "CUSTOMERS"; await LoadManagedCustomersAsync(); }
        private async Task ShowWarrantyModuleAsync() { ActiveModule = "WARRANTY"; await LoadWarrantiesAsync(); }
        private async Task ShowMaintenanceModuleAsync() { ActiveModule = "MAINTENANCE"; await LoadMaintenanceSchedulesAsync(); }
        private async Task ShowReportsModuleAsync() { ActiveModule = "REPORTS"; await LoadReportsAsync(); }
        private async Task ShowSettingsModuleAsync() { ActiveModule = "SETTINGS"; await LoadSettingsAsync(); }
        private async Task ShowUsersModuleAsync()
        {
            if (!CanAccessUsersModule)
            {
                StatusMessage = "You are not allowed to access Users module.";
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
            StatusMessage = "Dashboard loaded.";
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
                StatusMessage = $"{Products.Count} records fetched.";
            }
            finally { IsBusy = false; }
        }

        private void AddSelectedToCart() { if (SelectedProduct is not null) AddOrUpdateCartItem(SelectedProduct); }

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
            if (product is null) { StatusMessage = "Barcode not found in current search result."; return; }
            AddOrUpdateCartItem(product);
            BarcodeInput = string.Empty;
        }

        private void AddOrUpdateCartItem(ProductSearchDto product)
        {
            var existing = CartItems.FirstOrDefault(x => x.ProductId == product.Id);
            if (existing is not null) existing.Quantity += 1;
            else CartItems.Add(new PosCartItemViewModel { ProductId = product.Id, Sku = product.SKU, Name = product.Name, UnitPrice = product.SalePrice, UnitCost = product.SalePrice, Quantity = 1m });
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
            NotifyPaymentCommandsState();
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
            if (IsBusy || CartItems.Count == 0) { if (CartItems.Count == 0) StatusMessage = "Cart is empty."; return; }
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
                StatusMessage = $"Invoice #{invoiceId} saved successfully.";
                CancelSale();
            }
            catch (Exception ex) { StatusMessage = $"Save failed: {ex.Message}"; }
            finally { IsBusy = false; NotifyPaymentCommandsState(); }
        }

        private void CancelSale()
        {
            CartItems.Clear(); Discount = 0; Tax = 0; PaymentAmount = 0; SelectedCartItem = null;
            OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(Total)); NotifyPaymentCommandsState();
            StatusMessage = "Sale canceled and cart cleared.";
        }

        private async Task LoadManagedProductsAsync()
        {
            var result = await _productManagementService.GetProductsAsync(ProductsSearchTerm);
            ManagedProducts.Clear();
            foreach (var item in result) ManagedProducts.Add(item);
            StatusMessage = $"{ManagedProducts.Count} products loaded for management.";
        }

        private async Task SaveManagedProductAsync()
        {
            if (string.IsNullOrWhiteSpace(EditingProductSku) || string.IsNullOrWhiteSpace(EditingProductName)) { StatusMessage = "SKU and Name are required."; return; }
            await _productManagementService.UpsertAsync(new UpsertProductRequestDto { Id = EditingProductId, SKU = EditingProductSku, Name = EditingProductName, CostPrice = EditingProductCostPrice, SalePrice = EditingProductSalePrice, IsActive = EditingProductIsActive });
            await LoadManagedProductsAsync();
            ResetManagedProductForm();
            StatusMessage = "Product saved successfully.";
        }

        private async Task DeactivateManagedProductAsync()
        {
            if (SelectedManagedProduct is null) return;
            await _productManagementService.DeactivateAsync(SelectedManagedProduct.Id);
            await LoadManagedProductsAsync();
            StatusMessage = "Product deactivated.";
        }

        private async Task AdjustStockAsync(InventoryMovementType movementType)
        {
            if (SelectedManagedProduct is null) { StatusMessage = "Select a product first."; return; }
            if (StockMovementQuantity <= 0) { StatusMessage = "Quantity must be greater than zero."; return; }
            await _productManagementService.AdjustStockAsync(new AdjustStockRequestDto { ProductId = SelectedManagedProduct.Id, MovementType = movementType, Quantity = StockMovementQuantity, Reason = StockMovementReason });
            await LoadManagedProductsAsync();
            StatusMessage = "Stock updated.";
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
            StatusMessage = $"{ManagedCustomers.Count} customers loaded.";
        }

        private async Task SaveManagedCustomerAsync()
        {
            if (string.IsNullOrWhiteSpace(EditingCustomerFullName) || string.IsNullOrWhiteSpace(EditingCustomerPhone)) { StatusMessage = "Customer name and phone are required."; return; }
            await _customerManagementService.UpsertAsync(new UpsertCustomerRequestDto { Id = EditingCustomerId, FullName = EditingCustomerFullName, Phone = EditingCustomerPhone, Location = string.IsNullOrWhiteSpace(EditingCustomerLocation) ? null : EditingCustomerLocation, Notes = string.IsNullOrWhiteSpace(EditingCustomerNotes) ? null : EditingCustomerNotes, IsActive = true });
            await LoadManagedCustomersAsync();
            ResetManagedCustomerForm();
            StatusMessage = "Customer saved successfully.";
        }

        private async Task DeactivateManagedCustomerAsync()
        {
            if (SelectedManagedCustomer is null) return;
            await _customerManagementService.DeactivateAsync(SelectedManagedCustomer.Id);
            await LoadManagedCustomersAsync();
            StatusMessage = "Customer deactivated.";
        }

        private void ResetManagedCustomerForm()
        {
            EditingCustomerId = null; EditingCustomerFullName = string.Empty; EditingCustomerPhone = string.Empty; EditingCustomerLocation = string.Empty; EditingCustomerNotes = string.Empty;
        }

        private async Task LoadManagedUsersAsync()
        {
            if (!CanAccessUsersModule)
            {
                StatusMessage = "You are not allowed to access Users module.";
                return;
            }

            var result = await _userManagementService.GetUsersAsync(UsersSearchTerm);
            ManagedUsers.Clear();
            foreach (var item in result) ManagedUsers.Add(item);
            StatusMessage = $"{ManagedUsers.Count} users loaded.";
        }

        private async Task SaveManagedUserAsync()
        {
            if (!CanAccessUsersModule)
            {
                StatusMessage = "You are not allowed to manage users.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingUsername))
            {
                StatusMessage = "Username is required.";
                return;
            }

            if (EditingUserId is null && string.IsNullOrWhiteSpace(EditingUserPassword))
            {
                StatusMessage = "Password is required for new user.";
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
            StatusMessage = "User saved successfully.";
        }

        private async Task DeactivateManagedUserAsync()
        {
            if (!CanAccessUsersModule || SelectedManagedUser is null)
            {
                return;
            }

            await _userManagementService.DeactivateAsync(SelectedManagedUser.Id);
            await LoadManagedUsersAsync();
            StatusMessage = "User deactivated.";
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
            StatusMessage = $"{Warranties.Count} warranties loaded.";
        }

        private async Task RegisterWarrantyAsync()
        {
            if (WarrantyCustomerId <= 0 || WarrantyInvoiceId <= 0 || string.IsNullOrWhiteSpace(WarrantySerialNumber))
            {
                StatusMessage = "CustomerId, InvoiceId and Serial Number are required.";
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
            StatusMessage = $"Warranty #{id} registered.";
        }

        private async Task CancelWarrantyAsync()
        {
            if (SelectedWarranty is null) return;
            await _warrantyManagementService.CancelWarrantyAsync(SelectedWarranty.WarrantyId);
            await LoadWarrantiesAsync();
            StatusMessage = "Warranty canceled.";
        }

        private async Task LoadMaintenanceSchedulesAsync()
        {
            var result = await _maintenanceManagementService.GetSchedulesAsync(MaintenanceSearchTerm);
            MaintenanceSchedules.Clear();
            foreach (var item in result) MaintenanceSchedules.Add(item);
            StatusMessage = $"{MaintenanceSchedules.Count} maintenance schedules loaded.";
        }

        private async Task SetMaintenanceStatusAsync(MaintenanceStatus status)
        {
            if (SelectedMaintenanceSchedule is null) return;
            await _maintenanceManagementService.SetScheduleStatusAsync(SelectedMaintenanceSchedule.ScheduleId, status);
            await LoadMaintenanceSchedulesAsync();
            StatusMessage = $"Maintenance marked as {status}.";
        }

        private async Task AddMaintenanceVisitAsync()
        {
            if (SelectedMaintenanceSchedule is null || string.IsNullOrWhiteSpace(MaintenanceWorkType))
            {
                StatusMessage = "Select schedule and provide work type.";
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
            StatusMessage = "Maintenance visit added.";
        }

        private async Task LoadReportsAsync()
        {
            var from = ReportFromDate.Date;
            var to = ReportToDate.Date.AddDays(1).AddTicks(-1);
            if (to < from)
            {
                StatusMessage = "Report date range is invalid.";
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

            StatusMessage = "Reports generated successfully.";
        }

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
            StatusMessage = "Settings loaded.";
        }

        private async Task SaveSettingsAsync()
        {
            if (string.IsNullOrWhiteSpace(SettingsCompanyName) || string.IsNullOrWhiteSpace(SettingsInvoicePrefix))
            {
                StatusMessage = "Company Name and Invoice Prefix are required.";
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
            StatusMessage = "Settings saved successfully.";
        }

        private async Task CreateBackupAsync()
        {
            var path = await _backupRestoreService.CreateBackupAsync(string.IsNullOrWhiteSpace(BackupFilePath) ? null : BackupFilePath);
            LastBackupFilePath = path;
            BackupFilePath = path;
            StatusMessage = $"Backup created: {path}";
        }

        private async Task RestoreBackupAsync()
        {
            if (string.IsNullOrWhiteSpace(BackupFilePath))
            {
                StatusMessage = "Backup file path is required.";
                return;
            }

            await _backupRestoreService.RestoreBackupAsync(BackupFilePath);
            StatusMessage = "Database restored from backup successfully.";
        }

        private async Task VerifyBackupAsync()
        {
            if (string.IsNullOrWhiteSpace(BackupFilePath))
            {
                StatusMessage = "Backup file path is required.";
                return;
            }

            var valid = await _backupRestoreService.VerifyDatabaseAsync(BackupFilePath);
            StatusMessage = valid ? "Backup integrity is valid." : "Backup integrity check failed.";
        }

        private async Task PrintLastInvoiceAsync(PrintTemplateType templateType)
        {
            if (LastSavedInvoiceId is null || LastSavedInvoiceId <= 0)
            {
                StatusMessage = "No saved invoice available for printing.";
                return;
            }

            var printed = await _printingService.PrintInvoiceAsync(LastSavedInvoiceId.Value, templateType, SettingsPrinterName);
            StatusMessage = printed ? "Invoice sent to printer." : "Printing was canceled or failed.";
        }

        private void OnLanguageChanged()
        {
            ApplyLanguageState();
            ApplyCurrencyState(SettingsCurrency);
            StatusMessage = _localizationService.CurrentCultureCode == "ar-SA" ? "Switched to Arabic." : "Switched to English.";
        }

        private void ApplyLanguageState() => FlowDirection = _localizationService.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        private static string NormalizeCurrencyCode(string? currencyCode)
            => string.Equals(currencyCode, "SAR", StringComparison.OrdinalIgnoreCase) ? "SAR" : "USD";

        private void ApplyCurrencyState(string currencyCode)
        {
            var normalized = NormalizeCurrencyCode(currencyCode);
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.CurrencySymbol = normalized == "SAR" ? "ر.س" : "$";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            culture.NumberFormat.CurrencyNegativePattern = 1;

            CultureInfo.DefaultThreadCurrentCulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(ReportGrossSales));
            OnPropertyChanged(nameof(ReportNetSales));
            OnPropertyChanged(nameof(ReportProfit));
            OnPropertyChanged(nameof(ReportTotalPayments));
            OnPropertyChanged(nameof(DashboardSalesToday));
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

        partial void OnSelectedManagedUserChanged(UserManagementDto? value)
        {
            if (value is null) return;
            EditingUserId = value.Id;
            EditingUsername = value.Username;
            EditingUserPassword = string.Empty;
            EditingUserRole = value.Role;
            EditingUserIsActive = value.IsActive;
        }
    }
}
