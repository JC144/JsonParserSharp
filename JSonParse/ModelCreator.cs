using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSonParser.SDK.Utils;

namespace JSonParser.SDK
{
    public class ModelCreator
    {
        public static List<CSharpFile> GetCSharpFiles(JsonObject jsonObject, string @namespace, string className)
        {
            List<CSharpFile> files = new List<CSharpFile>();
            StringBuilder currentFile = new StringBuilder();
            StringBuilder currentParser = new StringBuilder();

            string indentClass = "\t";
            string indentMtdsAndProps = indentClass + "\t";
            string indentBody = indentMtdsAndProps + "\t";
            string parserName = className + "Parser";

            //Declare Using
            currentParser.AppendLine("using Newtonsoft.Json;");
            currentParser.AppendLine("using Newtonsoft.Json.Linq;");

            //Declare namespace
            currentFile.Append("namespace ");//namespace My.Namespace {
            currentFile.AppendLine(@namespace);
            currentFile.AppendLine("{");
            currentParser.Append("namespace ");
            currentParser.AppendLine(@namespace);
            currentParser.AppendLine("{");

            //Declare class
            currentFile.Append(indentClass);//public class MyClass {
            currentFile.Append("public class ");
            currentFile.AppendLine(className);
            currentFile.Append(indentClass);
            currentFile.AppendLine("{");
            currentParser.Append(indentClass);
            currentParser.Append("public class "); //public class MyClassParser {
            currentParser.AppendLine(parserName);
            currentParser.Append(indentClass);
            currentParser.AppendLine("{");

            //Generate parser method declaration
            currentParser.Append(indentMtdsAndProps);//public static MyModelClass Parse(JToken jobject){
            currentParser.Append("public static ");
            currentParser.Append(className);
            currentParser.AppendLine(" Parse(JToken jobject)");
            currentParser.Append(indentMtdsAndProps);
            currentParser.AppendLine("{");
            //Model declaration and instanciation
            currentParser.Append(indentBody);
            currentParser.Append(className);//MyModel myObject = new MyModel();
            currentParser.Append(" myObject = new ");
            currentParser.Append(className);
            currentParser.AppendLine("();");

            if (jsonObject.Children != null)
                foreach (JsonNode node in jsonObject.Children)
                {
                    string type = GetNodeType(files, node, @namespace);
                    string propertyName = node.Name.FirstLetterUpper();
                    //check of nullity
                    string newtonsoftObject = "jobject[\"" + node.Name + "\"]";
                    currentParser.Append(indentBody);
                    currentParser.Append("if("); //if(jobject["fieldName"]!= null){
                    currentParser.Append(newtonsoftObject);
                    currentParser.AppendLine("!=null){");

                    //It's a tab, we had a loop
                    if (node.Value is JsonArray)
                    {
                        JsonArray array = node.Value as JsonArray;
                        currentParser.Append(indentBody);
                        currentParser.AppendFormat("System.Collections.Generic.List<{0}> items = new System.Collections.Generic.List<{0}>();", type);
                        currentParser.AppendLine();
                        currentParser.Append(indentBody);
                        currentParser.Append("foreach (var item in "); //foreach(var item in jsonobject["items"]){
                        currentParser.Append(newtonsoftObject);
                        currentParser.AppendLine("){");
                        //{
                        currentParser.Append(indentBody);
                        currentParser.AppendFormat("items.Add(");

                        //if it's scalar (int, string, decimal,....)
                        if (array.Values.Any() && array.Values.First() is JsonScalarValue)
                        {
                            JsonScalarValue scalarValue = (JsonScalarValue)(((JsonArray)node.Value).Values.First());
                            //we cast value to target type
                            currentParser.Append("(");//(string)item;
                            currentParser.Append(scalarValue.Type.ToString());
                            currentParser.Append(")");
                            currentParser.Append("item");
                        }
                        else if (!array.Values.Any())//Empty array we parse to object, the generated code will not crash when array will not be empty
                        {
                            currentParser.Append("(");//(object)item;
                            currentParser.Append("object");
                            currentParser.Append(")");
                            currentParser.Append("item");
                        }
                        else
                        {
                            currentParser.Append(type);//myObject.Property = MyClassParser.Parse(item);
                            currentParser.Append("Parser.Parse(");
                            //currentParser.Append(newtonsoftObject);
                            currentParser.Append("item)");
                        }
                        currentParser.AppendLine(");");

                        currentParser.Append(indentBody);//Close foreach curly
                        currentParser.AppendLine("}");
                        //}
                        //myObject.Items = items.ToArray();
                        currentParser.Append(indentBody);
                        currentParser.Append("myObject.");
                        currentParser.Append(propertyName);
                        currentParser.Append(" = items.ToArray();");
                    }
                    else
                    {
                        currentParser.Append(indentBody);
                        currentParser.Append("myObject.");
                        currentParser.Append(propertyName);
                        currentParser.Append(" = ");
                        //if it's scalar (int, string, decimal,....)
                        if (node.Value is JsonScalarValue)
                        {
                            //we cast value to target type
                            currentParser.Append("(");//myObject.Property = (string)jobject["fieldName"];
                            currentParser.Append(((JsonScalarValue)node.Value).Type.ToString());
                            currentParser.Append(")");
                            currentParser.Append(newtonsoftObject);
                            currentParser.AppendLine(";");
                        }
                        else
                        {
                            currentParser.Append(type);//myObject.Property = MyClassParser.Parse(jobject["fieldName"]);h
                            currentParser.Append("Parser.Parse(");
                            currentParser.Append(newtonsoftObject);
                            currentParser.AppendLine(");");
                        }
                    }

                    //ending check of nullity
                    currentParser.Append(indentBody);
                    currentParser.AppendLine("}");

                    //Adding property declaration
                    currentFile.AppendFormat("{3}public {0} {1} {{get;set;}}{2}", type + (node.Value is JsonArray ? "[]" : string.Empty), propertyName, Environment.NewLine, indentMtdsAndProps);
                }

            //close parser method body
            currentParser.Append(indentBody);
            currentParser.AppendLine("return myObject;");
            currentParser.Append(indentMtdsAndProps);
            currentParser.AppendLine("}");

            //close class
            currentFile.Append(indentClass);
            currentFile.AppendLine("}");
            currentParser.Append(indentClass);
            currentParser.AppendLine("}");

            //close namespace
            currentFile.AppendLine("}");
            currentParser.AppendLine("}");


            files.Add(new CSharpFile() { Name = className + ".cs", Content = currentFile.ToString() });
            files.Add(new CSharpFile() { Name = parserName + ".cs", Content = currentParser.ToString() });

            return files;
        }

