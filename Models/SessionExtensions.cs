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
        // settings for serialization with polymorphism support
        private static readonly JsonSerializerOptions options;

        // Static constructor for initializing options
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

        // Method for configuring polymorphism for TariffBase
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

                polymorphismOptions.DerivedTypes.Add(
                    new JsonDerivedType(typeof(CosyTariff), "cosy"));

                typeInfo.PolymorphismOptions = polymorphismOptions;
            }
        }

        // Saving an object to the session
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            string json = JsonSerializer.Serialize(value, options);
            session.SetString(key, json);
        }

        // Loading an object from the session
        public static T? GetObject<T>(this ISession session, string key)
        {
            string json = session.GetString(key);
            if (json == null)
                return default;

            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}