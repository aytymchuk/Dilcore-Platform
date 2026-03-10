using System;
using System.Text.RegularExpressions;

var words = new Regex("[^a-zA-Z0-9]+").Split("camelCase");
Console.WriteLine(words[0].ToLowerInvariant());
