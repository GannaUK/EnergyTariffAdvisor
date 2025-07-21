using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using EnergyTariffAdvisor.Models;
using System.Collections.Generic;

namespace EnergyTariffAdvisor.Models
{
    public static class SessionExtensions
    {
        // Настройки сериализации с поддержкой полиморфизма
        private static readonly JsonSerializerOptions options;

        // Статический конструктор для инициализации options
        static SessionExtensions()
        {
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var resolver = new DefaultJsonTypeInfoResolver();

            resolver.Modifiers.Add(ConfigurePolymorphism);

            options.TypeInfoResolver = resolver;
        }

        // Метод настройки полиморфизма для TariffBase
        private static void ConfigurePolymorphism(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Type == typeof(TariffBase))
            {
                var polymorphismOptions = new JsonPolymorphismOptions();
                polymorphismOptions.TypeDiscriminatorPropertyName = "$type";
                polymorphismOptions.UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization;

                polymorphismOptions.DerivedTypes.Add(
                    new JsonDerivedType(typeof(FixedTariff), "fixed"));

                polymorphismOptions.DerivedTypes.Add(
                    new JsonDerivedType(typeof(IntervalTariff), "interval"));

                polymorphismOptions.DerivedTypes.Add(
                    new JsonDerivedType(typeof(DayNightTariff), "daynight"));

                typeInfo.PolymorphismOptions = polymorphismOptions;
            }
        }

        // Сохранение объекта в сессию
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            string json = JsonSerializer.Serialize(value, options);
            session.SetString(key, json);
        }

        // Загрузка объекта из сессии
        public static T? GetObject<T>(this ISession session, string key)
        {
            string json = session.GetString(key);
            if (json == null)
                return default;

            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}