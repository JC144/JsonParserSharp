# JsonParserSharp
## History
When you are building an app to consume a REST service you often need to recreate your model in C# to parse the json received. It's laborious and not really interesting… 
That's why we created this JSonParser. It takes a json file as input and create C# classes to represent the model and some parser classes.
For the long term, we would like to add other features like the possibility to jump some nodes.

## Solutions Projects
### JSonParser.SDK
The library in charge of the parsing. Read the json files and infer C# types and properties name.
It can Output model classes or parser classes (//A expliquer)

### JSonParser.Console
A console project that takes json input and creates C# classes. You can use following commands :
-i <path_json_file> : Json file input
-o <path_output_folder> : Folder where will be saved C# classes (default json folder)

### JSonParser.WPF
A simple GUI in WPF to manage your json and output code. Easy to use, not a lot to explain :)

### JSonParser.T4
Sometimes, you want more than just a basic class. This T4 project allow customizations of the output. For example, if you want to add a RaisePropertyChanged or some special attributes, you can use this T4 and add your customizations.
