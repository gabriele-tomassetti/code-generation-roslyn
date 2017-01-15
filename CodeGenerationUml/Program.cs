using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sprache;

namespace CodeGenerationUml
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("****************************************************************************");
            Console.WriteLine("*                                                                          *");
            Console.WriteLine("*   PlantUml to create C# skeleton from a PlantUml file                    *");
            Console.WriteLine("*   To convert specify a .plantUml file or a directory                     *");
            Console.WriteLine("*                                                                          *");
            Console.WriteLine("****************************************************************************");

            // to test UmlParser            
            // args = new[] { @"..\..\uml\", "cs" };

            if (args.Length < 1)
            {
                Console.WriteLine("Specify a file or directory");
                return;
            }

            var input = args[0];

            IEnumerable<string> files;
            if (Directory.Exists(input))
            {
                files = Directory.EnumerateFiles(Path.GetFullPath(input), "*.plantuml");
            }
            else if (File.Exists(input))
            {
                try
                {
                    var fullname = Path.GetFullPath(input);
                    files = new[] { fullname };
                }
                catch
                {
                    Console.WriteLine("Invalid name");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Specify an existing file or directory");
                return;
            }

            var outputDir = "";
            if (args.Length >= 2)
            {
                if (Directory.Exists(args[1]))
                {
                    outputDir = args[1];
                }
            }

            if (outputDir == "")
            {
                outputDir = Path.Combine(Path.GetDirectoryName(files.First()), "cs");
                Directory.CreateDirectory(outputDir);
            }

            foreach (var file in files)
            {
                Console.WriteLine($"Generating CSharp code for {file}...");
                string outputFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(file));

                using (var writer = new StreamWriter(new FileStream(outputFile + ".cs", FileMode.Create, FileAccess.Write)))
                {
                    try
                    {
                        IEnumerable<string> lines = File.ReadLines(file);

                        // we use the name of the directory for the namespace and the file name for the name of the class
                        UmlClass currentClass = new UmlClass(writer, (new DirectoryInfo(outputDir)).Name, Path.GetFileNameWithoutExtension(file));

                        // we parse the UML diagram line by line
                        foreach (var line in lines)
                        {
                            var attemptedClass = UmlParser.Class.TryParse(line);
                            if (attemptedClass.WasSuccessful)
                            {
                                currentClass.Name = attemptedClass.Value;                                
                            }

                            var attemptedMethod = UmlParser.Method.TryParse(line);
                            if (attemptedMethod.WasSuccessful)
                            {
                                currentClass.Declarations.Add(attemptedMethod.Value);
                                // we skip a cycle because a method could be parsed also as a field
                                continue;
                            }

                            var attemptedField = UmlParser.Field.TryParse(line);
                            if (attemptedField.WasSuccessful)
                            {
                                currentClass.Declarations.Add(attemptedField.Value);                                
                            }

                            var attempted = UmlParser.EndBracket.TryParse(line);
                            if (attempted.WasSuccessful)
                            {
                                currentClass.Generate();
                                currentClass = new UmlClass(writer, (new DirectoryInfo(outputDir)).Name, Path.GetFileNameWithoutExtension(file));                                
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex);
                    }
                }
            }

            Console.WriteLine("Completed");
        }
    }
}