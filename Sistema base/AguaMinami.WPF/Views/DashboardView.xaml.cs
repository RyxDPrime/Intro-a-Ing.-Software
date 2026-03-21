using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace AguaMinami.WPF.Views;

public partial class DashboardView : UserControl
{
    public event EventHandler? OfertasClicked;

    public DashboardView()
    {
        InitializeComponent();
        var culture = new CultureInfo("es-ES");
        TxtFecha.Text = DateTime.Now.ToString("dddd, d 'de' MMMM 'de' yyyy", culture);
    }

    private void BtnOfertas_Click(object sender, RoutedEventArgs e)
    {
        OfertasClicked?.Invoke(this, EventArgs.Empty);
    }
}
