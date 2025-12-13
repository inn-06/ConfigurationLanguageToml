using ConfigurationLanguage.Models;
using KeyValuePair = ConfigurationLanguage.Models.KeyValuePair;

namespace ConfigurationLanguage.Services
{

    public class ConfigVisitor : ConfigGrammarBaseVisitor<AstNode>
    {
        // Словарь для хранения значений констант
        private readonly Dictionary<string, object> _constants = [];

        public override AstNode VisitConfig(ConfigGrammarParser.ConfigContext context)
        {
            var config = new ConfigModel();

            foreach (var child in context.children)
            {
                if (child is ConfigGrammarParser.ConstantDeclContext constCtx)
                {
                    // Обрабатываем объявление константы
                    var constant = (ConstantDeclaration)VisitConstantDecl(constCtx);
                    config.Constants.Add(constant);

                    // Вычисляем значение константы и сохраняем
                    var value = EvaluateExpression(constant.Value);
                    _constants[constant.Name] = value;
                }
                else if (child is ConfigGrammarParser.StatementContext stmtCtx)
                {
                    // Обрабатываем statement
                    var statement = VisitStatement(stmtCtx);
                    config.Statements.Add(statement);
                }
            }

            return config;
        }

        public override AstNode VisitConstantDecl(ConfigGrammarParser.ConstantDeclContext context)
        {
            return new ConstantDeclaration
            {
                Name = context.name.Text,
                Value = (Expression)Visit(context.value)
            };
        }

        public override AstNode VisitConstExpr(ConfigGrammarParser.ConstExprContext context)
        {
            return new ConstantExpression
            {
                ConstantName = context.constantUse().name.Text
            };
        }

        public override AstNode VisitNumberExpr(ConfigGrammarParser.NumberExprContext context)
        {
            // Убираем префикс '0b' или '0B'
            var numberText = context.NUMBER().GetText();
            var binaryValue = numberText.Substring(2); // убираем первые 2 символа

            return new NumberExpression
            {
                BinaryValue = binaryValue
            };
        }

        public override AstNode VisitStringExpr(ConfigGrammarParser.StringExprContext context)
        {
            var text = context.STRING().GetText();
            // Убираем обрамляющие апострофы
            var value = text.Substring(1, text.Length - 2);

            return new StringExpression
            {
                Value = value
            };
        }

        public override AstNode VisitDictExpr(ConfigGrammarParser.DictExprContext context)
        {
            return VisitDictDeclaration(context.dictDeclaration());
        }

        public override AstNode VisitDictDeclaration(ConfigGrammarParser.DictDeclarationContext context)
        {
            var dict = new DictionaryDeclaration();

            if (context.dictPairs() != null)
            {
                foreach (var pairCtx in context.dictPairs().dictPair())
                {
                    var valueNode = Visit(pairCtx.value);

                    // Проверяем тип значения
                    Expression valueExpression = valueNode switch
                    {
                        Expression expr => expr,
                        DictionaryDeclaration dictDecl => new DictionaryExpression { Dictionary = dictDecl },
                        _ => throw new InvalidOperationException($"Неподдерживаемый тип значения: {valueNode?.GetType().Name}")
                    };

                    var pair = new KeyValuePair
                    {
                        Key = pairCtx.name.Text,
                        Value = valueExpression,  // Теперь это Expression
                        Separator = pairCtx.separator.Text[0]
                    };
                    dict.Pairs.Add(pair);
                }
            }

            return dict;
        }

        // Метод для вычисления значения выражения
        private object EvaluateExpression(Expression expr)
        {
            return expr switch
            {
                NumberExpression n => n.DecimalValue,
                StringExpression s => s.Value,
                ConstantExpression c => _constants[c.ConstantName],
                DictionaryExpression d => throw new NotImplementedException("Словари пока не поддерживаются как значения"),
                _ => throw new InvalidOperationException($"Неизвестный тип выражения: {expr.GetType().Name}")
            };
        }
    }
}