        private static string GetNodeType(List<CSharpFile> files, JsonNode node, string @namespace)
        {
            string type = string.Empty;
            if (node.Value is JsonArray)
            {
                JsonArray array = node.Value as JsonArray;
                type = "object"; //if we don't enumerate, we keep object as default type

                foreach (JsonValue arrayValue in array.Values)
                {
                    type = GetNodeType(files, new JsonNode() { Name = node.Name, Value = arrayValue }, @namespace);

                    break;
                }
            }
            else if (node.Value is JsonObject)
            {
                type = node.Name.FirstLetterUpper();
                files.AddRange(GetCSharpFiles(node.Value as JsonObject, @namespace, type));
            }
            else if (node.Value is JsonScalarValue)
            {
                type = (node.Value as JsonScalarValue).Type.ToString();
            }
            return type;
        }

        public static string GetNodeType(JsonNode node)
        {
            string type = string.Empty;
            if (node.Value is JsonArray)
            {
                JsonArray array = node.Value as JsonArray;
                type = "object"; //if we don't enumerate, we keep object as default type

                foreach (JsonValue arrayValue in array.Values)
                {
                    string arrayClassType = node.Name;
                    if (node.Name.ToLower().EndsWith("s"))
                    {
                        arrayClassType = node.Name.Remove(node.Name.Length - 1);
                    }

                    type = GetNodeType(new JsonNode() { Name = arrayClassType, Value = arrayValue });

                    break;
                }
            }
            else if (node.Value is JsonObject)
            {
                type = node.Name.FirstLetterUpper();                
            }
            else if (node.Value is JsonScalarValue)
            {
                type = (node.Value as JsonScalarValue).Type.ToString();
            }
            return type;
        }
    }
}
