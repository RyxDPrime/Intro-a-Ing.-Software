using System.Collections.ObjectModel;
using System.Windows;
using AguaMinami.Shared.DTOs;
using AguaMinami.WPF.Helpers;
using AguaMinami.WPF.Services;

namespace AguaMinami.WPF.ViewModels;

public class OfertaDescuentoViewModel : ViewModelBase
{
    private readonly ApiService _api;
    private const string Endpoint = "OfertaDescuento";

    public ObservableCollection<OfertaDescuentoDTO> Items { get; } = new();
    public ObservableCollection<OfertaDTO> Ofertas { get; } = new();

    private int _idOferta;
    public int IdOferta { get => _idOferta; set => SetProperty(ref _idOferta, value); }

    private double? _porcentajeDesc;
    public double? PorcentajeDesc { get => _porcentajeDesc; set => SetProperty(ref _porcentajeDesc, value); }

    private double? _montoFijo;
    public double? MontoFijo { get => _montoFijo; set => SetProperty(ref _montoFijo, value); }

    private double? _topeDescuento;
    public double? TopeDescuento { get => _topeDescuento; set => SetProperty(ref _topeDescuento, value); }

    private bool _esAcumulable;
    public bool EsAcumulable { get => _esAcumulable; set => SetProperty(ref _esAcumulable, value); }

    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

    public RelayCommand LoadCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand SelectCommand { get; }

    public OfertaDescuentoViewModel(ApiService api)
    {
        _api = api;
        LoadCommand = new RelayCommand(async _ => await LoadAsync());
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => IsEditing);
        NewCommand = new RelayCommand(_ => ClearForm());
        SelectCommand = new RelayCommand(p => SelectItem(p as OfertaDescuentoDTO));
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        foreach (var i in await _api.GetAllAsync<OfertaDescuentoDTO>(Endpoint)) Items.Add(i);
        Ofertas.Clear();
        foreach (var o in await _api.GetAllAsync<OfertaDTO>("Oferta")) Ofertas.Add(o);
    }

    private async Task SaveAsync()
    {
        if (IdOferta <= 0) { MessageBox.Show("Seleccione una oferta.", "Validación"); return; }

        var dto = new OfertaDescuentoCreateDTO
        {
            IdOferta = IdOferta, PorcentajeDesc = PorcentajeDesc,
            MontoFijo = MontoFijo, TopeDescuento = TopeDescuento, EsAcumulable = EsAcumulable
        };

        bool ok = IsEditing
            ? await _api.UpdateAsync(Endpoint, IdOferta, dto)
            : await _api.CreateAsync(Endpoint, dto);

        if (ok) { ClearForm(); await LoadAsync(); }
    }

    private async Task DeleteAsync()
    {
        if (MessageBox.Show("¿Eliminar esta oferta de descuento?", "Confirmar",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        if (await _api.DeleteAsync(Endpoint, IdOferta)) { ClearForm(); await LoadAsync(); }
    }

    private void SelectItem(OfertaDescuentoDTO? item)
    {
        if (item is null) return;
        IdOferta = item.IdOferta; PorcentajeDesc = item.PorcentajeDesc;
        MontoFijo = item.MontoFijo; TopeDescuento = item.TopeDescuento;
        EsAcumulable = item.EsAcumulable ?? false; IsEditing = true;
    }

    private void ClearForm()
    {
        IdOferta = 0; PorcentajeDesc = null; MontoFijo = null;
        TopeDescuento = null; EsAcumulable = false; IsEditing = false;
    }
}
