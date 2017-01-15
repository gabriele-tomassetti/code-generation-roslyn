using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace CodeGenerationUml
{
    public class UmlParser
    {        
        public static Parser<string> StartBracket = Parse.Char('{').Once().Text().Token();
        public static Parser<string> EndBracket = Parse.Char('}').Once().Text().Token();
        public static Parser<string> LessThen = Parse.Char('<').Once().Text().Token();
        public static Parser<string> GreaterThen = Parse.Char('>').Once().Text().Token();        

        public static Parser<string> UmlTags = Parse.Char('@').Once().Then(_ => Parse.CharExcept('\n').Many())
                                                .Text().Token();

        public static Parser<string> Identifier = Parse.CharExcept(" ,):").Many().Text().Token();

        public static Parser<string> Modifier = Parse.Char('+').Once().Return("public")
                                                .Or(Parse.Char('-').Once().Return("private"))
                                                .Or(Parse.Char('#').Once().Return("protected"))
                                                .Or(from start in StartBracket.Then(_ => StartBracket).Once()
                                                    from modifier in Parse.CharExcept('}').Many().Text().Token()
                                                    from end in EndBracket.Then(_ => EndBracket).Once()
                                                    select modifier
                                                )
                                                 .Or(from start in LessThen.Then(_ => LessThen).Once()
                                                     from modifier in Parse.CharExcept('>').Many().Text().Token()
                                                     from end in GreaterThen.Then(_ => GreaterThen).Once()
                                                     select modifier
                                                )
                                                .Text().Token();

        // Ref is used to avoid circular references
        // DelimitedBy to find many elements separated by one parser        
        public static Parser<Field> Field = from modifiers in Parse.Ref(() => Modifier).DelimitedBy(Parse.Char(' ').Many().Token()).Optional()
                                                  from name in Identifier
                                                  from delimeter in Parse.Char(':')
                                                  from type in Identifier
                                                  select new Field(name, type, modifiers.IsDefined ? modifiers.Get() : null);

        public static Parser<Method> Method = from modifiers in Parse.Ref(() => Modifier).DelimitedBy(Parse.Char(' ').Many().Token()).Optional()
                                              from name in Parse.CharExcept('(').Many().Text().Token()
                                              from startArg in Parse.Char('(')
                                              from arguments in Parse.Ref(() => Field)
                                                                .DelimitedBy(Parse.Char(',').Many().Token()).Optional()
                                              from endArg in Parse.Char(')')
                                              from delimeter in Parse.String(" : ").Optional()
                                              from returnType in Identifier.Optional()
                                              select new Method(modifiers.IsDefined ? modifiers.Get() : null, name, arguments.IsDefined ? arguments.Get() : null, returnType.IsDefined ? returnType.Get() : "");        

        public static Parser<String> Class = from keyword in Parse.String("class")
                                             from name in Identifier
                                             from bracket in StartBracket.Optional()
                                             select name;
    }
}