using EzEngine.ContentManagement.Mono.Interop.Enums;

namespace EzEngine.ContentManagement.Mono.Interop.Models;

public class ProcessedPolyOneFileCustomProperty
{
    public string InternalName { get; private set; }
    public string FriendlyName { get; private set; }
    public CustomPropertyType Type { get; private set; }
    public CustomPropertyLevel Level { get; private set; }
    private decimal _numericDefaultValue;
    private string _textDefaultValue;
    public object DefaultValue
    {
        get
        {
            return Type == CustomPropertyType.Numeric
                ? _numericDefaultValue
                : _textDefaultValue;
        }
        private set
        {
            if (Type == CustomPropertyType.Numeric)
            {
                _numericDefaultValue = decimal.Parse(value.ToString()!);
            }
            else
            {
                _textDefaultValue = value?.ToString()!;
            }
        }
    }

    public ProcessedPolyOneFileCustomProperty(string internalName, string friendlyName, CustomPropertyType type, CustomPropertyLevel level, string defaultValue)
    {
        InternalName = internalName;
        FriendlyName = friendlyName;
        Type = type;
        Level = level;
        DefaultValue = defaultValue;
    }
}