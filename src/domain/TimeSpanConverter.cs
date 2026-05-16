using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Кастомний конвертер для забезпечення коректної серіалізації (Serialization) 
/// та десеріалізації (Deserialization) об'єктів типу <see cref="TimeSpan"/> у JSON-формат.
/// </summary>
internal class TimeSpanConverter : JsonConverter<TimeSpan>
{
    /// <summary>
    /// Зчитує текстовий токен JSON та конвертує (десеріалізує) його у валидне значення <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="reader">Агрегат <see cref="Utf8JsonReader"/>, що відповідає за послідовне зчитування токенів JSON.</param>
    /// <param name="typeToConvert">Тип об'єкта, який підлягає конвертації.</param>
    /// <param name="options">Глобальні конфігураційні налаштування серіалізатора <see cref="JsonSerializerOptions"/>.</param>
    /// <returns>Відновлений об'єкт типу <see cref="TimeSpan"/>.</returns>
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TimeSpan.Parse(reader.GetString()!);

    /// <summary>
    /// Конвертує (серіалізує) поточне значення структури <see cref="TimeSpan"/> у строковий формат для запису в JSON-файл.
    /// </summary>
    /// <param name="writer">Агрегат <see cref="Utf8JsonWriter"/>, що виконує безпосередній запис токенів у вихідний потік.</param>
    /// <param name="value">Значення тривалості передачі, яке необхідно записати.</param>
    /// <param name="options">Глобальні конфігураційні налаштування серіалізатора <see cref="JsonSerializerOptions"/>.</param>
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}