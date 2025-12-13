using Antlr4.Runtime;
using ConfigurationLanguage.Models;
using ConfigurationLanguage.Services;
using System;
using System.CommandLine;

namespace ConfigurationLanguage;

class Program
{
    static int Main(string[] args)
    {
        var inputOption = new Option<FileInfo>("--input", "-i")
        {
            Description = "Входной файл конфигурации"
        };

        var outputOption = new Option<FileInfo>("--output", "-o")
        {
            Description = "Выходной TOML файл"
        };
        // Создаем корневую команду
        var command = new Command(
             name: "convert",
             description: "Конвертирует конфиг в TOML")
        {
            // 3. Добавляем опции
            inputOption,
            outputOption
        };

        // 4. Устанавливаем обработчик (через SetAction)
        command.SetAction((ParseResult parseResult) =>
        {
            // Получаем значения опций из ParseResult
            var input = parseResult.GetValue(inputOption);
            var output = parseResult.GetValue(outputOption);

            if (input == null || output == null)
            {
                Console.WriteLine("Ошибка: необходимо указать --input и --output");
                return 1;
            }

            ConvertConfigToToml(input, output);
            return 0;
        });

        // 5. Создаем RootCommand и добавляем команду
        var rootCommand = new RootCommand("Конвертер конфигураций")
        {
            command
        };

        // 6. Парсим и выполняем
        return rootCommand.Parse(args).Invoke();
    }


    static void ConvertConfigToToml(FileInfo inputFile, FileInfo outputFile)
    {
        Console.WriteLine($"Чтение файла: {inputFile.FullName}");

        // 1. Читаем входной файл
        if (!inputFile.Exists)
        {
            throw new FileNotFoundException($"Файл не найден: {inputFile.FullName}");
        }

        var inputText = File.ReadAllText(inputFile.FullName);
        Console.WriteLine($"Прочитано {inputText.Length} символов");

        // 2. Создаем лексер и парсер
        var inputStream = new AntlrInputStream(inputText);
        var lexer = new ConfigGrammarLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ConfigGrammarParser(tokenStream);

        // 3. Настраиваем обработку ошибок
        var errorHandler = new ErrorHandler();
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorHandler);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorHandler);

        // 4. Парсим
        Console.WriteLine("Парсинг...");
        var tree = parser.config();

        // 5. Проверяем ошибки
        if (errorHandler.HasErrors)
        {
            Console.WriteLine("\nОбнаружены ошибки:");
            foreach (var error in errorHandler.Errors)
            {
                Console.WriteLine($"  {error}");
            }
            Environment.Exit(1);
        }

        Console.WriteLine("Парсинг успешно завершен");

        // 6. Строим AST
        Console.WriteLine("Построение AST...");
        var visitor = new ConfigVisitor();
        var configModel = (ConfigModel)visitor.Visit(tree);

        // 7. Конвертируем в TOML
        Console.WriteLine("Конвертация в TOML...");
        var converter = new TomlConverter();
        var tomlText = converter.Convert(configModel);

        // 8. Сохраняем результат
        File.WriteAllText(outputFile.FullName, tomlText);
        Console.WriteLine($"Результат сохранен в: {outputFile.FullName}");

        // 9. Показываем результат
        Console.WriteLine("\nРезультат преобразования:");
        Console.WriteLine("=".PadRight(50, '='));
        Console.WriteLine(tomlText);
        Console.WriteLine("=".PadRight(50, '='));
    }
}
