namespace LowlandTech.Core.Application.ViewModels;

/// <summary>
/// Represents the application view model.
/// </summary>
public class AppVm : ObservableObject, IDisposable
{
    #region "Private fields"
    private const string LastVisitedUrlStateKey = "LastVisitedUrlState";
    private const string DrawerStartStateKey = "DrawerStartState";
    private const string DrawerEndStateKey = "DrawerEndState";
    private const string IsDarkModeStateKey = "IsDarkModeState";

    private readonly NavigationManager _navigationManager;
    private readonly ILocalStorageService _localStorageService;

    private readonly PaletteDark _darkPalette = new()
    {
        Primary = "#7e6fff",
        Surface = "#1e1e2d",
        Background = "#1a1a27",
        BackgroundGray = "#151521",
        AppbarText = "#92929f",
        AppbarBackground = "rgba(26,26,39,0.8)",
        DrawerBackground = "#1a1a27",
        ActionDefault = "#74718e",
        ActionDisabled = "#9999994d",
        ActionDisabledBackground = "#605f6d4d",
        TextPrimary = "#b2b0bf",
        TextSecondary = "#92929f",
        TextDisabled = "#ffffff33",
        DrawerIcon = "#92929f",
        DrawerText = "#92929f",
        GrayLight = "#2a2833",
        GrayLighter = "#1e1e2d",
        Info = "#4a86ff",
        Success = "#3dcb6c",
        Warning = "#ffb545",
        Error = "#ff3f5f",
        LinesDefault = "#33323e",
        TableLines = "#33323e",
        Divider = "#292838",
        OverlayLight = "#1e1e2d80",
    };
    private readonly PaletteLight _lightPalette = new()
    {
        Black = "#110e2d",
        AppbarText = "#424242",
        AppbarBackground = "rgba(255,255,255,0.8)",
        DrawerBackground = "#ffffff",
        GrayLight = "#e8e8e8",
        GrayLighter = "#f9f9f9",
    };
    #endregion

