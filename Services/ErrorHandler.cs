using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using ConfigurationLanguage.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConfigurationLanguage.Services
{
    // 1. Сначала создаем класс ErrorHandler, реализующий нужные интерфейсы
    public class ErrorHandler : IAntlrErrorListener<IToken>, IAntlrErrorListener<int>
    {
        private readonly List<string> _errors = new();

        // Для ошибок лексического анализа
        public void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            int offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            _errors.Add($"Лексическая ошибка в строке {line}:{charPositionInLine} - {msg}");
        }

        // Для ошибок синтаксического анализа
        public void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            var errorMessage = $"Синтаксическая ошибка в строке {line}:{charPositionInLine} - {msg}";

            // Дополнительная информация по токену
            if (offendingSymbol != null)
            {
                var tokenText = offendingSymbol.Text;

                // Проверяем на заглавные буквы в именах
                if (Regex.IsMatch(tokenText, @"^[A-Z]"))
                {
                    errorMessage += " (имена должны быть в нижнем регистре)";
                }

                // Проверяем на незакрытые комментарии
                if (tokenText?.Contains("{{! --") == true && !tokenText.Contains("--}}"))
                {
                    errorMessage += " (незакрытый комментарий)";
                }
            }

            _errors.Add(errorMessage);
        }

        public bool HasErrors => _errors.Count > 0;
        public IEnumerable<string> Errors => _errors;
        public void Clear() => _errors.Clear();

    }
}

