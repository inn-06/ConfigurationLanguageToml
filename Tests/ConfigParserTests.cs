using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using ConfigurationLanguage.Models;
using ConfigurationLanguage.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationLanguage.Tests
{
    // Tests/ConfigParserTests.cs
    [TestClass]
    public class ConfigParserTests
    {
        [TestMethod]
        public void ParseBinaryNumber_ValidInput_ReturnsCorrectValue()
        {
            var input = "0b1010";
            var lexer = new ConfigGrammarLexer(new AntlrInputStream(input));
            var parser = new ConfigGrammarParser(new CommonTokenStream(lexer));
            var tree = parser.expression();

            var visitor = new ConfigVisitor();
            var result = (NumberExpression)visitor.Visit(tree);

            Assert.AreEqual("1010", result.BinaryValue);
            Assert.AreEqual(10, result.DecimalValue);
        }

        /// <summary>
        /// Тестирует программу на предмет ошибок
        /// </summary>
        [TestMethod]
        public void ParseInvalidInput_ShouldProduceErrorMessages()
        {
            // Arrange
            var errorHandler = new ErrorHandler();

            var invalidInputs = new[]
            {
                "Test = 'test'",      // заглавные буквы в имени
                "0b1023123123",             // недопустимые символы в бинарном числе
                "'unclosed string",          // незакрытая строка
                "{{! -- незакрытый комментарий", // незакрытый комментарий
            };

            foreach (var input in invalidInputs)
            {
                // Act: парсим с отслеживанием ошибок
                var lexer = new ConfigGrammarLexer(new AntlrInputStream(input));
                lexer.RemoveErrorListeners(); // убираем стандартный обработчик
                lexer.AddErrorListener(errorHandler);

                var tokens = new CommonTokenStream(lexer);
                var parser = new ConfigGrammarParser(tokens);
                parser.RemoveErrorListeners(); // убираем стандартный обработчик
                parser.AddErrorListener(errorHandler);

                // Пытаемся распарсить как выражение
                var tree = parser.expression();

                // Assert: проверяем, что ошибки были зафиксированы
                Assert.IsTrue(errorHandler.HasErrors, $"Для некорректного ввода '{input}' должны быть ошибки");

                // Очищаем ошибки для следующего теста
                errorHandler.Clear();
            }
        }

        [TestMethod]
        public void ParseStrings_VariousContent_ReturnsCorrectValues()
        {
            // Arrange: тестовые данные с разным содержимым строк
            var testCases = new[]
            {
                ("'Привет это тест'", "Привет это тест"),       // обычная строка
                ("''", string.Empty),                   // пустая строка
                ("'test with spaces'", "test with spaces"), // строка с пробелами
                ("'special chars: @#$%'", "special chars: @#$%"), // специальные символы
                ("'12345'", "12345"),                   // цифры в строке
            };

            foreach (var (input, expected) in testCases)
            {
                var lexer = new ConfigGrammarLexer(new AntlrInputStream(input));
                var parser = new ConfigGrammarParser(new CommonTokenStream(lexer));
                var tree = parser.expression();
                var visitor = new ConfigVisitor();
                var result = (StringExpression)visitor.Visit(tree);

                Assert.AreEqual(expected, result.Value, $"Значение строки '{input}' должно быть '{expected}'");
            }
        }
    }
}
