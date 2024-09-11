using Esprima.Ast;
using Esprima.Utils;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace JavaScript_Deobfuscator_TFG.Deobfuscators.ObfuscatorIO
{
    public class SymbolRenamingRewriter : AstRewriter
    {
        private int _counter;
        private Dictionary<String, String> _references = new();

        public SymbolRenamingRewriter(int counter)
        {
            _counter = counter;
        }

        protected override object VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            List<VariableDeclarator> declarations = new();
            foreach (var declarator in variableDeclaration.Declarations)
            {
                Identifier id = (Identifier)declarator.Id;
                if (id.Name.StartsWith("_0x"))
                {
                    String newName = "variable" + _counter.ToString("D4");
                    Console.WriteLine("Encontrada variable protegida. Identificador: " + id.Name + ". Nuevo nombre: " + newName);
                    _counter++;
                    _references.Add(id.Name, newName);
                    declarations.Add(declarator.UpdateWith(new Identifier(newName), declarator.Init));
                }
                else
                {
                    declarations.Add(declarator);

                }
            }
            return variableDeclaration.UpdateWith(NodeList.Create(declarations));
        }
        protected override object VisitCallExpression(CallExpression callExpression)
        {

            return base.VisitCallExpression(callExpression);
        }
        public int GetCounter() {
            return _counter;
        }
        public Dictionary<String, String> GetReferences()
        {
            return _references;
        }

    }
}
