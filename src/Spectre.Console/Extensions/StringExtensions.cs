namespace Spectre.Console;

/// <summary>
/// Contains extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    // Cache whether or not internally normalized line endings
    // already are normalized. No reason to do yet another replace if it is.
    private static readonly bool _alreadyNormalized
        = Environment.NewLine.Equals("\n", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Escapes text so that it won’t be interpreted as markup.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>A string that is safe to use in markup.</returns>
    public static string EscapeMarkup(this string? text)
    {
        if (text == null)
        {
            return string.Empty;
        }

        return text
            .ReplaceExact("[", "[[")
            .ReplaceExact("]", "]]");
    }

    /// <summary>
    /// Removes markup from the specified string.
    /// </summary>
    /// <param name="text">The text to remove markup from.</param>
    /// <returns>A string that does not have any markup.</returns>
    public static string RemoveMarkup(this string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var result = new StringBuilder();

        var tokenizer = new MarkupTokenizer(text);
        while (tokenizer.MoveNext() && tokenizer.Current != null)
        {
            if (tokenizer.Current.Kind == MarkupTokenKind.Text)
            {
                result.Append(tokenizer.Current.Value);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Gets the cell width of the specified text.
    /// </summary>
    /// <param name="text">The text to get the cell width of.</param>
    /// <returns>The cell width of the text.</returns>
    public static int GetCellWidth(this string text)
    {
        return Cell.GetCellLength(text);
    }

    internal static string CapitalizeFirstLetter(this string? text, CultureInfo? culture = null)
    {
        if (text == null)
        {
            return string.Empty;
        }

        culture ??= CultureInfo.InvariantCulture;

        if (text.Length > 0 && char.IsLower(text[0]))
        {
            text = string.Format(culture, "{0}{1}", char.ToUpper(text[0], culture), text.Substring(1));
        }

        return text;
    }

    internal static string? RemoveNewLines(this string? text)
    {
        return text?.ReplaceExact("\r\n", string.Empty)
            ?.ReplaceExact("\n", string.Empty);
    }

    internal static string NormalizeNewLines(this string? text, bool native = false)
    {
        text = text?.ReplaceExact("\r\n", "\n");
        text ??= string.Empty;

        if (native && !_alreadyNormalized)
        {
            text = text.ReplaceExact("\n", Environment.NewLine);
        }

        return text;
    }

    internal static string[] SplitLines(this string text)
    {
        var result = text?.NormalizeNewLines()?.Split(new[] { '\n' }, StringSplitOptions.None);
        return result ?? Array.Empty<string>();
    }

    internal static string[] SplitWords(this string word, StringSplitOptions options = StringSplitOptions.None)
    {
        var result = new List<string>();

        static string Read(StringBuffer reader, Func<char, bool> criteria)
        {
            var buffer = new StringBuilder();
            while (!reader.Eof)
            {
                var current = reader.Peek();
                if (!criteria(current))
                {
                    break;
                }

                buffer.Append(reader.Read());
            }

            return buffer.ToString();
        }

        using (var reader = new StringBuffer(word))
        {
            while (!reader.Eof)
            {
                var current = reader.Peek();
                if (char.IsWhiteSpace(current))
                {
                    var x = Read(reader, c => char.IsWhiteSpace(c));
                    if (options != StringSplitOptions.RemoveEmptyEntries)
                    {
                        result.Add(x);
                    }
                }
                else
                {
                    result.Add(Read(reader, c => !char.IsWhiteSpace(c)));
                }
            }
        }

        return result.ToArray();
    }

    internal static string Repeat(this string text, int count)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (count <= 0)
        {
            return string.Empty;
        }

        if (count == 1)
        {
            return text;
        }

        return string.Concat(Enumerable.Repeat(text, count));
    }

    internal static string ReplaceExact(this string text, string oldValue, string? newValue)
    {
#if NETSTANDARD2_0
        return text.Replace(oldValue, newValue);
#else
        return text.Replace(oldValue, newValue, StringComparison.Ordinal);
#endif
    }

    internal static bool ContainsExact(this string text, string value)
    {
#if NETSTANDARD2_0
        return text.Contains(value);
#else
        return text.Contains(value, StringComparison.Ordinal);
#endif
    }

    /// <summary>
    /// "Masks" every character in a string.
    /// </summary>
    /// <param name="value">String value to mask.</param>
    /// <param name="mask">Character to use for masking.</param>
    /// <returns>Masked string.</returns>
    public static string Mask(this string value, char? mask)
    {
        var output = string.Empty;

        if (mask is null)
        {
            return output;
        }

        foreach (var c in value)
        {
            output += mask;
        }

        return output;
    }

    /// <summary>
    /// Truncates a string based on the length the string takes up in console.
    /// </summary>
    /// <param name="text">The string to truncate.</param>
    /// <param name="maxExpectedLength">The maximum length of the resultant string.</param>
    /// <returns>A truncated string.</returns>
    internal static string Truncate(this string text, int maxExpectedLength)
    {
        if (string.IsNullOrEmpty(text) || maxExpectedLength < 0)
        {
            return text;
        }

        var cellCount = text.GetCellWidth();
        if (maxExpectedLength > cellCount)
        {
            return text;
        }

        var builder = new StringBuilder(text);
        var i = builder.Length - 1;
        while (cellCount > maxExpectedLength && i >= 0)
        {
            cellCount -= UnicodeCalculator.GetWidth(builder[i]);
            i--;
        }

        builder.Remove(i + 1, builder.Length - i - 1);

        return builder.ToString();
    }
}