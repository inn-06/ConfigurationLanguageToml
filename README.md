# ConfigurationLanguageToml
Приложение для конвертации в файл toml. Реализация включает парсер на ANTLR4, AST-модель и преобразование в TOML.

Запуск приложения осуществляется благодаря ключу комнадной строки: dotnet run --convert --input C:\Program1\input2.conf --output C:\Program1\output.toml

Возможности:
1. Двоичные числа: 0b1010, 0B1101
2. Строки: 'текст'
3. Константы: имя := значение, использование через $имя$
4. Словари: table([ключ - значение])
5. Многострочные комментарии: {{! -- комментарий --}}
6. Преобразование в TOML: автоматическая конвертация словарей
7. Подробные сообщения об ошибках: с указанием строки и типа ошибки

Структура проекта:
  ConfigurationLanguage/
    Grammar -> ConfigGrammar #Грамматика ANTLR4
    Models -> ConfigModel #Модель данных
    Services/
      ConfigVisitor # Visitor для преобразования AST
      ErrorHandler # Обработчик ошибок парсера
      TomlConverter # Конвертер из AST в TOML
Примеры конфигураций:
/*
  timeout := 0b1111101000  # 1000 в десятичной
  app_name := 'MyApplication'
  
  # Словарь настроек
  table([
      name - $app_name$
      port - 0b111110100  # 500
      debug - false
  ])
*/

Парсер предоставляет подробные сообщения об ошибках:
1. Синтаксическая ошибка в строке 3:15 - неожиданный символ '='
2. Лексическая ошибка в строке 5:10 - недопустимые символы в бинарном числе
3. Синтаксическая ошибка в строке 2:8 - незакрытый комментарий

Проект включает модульные тесты для основных функций. Запуск тестов осуществляется по команде "dotnet test"
Тесты покрывают:
1. Парсинг бинарных чисел
2. Обработку строк
3. Выявление ошибок
4. Преобразование в TOML


Грамматика языка:
/*
config          : (constantDecl | statement)* EOF;
constantDecl    : NAME ':=' expression;
constantUse     : '$' NAME '$';
statement       : dictDeclaration | assignment;
dictDeclaration : 'table' '(' '[' dictPairs? ']' ')';
dictPair        : NAME ('-' | '"') expression;
expression      : constantUse | NUMBER | STRING | dictDeclaration;
*/
