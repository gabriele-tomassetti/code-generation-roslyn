using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace CodeGenerationUml
{
    public class Field : IDeclaration
    {
        public String Name { get; set; }        
        public String Type { get; set; }
        public IEnumerable<String> Modifiers { get; set; }

        public String DeclarationType { get; set; } = "field";

        public Field(String name, String type, IEnumerable<String> modifiers)
        {
            Name = name;
            Type = type;

            if (modifiers != null)
                Modifiers = modifiers;
            else
                Modifiers = new List<String>();
        }
    }

    public class Method : IDeclaration
    {
        public String Name { get; set; }
        public String Type { get; set; }
        public IEnumerable<Field> Arguments { get; set; }
        public IEnumerable<String> Modifiers { get; set; }

        public String DeclarationType { get; set; } = "method";

        public Method(IEnumerable<String> modifiers, string name, IEnumerable<Field> arguments, string returnType)
        {
            Name = name;
            Type = returnType;

            if (modifiers != null)
                Modifiers = modifiers;
            else
                Modifiers = new List<String>();

            if (arguments != null)
                Arguments = arguments;
            else
                Arguments = new List<Field>();
        }
    }

    public interface IDeclaration
    {
        String Name { get; set; }
        String Type { get; set; }
        IEnumerable<String> Modifiers { get; set; }

        String DeclarationType { get; set; }
    }    
}