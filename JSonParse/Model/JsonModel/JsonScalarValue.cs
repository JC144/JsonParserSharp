using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSonParser.SDK
{
    public class JsonScalarValue<T>: JsonScalarValue
    {
        public override Type Type { get { return typeof(T); } }

        public T Value { get; set; }

        public JsonScalarValue(T value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public abstract class JsonScalarValue : JsonValue
    {
        public abstract Type Type { get; }
    }
}
