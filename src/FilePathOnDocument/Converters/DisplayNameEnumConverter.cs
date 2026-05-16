using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace FilePathOnDocument.Converters;

internal class DisplayNameEnumConverter : EnumConverter
{
    private readonly Type _enumType;

    public DisplayNameEnumConverter(Type type) : base(type)
    {
        _enumType = type;
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value != null)
        {
            FieldInfo? field = _enumType.GetField(value.ToString()!);
            if (field != null)
            {
                DisplayAttribute? displayAttr = field.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
                {
                    return displayAttr.Name;
                }
            }
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue)
        {
            foreach (FieldInfo field in _enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                DisplayAttribute? displayAttr = field.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr != null && displayAttr.Name == stringValue)
                {
                    return field.GetValue(null);
                }
            }
        }

        return base.ConvertFrom(context, culture, value);
    }
}
