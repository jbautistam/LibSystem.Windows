namespace Bau.Libraries.LibSystem.Windows.KeyboardHook;

/// <summary>
///     Manager de tratamiento de Hotkeys
/// </summary>
public class KeyboardHookManager : IDisposable
{
    /// <summary>
    ///     Flags con los modificadores de teclas
    /// </summary>
    [Flags]
    public enum ModifierKeys
    {
        /// <summary>Tecla Alt</summary>
        Alt = 1,
        /// <summary>Tecla Control</summary>
        Control = 2,
        /// <summary>Tecla Shift</summary>
        Shift = 4,
        /// <summary>Tecla Windows</summary>
        WindowsKey = 8
    }

    public KeyboardHookManager()
    {
        HookInterop = new KeyboardHookInterop(this);
    }

    /// <summary>
    ///     Arranca el hook del teclado
    /// </summary>
    public void Start()
    {
        if (!IsStarted)
        {
            HookInterop.Start();
            IsStarted = true;
        }
    }

    /// <summary>
    ///     Detiene el hook del teclado (no deregistra las hotkeys existentes)
    /// </summary>
    public void Stop()
    {
        if (IsStarted)
        {
            HookInterop.Stop();
            IsStarted = false;
        }
    }

    /// <summary>
    ///     Registra un hotkey
    /// </summary>
    public Guid? RegisterHotkey(int virtualKeyCode, Action action, bool blocking = false) => RegisterHotkey([], virtualKeyCode, action, blocking);

    /// <summary>
    ///     Registra un hotkey con uno o más modificadores
    /// </summary>
    public Guid? RegisterHotkey(ModifierKeys modifiers, int virtualKeyCode, Action action, bool blocking = false)
    {
        ModifierKeys[] selectedModifiers = Enum.GetValues(typeof(ModifierKeys))
                                                    .Cast<ModifierKeys>()
                                                    .ToArray()
                                                    .Where(modifier => modifiers.HasFlag(modifier))
                                                    .ToArray();

            // Registra el Hotkey con sus modificadores
            return RegisterHotkey(selectedModifiers, virtualKeyCode, action, blocking);
    }

    /// <summary>
    ///     Registra una combinación de claves
    /// </summary>
    public Guid? RegisterHotkey(ModifierKeys[] modifiers, int virtualKeyCode, Action action, bool blocking = false)
    {
        KeybindStruct keybind = new(modifiers, virtualKeyCode);

            // Si ya existe esa combinación de teclas, lanza una excepción
            if (RegisteredCallbacks.ContainsKey(keybind))
                return null;
            else
            {
                // Añade la estructura al diccionario
                RegisteredCallbacks[keybind] = new KeyBindCallbackStruct(action, blocking);
                // Devuelve el Guid
                return keybind.Identifier;
            }
    }

    /// <summary>
    ///     Elimina todos los hotkeys
    /// </summary>
    public void UnregisterAll()
    {
        RegisteredCallbacks.Clear();
    }

    /// <summary>
    ///     Deregistra un hotkey
    /// </summary>
    public void UnregisterHotkey(int virtualKeyCode)
    {
        UnregisterHotkey([], virtualKeyCode);
    }

    /// <summary>
    ///     Deregistra un hotkey
    /// </summary>
    public bool UnregisterHotkey(ModifierKeys[] modifiers, int virtualKeyCode)
    {
        return RegisteredCallbacks.Remove(new KeybindStruct(modifiers, virtualKeyCode));
    }

    /// <summary>
    ///     Deregistra un Hotkey específico por su identificador
    /// </summary>
    public bool UnregisterHotkey(Guid keybindIdentity)
    {
        KeybindStruct? keybindToRemove = RegisteredCallbacks.Keys.FirstOrDefault(keybind => keybind.Identifier.Equals(keybindIdentity));

            // Borra el hotkey
            return keybindToRemove is not null && RegisteredCallbacks.Remove(keybindToRemove);
    }

    /// <summary>
    ///     Invoca una acción almacenada como objeto
    /// </summary>
    private void InvokeAction(object? actionObj)
    {
        if (actionObj is Action action)
            action.Invoke();
    }

    /// <summary>
    ///     Trata la pulsacion de una tecla
    /// </summary>
    internal bool HandleKeyPress(HashSet<ModifierKeys> downModifierKeys, int virtualKeyCode)
    {
        KeybindStruct currentKey = new(downModifierKeys, virtualKeyCode);

            // Busca si hay alguna acción vinculada a la tecla
            if (RegisteredCallbacks.TryGetValue(currentKey, out var callbackStruct))
            {
                // Lanza la acción
                ThreadPool.QueueUserWorkItem(InvokeAction, callbackStruct.Action);
                // Devuelve el valor que indica si se está bloqueando el resto de los hooks
                return callbackStruct.Blocking;
            }
            // Indica que no se ha encontrado nada que responda a esa tecla y por tanto no se bloquea
            return false;
    }

    /// <summary>
    ///     Libera la memoria
    /// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if (!Disposed)
		{
            // Detiene el hook
			if (disposing)
				Stop();
            // Indica que se ha liberado la memoria
			Disposed = true;
		}
	}

    /// <summary>
    ///     Libera la memoria
    /// </summary>
	public void Dispose()
	{
		// No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

    /// <summary>
    ///     Indica si se ha iniciado el manager
    /// </summary>
    public bool IsStarted { get; private set; }

    /// <summary>
    ///     Diccionario con las HotKeys registradas
    /// </summary>
    private Dictionary<KeybindStruct, KeyBindCallbackStruct> RegisteredCallbacks { get; } = [];

    /// <summary>
    ///     Tratamiento del hook de teclado utilizando funciones de windows
    /// </summary>
    private KeyboardHookInterop HookInterop { get; }

    /// <summary>
    ///     Indica si se ha eliminado
    /// </summary>
	public bool Disposed;
}