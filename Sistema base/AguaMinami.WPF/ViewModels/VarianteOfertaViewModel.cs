using System.Collections.ObjectModel;
using System.Windows;
using AguaMinami.Shared.DTOs;
using AguaMinami.WPF.Helpers;
using AguaMinami.WPF.Services;

namespace AguaMinami.WPF.ViewModels;

public class VarianteOfertaViewModel : ViewModelBase
{
    private readonly ApiService _api;
    private const string Endpoint = "VarianteOferta";

    public ObservableCollection<VarianteOfertaDTO> Items { get; } = new();

    private short _idVariante;
    public short IdVariante { get => _idVariante; set => SetProperty(ref _idVariante, value); }

    private string? _nombre;
    public string? Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }

    private string? _descripcion;
    public string? Descripcion { get => _descripcion; set => SetProperty(ref _descripcion, value); }

    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

    public RelayCommand LoadCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand SelectCommand { get; }

    public VarianteOfertaViewModel(ApiService api)
    {
        _api = api;
        LoadCommand = new RelayCommand(async _ => await LoadAsync());
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => IsEditing);
        NewCommand = new RelayCommand(_ => ClearForm());
        SelectCommand = new RelayCommand(p => SelectItem(p as VarianteOfertaDTO));
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        foreach (var i in await _api.GetAllAsync<VarianteOfertaDTO>(Endpoint)) Items.Add(i);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        { MessageBox.Show("El nombre es obligatorio.", "Validación"); return; }

        var dto = new VarianteOfertaCreateDTO { Nombre = Nombre, Descripcion = Descripcion };

        bool ok = IsEditing
            ? await _api.UpdateAsync(Endpoint, IdVariante, dto)
            : await _api.CreateAsync(Endpoint, dto);

        if (ok) { ClearForm(); await LoadAsync(); }
    }

    private async Task DeleteAsync()
    {
        if (MessageBox.Show($"¿Eliminar '{Nombre}'?", "Confirmar",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        if (await _api.DeleteAsync(Endpoint, IdVariante)) { ClearForm(); await LoadAsync(); }
    }

    private void SelectItem(VarianteOfertaDTO? item)
    {
        if (item is null) return;
        IdVariante = item.IdVariante; Nombre = item.Nombre;
        Descripcion = item.Descripcion; IsEditing = true;
    }

    private void ClearForm()
    {
        IdVariante = 0; Nombre = null; Descripcion = null; IsEditing = false;
    }
}
