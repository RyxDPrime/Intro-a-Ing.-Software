using System.Windows;
using System.Windows.Controls;
using AguaMinami.WPF.ViewModels;

namespace AguaMinami.WPF;

public partial class ShellWindow : Window
{
    private readonly MainViewModel _vm;
    private bool _ofertasLoaded;

    public ShellWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;

        DashboardPanel.OfertasClicked += (_, _) => NavigateToOfertas();
    }

    // ── Navegacion ──

    private async void NavigateToOfertas()
    {
        DashboardPanel.Visibility = Visibility.Collapsed;
        OfertasPanel.Visibility = Visibility.Visible;

        if (!_ofertasLoaded)
        {
            await _vm.LoadInitialAsync();
            _ofertasLoaded = true;
        }
    }

    private void BtnVolver_Click(object sender, RoutedEventArgs e)
    {
        OfertasPanel.Visibility = Visibility.Collapsed;
        DashboardPanel.Visibility = Visibility.Visible;
    }

    private async void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.OriginalSource != MainTabs) return;
        if (!_ofertasLoaded) return;

        switch (MainTabs.SelectedIndex)
        {
            case 0: await _vm.Ofertas.LoadAsync(); break;
            case 1: await _vm.OfertasCantidad.LoadAsync(); break;
            case 2: await _vm.OfertasDescuento.LoadAsync(); break;
            case 3: await _vm.OfertasAsignacion.LoadAsync(); break;
            case 4: await _vm.VariantesOferta.LoadAsync(); break;
        }
    }

    // ── Botones de ventana ──

    private void Minimize_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void MaxRestore_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void Close_Click(object sender, RoutedEventArgs e)
        => Close();

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);

        if (WindowState == WindowState.Maximized)
        {
            RootBorder.Padding = new Thickness(7);
            BtnMaxRestore.Content = "\uE923"; // Restore icon
        }
        else
        {
            RootBorder.Padding = new Thickness(0);
            BtnMaxRestore.Content = "\uE922"; // Maximize icon
        }
    }

  
}
