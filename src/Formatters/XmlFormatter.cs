#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Xml.Linq;
using DotnetMicroOrm.Utils;

namespace DotnetMicroOrm.Formatters;

/// <summary>
/// XML formatter for outputting data in XML format.
/// Converts objects to XML elements with proper escaping.
/// Handles collections as repeating elements.
/// </summary>
public class sealed XmlFormatter : IOutputFormatter
{
    private readonly bool _indented;
    private readonly string _rootElementName;
    private readonly string _itemElementName;

    public string ContentType => "application/xml";

    public XmlFormatter(string rootElementName = "root", string itemElementName = "item", bool indented = true)
    {
        _rootElementName = rootElementName;
        _itemElementName = itemElementName;
        _indented = indented;
    }

    public string Format(object? data)
    {
        if (data is null)
            return $"<{_rootElementName} />";

        var root = new XElement(_rootElementName);

        if (data is IEnumerable<object> collection)
        {
            foreach (var item in collection)
            {
                root.Add(ObjectToXmlElement(item, _itemElementName));
            }
        }
        else
        {
            root.Add(ObjectToXmlElement(data, _itemElementName));
        }

        return _indented ? root.ToString() : root.ToString(SaveOptions.DisableFormatting);
    }

    public string FormatCollection<T>(IEnumerable<T> items)
    {
        if (items is null)
            return $"<{_rootElementName} />";

        var root = new XElement(_rootElementName);

        foreach (var item in items)
        {
            if (item is not null)
            {
                root.Add(ObjectToXmlElement(item, _itemElementName));
            }
        }

        return _indented ? root.ToString() : root.ToString(SaveOptions.DisableFormatting);
    }

    public string FormatError(string code, string message, string requestId)
    {
        var error = new XElement("error",
            new XElement("code", code),
            new XElement("message", message),
            new XElement("requestId", requestId),
            new XElement("timestamp", DateTime.UtcNow.ToString("O"))
        );

        return _indented ? error.ToString() : error.ToString(SaveOptions.DisableFormatting);
    }

    private XElement ObjectToXmlElement(object? obj, string elementName)
    {
        if (obj is null)
            return new XElement(elementName);

        var element = new XElement(elementName);
        var type = obj.GetType();
        var properties = ReflectionHelper.GetProperties(type);

        foreach (var property in properties)
        {
            try
            {
                var value = property.GetValue(obj);

                if (value is null)
                {
                    element.Add(new XElement(property.Name));
                }
                else if (ReflectionHelper.IsSimpleType(property.PropertyType))
                {
                    var stringValue = value.ToString() ?? string.Empty;
                    element.Add(new XElement(property.Name, stringValue));
                }
                else if (value is IEnumerable<object> collection && property.PropertyType != typeof(string))
                {
                    var collectionElement = new XElement(property.Name);
                    foreach (var item in collection)
                    {
                        collectionElement.Add(ObjectToXmlElement(item, "item"));
                    }
                    element.Add(collectionElement);
                }
                else
                {
                    element.Add(ObjectToXmlElement(value, property.Name));
                }
            }
            catch
            {
                // Silently ignore serialization errors for individual properties
            }
        }

        return element;
    }
}
