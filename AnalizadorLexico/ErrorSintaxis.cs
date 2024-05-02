using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalizadorLexico
{
    class ErrorSintaxis
    {
		private int intLinea;

		public int Linea
		{
			get { return intLinea; }
			set { intLinea = value; }
		}

		private string strDescripcion;

		public string Descripcion
		{
			get { return strDescripcion; }
			set { strDescripcion = value; }
		}

	}
}
