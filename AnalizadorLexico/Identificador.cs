using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalizadorLexico
{
    class Identificador
    {
        private int numero;

        public int Numero
        {
            get { return numero; }
            set { numero = value; }
        }

        private string _Nombre;

        public string Nombre
        {
            get { return _Nombre; }
            set { _Nombre = value; }
        }

        private string _TipoDato;

        public string TipoDato
        {
            get { return _TipoDato; }
            set { _TipoDato = value; }
        }

    }
}
