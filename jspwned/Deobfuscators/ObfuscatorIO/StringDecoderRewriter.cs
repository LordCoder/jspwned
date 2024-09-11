using Esprima.Ast;
using Esprima.Utils;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScript_Deobfuscator_TFG.Deobfuscators.ObfuscatorIO
{
    public class StringDecoderRewriter : AstRewriter
    {
        private V8ScriptEngine _engine;
        private string _stringDecoderFunction;
        private List<string> _strObfuscatorIdentifiers;
        private string _jsCodeStringDecoder;
        public StringDecoderRewriter(string stringDecoderFunction, string jsCodeStringDecoder, List<string> globalStrObfuscatorIdentifiers)
        {
            _engine = new V8ScriptEngine();
            _strObfuscatorIdentifiers = globalStrObfuscatorIdentifiers.ToList();
            _stringDecoderFunction = stringDecoderFunction;
            _jsCodeStringDecoder = jsCodeStringDecoder;

        }

        protected override object VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            foreach (var varDeclarations in variableDeclaration.Declarations)
            {
                if (varDeclarations.Init != null && varDeclarations.Init.Type == Nodes.Identifier)
                {
                    Identifier id = (Identifier)varDeclarations.Id;
                    Identifier init = (Identifier)varDeclarations.Init;
                    if (init.Name.Equals(_stringDecoderFunction) || _strObfuscatorIdentifiers.Contains(init.Name))
                    {
                        Console.WriteLine("---> Encontrada Referencia a String Decoder: " + id.Name);
                        _strObfuscatorIdentifiers.Add(id.Name);
                    }
                }

            }
            return base.VisitVariableDeclaration(variableDeclaration);
        }
        protected override object VisitCallExpression(CallExpression callExpression)
        {
            if (callExpression.Callee is Identifier callee)
            {
                if (_strObfuscatorIdentifiers.Contains(callee.Name))
                {
                    String obfuscatedStrCall = callExpression.ToJavaScriptString();
                    foreach(String obf in _strObfuscatorIdentifiers)
                    {
                        obfuscatedStrCall = obfuscatedStrCall.Replace(obf, _stringDecoderFunction);
                    }
                    String deobfuscatedStr = DecodeString(obfuscatedStrCall);
                    if(deobfuscatedStr != null)
                    {
                        return new Literal("\"" + DecodeString(obfuscatedStrCall) + "\"");
                    }
                    
                }
            }

            return base.VisitCallExpression(callExpression);
        }

        private String DecodeString(String call)
        {
            return _engine.Evaluate(_jsCodeStringDecoder + " " + call).ToString();
        }

        public String[] GetStrObfuscatorIdentifiers()
        {
            return _strObfuscatorIdentifiers.ToArray();
        }
        
    }
}
