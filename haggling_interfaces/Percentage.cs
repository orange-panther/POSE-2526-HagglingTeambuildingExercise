/// <summary>
/// A small value type that represents a percentage constrained between 0 and 100.
/// The type provides implicit conversions to and from <see cref="int"/> for
/// convenience while guaranteeing the value remains clamped.
/// </summary>
public struct Percentage(int value)
{
    private int _value = Math.Clamp(value, 0, 100);

    /// <summary>
    /// The integer value of the percentage in the range [0, 100].
    /// </summary>
    public int Value
    {
        readonly get => _value;
        set => _value = Math.Clamp(value, 0, 100);
    }

    /// <summary>
    /// Create a <see cref="Percentage"/> from an integer. The value will be
    /// clamped into the valid [0, 100] range.
    /// </summary>
    /// <param name="value">The raw integer value to clamp.</param>
    public static implicit operator Percentage(int value) => new(value);

    /// <summary>
    /// Convert the <see cref="Percentage"/> back to an <see cref="int"/>.
    /// </summary>
    /// <param name="clamped">The percentage instance to convert.</param>
    public static implicit operator int(Percentage clamped) => clamped._value;
}
