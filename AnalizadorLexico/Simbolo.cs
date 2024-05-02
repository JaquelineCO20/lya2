using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalizadorLexico
{
    class Simbolo
    {
		private string _strLexema;

		public string Lexema
		{
			get { return _strLexema; }
			set { _strLexema = value; }
		}

		private string _strToken;

		public string Token
		{
			get { return _strToken; }
			set { _strToken = value; }
		}

		private string _strTipo;

		public string Tipo
		{
			get { return _strTipo; }
			set { _strTipo = value; }
		}

	}
}
