using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using System.IO;

namespace CodeGenerationUml
{
    public class UmlClass
    {
        private TextWriter writer;
        private string fileName;
        private string directoryName;

        public string Name { get; set; }
        public List<IDeclaration> Declarations { get; set; }

        public UmlClass(TextWriter writer, string directoryName, string fileName)
        {
            this.writer = writer;
            this.fileName = fileName.Capitalize();
            this.directoryName = directoryName.Capitalize();
            Declarations = new List<IDeclaration>();
        }

        public void Generate()
        {
            // standard using directives
            CompilationUnitSyntax cu = SyntaxFactory.CompilationUnit()
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Collections.Generic")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Linq")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Text")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Threading.Tasks")));

            NamespaceDeclarationSyntax localNamespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(directoryName));            

            ClassDeclarationSyntax localClass = SyntaxFactory.ClassDeclaration(Name);

            foreach (var member in Declarations)
            {                
                switch (member.DeclarationType)
                {
                    case "method":                        
                        var currentMethod = member as Method;

                        //currentMethod.Type is a string parsed from the uml diagram
                        MethodDeclarationSyntax method = SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(currentMethod.Type)), currentMethod.Name);

                        List<SyntaxToken> mods = new List<SyntaxToken>();

                        foreach (var modifier in currentMethod.Modifiers)
                            mods.Add(SyntaxFactory.ParseToken(modifier));

                        method = method.AddModifiers(mods.ToArray());
                        
                        SeparatedSyntaxList<ParameterSyntax> ssl = SyntaxFactory.SeparatedList<ParameterSyntax>();
                        foreach (var param in currentMethod.Arguments)
                        {                          
                            ParameterSyntax ps = SyntaxFactory.Parameter(
                                new SyntaxList<AttributeListSyntax>(),
                                new SyntaxTokenList(),
                                SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(param.Type)),
                                SyntaxFactory.Identifier(param.Name), null);

                            ssl = ssl.Add(ps);
                        }

                        method = method.AddParameterListParameters(ssl.ToArray());

                        // we add an exception to the body of an otherwise empty method
                        ThrowStatementSyntax notReady = SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("NotImplementedException"), SyntaxFactory.ArgumentList(), null));                        

                        method = method.AddBodyStatements(notReady);                        

                        localClass = localClass.AddMembers(method);
                        break;

                    case "field":
                        var currentField = member as Field;

                        SyntaxTokenList stl = new SyntaxTokenList();

                        foreach (var modifier in currentField.Modifiers)
                            stl = stl.Add(SyntaxFactory.ParseToken(modifier));
                        
                        SeparatedSyntaxList<VariableDeclaratorSyntax> svd = SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>();

                        svd = svd.Add(SyntaxFactory.VariableDeclarator(currentField.Name));

                        // currentField.Type is a string parsed from the uml diagram
                        VariableDeclarationSyntax variable = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(currentField.Type), svd);

                        FieldDeclarationSyntax field = SyntaxFactory.FieldDeclaration(
                                new SyntaxList<AttributeListSyntax>(),
                                stl,
                                variable
                               );                            

                        localClass = localClass.AddMembers(field);
                        break;
                }
            }
            
            localNamespace = localNamespace.AddMembers(localClass);
            cu = cu.AddMembers(localNamespace);

            AdhocWorkspace cw = new AdhocWorkspace();
            OptionSet options = cw.Options;
            cw.Options.WithChangedOption(CSharpFormattingOptions.IndentBraces, true);
            SyntaxNode formattedNode = Formatter.Format(cu, cw, options);

            formattedNode.WriteTo(writer);            
        }
    }
}
