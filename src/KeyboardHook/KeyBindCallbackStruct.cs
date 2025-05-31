namespace Bau.Libraries.LibSystem.Windows.KeyboardHook;

/// <summary>
///     Clase con los datos de una acción vinculada a un Hotkey
/// </summary>
internal class KeyBindCallbackStruct
{
    internal KeyBindCallbackStruct(Action action, bool blocking)
    {
        Action = action;
        Blocking = blocking;
    }

    /// <summary>
    ///     Acción a ejecutar
    /// </summary>
    internal Action Action { get; }

    /// <summary>
    ///     Indica si es bloqueante
    /// </summary>
    internal bool Blocking { get; }
}