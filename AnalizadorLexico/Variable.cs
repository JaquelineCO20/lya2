using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalizadorLexico
{
    class Variable<Tipo> : IEquatable<Variable<Tipo>>
    {
        private string _nombreVariable;

        public string NombreVariable
        {
            get { return _nombreVariable; }
            set { _nombreVariable = value; }
        }

        private string _tipoVariable;

        public string TipoVariable
        {
            get { return _tipoVariable; }
            set { _tipoVariable = value; }
        }

        private Tipo _contenidoVariable;

        public Tipo ContenidoVariable
        {
            get { return _contenidoVariable; }
            set { _contenidoVariable = value; }
        }

        public bool Equals(Variable<Tipo> other)
        {
            return this.NombreVariable == other.NombreVariable;
        }
    }
}
