using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace FilePathOnDocument.Converters;

internal class DisplayNameEnumConverter : EnumConverter
{
    private static readonly ConcurrentDictionary<Type, Cache> _cache = new();

    private readonly Cache _map;

    public DisplayNameEnumConverter(Type type) : base(type)
    {
        _map = _cache.GetOrAdd(type, Build);
    }

    private static Cache Build(Type type)
    {
        var displayToValue = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var valueToDisplay = new Dictionary<object, string>();

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var value = field.GetValue(null)!;

            var display = field.GetCustomAttribute<DisplayAttribute>()?.Name
                          ?? field.Name;

            displayToValue[display] = value;
            valueToDisplay[value] = display;
        }

        return new Cache(displayToValue, valueToDisplay);
    }

    public override object? ConvertTo(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value,
        Type destinationType)
    {
        if (destinationType == typeof(string) &&
            value != null &&
            _map.ValueToDisplay.TryGetValue(value, out var display))
        {
            return display;
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        if (value is string s &&
            _map.DisplayToValue.TryGetValue(s, out var enumValue))
        {
            return enumValue;
        }

        return base.ConvertFrom(context, culture, value);
    }

    private sealed class Cache
    {
        public Dictionary<string, object> DisplayToValue { get; }
        public Dictionary<object, string> ValueToDisplay { get; }

        public Cache(
            Dictionary<string, object> displayToValue,
            Dictionary<object, string> valueToDisplay)
        {
            DisplayToValue = displayToValue;
            ValueToDisplay = valueToDisplay;
        }
    }
}