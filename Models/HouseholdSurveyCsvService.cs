using CsvHelper;
using CsvHelper.Configuration;
using EnergyTariffAdvisor.Models;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EnergyTariffAdvisor.Models
{
    public class HouseholdSurveyCsvService
    {
        public HouseholdSurveyTab? LoadSurveyById(string csvPath, int id)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null,
                Delimiter = ",",
                IgnoreBlankLines = true,
                PrepareHeaderForMatch = args => args.Header.Replace("\"", "")
            };

            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<HouseholdSurveyTab>();
                return records.FirstOrDefault(r => r.Id == id);
            }
        }

        public void SaveSurvey(string csvPath, HouseholdSurveyTab survey)
        {
            // Пример: добавить/заменить строку анкеты с нужным ID
            // Для простоты: можно сериализовать весь список, либо только одну строку (для новых файлов)
            using (var writer = new StreamWriter(csvPath, append: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecord(survey);
                writer.WriteLine();
            }
        }
    }
}
