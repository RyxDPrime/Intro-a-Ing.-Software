using AguaMinami.WPF.Helpers;
using AguaMinami.WPF.Services;

namespace AguaMinami.WPF.ViewModels;

/// <summary>
/// ViewModel principal que contiene los sub-ViewModels de cada sección.
/// La MainWindow enlaza a este y navega entre vistas con TabControl.
/// </summary>
public class MainViewModel : ViewModelBase
{
    public OfertaViewModel Ofertas { get; }
    public OfertaCantidadViewModel OfertasCantidad { get; }
    public OfertaDescuentoViewModel OfertasDescuento { get; }
    public OfertaAsignacionViewModel OfertasAsignacion { get; }
    public VarianteOfertaViewModel VariantesOferta { get; }

    public RelayCommand LoadAllCommand { get; }

    private bool _isLoading;
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    public MainViewModel()
    {
        var api = new ApiService("http://127.0.0.1:5000");

        Ofertas = new OfertaViewModel(api);
        OfertasCantidad = new OfertaCantidadViewModel(api);
        OfertasDescuento = new OfertaDescuentoViewModel(api);
        OfertasAsignacion = new OfertaAsignacionViewModel(api);
        VariantesOferta = new VarianteOfertaViewModel(api);

        LoadAllCommand = new RelayCommand(async _ => await LoadAllAsync());
    }

    /// <summary>
    /// Espera a que la API esté lista y carga la primera pestaña.
    /// Las demás se cargan bajo demanda al cambiar de tab.
    /// </summary>
    public async Task LoadInitialAsync()
    {
        IsLoading = true;
        const int maxRetries = 5;
        const int delayMs = 1500;

        using var ping = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(2) };

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await ping.GetAsync("http://127.0.0.1:5000/health");
                break;
            }
            catch
            {
                if (attempt < maxRetries)
                    await Task.Delay(delayMs);
            }
        }

        // Solo carga la primera pestaña; las demás se cargan al seleccionar el tab
        await Ofertas.LoadAsync();
        IsLoading = false;
    }

    public async Task LoadAllAsync()
    {
        await Ofertas.LoadAsync();
        await OfertasCantidad.LoadAsync();
        await OfertasDescuento.LoadAsync();
        await OfertasAsignacion.LoadAsync();
        await VariantesOferta.LoadAsync();
    }
}
