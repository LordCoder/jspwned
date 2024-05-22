using Esprima.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScript_Deobfuscator_TFG
{
    public interface IDeobfuscator
    {
        Node Deobfuscate(NodeList<Statement> ast);
    }
}
