using System.Windows.Input;

namespace AguaMinami.WPF.Helpers;

/// <summary>
/// Implementación reutilizable de ICommand para enlazar acciones
/// desde la vista sin acoplar código-behind.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Func<object?, Task> _executeAsync;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isExecuting;

    public RelayCommand(Func<object?, Task> executeAsync, Func<object?, bool>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    // Constructor sincrónico para conveniencia
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _executeAsync = p => { execute(p); return Task.CompletedTask; };
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) =>
        !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

    public async void Execute(object? parameter)
    {
        if (_isExecuting) return;
        _isExecuting = true;
        CommandManager.InvalidateRequerySuggested();
        try { await _executeAsync(parameter); }
        finally
        {
            _isExecuting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
