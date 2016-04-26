using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSonParser.CLI.Templates
{
    public class GeneratorContext
    {
        public String Output { get; set; }
        public String JsonPath { get; set; }
        public String NamespaceName { get; set; }
        public String RootClassName { get; set; }

        public GeneratorContext(string output, string jsonPath, string namespaceName = "Namespace", string rootClassName = "RootClass")
        {
            Output = output;
            JsonPath = jsonPath;
            NamespaceName = namespaceName;
            RootClassName = rootClassName;

            if (String.IsNullOrEmpty(NamespaceName))
            {
                NamespaceName = "Namespace";
            }
            if (String.IsNullOrEmpty(RootClassName))
            {
                RootClassName = "RootClass";
            }
            if (String.IsNullOrEmpty(Output))
            {
                Output = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            }

        }
    }
    public partial class Generator
    {
        private GeneratorContext _context;

        public Generator(GeneratorContext generatorContext)
        {
            _context = generatorContext;
        }
    }
}
