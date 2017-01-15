using System;
using System.Linq;
using Xunit;
using Sprache;

namespace CodeGenerationUml.Tests
{
    public class ParserTests
    {
        [Fact]
        public void CanParseSimpleModifier()
        {
            string result = UmlParser.Modifier.Parse("+");

            Assert.Equal("public", result);
        }

        [Fact]
        public void CanParseComplexModifier()
        {
            string result = UmlParser.Modifier.Parse("<<override>>");

            Assert.Equal("override", result);
        }

        [Fact]
        public void CanParseFieldWithoutModifier()
        {
            Field result = UmlParser.Field.Parse("writer : TextWriter");

            Assert.Equal("writer", result.Name);
            Assert.Equal("TextWriter", result.Type);
            Assert.Equal("field", result.DeclarationType);
            Assert.Empty(result.Modifiers);                        
        }

        [Fact]
        public void CanParseFieldWithModifier()
        {
            Field result = UmlParser.Field.Parse("+ writer : TextWriter");

            Assert.Equal("writer", result.Name);
            Assert.Equal("TextWriter", result.Type);
            Assert.Equal("field", result.DeclarationType);
            Assert.Single(result.Modifiers);
        }

        [Fact]
        public void CanParseMethodWithoutModifierArgumentsReturnType()
        {
            Method result = UmlParser.Method.Parse("VisitInterfaceDeclaration()");

            Assert.Equal("VisitInterfaceDeclaration", result.Name); 
            Assert.Equal("method", result.DeclarationType);
            Assert.Empty(result.Modifiers);
            Assert.Empty(result.Arguments);
        }

        [Fact]
        public void CanParseMethodWithModifierArgumentsReturnType()
        {
            Method result = UmlParser.Method.Parse("+ <<override>> VisitEnumMemberDeclaration(node:EnumMemberDeclarationSyntax, text:String) : void");

            Assert.Equal("VisitEnumMemberDeclaration", result.Name);
            Assert.Equal("method", result.DeclarationType);
            Assert.Equal("void", result.Type);
            Assert.Equal(2, result.Modifiers.Count());
            Assert.Equal(2, result.Arguments.Count());
        }
    }
}
