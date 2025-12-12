grammar ConfigGrammar;

/*
  Грамматика учебного конфигурационного языка
  Многострочные комментарии: {{! -- ... --}}
  Числа: 0b1010 (двоичные)
  Имена: только нижний регистр и подчеркивание
  Строки: 'текст'
  Константы: имя := значение
  Использование: $имя$
  Словари: table([имя - значение, ...])
*/

// ПРАВИЛА ПАРСЕРА (синтаксис)
config: (constantDecl | statement)* EOF;

// Объявление константы
constantDecl: name=NAME ':=' value=expression;

// Использование константы
constantUse: '$' name=NAME '$';

// Statement (инструкция) - может быть словарь или присваивание
statement: 
    dictDeclaration
    | assignment
    ;

// Присваивание (если понадобится позже)
assignment: NAME '=' expression;

// Объявление словаря
dictDeclaration: 
    'table' '(' '[' dictPairs? ']' ')'
    ;

// Пары в словаре
dictPairs: dictPair (',' dictPair)*;

// Одна пара: имя РАЗДЕЛИТЕЛЬ значение
dictPair: 
    name=NAME separator=('-' | '"') value=expression;

// Выражение (может быть разных типов)
expression:
    constantUse                     # ConstExpr
    | NUMBER                       # NumberExpr
    | STRING                       # StringExpr
    | dictDeclaration              # DictExpr
    | '(' expression ')'           # ParenExpr
    ;

// ПРАВИЛА ЛЕКСЕРА (токены)

// Двоичные числа: 0b1010 или 0B1010
NUMBER: '0' [bB] [01]+;

// Строки в апострофах
STRING: '\'' (~['\r\n] | '\\\'')* '\'';

// Имена: только нижний регистр и подчеркивание
NAME: [_a-z]+;

// Многострочные комментарии: {{! -- ... --}}
MULTILINE_COMMENT: '{{! --' .*? '--}}' -> skip;

// Пропускаем пробелы и переводы строк
WS: [ \t\r\n]+ -> skip;

// Однострочные комментарии (если понадобятся)
LINE_COMMENT: '#' ~[\r\n]* -> skip;