using System;
using System.Collections.Generic;
using System.Xml;

namespace EnergyTariffAdvisor
{

    /// example usage:
    /// var normalizer = new Normalizer();
    /// var (min, max) = normalizer.GetNormalizationRules("Interval_0");
    /// double normalizedValue = normalizer.Normalize("Interval_0", 500.0);

    public class Normalizer
    {
        private readonly Dictionary<string, (decimal Min, decimal Max)> _normalizationRules = new();

      
        public Normalizer()
        {
            LoadPmml("D:\\Msc\\EnergyTariffAdvisor\\csv\\normalizer.pmml");
        }

        private void LoadPmml(string filePath)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("pmml", "http://www.dmg.org/PMML-4_2");

            var dataFields = xmlDoc.SelectNodes("//pmml:DataDictionary/pmml:DataField", namespaceManager);

            if (dataFields == null) return;

            foreach (XmlNode dataField in dataFields)
            {
                var name = dataField.Attributes?["name"]?.Value;
                var opType = dataField.Attributes?["optype"]?.Value;
                var dataType = dataField.Attributes?["dataType"]?.Value;

                if (string.IsNullOrEmpty(name) || opType != "continuous") continue;

                var intervalNode = dataField.SelectSingleNode("pmml:Interval", namespaceManager);
                if (intervalNode == null) continue;

                var closure = intervalNode.Attributes?["closure"]?.Value;
                if (closure != "closedClosed") continue; // Поддерживаем только closedClosed интервалы

                if (decimal.TryParse(intervalNode.Attributes?["leftMargin"]?.Value, out decimal min) &&
                    decimal.TryParse(intervalNode.Attributes?["rightMargin"]?.Value, out decimal max))
                {
                    _normalizationRules[name] = (min, max);
                }
            }
        }



        public (decimal Min, decimal Max) GetNormalizationRules(string fieldName)
        {
            if (_normalizationRules.TryGetValue(fieldName, out var rules))
            {
                return rules;
            }

            throw new KeyNotFoundException($"Правила нормализации для поля '{fieldName}' не найдены.");
        }

       
        /// [0, 1].
        ///  (value - min) / (max - min)
       
        public decimal Normalize(string fieldName, decimal value)
        {
            var (min, max) = GetNormalizationRules(fieldName);

            if (max == min)
            {
                //throw new InvalidOperationException($"Невозможно нормализовать: min и max равны для поля '{fieldName}'.");
                return value;
            }

            decimal normalized = (value - min) / (max - min);

            // clamping to [0, 1], if the value is out of interval
            normalized = Math.Max(0, Math.Min(1, normalized));

            return normalized;
        }
    }
}