using System.Runtime.InteropServices;

namespace Bau.Libraries.LibSystem.Windows.KeyboardHook;

/// <summary>
///     Estructuras de interop para los hooks de teclado en windows
/// </summary>
internal class KeyboardHookInterop
{
    // Constantes de teclas (https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/)
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    /// <summary>
    ///     Delegado asociado al hook
    /// </summary>
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    /// <summary>
    ///     Asigna un hook
    /// </summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    /// <summary>
    ///     Desasigna un hook
    /// </summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    /// <summary>
    ///     Llama al siguiente hook
    /// </summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    /// <summary>
    ///     Carga una librería
    /// </summary>
    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string lpFileName);

    // Variables privadas
    private LowLevelKeyboardProc? _hook;
    private static IntPtr _hookId = IntPtr.Zero;
    private readonly HashSet<int> _downKeys;
    private readonly HashSet<KeyboardHookManager.ModifierKeys> _downModifierKeys;
    private readonly object _modifiersLock = new object();

    internal KeyboardHookInterop(KeyboardHookManager manager)
    {
        Manager = manager;
        _downKeys = new HashSet<int>();
        _downModifierKeys = new HashSet<KeyboardHookManager.ModifierKeys>();
    }

    /// <summary>
    ///     Inicializa el hook
    /// </summary>
    internal void Start()
    {
        _hook = HookCallback;
        _hookId = SetHook(_hook);
    }

    /// <summary>
    ///     Detiene el hook
    /// </summary>
    internal void Stop()
    {
        UnhookWindowsHookEx(_hookId);
    }

    /// <summary>
    ///     Asigna el hook
    /// </summary>
    private IntPtr SetHook(LowLevelKeyboardProc proc) => SetWindowsHookEx(WH_KEYBOARD_LL, proc, LoadLibrary("User32"), 0);

    /// <summary>
    ///     Rutina a la que llama el hook de windows
    /// </summary>
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        // Trata la tecla
        if (nCode >= 0)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            bool blocking = HandleSingleKeyboardInput(wParam, vkCode);

                // Si se está bloqueando, no llama al siguiente hook
                if (blocking)
                    return 1;
        }
        // Llama al siguiente hook
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    /// <summary>
    ///     Trata una tecla recibida del hook
    /// </summary>
    private bool HandleSingleKeyboardInput(IntPtr wParam, int vkCode)
    {
        KeyboardHookManager.ModifierKeys? modifierKey = GetModifierKeyFromCode(vkCode);
        bool blocking = false;

            // Si el evento es la pulsación de una tecla
            if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
            {
                // Si es una tecla de modificación
                if (modifierKey != null)
                    lock (_modifiersLock)
                    {
                        _downModifierKeys.Add(modifierKey.Value);
                    }
                // Lanza las acciones registradas para esta tecla (sólo una vez por pulsación)
                if (!_downKeys.Contains(vkCode))
                {
                    blocking = Manager.HandleKeyPress(_downModifierKeys, vkCode);
                    _downKeys.Add(vkCode);
                }
            }
            // Si el evento es dejar de pulsar una tecla
            if (wParam == (IntPtr) WM_KEYUP || wParam == (IntPtr) WM_SYSKEYUP)
            {
                // Si la tecla liberada es una tecla de modificación, la elimina del HashSet
                if (modifierKey != null)
                    lock (_modifiersLock)
                    {
                        _downModifierKeys.Remove(modifierKey.Value);
                    }
                // Elimina la tecla
                _downKeys.Remove(vkCode);
            }
            // Devuelve el valor que indica si se bloquea el resto de hooks
            return blocking;
    }

    /// <summary>
    ///     Obtiene la tecla adicional a partir del código
    /// </summary>
    private KeyboardHookManager.ModifierKeys? GetModifierKeyFromCode(int keyCode)
    {
		return keyCode switch
		            {
			            0xA0 or 0xA1 or 0x10 => (KeyboardHookManager.ModifierKeys?)KeyboardHookManager.ModifierKeys.Shift,
			            0xA2 or 0xA3 or 0x11 => (KeyboardHookManager.ModifierKeys?)KeyboardHookManager.ModifierKeys.Control,
			            0x12 or 0xA4 or 0xA5 => (KeyboardHookManager.ModifierKeys?)KeyboardHookManager.ModifierKeys.Alt,
			            0x5B or 0x5C => (KeyboardHookManager.ModifierKeys?)KeyboardHookManager.ModifierKeys.WindowsKey,
			            _ => null,
		            };
	}

    /// <summary>
    ///     Manager principal
    /// </summary>
    private KeyboardHookManager Manager { get; }
}
