using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AguaMinami.WPF.Helpers;

/// <summary>
/// Clase base para todos los ViewModels. Implementa la notificación
/// de cambios de propiedad necesaria para el binding MVVM.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