    #region "IsLoaded"
    private bool _isLoaded;
    /// <summary>
    /// Gets/sets whether the view model is loaded.
    /// </summary>
    public bool IsLoaded
    {
        get => _isLoaded;
        set => SetProperty(ref _isLoaded, value);
    }
    #endregion
    #region "Title"
    private string _title = "LowlandTech";
    /// <summary>
    /// Gets/sets the title of the application.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
    #endregion
    #region "DrawerStartOpen"
    private bool _drawerStartOpen = true;
    /// <summary>
    /// Gets/sets whether the start drawer should be open.
    /// </summary>
    public bool DrawerStartOpen
    {
        get => _drawerStartOpen;
        set
        {
            if (SetProperty(ref _drawerStartOpen, value))
            {
                SaveStateAsync().ConfigureAwait(false);
            }
        }
    }
    /// <summary>
    /// Gets toggle drawer command.
    /// </summary>
    public RelayCommand DrawerStartToggle { get; }
    private void OnDrawerStartToggle()
    {
        DrawerStartOpen = !DrawerStartOpen;
    }
    #endregion
    #region "DrawerEndOpen"
    private bool _drawerEndOpen;
    /// <summary>
    /// Gets/sets whether the start drawer should be open.
    /// </summary>
    public bool DrawerEndOpen
    {
        get => _drawerEndOpen;
        set
        {
            if (SetProperty(ref _drawerEndOpen, value))
            {
                SaveStateAsync().ConfigureAwait(false);
            }
        }
    }
    /// <summary>
    /// Gets toggle drawer command.
    /// </summary>
    public RelayCommand DrawerEndToggle { get; }
    private void OnDrawerEndToggle()
    {
        DrawerEndOpen = !DrawerEndOpen;
    }
    #endregion
    #region "IsDarkMode"
    private bool _isDarkMode = true;
    /// <summary>
    /// Gets/sets whether the dark mode is enabled.
    /// </summary>
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value))
            {
                OnPropertyChanged(nameof(DarkLightModeButtonIcon));
                SaveStateAsync().ConfigureAwait(false);
            }
        }
    }
    /// <summary>
    /// Toggles the dark mode.
    /// </summary>
    public RelayCommand DarkModeToggle { get; }
    private void OnDarkModeToggle()
    {
        IsDarkMode = !IsDarkMode;
    }

    /// <summary>
    /// Dark/Light mode button icon.
    /// </summary>
    public string DarkLightModeButtonIcon => IsDarkMode switch
    {
        true => Icons.Material.Rounded.AutoMode,
        false => Icons.Material.Outlined.DarkMode,
    };
    #endregion
    #region "Theme"
    private MudTheme _theme;
    /// <summary>
    /// Gets/sets the theme of the application.
    /// </summary>
    public MudTheme Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }
    #endregion
    #region "FooterComponentType"
    /// <summary>
    /// Footer component changed event.
    /// </summary>
    public event Action? FooterComponentChanged;
    private Type? _footerComponentType;
    /// <summary>
    /// Gets/sets the footer component type.
    /// </summary>
    public Type? FooterComponentType
    {
        get => _footerComponentType;
        set
        {
            if (SetProperty(ref _footerComponentType, value))
            {
                FooterComponentChanged?.Invoke(); // Notify the layout
            }
        }
    }
    #endregion
    #region "Constructor"
    /// <summary>
    /// Initializes a new instance of the <see cref="AppVm"/> class.
    /// </summary>
    public AppVm(ILocalStorageService localStorageService, NavigationManager navigationManager)
    {
        _localStorageService = localStorageService;
        _navigationManager = navigationManager;

        // Initialize the theme
        _theme = new MudTheme
        {
            PaletteLight = _lightPalette,
            PaletteDark = _darkPalette,
            LayoutProperties = new LayoutProperties()
        };

        // Initialize the RelayCommands
        DrawerStartToggle = new RelayCommand(OnDrawerStartToggle);
        DrawerEndToggle = new RelayCommand(OnDrawerEndToggle);
        DarkModeToggle = new RelayCommand(OnDarkModeToggle);

        // Subscribe to location changes
        _navigationManager.LocationChanged += OnLocationChanged;
    }
    #endregion
    #region "OnLocationChanged"
    /// <summary>
    /// Handle location change events and store the current URL
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        // Save the last location to local storage
        _localStorageService.SetItemAsync(LastVisitedUrlStateKey, e.Location);
    }
    #endregion
    #region "LoadStateAsync"
    /// <summary>
    /// Load state from local storage
    /// </summary>
    /// <returns></returns>
    public async Task LoadStateAsync()
    {
        if (IsLoaded) return;

        var drawerStartState = await _localStorageService.GetItemAsync<bool?>(DrawerStartStateKey);
        if (drawerStartState.HasValue)
        {
            DrawerStartOpen = drawerStartState.Value;
        }

        var drawerEndState = await _localStorageService.GetItemAsync<bool?>(DrawerEndStateKey);
        if (drawerEndState.HasValue)
        {
            DrawerEndOpen = drawerEndState.Value;
        }

        var isDarkModeState = await _localStorageService.GetItemAsync<bool?>(IsDarkModeStateKey);
        if (isDarkModeState.HasValue)
        {
            IsDarkMode = isDarkModeState.Value;
        }

        var lastVisitedUrl = await _localStorageService.GetItemAsync<string>(LastVisitedUrlStateKey);
        if (!string.IsNullOrEmpty(lastVisitedUrl) && _navigationManager.IsBaseUri())
        {
            if (_navigationManager.Uri != lastVisitedUrl)
            {
                // Subscribe to the LocationChanged event to wait until the navigation is complete
                _navigationManager.LocationChanged += OnLocationLastVisited;

                _navigationManager.NavigateTo(lastVisitedUrl);

                // Wait for the location change to complete
                return;
            }
        }

        // If there's no navigation, set IsLoaded to true immediately
        IsLoaded = true;
    }

    private void OnLocationLastVisited(object? sender, LocationChangedEventArgs e)
    {
        // Unsubscribe from the event after navigation is complete
        _navigationManager.LocationChanged -= OnLocationLastVisited;

        // Set IsLoaded to true after navigation is completed
        IsLoaded = true;

        // Force UI update if necessary
        OnPropertyChanged(nameof(IsLoaded));
    }
    #endregion
    #region "SaveStateAsync"
    /// <summary>
    /// Save state to local storage.
    /// </summary>
    /// <returns></returns>
    public async Task SaveStateAsync()
    {
        await _localStorageService.SetItemAsync(DrawerStartStateKey, DrawerStartOpen);
        await _localStorageService.SetItemAsync(DrawerEndStateKey, DrawerEndOpen);
        await _localStorageService.SetItemAsync(IsDarkModeStateKey, IsDarkMode);
    }
    #endregion
    #region "Dispose"
    /// <summary>
    /// Dispose of the view model.
    /// </summary>
    public void Dispose()
    {
        _navigationManager.LocationChanged -= OnLocationChanged;
    }
    #endregion
}