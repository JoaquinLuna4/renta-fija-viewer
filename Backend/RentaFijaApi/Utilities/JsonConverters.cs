using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RentaFijaApi.Utilities
{
    // Custom converter to handle JSON values that can be either a number or a string for doubles
    public class StringToDoubleConverter : JsonConverter<double?>
    {
        public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                string? stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }

                // Define a culture that uses '.' as the decimal separator
                var culture = CultureInfo.InvariantCulture;

                // Attempt to parse the string to a double, allowing for signs, etc.
                if (double.TryParse(stringValue, NumberStyles.Any, culture, out double result))
                {
                    return result;
                }
                
                // Handle cases where the string might have a comma as a decimal separator
                if (double.TryParse(stringValue.Replace(',', '.'), NumberStyles.Any, culture, out result))
                {
                    return result;
                }
            }

            return null; // Return null if conversion is not possible
        }

        public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    // Custom converter for integers
    public class StringToIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                string? stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }
                
                // For integers, we don't need complex culture parsing
                if (int.TryParse(stringValue, out int result))
                {
                    return result;
                }
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
