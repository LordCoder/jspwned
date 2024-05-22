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
    public class SymbolRenamingRewriter : AstRewriter
    {
        public SymbolRenamingRewriter()
        {

        }

        protected override object VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {

            return base.VisitVariableDeclaration(variableDeclaration);
        }
        protected override object VisitCallExpression(CallExpression callExpression)
        {


            return base.VisitCallExpression(callExpression);
        }        
    }
}
