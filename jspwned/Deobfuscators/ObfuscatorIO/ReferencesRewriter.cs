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
        private Dictionary<String, String> _references;

        public ReferencesRewriter(Dictionary<String, String> references)
        {
            _references = references;
        }

        protected override object VisitIdentifier(Identifier identifier)
        {
            if (_references.ContainsKey(identifier.Name))
            {
                return new Identifier(_references[identifier.Name]);
            }
            return base.VisitIdentifier(identifier);
        }

    }
}
