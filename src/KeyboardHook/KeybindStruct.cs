namespace Bau.Libraries.LibSystem.Windows.KeyboardHook;

/// <summary>
///     Estructura para representar un vínculo a un HotKey (código de tecla + modificadores)
/// </summary>
internal class KeybindStruct : IEquatable<KeybindStruct>
{
    internal KeybindStruct(IEnumerable<KeyboardHookManager.ModifierKeys> modifiers, int virtualKeyCode)
    {
        VirtualKeyCode = virtualKeyCode;
        Modifiers = new List<KeyboardHookManager.ModifierKeys>(modifiers);
    }

    /// <summary>
    ///     Comprueba si es igual a otra estructura
    /// </summary>
    public bool Equals(KeybindStruct other)
    {
        // Comprueba si los objetos son iguales
        if (other is null || VirtualKeyCode != other.VirtualKeyCode || Modifiers.Count != other.Modifiers.Count)
            return false;
        else // ... comprueba si los modificadores son iguales
            foreach (KeyboardHookManager.ModifierKeys modifier in Modifiers)
                if (!other.Modifiers.Contains(modifier))
                    return false;
        // Si ha llegado hasta aquí es porque son iguales            
        return true;
    }

    /// <summary>
    ///     Comprueba si es igual a otro objeto
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj) || obj.GetType() != GetType()) 
            return false;
        else if (ReferenceEquals(this, obj)) 
            return true;
        else
            return Equals((KeybindStruct) obj);
    }

    /// <summary>
    ///     Obtiene el código Hash
    /// </summary>
    public override int GetHashCode()
    {
        int hash = 7 * (13 * 7 + VirtualKeyCode.GetHashCode());

            // Suma los Hash de los modificadores
            foreach (KeyboardHookManager.ModifierKeys modifier in Modifiers)
                hash += modifier.GetHashCode();
            // Devuelve el Hash calculado
            return hash;
    }

    /// <summary>
    ///     Id
    /// </summary>
    internal Guid Identifier { get; } = Guid.NewGuid();

    /// <summary>
    ///     Código de la tecla
    /// </summary>
    internal int VirtualKeyCode { get; }

    /// <summary>
    ///     Modificadores de la tecla
    /// </summary>
    internal List<KeyboardHookManager.ModifierKeys> Modifiers { get; }
}