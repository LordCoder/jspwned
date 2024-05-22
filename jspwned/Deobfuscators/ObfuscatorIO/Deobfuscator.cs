using Esprima.Ast;
using Esprima.Utils;
using JavaScript_Deobfuscator_TFG.Properties;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScript_Deobfuscator_TFG.Deobfuscators.ObfuscatorIO
{
    public static class Deobfuscator
    {

        public static NodeList<Statement> Deobfuscate(NodeList<Statement> AST)
        {
            StringDecoder decoderFunc = FindStringDecoderFunction(AST);
            NodeList<Statement> stringDeobfuscation = DeobfuscateStrings(AST, decoderFunc);
            return stringDeobfuscation;

        }

        public static bool IsObfuscatedWith(NodeList<Statement> AST)
        {
            return FindStringDecoderFunction(AST).Found;
        }

        private static NodeList<Statement> DeobfuscateStrings(NodeList<Statement> AST, StringDecoder decoderFunc)
        {
            List<Statement> statements = new();
            List<String> GlobalStrObfuscatorIdentifiers = [];
            foreach (Esprima.Ast.Node node in AST)
            {
                ExpressionStatement? exp = node as ExpressionStatement;
                if ((exp != null &&
                    exp.Expression is CallExpression callExpr &&
                    callExpr.Arguments.FirstOrDefault() is Identifier identifier &&
                    identifier.Name == decoderFunc.ArrayValuesFunction) ||
                    node is FunctionDeclaration fn && fn.Id != null && fn.Id.Name.Equals(decoderFunc))
                {
                    statements.Add(node as Statement);
                    continue;
                }
                var rewriter = new StringDecoderRewriter(decoderFunc.DecoderFunction, decoderFunc.JsCode, [.. GlobalStrObfuscatorIdentifiers]);
                statements.Add(rewriter.VisitAndConvert(node) as Statement);

                GlobalStrObfuscatorIdentifiers.AddRange(rewriter.GetStrObfuscatorIdentifiers());
                
                
            }
            return NodeList.Create(statements);
        }

        private static StringDecoder FindStringDecoderFunction(NodeList<Statement> AST)
        {
            StringDecoder CurrentStringDecoder = new() { Found = false };
            var schemaTask = JsonSchema.FromJsonAsync(Resources.ObfuscatorIOStringDecoderSchema);
            schemaTask.Wait();
            var schema = schemaTask.Result;

            StringBuilder functions = new();

            // 1. Búsqueda inicial de la función de decodificación de strings
            foreach (Statement statement in AST)
            {
                if (statement.Type == Esprima.Ast.Nodes.FunctionDeclaration)
                {
                        FunctionDeclaration funcDeclare = (FunctionDeclaration)statement;
                        if (!CurrentStringDecoder.Found)
                        {
                            String jsonFunc = funcDeclare.ToJsonString();
                            var errors = schema.Validate(jsonFunc);
                            if (errors.Count == 0)
                            {
                                // Buscamos la primera declaración de variable => Llamada al array de strings.
                                String ArrayValuesFunc = "";
                                VariableDeclaration? vd = funcDeclare.Body.Body
                                    .Where(st => st.Type == Esprima.Ast.Nodes.VariableDeclaration).FirstOrDefault() as VariableDeclaration;
                                if (vd != null)
                                {
                                    CallExpression? callToArrayFunc = vd.Declarations.FirstOrDefault().Init as CallExpression;
                                    var identifier = callToArrayFunc.Callee as Identifier;
                                    ArrayValuesFunc = identifier.Name;
                                    Console.WriteLine("Found String Decryptor: " + funcDeclare.Id.Name + " with Array: " + ArrayValuesFunc);
                                    CurrentStringDecoder.Found = true;
                                    CurrentStringDecoder.DecoderFunction = funcDeclare.Id.Name;
                                    CurrentStringDecoder.ArrayValuesFunction = ArrayValuesFunc;
                                    functions.AppendLine(funcDeclare.ToJavaScriptString());
                                }

                            }
                        }
                }
            }
            // 2. Si se ha encontrado la función principal, buscar ahora:
            // - Función ArrayValues
            // - Función Shuffler

            if (CurrentStringDecoder.Found)
            {
                int functionsFound = 0;
                foreach (Statement statement in AST)
                {
                    switch (statement.Type)
                    {
                        case Nodes.FunctionDeclaration:
                            // Array Values Function
                            FunctionDeclaration funcDeclaration = (FunctionDeclaration)statement;
                            if (funcDeclaration.Id.Name.Equals(CurrentStringDecoder.ArrayValuesFunction))
                            {
                                functions.AppendLine(funcDeclaration.ToJavaScriptString());
                                functionsFound++;

                            }
                            break;
                        case Esprima.Ast.Nodes.ExpressionStatement:
                            // Shuffler
                            ExpressionStatement expDeclare = (ExpressionStatement)statement;
                            if (expDeclare.Expression is CallExpression callExpr)
                            {
                                if(callExpr.Arguments.Count == 2
                                    && callExpr.Arguments.FirstOrDefault() is Identifier identifier
                                    && identifier.Name.Equals(CurrentStringDecoder.ArrayValuesFunction))
                                {
                                    functions.AppendLine(expDeclare.ToJavaScriptString());
                                    functionsFound++;
                                } 
                            }
                            break;
                    }
                    if (functionsFound == 2)
                        break;

                }
            }
            
            CurrentStringDecoder.JsCode = functions.ToString();
            return CurrentStringDecoder;
        }

        private class StringDecoder
        {
            public bool Found;
            public String DecoderFunction = "";
            public String ArrayValuesFunction = "";
            public String JsCode = "";
        }

    }
}
