using System;
using System.ComponentModel;
using System.Globalization;

namespace FilePathOnDocument.Converters;

internal class IntRangeConverter : Int32Converter
{
    private const int MinValue = 1;
    private const int MaxValue = 10;

    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        object? result = base.ConvertFrom(context, culture, value);

        return result is int intValue
            ? Math.Min(MaxValue, Math.Max(MinValue, intValue))
            : result;
    }
}