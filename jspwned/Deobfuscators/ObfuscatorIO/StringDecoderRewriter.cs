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
        private string _StringDecoderFunction;
        private List<string> _StrObfuscatorIdentifiers;
        private string _JsCodeStringDecoder;
        public StringDecoderRewriter(string StringDecoderFunction, string JsCodeStringDecoder, string[] GlobalStrObfuscatorIdentifiers)
        {
            _engine = new V8ScriptEngine();
            _StrObfuscatorIdentifiers = [.. GlobalStrObfuscatorIdentifiers];
            _StringDecoderFunction = StringDecoderFunction;
            _JsCodeStringDecoder = JsCodeStringDecoder;

        }

        protected override object VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            foreach (var varDeclarations in variableDeclaration.Declarations)
            {
                if (varDeclarations.Init.Type == Nodes.Identifier)
                {
                    Identifier id = (Identifier)varDeclarations.Id;
                    Identifier init = (Identifier)varDeclarations.Init;
                    if (init.Name.Equals(_StringDecoderFunction) || _StrObfuscatorIdentifiers.Contains(init.Name))
                    {
                        Console.WriteLine("---> Found String Decryptor: " + id.Name);
                        _StrObfuscatorIdentifiers.Add(id.Name);
                    }
                }

            }
            return base.VisitVariableDeclaration(variableDeclaration);
        }
        protected override object VisitCallExpression(CallExpression callExpression)
        {
            if (callExpression.Callee is Identifier callee)
            {
                if (_StrObfuscatorIdentifiers.Contains(callee.Name))
                {
                    String obfuscatedStrCall = callExpression.ToJavaScriptString();
                    foreach(String obf in _StrObfuscatorIdentifiers)
                    {
                        obfuscatedStrCall = obfuscatedStrCall.Replace(obf, _StringDecoderFunction);
                    }
                    String? deobfuscatedStr = DecodeString(obfuscatedStrCall);
                    if(deobfuscatedStr != null)
                    {
                        return new Literal("\"" + DecodeString(obfuscatedStrCall) + "\"");
                    }
                    
                }
            }

            return base.VisitCallExpression(callExpression);
        }

        private String? DecodeString(String call)
        {
            return _engine.Evaluate(_JsCodeStringDecoder + " " + call).ToString();
        }

        public String[] GetStrObfuscatorIdentifiers()
        {
            return _StrObfuscatorIdentifiers.ToArray();
        }
        
    }
}
