using System.Collections.ObjectModel;
using System.Windows;
using AguaMinami.Shared.DTOs;
using AguaMinami.WPF.Helpers;
using AguaMinami.WPF.Services;

namespace AguaMinami.WPF.ViewModels;

public class OfertaViewModel : ViewModelBase
{
    private readonly ApiService _api;
    private const string Endpoint = "Oferta";

    public ObservableCollection<OfertaDTO> Items { get; } = new();

    // ── Campos del formulario ─────────────────────────────
    private int _idOferta;
    public int IdOferta { get => _idOferta; set => SetProperty(ref _idOferta, value); }

    private string _nombre = string.Empty;
    public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }

    private bool _estado = true;
    public bool Estado { get => _estado; set => SetProperty(ref _estado, value); }

    private DateTime? _fechaInicio;
    public DateTime? FechaInicio { get => _fechaInicio; set => SetProperty(ref _fechaInicio, value); }

    private DateTime? _fechaFin;
    public DateTime? FechaFin { get => _fechaFin; set => SetProperty(ref _fechaFin, value); }

    private string? _descripcion;
    public string? Descripcion { get => _descripcion; set => SetProperty(ref _descripcion, value); }

    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

    // ── Comandos ──────────────────────────────────────────
    public RelayCommand LoadCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand SelectCommand { get; }

    public OfertaViewModel(ApiService api)
    {
        _api = api;
        LoadCommand = new RelayCommand(async _ => await LoadAsync());
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => IsEditing);
        NewCommand = new RelayCommand(_ => ClearForm());
        SelectCommand = new RelayCommand(p => SelectItem(p as OfertaDTO));
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        foreach (var item in await _api.GetAllAsync<OfertaDTO>(Endpoint))
            Items.Add(item);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            MessageBox.Show("El nombre es obligatorio.", "Validación");
            return;
        }

        var dto = new OfertaCreateDTO
        {
            Nombre = Nombre,
            Estado = Estado,
            FechaInicio = FechaInicio,
            FechaFin = FechaFin,
            Descripcion = Descripcion
        };

        bool ok = IsEditing
            ? await _api.UpdateAsync(Endpoint, IdOferta, dto)
            : await _api.CreateAsync(Endpoint, dto);

        if (ok)
        {
            ClearForm();
            await LoadAsync();
        }
    }

    private async Task DeleteAsync()
    {
        if (MessageBox.Show($"¿Eliminar la oferta '{Nombre}'?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        if (await _api.DeleteAsync(Endpoint, IdOferta))
        {
            ClearForm();
            await LoadAsync();
        }
    }

    private void SelectItem(OfertaDTO? item)
    {
        if (item is null) return;
        IdOferta = item.IdOferta;
        Nombre = item.Nombre;
        Estado = item.Estado;
        FechaInicio = item.FechaInicio;
        FechaFin = item.FechaFin;
        Descripcion = item.Descripcion;
        IsEditing = true;
    }

    private void ClearForm()
    {
        IdOferta = 0; Nombre = string.Empty; Estado = true;
        FechaInicio = null; FechaFin = null; Descripcion = null;
        IsEditing = false;
    }
}
