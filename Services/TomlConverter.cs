using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigurationLanguage.Models;
using Tomlyn;
using Tomlyn.Model;

namespace ConfigurationLanguage.Services
{

    public class TomlConverter
    {
        private readonly Dictionary<string, object> _constants = new();
        private readonly List<TomlTable> _tables = new();

        public string Convert(ConfigModel config)
        {
            // 1. Обрабатываем константы
            foreach (var constant in config.Constants)
            {
                _constants[constant.Name] = EvaluateExpression(constant.Value);
            }

            // 2. Обрабатываем statements
            foreach (var statement in config.Statements)
            {
                ProcessStatement(statement);
            }

            // 3. Формируем TOML
            var result = new StringBuilder();

            if (_tables.Count == 1)
            {
                // Если только одна таблица, выводим как глобальную
                result.AppendLine(Toml.FromModel(_tables[0]));
            }
            else
            {
                // Если несколько таблиц, нумеруем их
                for (int i = 0; i < _tables.Count; i++)
                {
                    if (i > 0) result.AppendLine();
                    result.AppendLine($"[table_{i + 1}]");
                    var tableToml = Toml.FromModel(_tables[i]);
                    var lines = tableToml.Split('\n');
                    result.AppendLine(string.Join("\n", lines));
                }
            }

            return result.ToString().Trim();
        }

        private void ProcessStatement(AstNode statement)
        {
            if (statement is DictionaryDeclaration dict)
            {
                var tomlTable = new TomlTable();

                foreach (var pair in dict.Pairs)
                {
                    var value = EvaluateExpression(pair.Value);
                    tomlTable[pair.Key] = ConvertToTomlValue(value);
                }

                _tables.Add(tomlTable);
            }
        }

        private object EvaluateExpression(Expression expr)
        {
            return expr switch
            {
                NumberExpression n => n.DecimalValue,
                StringExpression s => s.Value,
                ConstantExpression c =>_constants.TryGetValue(c.ConstantName, out var value) ? value : throw new Exception($"Неизвестная константа: {c.ConstantName}"),
                DictionaryExpression d => ProcessDictionary(d.Dictionary),
                _ => throw new Exception($"Неподдерживаемый тип выражения")
            };
        }

        private Dictionary<string, object> ProcessDictionary(DictionaryDeclaration dict)
        {
            var result = new Dictionary<string, object>();

            foreach (var pair in dict.Pairs)
            {
                result[pair.Key] = EvaluateExpression(pair.Value);
            }

            return result;
        }

        private object ConvertToTomlValue(object value)
        {
            return value switch
            {
                long l => l,
                int i => i,
                string s => s,
                Dictionary<string, object> dict =>
                    dict.ToDictionary(kv => kv.Key, kv => ConvertToTomlValue(kv.Value)),
                _ => value?.ToString() ?? "null"
            };
        }
    }
}


