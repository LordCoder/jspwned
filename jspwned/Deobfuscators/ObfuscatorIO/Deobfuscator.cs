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
    public class Deobfuscator: IDeobfuscator
    {

        public static NodeList<Statement> Deobfuscate(NodeList<Statement> AST)
        {
            Console.WriteLine("---- Ejecutando String Decoding ----");
            StringDecoder decoderFunc = FindStringDecoderFunction(AST, false);
            NodeList<Statement> stringDeobfuscation = DeobfuscateStrings(AST, decoderFunc);
            Console.WriteLine("---- Ejecutando Symbol Renaming ----");
            NodeList<Statement> symbolRenaming = DeobfuscateSymbols(stringDeobfuscation);

            return symbolRenaming;
        }

        public static bool IsObfuscatedWith(NodeList<Statement> AST)
        {
            return FindStringDecoderFunction(AST, true).Found;
        }

        private static NodeList<Statement> DeobfuscateSymbols(NodeList<Statement> AST)
        {
            List<Statement> statements = new();
            int counter = 1;
            Dictionary<String, String> references = new();

            foreach (Esprima.Ast.Node node in AST)
            {
                // Renombrar los símbolos
                var rewriter = new SymbolRenamingRewriter(counter);
                var statement = rewriter.VisitAndConvert(node, true, null) as Statement;
                counter = rewriter.GetCounter();
                references = rewriter.GetReferences();
                // Actualizar las referencias
                var rewriter2 = new ReferencesRewriter(references);
                statements.Add(rewriter2.VisitAndConvert(statement, true, null) as Statement);
            }

            return NodeList.Create(statements);
        }
        //private static NodeList<Statement> FixReferences(NodeList<Statement> AST)
        //{
        //    List<Statement> statements = new();
        //    int counter = 1;
        //    foreach (Esprima.Ast.Node node in AST)
        //    {

        //        var rewriter = new SymbolRenamingRewriter(counter);
        //        statements.Add(rewriter.VisitAndConvert(node, true, null) as Statement);
        //    }
        //    return NodeList.Create(statements);
        //}

        private static NodeList<Statement> DeobfuscateStrings(NodeList<Statement> AST, StringDecoder decoderFunc)
        {
            List<Statement> statements = new();
            List<String> globalStrObfuscatorIdentifiers = [];
            foreach (Esprima.Ast.Node node in AST)
            {

                ExpressionStatement exp = node as ExpressionStatement;
                if ((exp != null &&
                    exp.Expression is CallExpression callExpr &&
                    callExpr.Arguments.FirstOrDefault() is Identifier identifier &&
                    identifier.Name == decoderFunc.ArrayValuesFunction) ||
                    node is FunctionDeclaration fn && fn.Id != null && fn.Id.Name.Equals(decoderFunc))
                {
                    statements.Add(node as Statement);
                    continue;
                }
                var rewriter = new StringDecoderRewriter(decoderFunc.DecoderFunction, decoderFunc.JsCode, globalStrObfuscatorIdentifiers);
                statements.Add(rewriter.VisitAndConvert(node, true, null) as Statement);

                globalStrObfuscatorIdentifiers.AddRange(rewriter.GetStrObfuscatorIdentifiers());
               
            }
            return NodeList.Create(statements);
        }

        private static StringDecoder FindStringDecoderFunction(NodeList<Statement> AST, bool isDetectionOnly)
        {
            StringDecoder currentStringDecoder = new() { Found = false };
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
                        if (!currentStringDecoder.Found)
                        {
                            String jsonFunc = funcDeclare.ToJsonString();
                            var errors = schema.Validate(jsonFunc);
                            if (errors.Count == 0)
                            {
                                // Buscamos la primera declaración de variable => Llamada al array de strings.
                                String ArrayValuesFunc = "";
                                VariableDeclaration vd = funcDeclare.Body.Body
                                    .Where(st => st.Type == Esprima.Ast.Nodes.VariableDeclaration).FirstOrDefault() as VariableDeclaration;
                                if (vd != null)
                                {

                                    CallExpression callToArrayFunc = vd.Declarations.FirstOrDefault().Init as CallExpression;
                                    Identifier identifier = callToArrayFunc.Callee as Identifier;
                                    ArrayValuesFunc = identifier.Name;
                                    Console.WriteLine("Encontrado String Decryptor: " + funcDeclare.Id.Name + " con Array: " + ArrayValuesFunc);
                                    currentStringDecoder.Found = true;
                                    currentStringDecoder.DecoderFunction = funcDeclare.Id.Name;
                                    currentStringDecoder.ArrayValuesFunction = ArrayValuesFunc;
                                    functions.AppendLine(funcDeclare.ToJavaScriptString());
                                }

                            }
                        }
                }
            }
            if (isDetectionOnly)
            {
                return currentStringDecoder;
            }
            // 2. Si se ha encontrado la función principal, buscar ahora:
            // - Función ArrayValues
            // - Función Shuffler

            if (currentStringDecoder.Found)
            {
                int functionsFound = 0;
                foreach (Statement statement in AST)
                {
                    switch (statement.Type)
                    {
                        case Nodes.FunctionDeclaration:
                            // Array Values Function
                            FunctionDeclaration funcDeclaration = (FunctionDeclaration)statement;
                            if (funcDeclaration.Id.Name.Equals(currentStringDecoder.ArrayValuesFunction))
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
                                    && identifier.Name.Equals(currentStringDecoder.ArrayValuesFunction))
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
            
            currentStringDecoder.JsCode = functions.ToString();
            return currentStringDecoder;
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
