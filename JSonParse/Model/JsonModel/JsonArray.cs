﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSonParser.SDK
{
    public class JsonArray : JsonValue
    {
        public IEnumerable<JsonValue> Values { get; set; }
    }
}
