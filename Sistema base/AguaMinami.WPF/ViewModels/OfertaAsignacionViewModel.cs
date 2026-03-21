using System.Collections.ObjectModel;
using System.Windows;
using AguaMinami.Shared.DTOs;
using AguaMinami.WPF.Helpers;
using AguaMinami.WPF.Services;

namespace AguaMinami.WPF.ViewModels;

public class OfertaAsignacionViewModel : ViewModelBase
{
    private readonly ApiService _api;
    private const string Endpoint = "OfertaAsignacion";

    public ObservableCollection<OfertaAsignacionDTO> Items { get; } = new();
    public ObservableCollection<OfertaDTO> Ofertas { get; } = new();

    private int _idAsignacion;
    public int IdAsignacion { get => _idAsignacion; set => SetProperty(ref _idAsignacion, value); }

    private int? _idOferta;
    public int? IdOferta { get => _idOferta; set => SetProperty(ref _idOferta, value); }

    private string? _idEntidad;
    public string? IdEntidad { get => _idEntidad; set => SetProperty(ref _idEntidad, value); }

    private int? _idVariante;
    public int? IdVariante { get => _idVariante; set => SetProperty(ref _idVariante, value); }

    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

    public RelayCommand LoadCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand SelectCommand { get; }

    public OfertaAsignacionViewModel(ApiService api)
    {
        _api = api;
        LoadCommand = new RelayCommand(async _ => await LoadAsync());
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => IsEditing);
        NewCommand = new RelayCommand(_ => ClearForm());
        SelectCommand = new RelayCommand(p => SelectItem(p as OfertaAsignacionDTO));
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        foreach (var i in await _api.GetAllAsync<OfertaAsignacionDTO>(Endpoint)) Items.Add(i);
        Ofertas.Clear();
        foreach (var o in await _api.GetAllAsync<OfertaDTO>("Oferta")) Ofertas.Add(o);
    }

    private async Task SaveAsync()
    {
        var dto = new OfertaAsignacionCreateDTO
        {
            IdOferta = IdOferta, IdEntidad = IdEntidad, IdVariante = IdVariante
        };

        bool ok = IsEditing
            ? await _api.UpdateAsync(Endpoint, IdAsignacion, dto)
            : await _api.CreateAsync(Endpoint, dto);

        if (ok) { ClearForm(); await LoadAsync(); }
    }

    private async Task DeleteAsync()
    {
        if (MessageBox.Show("¿Eliminar esta asignación?", "Confirmar",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        if (await _api.DeleteAsync(Endpoint, IdAsignacion)) { ClearForm(); await LoadAsync(); }
    }

    private void SelectItem(OfertaAsignacionDTO? item)
    {
        if (item is null) return;
        IdAsignacion = item.IdAsignacion; IdOferta = item.IdOferta;
        IdEntidad = item.IdEntidad; IdVariante = item.IdVariante; IsEditing = true;
    }

    private void ClearForm()
    {
        IdAsignacion = 0; IdOferta = null; IdEntidad = null;
        IdVariante = null; IsEditing = false;
    }
}
