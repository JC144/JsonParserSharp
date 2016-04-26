using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSonParser.CLI
{
    public class Options
    {
        [Option('i', "input", Required = true,HelpText = "Input Json file to be processed.")]
        public string Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output folder where C# classes will be stored.")]
        public string Output { get; set; }

        [Option('n', "namespace name", Required = false, HelpText = "Namespace of generated classes.")]
        public string NamespaceName { get; set; }

        [Option('c', Required = false, HelpText = "Root class name.")]
        public string RootClassName { get; set; }
    }
}
