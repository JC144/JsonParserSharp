using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSonParser.SDK
{
    public class JsonNode
    {
        public string Name { get; set; }

        public JsonValue Value { get; set; }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Ignore only the leaf but doesn't cut the whole branch
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Fully ignore the leaf and the descending branch
        /// </summary>
        public bool Cut { get; set; }
    }
}
