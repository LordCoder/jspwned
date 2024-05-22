using Esprima;
using Esprima.Ast;
using Esprima.Utils;
using JavaScript_Deobfuscator_TFG.Deobfuscators;
using NJsonSchema;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace JavaScript_Deobfuscator_TFG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            String obfuscatedJs = System.IO.File.ReadAllText("obfuscatedTFG.txt");
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(obfuscatedJs);
            var newBody = Deobfuscators.ObfuscatorIO.Deobfuscator.Deobfuscate(program.Body);
            Console.WriteLine(program.UpdateWith(newBody).ToJavaScriptString(true));
        }

    }
}
