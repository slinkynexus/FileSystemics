namespace FileSystemics.IO;

/// <summary>Appends characters into a fixed <see cref="Span{Char}"/> buffer without allocating.</summary>
public ref struct SpanBuilder(Span<char> span) {
    private readonly Span<char> _span = span;
    private int _position;

    /// <summary>Gets or sets the number of characters written to the buffer.</summary>
    public int Length {
        get => _position;
        set {
            if (value < 0 || value > _position) {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _position = value;
        }
    }

    /// <summary>Gets the maximum number of characters the buffer can hold.</summary>
    public int MaxLength => _span.Length;

    /// <summary>Appends a single character when space remains.</summary>
    public bool TryAppend(char c) {
        if (_position >= _span.Length) {
            return false;
        }

        _span[_position++] = c;
        return true;
    }

    /// <summary>Appends a span of characters when space remains.</summary>
    public bool TryAppend(ReadOnlySpan<char> value) {
        if (_position + value.Length > _span.Length) {
            return false;
        }

        value.CopyTo(_span[_position..]);
        _position += value.Length;
        return true;
    }

    /// <summary>Returns the written portion of the buffer.</summary>
    public ReadOnlySpan<char> AsSpan() => _span[.._position];

    /// <inheritdoc/>
    public override string ToString() => _span[.._position].ToString();
}
