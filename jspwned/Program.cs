using Esprima;
using Esprima.Ast;
using Esprima.Utils;

namespace JavaScript_Deobfuscator_TFG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("Se debe proporcionar un fichero en los argumentos.");
                return;
            }
            String obfuscatedJs = System.IO.File.ReadAllText(args[0]);
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(obfuscatedJs);
            String s = program.ToJsonString();
            NodeList<Statement> programBody = program.Body;

            if (Deobfuscators.ObfuscatorIO.Deobfuscator.IsObfuscatedWith(programBody))
            {
                var newBody = Deobfuscators.ObfuscatorIO.Deobfuscator.Deobfuscate(program.Body);
                var newCode = program.UpdateWith(newBody);
                Console.WriteLine("------- CODIGO DEOBFUSCADO -------");
                Console.WriteLine(newCode.ToJavaScriptString(true));
            }
            else
            {
                Console.WriteLine("El script proporcionado no ha sido protegido con Obfuscator.io");
            }
            
            Console.ReadLine();
        }

    }
}
