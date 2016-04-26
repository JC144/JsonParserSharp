using JSonParser.SDK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using JSonParser.CLI.Templates;

namespace JSonParser.CLI
{
    class Program
    {
        private static void Main(string[] args)
        {
            Process(CommandLine.Parser.Default.ParseArguments<Options>(args).Value);
        }

        private static void Process(Options opts)
        {
            string inputPath = String.Empty;
            inputPath = GetInputPath(opts, inputPath);

            if (String.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
            {
                throw new FileNotFoundException(inputPath);
            }
            else
            {
                bool isDirectory = false;
                FileAttributes attr = File.GetAttributes(inputPath);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    isDirectory = true;
                }

                string outputFolder = opts.Output;
                if (String.IsNullOrEmpty(outputFolder))
                {
                    outputFolder = Path.GetDirectoryName(inputPath);
                }


                if (isDirectory)
                {
                    ComputeJsonFilesFromDirectory(opts, inputPath, outputFolder);
                }
                else
                {
                    ComputeJsonFile(opts, inputPath, outputFolder);
                }
            }

        }        

        private static string GetInputPath(Options opts, string inputPath)
        {
            if (opts != null && !String.IsNullOrEmpty(opts.Input))
            {
                inputPath = Path.GetFullPath(opts.Input);
            }
            else
            {
                inputPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            }
            return inputPath;
        }

        private static void ComputeJsonFilesFromDirectory(Options opts, string inputPath, string outputFolder)
        {
            var files = Directory.GetFiles(inputPath, "*.json");
            foreach (var file in files)
            {
                ComputeJsonFile(opts, file, outputFolder);
            }
        }

        private static void ComputeJsonFile(Options opts, string inputPath, string outputFolder)
        {
            Generator generator = new Generator(new GeneratorContext(outputFolder, inputPath, opts.NamespaceName, opts.RootClassName));
            generator.TransformText();
        }        
    }
}
