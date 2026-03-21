using System.Windows;
using System.Windows.Controls;
using AguaMinami.WPF.ViewModels;

namespace AguaMinami.WPF;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private bool _initialLoadDone;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _vm.LoadInitialAsync();
        _initialLoadDone = true;
    }

    private async void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Solo reaccionar al TabControl principal, no a controles hijos
        if (e.OriginalSource != MainTabs) return;
        // No recargar si aun estamos en la carga inicial
        if (!_initialLoadDone) return;

        switch (MainTabs.SelectedIndex)
        {
            case 0: await _vm.Ofertas.LoadAsync(); break;
            case 1: await _vm.OfertasCantidad.LoadAsync(); break;
            case 2: await _vm.OfertasDescuento.LoadAsync(); break;
            case 3: await _vm.OfertasAsignacion.LoadAsync(); break;
            case 4: await _vm.VariantesOferta.LoadAsync(); break;
        }
    }
}
