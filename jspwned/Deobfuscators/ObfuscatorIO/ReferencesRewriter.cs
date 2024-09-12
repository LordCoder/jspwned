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
    public class ReferencesRewriter : AstRewriter
    {
        private Dictionary<String, String> _variablesReferences;
        private Dictionary<String, String> _functionsReferences;

        public ReferencesRewriter(Dictionary<String, String> variablesReferences, Dictionary<String, String> functionsReferences)
        {
            _variablesReferences = variablesReferences;
            _functionsReferences = functionsReferences;
        }

        protected override object VisitIdentifier(Identifier identifier)
        {
            if (_variablesReferences.ContainsKey(identifier.Name))
            {
                return new Identifier(_variablesReferences[identifier.Name]);
            }
            if (_functionsReferences.ContainsKey(identifier.Name))
            {
                return new Identifier(_functionsReferences[identifier.Name]);
            }
            return base.VisitIdentifier(identifier);
        }

    }
}
