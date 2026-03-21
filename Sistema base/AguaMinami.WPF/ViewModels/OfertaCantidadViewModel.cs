using System.Collections.ObjectModel;
using System.Windows;
using AguaMinami.Shared.DTOs;
using AguaMinami.WPF.Helpers;
using AguaMinami.WPF.Services;

namespace AguaMinami.WPF.ViewModels;

public class OfertaCantidadViewModel : ViewModelBase
{
    private readonly ApiService _api;
    private const string Endpoint = "OfertaCantidad";

    public ObservableCollection<OfertaCantidadDTO> Items { get; } = new();
    public ObservableCollection<OfertaDTO> Ofertas { get; } = new();

    private int _idOferta;
    public int IdOferta { get => _idOferta; set => SetProperty(ref _idOferta, value); }

    private short? _idVariante;
    public short? IdVariante { get => _idVariante; set => SetProperty(ref _idVariante, value); }

    private int? _cantRequerida;
    public int? CantRequerida { get => _cantRequerida; set => SetProperty(ref _cantRequerida, value); }

    private int? _cantGratis;
    public int? CantGratis { get => _cantGratis; set => SetProperty(ref _cantGratis, value); }

    private bool _esAcumulable;
    public bool EsAcumulable { get => _esAcumulable; set => SetProperty(ref _esAcumulable, value); }

    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

    public RelayCommand LoadCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand SelectCommand { get; }

    public OfertaCantidadViewModel(ApiService api)
    {
        _api = api;
        LoadCommand = new RelayCommand(async _ => await LoadAsync());
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => IsEditing);
        NewCommand = new RelayCommand(_ => ClearForm());
        SelectCommand = new RelayCommand(p => SelectItem(p as OfertaCantidadDTO));
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        foreach (var i in await _api.GetAllAsync<OfertaCantidadDTO>(Endpoint)) Items.Add(i);

        Ofertas.Clear();
        foreach (var o in await _api.GetAllAsync<OfertaDTO>("Oferta")) Ofertas.Add(o);
    }

    private async Task SaveAsync()
    {
        if (IdOferta <= 0) { MessageBox.Show("Seleccione una oferta.", "Validación"); return; }

        var dto = new OfertaCantidadCreateDTO
        {
            IdOferta = IdOferta, IdVariante = IdVariante,
            CantRequerida = CantRequerida, CantGratis = CantGratis, EsAcumulable = EsAcumulable
        };

        bool ok = IsEditing
            ? await _api.UpdateAsync(Endpoint, IdOferta, dto)
            : await _api.CreateAsync(Endpoint, dto);

        if (ok) { ClearForm(); await LoadAsync(); }
    }

    private async Task DeleteAsync()
    {
        if (MessageBox.Show("¿Eliminar esta oferta por cantidad?", "Confirmar",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        if (await _api.DeleteAsync(Endpoint, IdOferta)) { ClearForm(); await LoadAsync(); }
    }

    private void SelectItem(OfertaCantidadDTO? item)
    {
        if (item is null) return;
        IdOferta = item.IdOferta; IdVariante = item.IdVariante;
        CantRequerida = item.CantRequerida; CantGratis = item.CantGratis;
        EsAcumulable = item.EsAcumulable ?? false; IsEditing = true;
    }

    private void ClearForm()
    {
        IdOferta = 0; IdVariante = null; CantRequerida = null;
        CantGratis = null; EsAcumulable = false; IsEditing = false;
    }
}
