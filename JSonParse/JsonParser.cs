using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSonParser.SDK
{
    public class JsonParser
    {
        public const int TOKEN_NONE = 0;
        public const int TOKEN_CURLY_OPEN = 1;
        public const int TOKEN_CURLY_CLOSE = 2;
        public const int TOKEN_SQUARED_OPEN = 3;
        public const int TOKEN_SQUARED_CLOSE = 4;
        public const int TOKEN_COLON = 5;
        public const int TOKEN_COMMA = 6;
        public const int TOKEN_STRING = 7;
        public const int TOKEN_NUMBER = 8;
        public const int TOKEN_TRUE = 9;
        public const int TOKEN_FALSE = 10;
        public const int TOKEN_NULL = 11;

        private const int BUILDER_CAPACITY = 2000;

        public static JsonValue JsonDecode(string json)
        {
            bool success = true;

            return JsonDecode(json, ref success);
        }

        public static JsonValue JsonDecode(string json, ref bool success)
        {
            success = true;
            if (json != null)
            {
                char[] charArray = json.ToCharArray();
                int index = 0;
                JsonValue value = ParseValue(charArray, ref index, ref success);
                return value;
            }
            else
            {
                return null;
            }
        }

        private static JsonValue ParseValue(char[] json, ref int index, ref bool success)
        {
            int headResult = LookHead(json, index);
            switch (headResult)
            {
                case JsonParser.TOKEN_STRING:
                    return new JsonScalarValue<string>(ParseString(json, ref index, ref success));
                case JsonParser.TOKEN_NUMBER:
                    double number = ParseNumber(json, ref index, ref success);
                    if (number % 1 == 0)
                        return new JsonScalarValue<int>((int)number);
                    else
                        return new JsonScalarValue<double>(number);
                case JsonParser.TOKEN_CURLY_OPEN:
                    return ParseObject(json, ref index, ref success);
                case JsonParser.TOKEN_SQUARED_OPEN:
                    return ParseArray(json, ref index, ref success);
                case JsonParser.TOKEN_TRUE:
                    NextToken(json, ref index);
                    return new JsonScalarValue<bool>(true);
                case JsonParser.TOKEN_FALSE:
                    NextToken(json, ref index);
                    return new JsonScalarValue<bool>(true);
                case JsonParser.TOKEN_NULL:
                    NextToken(json, ref index);
                    return new JsonObject();
                case JsonParser.TOKEN_NONE:
                    break;
            }

            success = false;
            return null;
        }

        private static double ParseNumber(char[] json, ref int index, ref bool success)
        {
            EatWhitespace(json, ref index);

            int lastIndex = GetLastIndexOf(json, index);
            int charLength = (lastIndex - index) + 1;

            double number;
            success = double.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);

            index = lastIndex + 1;
            return number;
        }

        private static int GetLastIndexOf(char[] json, int index)
        {
            int lastIndex;

            for (lastIndex = index; lastIndex < json.Length; lastIndex++)
            {
                if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1)
                {
                    break;
                }
            }
            return lastIndex - 1;
        }

        private static JsonArray ParseArray(char[] json, ref int index, ref bool success)
        {
            JsonArray array = new JsonArray();
            List<JsonValue> values = new List<JsonValue>();
            array.Values = values;

            NextToken(json, ref index);

            bool done = false;
            while(!done)
            {
                int token = LookHead(json, index);
                if (token == JsonParser.TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if(token == JsonParser.TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if(token == JsonParser.TOKEN_SQUARED_CLOSE)
                {
                    NextToken(json, ref index);
                    break;
                }
                else
                {
                    JsonValue value = ParseValue(json, ref index, ref success);
                    if(!success)
                    {
                        return null;
                    }

                    values.Add(value);
                }
            }

            return array;
        }

        protected static JsonObject ParseObject(char[] json, ref int index, ref bool success)
        {
            JsonObject table = new JsonObject();
            List<JsonNode> children = new List<JsonNode>();
            table.Children = children;
            int token;

            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                token = LookHead(json, index);
                if (token == JsonParser.TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if (token == JsonParser.TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if (token == JsonParser.TOKEN_CURLY_CLOSE)
                {
                    NextToken(json, ref index);
                    return table;
                }
                else
                {

                    string name = ParseString(json, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    token = NextToken(json, ref index);
                    if (token != JsonParser.TOKEN_COLON)
                    {
                        success = false;
                        return null;
                    }

                    JsonValue value = ParseValue(json, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }
                    children.Add(new JsonNode() { Name = name, Value = value });
                }
            }
            return table;
        }

        protected static string ParseString(char[] json, ref int index, ref bool success)
        {
            StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
            char c;

            EatWhitespace(json, ref index);

            // "
            c = json[index++];

            bool complete = false;
            while (!complete)
            {

                if (index == json.Length)
                {
                    break;
                }

                c = json[index++];
                if (c == '"')
                {
                    complete = true;
                    break;
                }
                else if (c == '\\')
                {

                    if (index == json.Length)
                    {
                        break;
                    }
                    c = json[index++];
                    if (c == '"')
                    {
                        s.Append('"');
                    }
                    else if (c == '\\')
                    {
                        s.Append('\\');
                    }
                    else if (c == '/')
                    {
                        s.Append('/');
                    }
                    else if (c == 'b')
                    {
                        s.Append('\b');
                    }
                    else if (c == 'f')
                    {
                        s.Append('\f');
                    }
                    else if (c == 'n')
                    {
                        s.Append('\n');
                    }
                    else if (c == 'r')
                    {
                        s.Append('\r');
                    }
                    else if (c == 't')
                    {
                        s.Append('\t');
                    }
                    else if (c == 'u')
                    {
                        int remainingLength = json.Length - index;
                        if (remainingLength >= 4)
                        {
                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint;
                            if (!(success = UInt32.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint)))
                            {
                                return "";
                            }
                            // convert the integer codepoint to a unicode char and add to string
                            s.Append(Char.ConvertFromUtf32((int)codePoint));
                            // skip 4 chars
                            index += 4;
                        }
                        else
                        {
                            break;
                        }
                    }

                }
                else
                {
                    s.Append(c);
                }

            }

            if (!complete)
            {
                success = false;
                return null;
            }

            return s.ToString();
        }

        protected static int LookHead(char[] json, int index)
        {
            int saveIndex = index;
            return NextToken(json, ref saveIndex);
        }

        protected static int NextToken(char[] json, ref int index)
        {
            EatWhitespace(json, ref index);

            if (index == json.Length)
            {
                return JsonParser.TOKEN_NONE;
            }

            char c = json[index];
            index++;
            switch (c)
            {
                case '{':
                    return JsonParser.TOKEN_CURLY_OPEN;
                case '}':
                    return JsonParser.TOKEN_CURLY_CLOSE;
                case '[':
                    return JsonParser.TOKEN_SQUARED_OPEN;
                case ']':
                    return JsonParser.TOKEN_SQUARED_CLOSE;
                case ',':
                    return JsonParser.TOKEN_COMMA;
                case '"':
                    return JsonParser.TOKEN_STRING;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return JsonParser.TOKEN_NUMBER;
                case ':':
                    return JsonParser.TOKEN_COLON;
            }
            index--;

            int remainingLength = json.Length - index;

            //false
            if (remainingLength >= 5)
            {
                if (json[index] == 'f' &&
                    json[index + 1] == 'a' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 's' &&
                    json[index + 4] == 'e'
                    )
                {
                    index += 5;
                    return JsonParser.TOKEN_FALSE;
                }
            }

            //true
            if (remainingLength >= 4)
            {
                if (json[index] == 't' &&
                    json[index + 1] == 'r' &&
                    json[index + 2] == 'u' &&
                    json[index + 3] == 'e'
                    )
                {
                    index += 4;
                    return JsonParser.TOKEN_TRUE;
                }
            }

            //null
            if (remainingLength >= 4)
            {
                if (json[index] == 'n' &&
                    json[index + 1] == 'u' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 'l'
                    )
                {
                    index += 4;
                    return JsonParser.TOKEN_NULL;
                }
            }

            return JsonParser.TOKEN_NONE;
        }

        private static void EatWhitespace(char[] json, ref int index)
        {
            for (; index < json.Length; index++)
            {
                if (" \t\n\r".IndexOf(json[index]) == -1)
                {
                    break;
                }
            }
        }
    }
}
