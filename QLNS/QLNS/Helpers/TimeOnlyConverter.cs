using System.Text.Json;
using System.Text.Json.Serialization;

namespace QLNS.Helpers
{
    public class TimeOnlyConverter : JsonConverter<TimeOnly>
    {
        private const string Format = "HH:mm";

        public override TimeOnly Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (string.IsNullOrEmpty(value))
                return default;

            return TimeOnly.ParseExact(value, Format);
        }

        public override void Write(
            Utf8JsonWriter writer,
            TimeOnly value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }
}