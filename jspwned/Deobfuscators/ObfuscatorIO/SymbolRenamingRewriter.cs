using Esprima.Ast;
using Esprima.Utils;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JavaScript_Deobfuscator_TFG.Deobfuscators.ObfuscatorIO
{
    public class SymbolRenamingRewriter : AstRewriter
    {
        private int _counterVariables;
        private int _counterFunctions;
        private Dictionary<String, String> _variablesReferences = new();
        private Dictionary<String, String> _functionsReferences = new();


        public SymbolRenamingRewriter(int counterVariables, int counterFunctions)
        {
            _counterVariables = counterVariables;
            _counterFunctions = counterFunctions;
        }

        protected override object VisitVariableDeclarator(VariableDeclarator variableDeclarator)
        {
            Identifier id = (Identifier)variableDeclarator.Id;
            if (id.Name.StartsWith("_0x"))
            {
                String newName = "variable" + _counterVariables.ToString("D4");
                Console.WriteLine("Encontrada variable con nombre protegido. Identificador: " + id.Name + ". Nuevo nombre: " + newName);
                _counterVariables++;
                _variablesReferences.Add(id.Name, newName);
                return variableDeclarator.UpdateWith(new Identifier(newName), variableDeclarator.Init);
            }
            return base.VisitVariableDeclarator(variableDeclarator);
        }
        protected override object VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            Identifier id = functionDeclaration.Id;
            if (id.Name.StartsWith("_0x"))
            {
                // Renombrar el identificador de la función
                String newNameFunction = "function" + _counterFunctions.ToString("D4");
                Console.WriteLine("Encontrada función con nombre protegido. Identificador: " + id.Name + ". Nuevo nombre: " + newNameFunction);
                _counterFunctions++;
                _functionsReferences.Add(id.Name, newNameFunction);
                // Renombrar los parámetros
                int counterParameters = 1;
                List<Node> newParametersIdentifiers = new();
                foreach (Node parameter in functionDeclaration.Params)
                {
                    Identifier parameterCast = parameter as Identifier;
                    if (parameterCast.Name.StartsWith("_0x"))
                    {
                        String newNameParam = "param" + counterParameters.ToString("D4");
                        Console.WriteLine("Encontrado parámetro con nombre protegido. Identificador: " + parameterCast.Name + ". Nuevo nombre: " + newNameParam);
                        _variablesReferences.Add(parameterCast.Name, newNameParam);

                        newParametersIdentifiers.Add(new Identifier(newNameParam));
                        counterParameters++;
                    }
                    else
                    {
                        newParametersIdentifiers.Add(parameterCast);
                    }
                }
                return base.VisitFunctionDeclaration(functionDeclaration.UpdateWith(new Identifier(newNameFunction), NodeList.Create(newParametersIdentifiers), functionDeclaration.Body));

            }
            return base.VisitFunctionDeclaration(functionDeclaration);
        }
        public int GetCounterVariables()
        {
            return _counterVariables;
        }
        public int GetCounterFunctions()
        {
            return _counterFunctions;
        }
        public Dictionary<String, String> GetVariablesReferences()
        {
            return _variablesReferences;
        }
        public Dictionary<String, String> GetFunctionsReferences()
        {
            return _functionsReferences;
        }

    }
}
