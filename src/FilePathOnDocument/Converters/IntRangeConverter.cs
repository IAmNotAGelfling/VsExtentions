using System;
using System.ComponentModel;
using System.Globalization;

namespace FilePathOnDocument.Converters;

internal class IntRangeConverter : Int32Converter
{
    private const int MinValue = 1;
    private const int MaxValue = 10;

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        object? result = base.ConvertFrom(context, culture, value);

        if (result is int intValue)
        {
            if (intValue < MinValue)
                return MinValue;
            if (intValue > MaxValue)
                return MaxValue;
        }

        return result;
    }

    public override bool IsValid(ITypeDescriptorContext? context, object? value)
    {
        if (value is int intValue)
        {
            return intValue >= MinValue && intValue <= MaxValue;
        }

        return base.IsValid(context, value);
    }
}
