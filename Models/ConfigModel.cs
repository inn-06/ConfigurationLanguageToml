using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationLanguage.Models
{
    // Базовый класс для всех узлов AST
    public abstract class AstNode { }

    // Конфигурация - корневой элемент
    public class ConfigModel : AstNode
    {
        public List<ConstantDeclaration> Constants { get; } = [];
        public List<AstNode> Statements { get; } = [];
    }

    // Объявление константы: имя := значение
    public class ConstantDeclaration : AstNode
    {
        public string Name { get; set; } = string.Empty;
        public Expression Value { get; set; } = null!;
    }

    // Базовый класс для выражений
    public abstract class Expression : AstNode { }

    // Использование константы: $имя$
    public class ConstantExpression : Expression
    {
        public string ConstantName { get; set; } = string.Empty;
    }

    // Двоичное число: 0b1010
    public class NumberExpression : Expression
    {
        public string BinaryValue { get; set; } = string.Empty;

        // Преобразуем в десятичное число
        public long DecimalValue
        {
            get
            {
                try
                {
                    return Convert.ToInt64(BinaryValue, 2);
                }
                catch
                {
                    return 0;
                }
            }
        }
    }

    // Строка: 'текст'
    public class StringExpression : Expression
    {
        public string Value { get; set; } = string.Empty;
    }

    // Словарь: table([...])
    public class DictionaryExpression : Expression
    {
        //public List<KeyValuePair> Pairs { get; } = new();
        public DictionaryDeclaration Dictionary { get; set; } = null!;
    }

    // Объявление словаря
    public class DictionaryDeclaration : AstNode
    {
        public List<KeyValuePair> Pairs { get; } = new();
    }

    // Пара ключ-значение в словаре
    public class KeyValuePair
    {
        public string Key { get; set; } = string.Empty;
        public Expression Value { get; set; } = null!;
        public char Separator { get; set; } // '-' или '"'
    }

    // Присваивание (если понадобится)
    public class Assignment : AstNode
    {
        public string VariableName { get; set; } = string.Empty;
        public Expression Value { get; set; } = null!;
    }
}
