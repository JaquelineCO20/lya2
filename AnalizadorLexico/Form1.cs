using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Text;
using System.Drawing.Drawing2D;

namespace AnalizadorLexico
{
    public partial class Form1 : Form
    {
        List<string> TriploLR = new List<string>();
        List<string> TriploAritmetico = new List<string>();
        List<Ensamblador> TriploEnsamblador = new List<Ensamblador>();
        List<Datos> listaDatos = new List<Datos>();
        string Cadena = "";
        string Inicio = "";
        string Op = "";
        string Global = "";
        string VLR = "";
        string LRaux = "";
        string CondicionV;
        string CondicionF;
        string CondicionHacer;
        bool ExisteLR = false;
        bool ExisteDoWhile = false;
        bool ExisteLeer = false;
        string Imprimir = "";
        string IM1 = "";
        string IM2 = "";
        string IM3 = "";
        string Salida;
        int valorB = 0;
        int valorC = 0;
        #region Globales
        int posicion = 0;
            int Lineas = 1;
            string CadenaAux = "";
            int TotalErrores;
            List<String> lstErrores = new List<String>();
            // string[] Reservadas = new string[] { "ERROR PALABRA RESERVADA NO VALIDA", "ERROR DE IDENTIFICADOR VALIDO", "ERROR DE SINTAXIS" };
            // List<Identificador> lstIdentificadores = new List<Identificador>();
            // lstIdentificadores.Clear();
            DataTable dtMatriz = new DataTable();        
            SaveFileDialog guardar = new SaveFileDialog();
            string CodigoFuente = "", strTipo;

            //AGO DIC 2022 LYA 2
            List<Simbolo> ListaSimbolos = new List<Simbolo>();//Lista para guardar simbolos en objetos
            List<ErrorSintaxis> ListaErroresSintaxis = new List<ErrorSintaxis>(); //Lista para guardar los errores de sintaxis
            List<String> ListaErroresSemantica = new List<String>();
            DataTable dtGramaticas = new DataTable(); //Datatable para gramaticas - Sintaxis
            DataTable dtReglasSemanticas = new DataTable(); //Datatable para reglas semanticas - Semantica
            DataTable dtGramaticasJELU = new DataTable(); //Datatable para gramaticas de JELU - Semantica
            Dictionary<string, string> dictGramaticas = new Dictionary<string, string>(); //Diccionario para gramaticas
            Dictionary<string, string> dictReglasSemanticas = new Dictionary<string, string>(); //Diccionario para reglas semanticas
            Dictionary<string, string> dictGramaticasJELU = new Dictionary<string, string>();//Diccionario para gramaticas JELU
            int intLineaErrorSintaxis;
            int intLineaErrorSemantica;
            List<String> lstSalidaJELU = new List<String>();
        #endregion

        #region Constructor Form
        public Form1()
        {
            InitializeComponent();
            SqlConnection cn = new SqlConnection("Server =DESKTOP-9GCKMTO\\SQLEXPRESS ; database = LYA2024; User ID = jaqueline; Password=Jaqueline20");
            cn.Open();
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Matriz", cn); //Final
            adapter.Fill(dtMatriz);

            SqlDataAdapter adapterGramaticas = new SqlDataAdapter("SELECT * FROM Gramaticas", cn); //Adapter para el select de las gramaticas
            adapterGramaticas.Fill(dtGramaticas);

            SqlDataAdapter adapterReglasSemanticas = new SqlDataAdapter("SELECT * FROM ReglasSemanticas", cn); //Adapter para el select de las reglas semanticas
            adapterReglasSemanticas.Fill(dtReglasSemanticas);

            SqlDataAdapter adapterGramaticasJELU = new SqlDataAdapter("SELECT * FROM GramaticasJELU", cn); //Adapter para el select de las gramaticas JELU
            adapterGramaticasJELU.Fill(dtGramaticasJELU);
            cn.Close();
        }
        #endregion

        #region Postfija/Prefija
        string Codificado;

        string SimplificarExpresion(string expresion)
        {
            
            // Realiza otros reemplazos según tus necesidades
            expresion = expresion.Replace("INICIO", "");
            // Elimina "ENTERO" seguido de una variable
            expresion = Regex.Replace(expresion, "ENTERO @\\w+", "");
            expresion = Regex.Replace(expresion, "REAL @\\w+", "");
            expresion = Regex.Replace(expresion, "@B =\\s*\\d+", "");
            expresion = Regex.Replace(expresion, "@C =\\s*\\d+", "");
            // Eliminar la parte de "IMPRIMIR" y el texto entre comillas
            expresion = Regex.Replace(expresion, "IMPRIMIR \"[^\"]+\"", "");
            expresion = Regex.Replace(expresion, "LEER @\\w+", "");
            expresion = expresion.Replace("FIN", "");

            // Reemplaza los caracteres '@' por una cadena vacía
            expresion = expresion.Replace("@", "");

            // Realiza una serie de reemplazos para simplificar la expresión matemática
            expresion = expresion.Replace("(", "");
            expresion = expresion.Replace(")", "");

            return expresion;
        }
        public void CadenaInfija()
        {
            string[] lineas = rtxFuente.Text.Split('\n');
            // Procesa cada línea y simplifica las expresiones
            string resultado = "";
            foreach (string linea in lineas)
            {
                if (!string.IsNullOrEmpty(linea))
                {
                    resultado += SimplificarExpresion(linea);
                }
            }
            txtExpresionInfija.Text = resultado;
            txtTokensInfijos.Text = Decodificador(txtExpresionInfija.Text);
        }
        static string Postfijo(string Entrada)
        {
            string s1 = null, s2 = null, s3 = null, aux1 = null;
            int largo = 0, parcial = 1, ll = 0, jc = 0, ja = 0, lo, laux = 0, largo2 = 0, ce1 = 1, conta = 0;
            char oo, carac2 = ' ';

            largo = Entrada.Length;
            while (parcial <= largo)
            {
                char carac;
                carac = Convert.ToChar(Entrada.Substring(parcial - 1, 1));

                switch (carac)
                {
                    case var _ when char.IsLetter(carac):
                    case var _ when char.IsDigit(carac):
                        s1 = s1 + carac;
                        break;
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '=':
                        if (ll == 0)
                        {
                            aux1 = aux1 + carac;
                            ll = 1;
                        }
                        else
                        {
                            lo = aux1.Length - 1;
                            //Aqui se debe tomar el ultimo caracter y no el primero
                            oo = Convert.ToChar(aux1.Substring(0, 1));
                            jc = carac;
                            ja = oo;
                            if (jc > ja)
                            {
                                aux1 = "";
                                aux1 = aux1 + carac + oo;
                            }
                            else
                            {
                                if (jc == ja)
                                {
                                    s1 = s1 + aux1.Substring(0, 1);
                                    laux = aux1.Length;
                                    aux1 = aux1.Substring(1, laux - 1);
                                    aux1 = carac + aux1;
                                }
                                else
                                {
                                    s1 = s1 + aux1;
                                    aux1 = Convert.ToString(carac);
                                }
                            }
                        }
                        break;
                    case '(':
                        largo2 = parcial;
                        while (ce1 != 0)
                        {
                            carac2 = Convert.ToChar(Entrada.Substring(largo2, 1));
                            s2 = s2 + carac2;
                            conta++;
                            largo2++;
                            switch (carac2)
                            {
                                case '(':
                                    ce1++;
                                    break;
                                case ')':
                                    ce1--;
                                    break;
                            }
                        }
                        s2 = s2.Substring(0, conta - 1);
                        s3 = Postfijo(s2);
                        s1 = s1 + s3;
                        s2 = null;
                        conta = 0;
                        ce1 = 1;
                        parcial = largo2;
                        break;
                }
                parcial++;
            }
            s1 = s1 + aux1;
            return s1;
        }
        public static string Decodificador(string C)
        {
            // Reemplazar letras por "IDEN"
            C = Regex.Replace(C, "[A-Za-z]", "IDEN");

            // Reemplazar los operadores y caracteres específicos
            C = C.Replace("+", "OA01");
            C = C.Replace("-", "OA02");
            C = C.Replace("*", "OA03");
            C = C.Replace("/", "OA04");
            C = C.Replace("=", "OPAS");

            // Reemplazar números enteros por "COEN"
            C = Regex.Replace(C, @"\b\d+\b", "COEN");

            // Reemplazar números decimales por "CORE"
            C = Regex.Replace(C, @"\b\d+\.\d+\b", "CORE");

            return C;
        }
        public void ObtenerValorBC()
        {
            // Expresión regular para encontrar los valores de @B y @C
            Regex regex = new Regex(@"@B\s*=\s*(\d+).*@C\s*=\s*(\d+)", RegexOptions.Singleline);

            // Coincidencias
            Match match = regex.Match(rtxFuente.Text);

            // Verificar si hay coincidencias
            if (match.Success)
            {
                // Obtener los valores de @B y @C
                int vB = int.Parse(match.Groups[1].Value);
                int vC = int.Parse(match.Groups[2].Value);

                // Hacer algo con los valores (en este caso, imprimirlos)
                valorB = vB;
                valorC = vC;
            }
        }
        #endregion
        public void QueSoy()
        {
            foreach (string linea in rtxFuente.Lines)
            {
                if (linea.Contains("HACER"))
                {
                    ExisteDoWhile = true;
                    break;
                }
            }
            if(ExisteDoWhile == false)
            {
                foreach (string linea in rtxFuente.Lines)
                {
                    if (linea.Contains("SI"))
                    {
                        ExisteLR = true;

                    }
                    if (linea.Contains("LEER"))
                    {
                        ExisteLeer = true; 
                    }
                }
            }
        }

        #region DO WHILE
        private string ObtenerCondicionWhile(string codigo)
        {
            // Utilizando expresiones regulares para extraer la condición entre MIENTRAS y FIN
            string patron = @"MIENTRAS\s+@?(.*?)\s+FIN";
            Match match = Regex.Match(codigo, patron, RegexOptions.Singleline);

            if (match.Success)
            {
                // Quita el carácter '@' de la condición
                string condicion = match.Groups[1].Value.TrimStart('@').Trim();
                return condicion;
            }
            else
            {
                return null;
            }
        }
        private string ObtenerContenidoEntreComillas(string texto)
        {
            // Utilizar expresiones regulares para encontrar el contenido entre comillas
            MatchCollection coincidencias = Regex.Matches(texto, "\"(.*?)\"");

            // Concatenar los resultados encontrados
            string resultado = "";
            foreach (Match coincidencia in coincidencias)
            {
                resultado += coincidencia.Groups[1].Value + Environment.NewLine;
            }

            return resultado;
        }
        private static string ObtenerContenidoEntreHacerYImprimir(string texto)
        {
            // Utilizar expresiones regulares para encontrar el contenido entre HACER e IMPRIMIR
            Match match = Regex.Match(texto, @"HACER(.*?)IMPRIMIR", RegexOptions.Singleline);

            if (match.Success)
            {
                // Obtener el contenido y quitar los arrobas de las variables
                string contenido = match.Groups[1].Value;
                contenido = Regex.Replace(contenido, @"@([A-Za-z_][A-Za-z0-9_]*)", "$1");

                // Eliminar espacios adicionales y retornar el resultado
                return contenido.Trim();
            }

            return string.Empty;
        }

        public void TransformaHacer(string Condicion)
        {
            Salida = "";
            for (int i = 0; i < Condicion.Length; i++)
            {
                if (i == 0 & char.IsLetter(Condicion[i]))
                {
                    if (Condicion[i + 2] == '=')
                    {
                        if (Condicion[i + 6] == '+' )
                        {
                            if (char.IsDigit(Condicion[i + 8]))
                            {
                                Salida = "ADD BX, " + Condicion[i + 8];
        }
                        }
                        if (Condicion[i + 6] == '-')
                        {
                            if (char.IsDigit(Condicion[i + 8]))
                            {
                                Salida = "SUB BX, " + Condicion[i + 8];
        }
                        }
                    }
                }
            }
        }
        #endregion

        #region Expresiones Aritmeticas
        private Datos ExtraerDatosDeCadena(string cadena)
        {
            string[] partes = cadena.Split(' ');
            if (partes.Length >= 3)
            {
                Datos datos = new Datos
                {
                    Columna1 = partes[0],
                    Columna2 = partes[1],
                    Columna3 = partes[2]
                };
                return datos;
            }

            return null;
        }
        public void AgregarAritmetico()
        {
            dtgALR.Rows.Clear();
            listaDatos.Clear();
            foreach (string cadena in TriploAritmetico)
            {
                Datos datos = ExtraerDatosDeCadena(cadena);
                if (datos != null)
                {
                    listaDatos.Add(datos);
                }
            }
            foreach (Datos d in listaDatos)
            {
                dtgALR.Rows.Add(d.Columna1, d.Columna2, d.Columna3);
            }
        }
        public void SepararAritmetico()
        {
            for (int i = 0; i < Global.Length; i++)
            {
                if (i + 2 < Global.Length && Global[i] == 'T' && Global[i + 1] == '1')
                {
                    if (char.IsDigit(Global[i + 3]))
                    {
                        if (i + 4 < Global.Length && char.IsDigit(Global[i + 4]))
                        {
                            TriploAritmetico.Add("T1" + " " + Global[i + 3] + Global[i + 4] + " " + Global[i + 6]);
                            i += 6; // Avanzar el índice para evitar procesar el mismo elemento nuevamente
                        }
                        else
                        {
                            TriploAritmetico.Add("T1" + " " + Global[i + 3] + " " + Global[i + 5]);
                            i += 5; // Avanzar el índice
                        }
                    }
                    else
                    {
                        TriploAritmetico.Add("T1" + " " + Global[i + 3] + " " + Global[i + 5]);
                        i += 5; // Avanzar el índice
                    }
                }
                else if (char.IsLetter(Global[i]) && i + 2 < Global.Length &&
                    Global[i + 2] == 'T' && Global[i + 3] == '1')
                {
                    TriploAritmetico.Add(Global[i] + " " + Global[i + 2] + Global[i + 3] + " " + Global[i + 5]);
                    i += 5; // Avanzar el índice
                }
            }
        }
        public void ExpresionAritmetica()
        {
            for (int i = 0; i < Cadena.Length; i++)
            {
                if (i == 0)
                {
                    Inicio = Cadena[i].ToString() + " T1 =";
                }
                if (Cadena[i] == '=')
                {
                    Op = "T1" + " " + Cadena[i + 2] + " " + Cadena[i];
                    Global = Global + Op + " ";
                }
                if (Cadena[i] == '+' || Cadena[i] == '-' || Cadena[i] == '*' || Cadena[i] == '/')
                {
                    if (char.IsDigit(Cadena[i + 2]))
                    {
                        if (i + 3 < Cadena.Length)
                        {
                            if (char.IsDigit(Cadena[i + 3]))
                            {
                                Op = "T1 " + Cadena[i + 2] + Cadena[i + 3] + " " + Cadena[i];
                            }
                            else
                            {
                                Op = "T1 " + Cadena[i + 2] + " " + Cadena[i];
                            }
                        }
                        else
                        {
                            Op = "T1 " + Cadena[i + 2] + " " + Cadena[i];
                        }
                    }
                    if (char.IsLetter(Cadena[i + 2]))
                    {
                        Op = "T1 " + Cadena[i + 2] + " " + Cadena[i];
                    }
                    Global = Global + Op + " ";
                }
            }
        }
        #endregion

        #region Expresiones Logicas y Relacionales
        private Datos ExtraerDatosLR(string cadena)
        {
            string[] partes = cadena.Split(' ');
            if (partes.Length >= 3)
            {
                Datos datos = new Datos
                {
                    Columna1 = partes[0],
                    Columna2 = partes[1],
                    Columna3 = partes[2]
                };
                return datos;
            }

            return null;
        }
        public void AgregarLR()
        {
            dtgALR.Rows.Clear();
            listaDatos.Clear();
            foreach (string cadena in TriploLR)
            {
                Datos datos = ExtraerDatosLR(cadena);

                if (datos != null)
                {
                    listaDatos.Add(datos);
                }
            }
            foreach (Datos d in listaDatos)
            {
                dtgALR.Rows.Add(d.Columna1, d.Columna2, d.Columna3);
            }
        }
        void ExpresionLR()
        {
            for (int i = 0; i < Cadena.Length; i++)
            {
                if (i == 0 & char.IsLetterOrDigit(Cadena[i]))
                {
                    VLR = "T1 " + Cadena[i] + " = ";
                    if (Cadena[i + 2] == '<' || Cadena[i + 2] == '>' || Cadena[i + 2] == '=')
                    {
                        if (Cadena[i + 3] == '=')
                        {
                            if (Cadena.Length == 6 || i + 7 == Cadena.Length)
                            {
                                LRaux = "T1 T2 " + Cadena[i + 2] + Cadena[i + 3];
                            }
                            else
                            {
                                LRaux = "T1 T2 " + Cadena[i + 2] + Cadena[i + 3] + " ";
                            }
                        }
                        else
                        {
                            if (Cadena.Length == 6 || i + 7 == Cadena.Length)
                            {
                                LRaux = "T1 T2 " + Cadena[i + 2];
                            }
                            else
                            {
                                LRaux = "T1 T2 " + Cadena[i + 2] + " ";
                            }
                        }
                    }
                }

                if (i == 4)
                {
                    if (i + 1 < Cadena.Length && char.IsDigit(Cadena[i + 1]))
                    {
                        VLR = VLR + "T2 " + Cadena[i] + Cadena[i + 1] + " = ";
                    }
                    else
                    {
                        VLR = VLR + "T2 " + Cadena[i] + " = ";
                    }
                }

                if (Cadena[i] == '&' || Cadena[i] == '|' || Cadena[i] == '!')
                {
                    if (char.IsDigit(Cadena[i + 6]))
                    {
                        if (char.IsDigit(Cadena[i + 7]))
                        {
                            LRaux = LRaux + "T3 " + Cadena[i + 6] + Cadena[i + 7] + " = ";
                        }
                        else
                        {
                            LRaux = LRaux + "T3 " + Cadena[i + 6] + " = ";
                        }
                    }
                    if (Cadena[i + 4] == '<' || Cadena[i + 4] == '>' || Cadena[i + 4] == '=')
                    {
                        if (Cadena[i + 5] == '=')
                        {
                            LRaux = LRaux + "T1 T3 " + Cadena[i + 4] + Cadena[i + 5];
                            break;
                        }
                        else
                        {
                            LRaux = LRaux + "T1 T3 " + Cadena[i + 4];
                            break;
                        }
                    }
                }
            }
        }
        void SepararLR()
        {
            for (int i = 0; i < VLR.Length; i++)
            {
                if (i == 0)
                {
                    TriploLR.Add("T1 " + VLR[i + 3] + " " + VLR[i + 5]);
                    i = 7;
                }
                if (i == 7)
                {
                    if (char.IsDigit(VLR[i + 3]))
                    {
                        if (char.IsDigit(VLR[i + 4]))
                        {
                            TriploLR.Add("T2 " + VLR[i + 3] + VLR[i + 4] + " " + VLR[i + 6]);
                            i = 15;
                        }
                        else
                        {
                            TriploLR.Add("T2 " + VLR[i + 3] + " " + VLR[i + 5]);
                            i = 15;
                        }
                    }
                }
                if (i == 15)
                {
                    if (VLR[i + 5] == '<' || VLR[i + 5] == '>' || VLR[i + 5] == '=')
                    {
                        TriploLR.Add("T1 T2 " + VLR[i + 5]);
                        if (VLR.Length == 22)
                        {
                            break;
                        }
                        else
                        {
                            i = 23;
                        }
                    }
                    if (VLR[i + 6] == '<' || VLR[i + 6] == '>' || VLR[i + 6] == '=')
                    {
                        if (i + 7 < VLR.Length)
                        {
                            if (VLR[i + 7] == '=')
                            {
                                TriploLR.Add("T1 T2 " + VLR[i + 6] + VLR[i + 7]);
                                i = 24;
                            }

                        }
                        else
                        {
                            TriploLR.Add("T1 T2 " + VLR[i + 6]);
                            if (VLR.Length == 22)
                            {
                                break;
                            }
                            else
                            {
                                i = 23;
                            }
                        }
                    }
                }
                if (i == 23)
                {
                    if (char.IsDigit(VLR[i + 3]))
                    {
                        if (char.IsDigit(VLR[i + 4]))
                        {
                            TriploLR.Add("T3 " + VLR[i + 3] + VLR[i + 4] + " " + VLR[i + 6]);
                            i = 31;
                        }
                        else
                        {
                            TriploLR.Add("T3 " + VLR[i + 3] + " " + VLR[i + 5]);
                            i = 31;
                        }
                    }
                }
                if (i == 31)
                {
                    if (VLR[i + 6] == '<' || VLR[i + 6] == '>' || VLR[i + 6] == '=')
                    {
                        if (i + 7 < VLR.Length)
                        {
                            TriploLR.Add("T1 T3 " + VLR[i + 6] + "=");
                            break;
                        }
                        else
                        {
                            TriploLR.Add("T1 T3 " + VLR[i + 6]);
                            break;
                        }
                    }
                }
            }
        }
        public void CadenaLR()
        {
            string[] lineas = rtxFuente.Lines;

            for (int i = 0; i < lineas.Length; i++)
            {
                if (lineas[i].Trim().StartsWith("SI", StringComparison.OrdinalIgnoreCase))
                {
                    int indiceEntonces = lineas[i].IndexOf("ENTONCES", StringComparison.OrdinalIgnoreCase);
                    if (indiceEntonces != -1)
                    {
                        string condicion = lineas[i].Substring(2, indiceEntonces - 2).Trim();

                        // Reemplazar arrobas
                        condicion = condicion.Replace("@", "");

                        txtExpresionLR.Text = condicion;
                        return;
                    }
                }
            }
        }
        private void ObtenerCondiciones()
        {
            string entrada = rtxFuente.Text;

            // Patrón de expresión regular para encontrar lo que está entre "ENTONCES" y "SINO"
            string patronEntoncesSino = @"ENTONCES\s+([^SINO]+)\s+SINO";

            // Patrón de expresión regular para encontrar lo que está entre "SINO" y "FINSI"
            string patronSinoFinsi = @"SINO\s+([^FINSI]+)\s+FINSI";

            // Función para quitar el símbolo "@" y los paréntesis de las variables encontradas en la expresión
            Func<string, string> quitarArrobaYParéntesis = condicion =>
                Regex.Replace(condicion, @"[@()]+", "");

            // Buscar coincidencias y extraer los valores
            Match matchEntoncesSino = Regex.Match(entrada, patronEntoncesSino);
            Match matchSinoFinsi = Regex.Match(entrada, patronSinoFinsi);

            if (matchEntoncesSino.Success)
            {
                string condicionV = quitarArrobaYParéntesis(matchEntoncesSino.Groups[1].Value.Trim());
                txtCondicionV.Text = condicionV;
            }
            else
            {
                txtCondicionV.Text = "NA";
            }

            if (matchSinoFinsi.Success)
            {
                string condicionF = quitarArrobaYParéntesis(matchSinoFinsi.Groups[1].Value.Trim());
                txtCondicionF.Text = condicionF;
            }
            else
            {
                txtCondicionF.Text = "NA";
            }
        }
        public void ImprimirLR()
        {
            string codigo = rtxFuente.Text;

            IM1 = ObtenerContenidoEntreComillasLR(ObtenerSeccion(codigo, "ENTERO @A", "LEER @A"));
            IM2 = ObtenerContenidoEntreComillasLR(ObtenerSeccion(codigo, "SI @A < 18 ENTONCES", "SINO"));
            IM3 = ObtenerContenidoEntreComillasLR(ObtenerSeccion(codigo, "SINO", "FINSI"));
        }

        private string ObtenerSeccion(string texto, string inicio, string fin)
        {
            int inicioIndex = texto.IndexOf(inicio);
            int finIndex = texto.IndexOf(fin, inicioIndex + inicio.Length);

            if (inicioIndex != -1 && finIndex != -1)
            {
                return texto.Substring(inicioIndex + inicio.Length, finIndex - inicioIndex - inicio.Length);
            }
            else
            {
                return "NA";
            }
        }

        private string ObtenerContenidoEntreComillasLR(string texto)
        {
            // Utilizar expresiones regulares para encontrar el contenido entre comillas
            MatchCollection coincidencias = Regex.Matches(texto, "\"(.*?)\"");

            // Concatenar los resultados encontrados
            string resultado = "";
            foreach (Match coincidencia in coincidencias)
            {
                resultado += coincidencia.Groups[1].Value + Environment.NewLine;
            }

            return resultado;
        }

        public void CondicionesEnsamblador(string Condicion)
        {
            Salida = "";
            for (int i = 0; i < Condicion.Length; i++)
            {
                if (i == 0 & char.IsLetter(Condicion[i]))
                {
                    if (i + 2 < Condicion.Length && Condicion[i + 2] == '=')
                    {
                        if (i + 4 < Condicion.Length && char.IsDigit(Condicion[i + 4]))
                        {
                            if (i + 5 < Condicion.Length && char.IsDigit(Condicion[i + 5]))
                            {
                                Salida = "MOV AX, " + Condicion[i + 4] + Condicion[i + 5];
                                break;
                            }
                            else
                            {
                                Salida = "MOV AX, " + Condicion[i + 4];
                                break;
                            }
                        }
                        else
                        {
                            if (i + 6 < Condicion.Length && Condicion[i + 6] == '+')
                            {
                                if (i + 8 < Condicion.Length && char.IsDigit(Condicion[i + 8]))
                                {
                                    if (i + 9 < Condicion.Length && char.IsDigit(Condicion[i + 9]))
                                    {
                                        Salida = "ADD AX, " + Condicion[i + 8] + Condicion[i + 9];
                                        break;
                                    }
                                    else
                                    {
                                        Salida = "ADD AX, " + Condicion[i + 8];
                                        break;
                                    }
                                }
                            }
                            if (i + 6 < Condicion.Length && Condicion[i + 6] == '-')
                            {
                                if (i + 8 < Condicion.Length && char.IsDigit(Condicion[i + 8]))
                                {
                                    if (i + 9 < Condicion.Length && char.IsDigit(Condicion[i + 9]))
                                    {
                                        Salida = "SUB AX, " + Condicion[i + 8] + Condicion[i + 9];
                                        break;
                                    }
                                    else
                                    {
                                        Salida = "SUB AX, " + Condicion[i + 8];
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Load
        private void Form1_Load(object sender, EventArgs e)
        {
            dgvErrores.Columns.Add("Linea", "Linea");
            dgvErrores.Columns.Add("Error", "Error");
            dgvErrores.Columns.Add("Tipo", "Tipo");
            dgvTablaSimbolos.Columns.Add("Lexema", "Lexema");
            dgvTablaSimbolos.Columns.Add("Token", "Token");
            dgvTablaSimbolos.Columns.Add("Tipo", "Tipo");
            dgvErroresSintaxis.Columns.Add("Error", "Error");//Para errores de sintaxis
            dgvErroresSemantica.Columns.Add("Error", "Error"); //Para errores de semantica
            dgvMatriz.DataSource = dtMatriz;
            dictGramaticas = ConvertirTablaEnDiccionario(dtGramaticas); //Se trae la tabla ya convertida en diccionario
            dictReglasSemanticas = ConvertirTablaEnDiccionario(dtReglasSemanticas);
            dictGramaticasJELU = ConvertirTablaEnDiccionario(dtGramaticasJELU);
            rtxLinea2.Font = rtxFuente.Font;//Numeros de linea
            rtxFuente.Select();
            AgregarNumerosDeLinea(rtxFuente, rtxLinea2);

            dtgALR.Columns.Add("Dato objeto", "Dato objeto");
            dtgALR.Columns.Add("Dato fuete", "Dato fuente");
            dtgALR.Columns.Add("Operador", "Operador");
            dtgALR.ReadOnly = true;
            dtgALR.AllowUserToAddRows = false;
            dtgALR.AllowUserToResizeColumns = false;
            dtgALR.AllowUserToResizeRows = false;
            dtgALR.AllowUserToDeleteRows = false;
            dtgALR.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dtgALR.MultiSelect = false;
            dtgALR.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dtgEnsamblador.Columns.Add("Variable", "Variable");
            dtgEnsamblador.Columns.Add("Dato 1", "Dato 1");
            dtgEnsamblador.Columns.Add("Dato 2", "Dato 2");
            dtgEnsamblador.ReadOnly = true;
            dtgEnsamblador.AllowUserToAddRows = false;
            dtgEnsamblador.AllowUserToResizeColumns = false;
            dtgEnsamblador.AllowUserToResizeRows = false;
            dtgEnsamblador.AllowUserToDeleteRows = false;
            dtgEnsamblador.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dtgEnsamblador.MultiSelect = false;
            dtgEnsamblador.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        #endregion

        #region Cargar Programa
        private void btnCargarPrograma_Click(object sender, EventArgs e)
        {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "(*.txt) | *.txt";
                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    CodigoFuente = fileDialog.FileName;
                    rtxFuente.LoadFile(CodigoFuente, RichTextBoxStreamType.PlainText);
                    int ln = rtxFuente.Lines.Count();

                    for (int i = 1; i <= ln; i++) {
                       // rtxLinea2.Text += Lineas + "\n";                  
                        //rtxLineas.Text += Lineas + "\n" ;
                       // Lineas++;
                    }
                }
            AgregarNumerosDeLinea(rtxFuente, rtxLinea2);

        }
        #endregion

        #region Guardar Programa
            private void GuardarPrograma_Click(object sender, EventArgs e) {
                guardar.Filter = "documento de texto|*.txt";
                guardar.Title = "GUARDAR";
                guardar.FileName = "Sin titulo";
                var resultado = guardar.ShowDialog();
                if (resultado == DialogResult.OK) {
                    StreamWriter escribir = new StreamWriter(guardar.FileName);
                    foreach (object line in rtxFuente.Lines)
                        escribir.WriteLine(line);
                    escribir.Close();
                }
            }
        #endregion

        #region Limpiar Pantallas
            private void btnLimpiar_Click(object sender, EventArgs e) {
                //rtxLineas.Clear();
                rtxLinea2.Clear();
                rtxToken.Clear();
                rtxFuente.Clear();
                rtxTokenSintaxis.Clear();
                rtxDerivaciones.Clear();
              //  rtxLineas.Text = " ";
                rtxLinea2.Text = " ";
                rtxToken.Text = " ";
                rtxFuente.Text = " ";
                rtxTokenSintaxis.Text = " ";
                rtxDerivaciones.Text = "";
                rtxDerivacionesSemantica.Text = "";
                rtxDerivacionesJELU.Text = "";
                rtxTokenSemantica.Text = "";
                rtxJELUVertical.Text = "";
                lblNumErrores.Text = "";
                
                dgvTablaSimbolos.Rows.Clear();
                ListaErroresSintaxis.Clear();
                ListaErroresSemantica.Clear();
                dgvErroresSintaxis.Rows.Clear();
                dgvErroresSemantica.Rows.Clear();
            }
        #endregion

        #region Guardar Tokens
            private void btnGuardarTokens_Click(object sender, EventArgs e)
            {
                guardar.Filter = "documento de texto|*.txt";
                guardar.Title = "GUARDAR";
                guardar.FileName = "Sin titulo";
                var resultado = guardar.ShowDialog();
                if (resultado == DialogResult.OK) {
                    StreamWriter escribir = new StreamWriter(guardar.FileName);
                    foreach (object line in rtxToken.Lines)
                        escribir.WriteLine(line);
                    escribir.Close();
                }
            }
        #endregion

        #region Analizador Léxico
        private void btnAnalizador_Click(object sender, EventArgs e)
        {
            TriploAritmetico.Clear();
            TriploEnsamblador.Clear();
            TriploLR.Clear();
            listaDatos.Clear();
            dtgALR.Rows.Clear();
            dtgEnsamblador.Rows.Clear();
            Cadena = "";
            Inicio = "";
            Op = "";
            Global = "";
            VLR = "";
            LRaux = "";
            CondicionV = "";
            CondicionF = "";
            CondicionHacer = "";
            ExisteLR = false;
            ExisteDoWhile = false;
            bool ExisteLeer = false;
            string Imprimir = "";
            Salida ="";
            IM1 = "";
            IM2 = "";
            IM3 = "";
            TotalErrores = 0;
            rtxToken.Text = "";
            dgvErrores.Rows.Clear();
            dgvTablaSimbolos.Rows.Clear();
            //arreglo obtiene el largo de las lineas y su longitud
            string[] ArregloFuente = new string[rtxFuente.Lines.Length];
            //guarda cada caracter del texto 
            for (int i = 0; i <= rtxFuente.Lines.Length - 1; i++) {
                ArregloFuente[i] = Blanco(rtxFuente.Lines[i]);
            }
            // * Inicia recorrido de las líneas del código
            for (int j = 0; j <= ArregloFuente.Length - 1; ++j) {
                int intEstado = 0;
                string Cadena = "";
                // * Foreach que permite hacer un recorrido para analizar la cadena
                foreach (char chrCaracter in ArregloFuente[j]) {
                    if (chrCaracter.ToString() != " ") {
                        // * DataSet --> Huecos en memoria que almacenan base de datos, similar a access
                        // * DataTable --> Es un elemento de DataSet.La info que llega aqui, se manda en automatico a DataSet
                        // * Un DataTable contiene un DataColumn y DataRows
                        // * DataColumn --> Columnas de DataTable
                        // * DataRows --> Filas de DataTable

                        // * El DataAdapter conduce datos de la base de datos al DataSet y viceversa. Además se puede abrir y cerrar
                        // * una conexión por sí solo
                        Cadena += chrCaracter.ToString();
                        foreach (DataColumn Columna in dtMatriz.Columns) {
                            //*Compara el caracter de arreglo con el caracter de la columna de la matriz
                            if (chrCaracter.ToString() == Columna.ColumnName) {
                                if (dtMatriz.Rows[intEstado][Columna].ToString() != "FDC") {
                                    String encabezado = Columna.ColumnName;
                                    intEstado = int.Parse(dtMatriz.Rows[intEstado][encabezado].ToString());
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if(intEstado == 0) {
                            break;
                        }
                        //Se obtiene el estado final
                        intEstado = int.Parse(dtMatriz.Rows[intEstado][dtMatriz.Columns.Count - 2].ToString());
                        string strToken = dtMatriz.Rows[intEstado][dtMatriz.Columns.Count - 1].ToString();
                        if(strToken=="ERROR") {
                            dgvErrores.Rows.Add(j + 1, strToken, "ERROR EN LA LÍNEA " + Lineas);
                            rtxToken.Text += strToken + " ";
                            TotalErrores++;
                            lblNumErrores.Text = TotalErrores.ToString();
                            lstErrores.Add(strToken);
                        }
                        else if(strToken=="PR02") {
                            strTipo = "ENTERO";
                            rtxToken.Text += strToken + " ";
                        }
                        else if(strToken=="PR08") {
                            strTipo = "REAL";
                            rtxToken.Text += strToken + " ";
                        }
                        else {
                            rtxToken.Text += strToken + " ";
                        }                       
                        /*LLENADO TABLA DE SÍMBOLOS
                        AQUI ENTRARA EN CASO DE SER UN IDENTIFICADOR, SE ASEGURARA MEDIANTE EL USO DE LA CADENAAUX QUE CONTENDRA EL
                        TIPO DE DATO
                        ENTERO VAR1 O REAL VAR2
                        */
                        if (strToken == "IDEN" && CadenaAux == "ENTERO" || CadenaAux == "REAL" || CadenaAux == "CADENA") {

                            switch (CadenaAux)
                            {
                                case "ENTERO":
                                    CadenaAux = "ENTR";
                                    dgvTablaSimbolos.Rows.Add(Cadena, strToken, CadenaAux);
                                    break;
                                case "REAL":
                                    dgvTablaSimbolos.Rows.Add(Cadena, strToken, CadenaAux);
                                    break;
                                case "CADENA":
                                    CadenaAux = "CADE";
                                    dgvTablaSimbolos.Rows.Add(Cadena, strToken, CadenaAux);
                                    break;
                            }                               
                        }
                        /*SINO EN CASO DE QUE EL TOKEN SEA COEN...*/                      
                        else if (strToken == "COEN") {       
                            //LA COLUMNA VALOR SE LLENA CON UNA CADENA
                            dgvTablaSimbolos.Rows.Add(Cadena, strToken, "ENTR");
                        }
                        /*SINO EN CASO DE QUE EL TOKEN SEA CORE...*/
                        else if (strToken == "CORE") {         
                            dgvTablaSimbolos.Rows.Add(Cadena, strToken, "REAL");
                        }
                        CadenaAux = Cadena;
                        Cadena = "";
                        intEstado = 0;
                    }
                }
                rtxToken.Text += "\n";
            }
            rtxToken.SelectionStart = rtxToken.Text.Length;
            posicion = rtxToken.SelectionStart;
            //Borra duplicados de dgvTablaSimbolos
            RemoverDuplicados(dgvTablaSimbolos);
            //Guarda la tabla de simbolos a una lista de objetos
            GuardarSimbolos();        
        }
        #endregion

        #region Espacios en blanco
            public static string Blanco(String Cadena) {
                while (Cadena.Contains("  ")) {
                    Cadena = Cadena.Replace("  ", " ");
                }
                return (Cadena.Trim() + " ");
            }
        #endregion

        #region Remover Duplicados en Tabla de símbolos
        public void RemoverDuplicados(DataGridView grid) {
            for (int currentRow = 0; currentRow < grid.Rows.Count - 1; currentRow++) {
                DataGridViewRow rowToCompare = grid.Rows[currentRow];

                for (int otherRow = currentRow + 1; otherRow < grid.Rows.Count; otherRow++) {
                    DataGridViewRow row = grid.Rows[otherRow];

                    bool duplicateRow = true;

                    for (int cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++) {
                        if (!rowToCompare.Cells[cellIndex].Value.Equals(row.Cells[cellIndex].Value)) {
                            duplicateRow = false;
                            break;
                        }
                    }
                    if (duplicateRow) {
                        grid.Rows.Remove(row);
                        otherRow--;
                    }
                }
            }
        }
        #endregion

        #region Analizador Sintáctico
        private void btnSintaxis_Click(object sender, EventArgs e)
        {
            ListaErroresSintaxis.Clear();
            dgvErroresSintaxis.Rows.Clear();
            rtxDerivaciones.Text = "";
            /* Se intercambian los operadores relacionales por OPRE,aritmeticos por OPAR y constantes por CONS para poder comparar con gramaticas */
            rtxTokenSintaxis.Text = AjustarTokensParaSintaxis(rtxToken.Text.TrimEnd(char.Parse("\n")));
            string[] ArregloTokensSintaxis = new string[rtxTokenSintaxis.Lines.Length];

            for (int i = 0; i <= rtxTokenSintaxis.Lines.Length - 1; i++)
            {
                intLineaErrorSintaxis = i+1;
                ArregloTokensSintaxis[i] = rtxTokenSintaxis.Lines[i].Trim(' '); //Guarda cada linea en una celda del arreglo - Tambien le quita los espacios al final de la cadena
                int numeroTokens = ArregloTokensSintaxis[i].Split(' ').Length; //Obtiene el numero de tokens en la linea - i -     
                string[] AuxiliarTokens = new string[numeroTokens]; //Inicializa el arreglo auxiliar con el numero de tokens        
                ArregloTokensSintaxis[i].Split(' ').CopyTo(AuxiliarTokens, 0); //Almacenar la linea - i - en un arreglo auxiliar (cada token en una celda)

                bool enviarABottomUp = true;
                foreach (var token in AuxiliarTokens) //Valida si hay un token de ERROR en la salida del léxico
                    enviarABottomUp = !token.Contains("ERROR");

                if (enviarABottomUp) { //Si no hay error envía los tokens de la linea actual a BottomUp
                    BottomUp(AuxiliarTokens);
                } else { //Si encuentra un error (de una palabra que no existe) avisa para corregir desde léxico
                    rtxTokenSintaxis.Text = "";
                    rtxDerivaciones.Text = "";
                    MessageBox.Show("Existe un error en léxico", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }
            }
        }    
        public void BottomUp(string[] arrTokens) //Recibe un arreglo con una linea de tokens
        {
            int cantidadTokens = arrTokens.Length; //Obtiene el tamaño de la cadena de tokens
            string cadenaTokens = string.Join(" ", arrTokens); //Convertir el arreglo de la cadena de tokens a un string

            rtxDerivaciones.Text += cadenaTokens + "\n"; //Agrega la cadena de tokens al textbox de derivaciones

            if (dictGramaticas.ContainsKey(cadenaTokens) && dictGramaticas[cadenaTokens] == "S") { //Si encuentra una S
                rtxDerivaciones.Text += dictGramaticas[cadenaTokens] + "\n";
                rtxDerivaciones.Text += dictGramaticas[cadenaTokens] == "S" ? "\n" : "";
            }
            else //Caso: No encuentra una S directa, necesita derivarse o validar que realmente no existe
            { 
                string cadenaTokensModificada = cadenaTokens, strBloque;
                int aux = 0, restador = 1; 
                
                do {                   
                    int cantidadAuxiliar = cadenaTokensModificada.Split(' ').Length; //toma la cantidad de tokens en la cadena modificada
                    string[] ArrTokensModificados = new string[cantidadAuxiliar];           
                    cadenaTokensModificada.Split(' ').CopyTo(ArrTokensModificados, 0); // Convierto en arreglo la cadena modificada
                    
                    strBloque = string.Join(" ", ArrTokensModificados.Skip(aux).Take(cantidadAuxiliar-restador));    //Se obtiene el bloque actual del recorrido   
                    
                    if (dictGramaticas.ContainsKey(strBloque)) //Pregunta si el diccionario contiene el bloque
                    { 
                        cadenaTokensModificada = cadenaTokensModificada.Replace(strBloque, dictGramaticas[strBloque]); //Si encuentra el bloque lo reemplaza con la gramatica
                        rtxDerivaciones.Text += cadenaTokensModificada + "\n"; //Visuales
                        rtxDerivaciones.Text += dictGramaticas[strBloque] == "S" & cadenaTokensModificada.Split(' ').Length == 1 ? "\n" : "";//Visuales
                        
                        if (dictGramaticas[strBloque] != "S" & cadenaTokensModificada.Split(' ').Length == 1) {
                            rtxDerivaciones.Text += "ERROR DE SINTAXIS\n\n";
                            AgregarErrorSintaxis();
                        } 
                        else 
                        {
                            rtxDerivaciones.Text += "";
                        }
                        aux = 0;                      
                        restador = 0;
                    }
                    else //Si no lo contiene lleva a cabo el sig. proceso para saber si ya recorrió toda la cadena antes de decrementar el numero de tkns
                    { 
                        if ((aux + (cantidadAuxiliar - restador)) == cantidadAuxiliar) 
                        {
                            aux = 0;
                            restador++;
                        } 
                        else { aux++; }

                        if (strBloque=="" | (cadenaTokensModificada.Split(' ').Contains("S") & cadenaTokensModificada.Split(' ').Length > 1))
                        {
                            rtxDerivaciones.Text += "ERROR DE SINTAXIS\n\n"; //Visuales
                            AgregarErrorSintaxis();
                            break; //Controlar si encuentra un error de sintaxis romper para que no se cicle
                        }
                    }
                } while (cadenaTokensModificada.Split(' ').Length > 1);
            }
        }
        #endregion

        #region Analizador Semantico
        public void IntercambiarIdentificadoresParaSemantica()
        {
            rtxTokenSemantica.Text = "";
            string[] ArregloLineasFuente = new string[rtxFuente.Lines.Length];
            string[] ArregloLineasLexico = new string[rtxToken.Lines.Length];

            for (int i = 0; i <= rtxFuente.Lines.Length - 1; i++)
            {
                ArregloLineasFuente[i] = rtxFuente.Lines[i].Trim(' '); //Guarda cada linea en una celda del arreglo - Tambien le quita los espacios al final de la cadena
                int numeroTokens = ArregloLineasFuente[i].Split(' ').Length; //Obtiene el numero de tokens en la linea - i -     
                string[] AuxiliarTokensFuente = new string[numeroTokens]; //Inicializa el arreglo auxiliar con el numero de tokens        
                ArregloLineasFuente[i].Split(' ').CopyTo(AuxiliarTokensFuente, 0); //Almacenar la linea - i - en un arreglo auxiliar (cada token en una celda)

                ArregloLineasLexico[i] = rtxToken.Lines[i].Trim(' '); //Guarda cada linea en una celda del arreglo - Tambien le quita los espacios al final de la cadena
                int numeroTokensLexico = ArregloLineasLexico[i].Split(' ').Length; //Obtiene el numero de tokens en la linea - i -     
                string[] AuxiliarTokensLexico = new string[numeroTokensLexico]; //Inicializa el arreglo auxiliar con el numero de tokens        
                ArregloLineasLexico[i].Split(' ').CopyTo(AuxiliarTokensLexico, 0); //Almacenar la linea - i - en un arreglo auxiliar (cada token en una celda)

                if (Array.Exists(AuxiliarTokensFuente, element => element.StartsWith("@")))
                {
                    for (int j = 0; j <= AuxiliarTokensFuente.Length - 1; j++)
                    {
                        if (AuxiliarTokensLexico[j] == "IDEN" && AuxiliarTokensFuente[j].Contains('@'))
                        {
                            AuxiliarTokensLexico[j] = AuxiliarTokensFuente[j];
                        }
                    }
                }
                string strLinea = string.Join(" ", AuxiliarTokensLexico);
                rtxTokenSemantica.AppendText(strLinea+"\n");
            }

            string[] ArregloLineasSemantica = new string[rtxTokenSemantica.Lines.Length];
            string strTextoRtxSemanticaNuevo = "";

            for (int i = 0; i <= rtxTokenSemantica.Lines.Length - 1; i++)
            {
                ArregloLineasSemantica[i] = rtxTokenSemantica.Lines[i].Trim(' '); //Guarda cada linea en una celda del arreglo - Tambien le quita los espacios al final de la cadena
                int numeroTokens = ArregloLineasSemantica[i].Split(' ').Length; //Obtiene el numero de tokens en la linea - i -     
                string[] AuxiliarTokensSemantica = new string[numeroTokens]; //Inicializa el arreglo auxiliar con el numero de tokens    
                ArregloLineasSemantica[i].Split(' ').CopyTo(AuxiliarTokensSemantica, 0); //Almacenar la linea - i - en un arreglo auxiliar (cada token en una celda)

                for (int j = 0; j <= AuxiliarTokensSemantica.Length - 1; j++)
                {
                    foreach (Simbolo simbolo in ListaSimbolos)
                    {
                        if (AuxiliarTokensSemantica[j] == simbolo.Lexema)
                        {
                            AuxiliarTokensSemantica[j] = simbolo.Tipo;
                        }
                    }

                }
                string strLinea = string.Join(" ", AuxiliarTokensSemantica);
                strTextoRtxSemanticaNuevo += strLinea + "\n";
            }
            rtxTokenSemantica.Text = "";
            rtxTokenSemantica.Text = strTextoRtxSemanticaNuevo;
        }

        public string AjustarTokensParaSemantica (string strCadena)
        {
            var reemplazos = new Dictionary<string, string>();
            reemplazos.Add("CORE", "REAL");
            reemplazos.Add("COEN", "ENTR");
            reemplazos.Add("LETR", "CADE");

           foreach (var reemplazo in reemplazos) {
              strCadena = strCadena.Replace(reemplazo.Key, reemplazo.Value);
           }
           
           return strCadena;
        }

        private void btnSemantica_Click(object sender, EventArgs e)
        {
            ListaErroresSemantica.Clear();
            dgvErroresSemantica.Rows.Clear();
            rtxDerivacionesJELU.Text = "";
            rtxDerivacionesSemantica.Text = "";
            rtxJELUVertical.Text = "";
            lstSalidaJELU.Clear();
            IntercambiarIdentificadoresParaSemantica(); //Cambiar los identificadores por su tipo
            rtxTokenSemantica.Text = AjustarTokensParaSemantica(rtxTokenSemantica.Text.TrimEnd(char.Parse("\n"))); //Ajuste de tokens restantes: CORE, COEN, LETR

            string[] ArregloLineasSemantica = new string[rtxTokenSemantica.Lines.Length];
            bool enviarABottomUp = true;

            for (int i = 0; i <= rtxTokenSemantica.Lines.Length - 1; i++)
            {
                intLineaErrorSemantica = i + 1;
                ArregloLineasSemantica[i] = rtxTokenSemantica.Lines[i].Trim(' '); //Guarda cada linea en una celda del arreglo - Tambien le quita los espacios al final de la cadena
                int numeroTokens = ArregloLineasSemantica[i].Split(' ').Length; //Obtiene el numero de tokens en la linea - i -     
                string[] AuxiliarTokensSemantica = new string[numeroTokens]; //Inicializa el arreglo auxiliar con el numero de tokens    
                ArregloLineasSemantica[i].Split(' ').CopyTo(AuxiliarTokensSemantica, 0); //Almacenar la linea - i - en un arreglo auxiliar (cada token en una celda)

                //Valida si hay un ERROR en la salida de sintaxis
                enviarABottomUp = !rtxDerivaciones.Text.Contains("ERROR DE SINTAXIS");

                if (enviarABottomUp)
                { //Si no hay error envía los tokens de la linea actual a BottomUpSemantico
                    BottomUpSemantico(AuxiliarTokensSemantica);
                }
                else
                { //Si encuentra un error (de sintaxis) avisa para corregir desde código fuente
                    rtxTokenSemantica.Text = "";
                    rtxDerivacionesSemantica.Text = "";
                    MessageBox.Show("Existe un error en sintáxis", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }
            }

            //Arreglo de lineas de sintaxis para metodo JELU
            string[] ArregloTokensSintaxis = new string[rtxTokenSintaxis.Lines.Length];

            if (enviarABottomUp) {
                //Para metodo JELU
                for (int i = 0; i <= rtxTokenSintaxis.Lines.Length - 1; i++)
                {
                    ArregloTokensSintaxis[i] = rtxTokenSintaxis.Lines[i].Trim(' '); //Guarda cada linea en una celda del arreglo - Tambien le quita los espacios al final de la cadena
                    int numeroTokens = ArregloTokensSintaxis[i].Split(' ').Length; //Obtiene el numero de tokens en la linea - i -     
                    string[] AuxiliarTokens = new string[numeroTokens]; //Inicializa el arreglo auxiliar con el numero de tokens        
                    ArregloTokensSintaxis[i].Split(' ').CopyTo(AuxiliarTokens, 0); //Almacenar la linea - i - en un arreglo auxiliar (cada token en una celda)    
                    BottomUpJELU(AuxiliarTokens);
                }
                PintarErroresSemantica("ERROR DE SEMANTICA", Color.Red, 0);
                BottomUpJELUVertical(lstSalidaJELU.ToArray());
                PintarErroresJELU("ERROR DE SEMANTICA", Color.Red, 0);
            }
            if(dgvErroresSemantica.ColumnCount == 1)
            {
                QueSoy();
                if (ExisteDoWhile == true)
                {
                    txtCondicionWhile.Text = ObtenerCondicionWhile(rtxFuente.Text);
                    txtCadenaHacer.Text = ObtenerContenidoEntreComillas(rtxFuente.Text);
                    txtCondicionHacer.Text = ObtenerContenidoEntreHacerYImprimir(rtxFuente.Text);
                }
                if (ExisteDoWhile == false)
                {
                    if (ExisteLR == false)
                    {
                        if (ExisteLeer == false)
                        {
                            ObtenerValorBC();
                        }
                        CadenaInfija();
                        Codificado = Postfijo(txtExpresionInfija.Text);
                        StringBuilder output = new StringBuilder();

                        foreach (char c in Codificado)
                        {
                            output.Append(c); // Agregar el carácter original
                            output.Append(' '); // Agregar un espacio después de cada carácter
                        }

                        string CodificadoConEspacio = output.ToString().Trim(); // Eliminar el espacio adicional al final
                        txtExpresionResultado.Text = CodificadoConEspacio;
                        txtTokensResultado.Text = Decodificador(CodificadoConEspacio);
                    }
                    if (ExisteLR == true)
                    {
                        CadenaLR();
                        ObtenerCondiciones();
                    }
                }
            }
        }

        public void BottomUpSemantico(string[] arrTokens)
        {
            int cantidadTokens = arrTokens.Length; //Obtiene el tamaño de la cadena de tokens
            string cadenaTokens = string.Join(" ", arrTokens); //Convertir el arreglo de la cadena de tokens a un string

            rtxDerivacionesSemantica.Text += cadenaTokens + "\n"; //Agrega la cadena de tokens al textbox de derivaciones

            if (dictReglasSemanticas.ContainsKey(cadenaTokens) && dictReglasSemanticas[cadenaTokens] == "S" || dictReglasSemanticas.ContainsKey(cadenaTokens) && dictReglasSemanticas[cadenaTokens] == "NOAP")
            { //Si encuentra una S
                rtxDerivacionesSemantica.Text += dictReglasSemanticas[cadenaTokens] + "\n";
                rtxDerivacionesSemantica.Text += dictReglasSemanticas[cadenaTokens] == "S" ? "\n" : "";
                rtxDerivacionesSemantica.Text += dictReglasSemanticas[cadenaTokens] == "NOAP" ? "\n" : "";
            }
            else //Caso: No encuentra una S directa, necesita derivarse o validar que realmente no existe
            {
                string cadenaTokensModificada = cadenaTokens, strBloque;
                int aux = 0, restador = 1;

                do
                {
                    int cantidadAuxiliar = cadenaTokensModificada.Split(' ').Length; //toma la cantidad de tokens en la cadena modificada
                    string[] ArrTokensModificados = new string[cantidadAuxiliar];
                    cadenaTokensModificada.Split(' ').CopyTo(ArrTokensModificados, 0); // Convierto en arreglo la cadena modificada

                    strBloque = string.Join(" ", ArrTokensModificados.Skip(aux).Take(cantidadAuxiliar - restador));    //Se obtiene el bloque actual del recorrido   

                    if (dictReglasSemanticas.ContainsKey(strBloque)) //Pregunta si el diccionario contiene el bloque
                    {
                        cadenaTokensModificada = cadenaTokensModificada.Replace(strBloque, dictReglasSemanticas[strBloque]); //Si encuentra el bloque lo reemplaza con la gramatica
                        rtxDerivacionesSemantica.Text += cadenaTokensModificada + "\n"; //Visuales
                        rtxDerivacionesSemantica.Text += dictReglasSemanticas[strBloque] == "S" & cadenaTokensModificada.Split(' ').Length == 1 ? "\n" : "";//Visuales      
                        
                        if ((dictReglasSemanticas[strBloque] != "S" && dictReglasSemanticas[strBloque] != "NOAP") & cadenaTokensModificada.Split(' ').Length == 1)
                        {   
                            rtxDerivacionesSemantica.Text += "ERROR DE SEMANTICA\n\n";
                            AgregarErroresSemantica();
                        }
                        else
                        {
                            rtxDerivacionesSemantica.Text += "";
                        }
                        aux = 0;
                        restador = 0;
                    }
                    else //Si no lo contiene lleva a cabo el sig. proceso para saber si ya recorrió toda la cadena antes de decrementar el numero de tkns
                    {
                        if ((aux + (cantidadAuxiliar - restador)) == cantidadAuxiliar)
                        {
                            aux = 0;
                            restador++;
                        }
                        else { aux++; }

                        if (strBloque == "" | (cadenaTokensModificada.Split(' ').Contains("S") & cadenaTokensModificada.Split(' ').Length > 1))
                        {
                            rtxDerivacionesSemantica.Text += "ERROR DE SEMANTICA\n\n"; //Visuales
                            AgregarErroresSemantica();
                            break; //Controlar si encuentra un error de sintaxis romper para que no se cicle
                        }
                    }
                } while (cadenaTokensModificada.Split(' ').Length > 1);
            }
        }

        public void BottomUpJELU(string[] arrTokens)
        {
            string cadenaTokens = string.Join(" ", arrTokens);
            rtxDerivacionesJELU.Text += cadenaTokens + "\n"; //Agrega la cadena de tokens al richtextbox de derivaciones
            
            string valorEncontrado; //Para saber si el diccionario contiene el valor buscado, si no lo encuentra esta variable será null
            if (dictGramaticasJELU.ContainsKey(cadenaTokens) && dictGramaticasJELU.TryGetValue(cadenaTokens, out valorEncontrado))
            {
                rtxDerivacionesJELU.Text += string.Concat(valorEncontrado, "\n\n");
                lstSalidaJELU.Add(valorEncontrado);
            }
            else
            { //BottomUp Core
                string cadenaTokensModificada = cadenaTokens, strBloque;
                int aux = 0, restador = 1;

                do
                {
                    int cantidadAuxiliar = cadenaTokensModificada.Split(' ').Length; //toma la cantidad de tokens en la cadena modificada
                    string[] ArrTokensModificados = new string[cantidadAuxiliar];
                    cadenaTokensModificada.Split(' ').CopyTo(ArrTokensModificados, 0); // Convierto en arreglo la cadena modificada

                    strBloque = string.Join(" ", ArrTokensModificados.Skip(aux).Take(cantidadAuxiliar - restador)); //Se obtiene el bloque actual del recorrido

                    if (dictGramaticasJELU.ContainsKey(strBloque))
                    {
                        cadenaTokensModificada = cadenaTokensModificada.Replace(strBloque, dictGramaticasJELU[strBloque]);
                        
                        rtxDerivacionesJELU.Text += string.Concat(cadenaTokensModificada, "\n"); //VISUALES
                        //rtxDerivacionesJELU.Text += cadenaTokensModificada.Split(' ').Length == 1 ? "\n" : ""; //VISUALES
                        if (cadenaTokensModificada.Split(' ').Length == 1)
                        {
                            rtxDerivacionesJELU.Text += "\n";
                            lstSalidaJELU.Add(cadenaTokensModificada);
                        }
                        //lstSalidaJELU.Add(valorEncontrado);

                        dictGramaticasJELU.TryGetValue(cadenaTokens, out valorEncontrado);

                        if (valorEncontrado != null && cadenaTokensModificada.Split(' ').Length != 1) {
                            rtxDerivacionesJELU.Text += "";
                        }

                        aux = 0;
                        restador = 0;
                    }
                    else
                    {
                        if ((aux + (cantidadAuxiliar - restador)) == cantidadAuxiliar) {
                            aux = 0;
                            restador++;
                        }
                        else {
                          aux++;
                        }

                        if ((strBloque == "" || ((cadenaTokensModificada.Split(' ').Contains("S") || cadenaTokensModificada.Split(' ').Contains("CON")))) && cadenaTokensModificada.Split(' ').Length > 1)
                        {
                            rtxDerivacionesJELU.Text += "ERROR DE SEMANTICA\n\n"; //Visuales
                            break; //Controlar si encuentra un error de sintaxis romper para que no se cicle
                        }
                    }

                } while (cadenaTokensModificada.Split(' ').Length > 1);
            }
        }

        public void BottomUpJELUVertical(string[] arrTokens)
        {
            string cadenaTokens = string.Join(" ", arrTokens);
            rtxJELUVertical.Text += cadenaTokens + "\n"; //Agrega la cadena de tokens al richtextbox de derivaciones

            string valorEncontrado; //Para saber si el diccionario contiene el valor buscado, si no lo encuentra esta variable será null
            if (dictGramaticasJELU.ContainsKey(cadenaTokens) && dictGramaticasJELU.TryGetValue(cadenaTokens, out valorEncontrado))
            {
                rtxJELUVertical.Text += string.Concat(valorEncontrado, "\n\n");
            }
            else
            { //BottomUp Core
                string cadenaTokensModificada = cadenaTokens, strBloque;
                int aux = 0, restador = 1;

                do
                {
                    int cantidadAuxiliar = cadenaTokensModificada.Split(' ').Length; //toma la cantidad de tokens en la cadena modificada
                    string[] ArrTokensModificados = new string[cantidadAuxiliar];
                    cadenaTokensModificada.Split(' ').CopyTo(ArrTokensModificados, 0); // Convierto en arreglo la cadena modificada

                    strBloque = string.Join(" ", ArrTokensModificados.Skip(aux).Take(cantidadAuxiliar - restador)); //Se obtiene el bloque actual del recorrido

                    if (dictGramaticasJELU.ContainsKey(strBloque))
                    {
                        cadenaTokensModificada = cadenaTokensModificada.Replace(strBloque, dictGramaticasJELU[strBloque]);

                        rtxJELUVertical.Text += string.Concat(cadenaTokensModificada, "\n"); //VISUALES
                        if (cadenaTokensModificada.Split(' ').Length == 1 && cadenaTokensModificada.Split(' ').Contains("CON"))
                        {
                            rtxJELUVertical.Text += "ERROR DE SEMANTICA\n\n";
                            ListaErroresSemantica.Add("ERROR: FALTA ABRIR O CERRAR UNA INSTRUCCIÓN");
                            ActualizarErroresSemantica();
                        }

                        dictGramaticasJELU.TryGetValue(cadenaTokens, out valorEncontrado);

                        if (valorEncontrado != null && cadenaTokensModificada.Split(' ').Length != 1)
                        {
                            rtxJELUVertical.Text += "";
                        }

                        aux = 0;
                        restador = 0;
                    }
                    else
                    {
                        if ((aux + (cantidadAuxiliar - restador)) == cantidadAuxiliar)
                        {
                            aux = 0;
                            restador++;
                        }
                        else
                        {
                            aux++;
                        }

                        if ((strBloque == "" || cadenaTokensModificada.Split(' ').Contains("S")) && cadenaTokensModificada.Split(' ').Length > 1)
                        {
                            rtxJELUVertical.Text += "ERROR DE SEMANTICA\n\n"; //Visuales
                            ListaErroresSemantica.Add("ERROR: FALTA ABRIR O CERRAR UNA INSTRUCCIÓN");
                            ActualizarErroresSemantica();
                            break; //Controlar si encuentra un error de sintaxis romper para que no se cicle
                        }
                    }

                } while (cadenaTokensModificada.Split(' ').Length > 1);
            }
        }

        #endregion

        #region Guardar tabla de símbolos en Lista de Objetos
        public void GuardarSimbolos()
        {
            ListaSimbolos.Clear();
            foreach (DataGridViewRow dr in dgvTablaSimbolos.Rows) {
                Simbolo sim = new Simbolo();
                sim.Lexema = dr.Cells[0].Value.ToString();
                sim.Token = dr.Cells[1].Value.ToString();
                sim.Tipo = dr.Cells[2].Value.ToString();
                ListaSimbolos.Add(sim);
            }
        }
        #endregion

        #region Convertir gramaticas en diccionario
        public Dictionary<string, string> ConvertirTablaEnDiccionario(DataTable tabla)
        {
            Dictionary<string, string> diccionario = new Dictionary<string, string>();
            foreach (DataRow fila in tabla.Rows) {
                diccionario.Add(fila["Gramatica"].ToString(), fila["Simbolo"].ToString());
            }
            return diccionario;
        }
        #endregion

        #region Ajustar Tokens Para Sintaxis
        public string AjustarTokensParaSintaxis(string strCadena)
        {
            var reemplazos = new Dictionary<string, string>();
            reemplazos.Add("CORE", "CONS");
            reemplazos.Add("COEN", "CONS");
            reemplazos.Add("OR01", "OPRE");
            reemplazos.Add("OR02", "OPRE");
            reemplazos.Add("OR03", "OPRE");
            reemplazos.Add("OR04", "OPRE");
            reemplazos.Add("OR05", "OPRE");
            reemplazos.Add("OA01", "OPAR");
            reemplazos.Add("OA02", "OPAR");
            reemplazos.Add("OA03", "OPAR");
            reemplazos.Add("OA04", "OPAR");

            foreach (var reemplazo in reemplazos) {
                strCadena = strCadena.Replace(reemplazo.Key, reemplazo.Value);
            }

            return strCadena;
        }
        #endregion

        #region Pintar errores de Semantica en Rojo

        public void PintarErroresSemantica(string word, Color color, int startIndex)
        {
            if (this.rtxDerivacionesSemantica.Text.Contains(word))
            {
                int index = -1;
                int selectStart = this.rtxDerivacionesSemantica.SelectionStart;
                while ((index = this.rtxDerivacionesSemantica.Text.IndexOf(word, (index + 1))) != -1) {
                    this.rtxDerivacionesSemantica.Select((index + startIndex), word.Length);
                    this.rtxDerivacionesSemantica.SelectionColor = color;
                    this.rtxDerivacionesSemantica.Select(selectStart, 0);
                    this.rtxDerivacionesSemantica.SelectionColor = Color.Black;
                }
            }
        }

        public void PintarErroresJELU(string word, Color color, int startIndex)
        {
            if (this.rtxJELUVertical.Text.Contains(word))
            {
                int index = -1;
                int selectStart = this.rtxJELUVertical.SelectionStart;
                while ((index = this.rtxJELUVertical.Text.IndexOf(word, (index + 1))) != -1)
                {
                    this.rtxJELUVertical.Select((index + startIndex), word.Length);
                    this.rtxJELUVertical.SelectionColor = color;
                    this.rtxJELUVertical.Select(selectStart, 0);
                    this.rtxJELUVertical.SelectionColor = Color.Black;
                }
            }
        }

        #endregion

        #region Pintar Errores de Sintaxis en Rojo
        private void rtxDerivaciones_TextChanged(object sender, EventArgs e)
        {
            this.PintarErrores("ERROR DE SINTAXIS", Color.Red, 0);
        }

        public void PintarErrores(string word, Color color, int startIndex)
        {  
            if (this.rtxDerivaciones.Text.Contains(word)) {
                int index = -1;
                int selectStart = this.rtxDerivaciones.SelectionStart;
                    while ((index = this.rtxDerivaciones.Text.IndexOf(word, (index + 1))) != -1) {
                        this.rtxDerivaciones.Select((index + startIndex), word.Length);
                        this.rtxDerivaciones.SelectionColor = color;
                        this.rtxDerivaciones.Select(selectStart, 0);
                        this.rtxDerivaciones.SelectionColor = Color.Black;
                    }
            }  
        }

        #endregion

        #region Agregar Errores de Sintaxis / Semantica
        public void AgregarErrorSintaxis()
        {
            dgvErroresSintaxis.Rows.Clear();
            ErrorSintaxis miError = new ErrorSintaxis();
            miError.Descripcion = "Error en la línea: ";
            miError.Linea = intLineaErrorSintaxis;
            ListaErroresSintaxis.Add(miError);

            foreach (var error in ListaErroresSintaxis) {
                dgvErroresSintaxis.Rows.Add(error.Descripcion + error.Linea.ToString());
            }
        }

        public void AgregarErroresSemantica()
        {
            //dgvErroresSemantica.Rows.Clear();
            string desc = "Error en la linea: " + intLineaErrorSemantica.ToString();
            ListaErroresSemantica.Add(desc);
            ActualizarErroresSemantica();
        }

        public void ActualizarErroresSemantica()
        {
            dgvErroresSemantica.Rows.Clear();
            foreach (var error in ListaErroresSemantica)
            {
                dgvErroresSemantica.Rows.Add(error);
            }
        }


        #endregion

        #region Numeros de linea
        private void rtxFuente_TextChanged(object sender, EventArgs e)
        {
            if (rtxFuente.Text == "")
            {
                AgregarNumerosDeLinea(rtxFuente, rtxLinea2);
            }
        }

        private void rtxFuente_FontChanged(object sender, EventArgs e)
        {
            rtxLinea2.Font = rtxFuente.Font;
            rtxFuente.Select();
            AgregarNumerosDeLinea(rtxFuente, rtxLinea2);
        }

        public int getWidth()
        {
            int w = 25;

            int line = rtxToken.Lines.Length;

            if (line <= 99)
            {
                w = 20 + (int)rtxToken.Font.Size;
            }
            else if (line <= 999)
            {
                w = 30 + (int)rtxToken.Font.Size;
            }
            else
            {
                w = 50 + (int)rtxToken.Font.Size;
            }
            return w;
        }

        private void rtxFuente_SelectionChanged(object sender, EventArgs e)
        {
            Point pt = rtxFuente.GetPositionFromCharIndex(rtxFuente.SelectionStart);
            if (pt.X == 1)
            {
                AgregarNumerosDeLinea(rtxFuente, rtxLinea2);
            }
        }

        private void rtxFuente_VScroll(object sender, EventArgs e)
        {
            rtxLinea2.Text = "";
            AgregarNumerosDeLinea(rtxFuente, rtxLinea2);
            rtxLinea2.Invalidate();
        }

        private void lblLink_Click(object sender, EventArgs e)
        {
            tabPestania.SelectedIndex = 2;
        }

        private void tabCodigoIntermedio_Click(object sender, EventArgs e)
        {

        }

        public void AgregarNumerosDeLinea(RichTextBox codigo, RichTextBox numeracion)
        {
            Point pt = new Point(0, 0);

            int First_Index = codigo.GetCharIndexFromPosition(pt);
            int First_Line = codigo.GetLineFromCharIndex(First_Index);

            pt.X = ClientRectangle.Width;
            pt.Y = ClientRectangle.Height;

            int Last_Index = codigo.GetCharIndexFromPosition(pt);
            int Last_Line = codigo.GetLineFromCharIndex(Last_Index);

            numeracion.SelectionAlignment = HorizontalAlignment.Center;

            numeracion.Text = "";
            numeracion.Width = getWidth();

            for (int i = First_Line; i <= Last_Line + 1; i++)
            {
                numeracion.Text += i + 1 + "\n";
            }
        }

        #endregion

        private void btnConvertir_Click(object sender, EventArgs e)
        {
            // Supongamos que tienes un DataGridView llamado dataGridView1 con tres columnas Columna1, Columna2 y Columna3
            dtgEnsamblador.Rows.Clear();
            TriploEnsamblador.Clear();
            bool Entro = false;
            foreach (DataGridViewRow fila in dtgALR.Rows)
            {
                if (!fila.IsNewRow) // Verifica si la fila no es la fila nueva de ingreso de datos
                {
                    // Accede a los valores de las celdas de cada fila
                    string valorColumna1 = fila.Cells["Dato objeto"].Value.ToString();
                    string valorColumna2 = fila.Cells["Dato fuete"].Value.ToString();
                    string valorColumna3 = fila.Cells["Operador"].Value.ToString();
                    Ensamblador ensamblador = new Ensamblador();
                    if (valorColumna3 == "+")
                    {
                        ensamblador.Columna1 = "ADD";
                    }
                    if (valorColumna3 == "-")
                    {
                        ensamblador.Columna1 = "SUB";
                    }
                    if (valorColumna3 == "*")
                    {
                        ensamblador.Columna1 = "MUL";
                    }
                    if (valorColumna3 == "/")
                    {
                        ensamblador.Columna1 = "DIV";
                    }
                    if (valorColumna3 == "=")
                    {
                        ensamblador.Columna1 = "MOV";
                    }
                    if(valorColumna3 == "<" || valorColumna3 == ">")
                    {
                        ensamblador.Columna1 = "CMP";
                    }
                    if (ExisteLR == false && ExisteDoWhile == false)
                    {
                        if (valorColumna1 == "T1" || char.IsLetter(char.Parse(valorColumna1)))
                        {
                            ensamblador.Columna2 = "AX";
                        }
                    }
                    else
                    {
                        if (valorColumna1 == "T1" && ExisteDoWhile == false)
                        {
                            ensamblador.Columna2 = "AX";
                        }
                        else
                        {
                            ensamblador.Columna2 = "BX";
                        }
                    }
                    if(valorColumna1 == "T2" && ExisteDoWhile == false)
                    {
                        ensamblador.Columna2 = "BX";
                    }
                    if (valorColumna1 == "T2" && ExisteDoWhile == true)
                    {
                        ensamblador.Columna2 = "CX";
                    }
                    if (valorColumna1 == "T3")
                    {
                        ensamblador.Columna2 = "CX";
                    }
                    if(ExisteLR == true || ExisteDoWhile == true)
                    {
                        Entro = false;
                        if (valorColumna2 == "T2" && ExisteDoWhile == false)
                        {
                            ensamblador.Columna3 = "BX";
                            Entro = true;
                        }
                        if (valorColumna2 == "T2" && ExisteDoWhile == true)
                        {
                            ensamblador.Columna3 = "CX";
                            Entro = true;
                        }
                        if (valorColumna2 == "T3")
                        {
                            ensamblador.Columna3 = "CX";
                            Entro = true;
                        }
                        if(Entro == false)
                        {
                            ensamblador.Columna3 = valorColumna2;
                        }
                    }
                    else
                    {
                        ensamblador.Columna3 = valorColumna2;
                    }
                    TriploEnsamblador.Add(ensamblador);
                }
            }
            foreach (Ensamblador E in TriploEnsamblador)
            {
                dtgEnsamblador.Rows.Add(E.Columna1, E.Columna2, E.Columna3);
            }

        }

        private void btnEnsamblador_Click(object sender, EventArgs e)
        {
            rtbEnsamblador.Text = "";
            string CMP1 = "";
            string CMP2 = "";
            bool Existe = false;
            rtbEnsamblador.Text = ".MODEL SMALL\n";
            int contador = 1;
            if(ExisteDoWhile == true)
            {
                if(txtCadenaHacer.Text == "")
                {
                    TransformaHacer(txtCondicionHacer.Text);
                    CondicionHacer = Salida;
                    foreach (DataGridViewRow fila in dtgEnsamblador.Rows)
                    {
                        if (!fila.IsNewRow) // Verifica si la fila no es la fila nueva de ingreso de datos
                        {
                            // Accede a los valores de las celdas de cada fila
                            string valorColumna1 = fila.Cells["Variable"].Value.ToString();
                            string valorColumna2 = fila.Cells["Dato 1"].Value.ToString();
                            string valorColumna3 = fila.Cells["Dato 2"].Value.ToString();
                            if (contador == 1)
                            {
                                rtbEnsamblador.Text = rtbEnsamblador.Text + ".DATA\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "BUFFER DB 6 DUP(?)\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "SALTO DB \"\",0AH,0DH,\"$\"\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "DECENAS DB 30H\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "UNIDADES DB 0\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                            }
                            if (valorColumna2 == "BX" && contador == 1)
                            {
                                rtbEnsamblador.Text = rtbEnsamblador.Text + ".CODE\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MAIN PROC\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AX, @DATA\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV DS, AX\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV BX, 0 ;Inicializar BX\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                            }
                            if (valorColumna2 == "CX" && contador == 2)
                            {
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV CX, " + valorColumna3 + ";Inicializar CX\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                            }
                            if (valorColumna1 == "CMP" && contador == 3)
                            {
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "DO_WHILE:\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + ";IMPRIME EL NUMERO\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AX,BX\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "CALL IMPRIMIR\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + ";INCREMENTO\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + CondicionHacer + "\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + ";COMPARACION\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna1 + " " + valorColumna2 + ", " +
                                    valorColumna3 + "\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "jl DO_WHILE ;Bucle\n";
                            }
                            contador++;
                        }
                    }
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + ";TERMINA EL PROGRAMA\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 4CH\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MAIN ENDP\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "IMPRIMIR PROC\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH,2\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV DL,DECENAS\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AX,BX\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "XOR AL,30H\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "ADD UNIDADES,AL\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH,2\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV DL,UNIDADES\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "CMP UNIDADES,39H\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV UNIDADES,0\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "JE DECE\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "JMP CONT\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "DECE:\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "ADD DECENAS,1\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "SUB UNIDADES,10\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "JMP SALIDA\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "CONT:\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "CMP DECENAS,30H\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "JNE LIMP\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "JMP SALIDA\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "LIMP:\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "SUB UNIDADES,10\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "SALIDA:\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH,09H\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "LEA DX,SALTO\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "RET\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "IMPRIMIR ENDP\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "END MAIN\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                }
                else
                {
                    TransformaHacer(txtCondicionHacer.Text);
                    CondicionHacer = Salida;
                    foreach (DataGridViewRow fila in dtgEnsamblador.Rows)
                    {
                        if (!fila.IsNewRow) // Verifica si la fila no es la fila nueva de ingreso de datos
                        {
                            // Accede a los valores de las celdas de cada fila
                            string valorColumna1 = fila.Cells["Variable"].Value.ToString();
                            string valorColumna2 = fila.Cells["Dato 1"].Value.ToString();
                            string valorColumna3 = fila.Cells["Dato 2"].Value.ToString();
                            if (contador == 1)
                            {
                                rtbEnsamblador.Text = rtbEnsamblador.Text + ".DATA\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "mensaje db '" + txtCadenaHacer.Text + "',0ah,0dh,\"$\", 0\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                            }
                            if (valorColumna2 == "BX" && contador == 1)
                            {
                                rtbEnsamblador.Text = rtbEnsamblador.Text + ".CODE\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MAIN:\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AX, @DATA\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV DS, AX\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV BX, 0 ;Inicializar BX\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                            }
                            if (valorColumna2 == "CX" && contador == 2)
                            {
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV CX, " + valorColumna3 + ";Inicializar CX\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                            }
                            if (valorColumna1 == "CMP" && contador == 3)
                            {
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "DO_WHILE:\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + ";Imprime el mensaje\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 9\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "LEA DX, mensaje\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + ";INCREMENTO\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + CondicionHacer + "\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + ";COMPARACION\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna1 + " " + valorColumna2 + ", " +
                                    valorColumna3 + "\n";
                                rtbEnsamblador.Text = rtbEnsamblador.Text + "jl DO_WHILE ;Bucle\n";
                            }
                            contador++;
                        }
                    }
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + ";TERMINA EL PROGRAMA\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 4CH\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                    rtbEnsamblador.Text = rtbEnsamblador.Text + "END MAIN\n";
                }
            }
            else
            {
                if (ExisteLR == false)
                {
                    if(ExisteLeer == true)
                    {
                        Imprimir = ObtenerContenidoEntreComillas(rtxFuente.Text);
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ".STACK 100H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        foreach (DataGridViewRow fila in dtgEnsamblador.Rows)
                        {
                            if (!fila.IsNewRow) // Verifica si la fila no es la fila nueva de ingreso de datos
                            {
                                // Accede a los valores de las celdas de cada fila
                                string valorColumna1 = fila.Cells["Variable"].Value.ToString();
                                string valorColumna2 = fila.Cells["Dato 1"].Value.ToString();
                                string valorColumna3 = fila.Cells["Dato 2"].Value.ToString();

                                if (valorColumna3.Length == 1 && contador == 1)
                                {
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + ".DATA\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna3 + " DW ? ;Cambiar valor\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MSG DB '" + Imprimir + " $' ; Mensaje para imprimir\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "NL  DB 0DH, 0AH, '$' ; Nueva linea\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                }
                                if (valorColumna2 == "AX" && contador == 1)
                                {
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + ".CODE\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AX, @DATA\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV DS, AX\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "; Imprimir mensaje para ingresar\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 09H\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "LEA DX, MSG\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "; Leer el valor desde el teclado y almacenarlo\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 01H ; Funcion de lectura del teclado\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "SUB AL, '0' ; Convertir el caracter ASCII a valor numerico\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV B, AX\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                }
                                if (contador == 3)
                                {
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                }
                                else
                                {
                                    if (valorColumna1 == "MUL" || valorColumna1 == "DIV")
                                    {
                                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV CX, "
                                            + valorColumna3 + "\n";
                                        rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna1 + " CX\n";
                                    }
                                    else
                                    {
                                        rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna1 + " " + valorColumna2 + ", "
                                            + valorColumna3 + "\n";
                                    }
                                }
                                contador++;
                            }
                        }
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ";Convertir AX en decimal\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV CX, 10\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV BX, AX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 0\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "CONVERTIR:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "XOR DX, DX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "DIV CX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "ADD DL, '0'\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "PUSH DX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "CMP AX, 0\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JNZ CONVERTIR\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "; Imprimir nueva linea\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 09H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "LEA DX, NL\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "; Mostrar la primera cifra\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "POP DX ;Recuperar el primer digito\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 02H ;Imprimir un caracter\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "POP DX ;Recuperar el segundo digito (si existe)\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "CMP DX, 0 ;Verificar si existe\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JE FIN ;Finaliza programa sino existe\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "; Mostrar la segunda cifra\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 02H ;Imprimir un caracter\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "; Imprimir nueva linea\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 09H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "LEA DX, NL\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ";Finaliza el programa\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "FIN:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 4CH\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "END\n";
                    }
                    else
                    {
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ".STACK 100H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        foreach (DataGridViewRow fila in dtgEnsamblador.Rows)
                        {
                            if (!fila.IsNewRow) // Verifica si la fila no es la fila nueva de ingreso de datos
                            {
                                // Accede a los valores de las celdas de cada fila
                                string valorColumna1 = fila.Cells["Variable"].Value.ToString();
                                string valorColumna2 = fila.Cells["Dato 1"].Value.ToString();
                                string valorColumna3 = fila.Cells["Dato 2"].Value.ToString();

                                if (valorColumna3.Length == 1 && contador == 1)
                                {
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + ".DATA\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna3 + " DW " + valorB + " ;Cambiar valor\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "C DW " + valorC + " ;Cambiar valor\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                }
                                if (valorColumna2 == "AX" && contador == 1)
                                {
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + ".CODE\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AX, @DATA\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV DS, AX\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                }
                                if (contador == 3)
                                {
                                    if (valorColumna1 == "SUB")
                                    {
                                        rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna1 + " " + valorColumna2 + ", "
                                            + valorColumna3 + "\n";
                                    }
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                }
                                else
                                {
                                    if (valorColumna1 == "MUL" || valorColumna1 == "DIV")
                                    {
                                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV CX, "
                                            + valorColumna3 + "\n";
                                        rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna1 + " CX\n";
                                    }
                                    else
                                    {
                                        if (valorColumna3 == "T1")
                                        {

                                        }
                                        else
                                        {
                                            rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna1 + " " + valorColumna2 + ", "
                                            + valorColumna3 + "\n";
                                        }
                                    }
                                }
                                contador++;
                            }
                        }
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ";Convertir AX en decimal\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV CX, 10\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV BX, AX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 0\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "CONVERTIR:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "XOR DX, DX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "DIV CX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "ADD DL, '0'\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "PUSH DX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "CMP AX, 0\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JNZ CONVERTIR\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ";Mostrar la cadena\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "POP DX ;Recuperar el primer digito\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 02H ;Imprimir un caracter\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "POP DX ;Recuperar el segundo digito (si existe)\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "CMP DX, 0 ;Verificar si existe\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JE FIN ;Finaliza programa sino existe\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 02H ;Imprimir un caracter\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ";Finaliza el programa\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "FIN:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 4CH\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "END\n";
                    }
                }
                if (ExisteLR == true)
                {
                    if(ExisteLeer == true)
                    {
                        string ValorComparacion = "";
                        string Variable = "";
                        ImprimirLR();
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ".STACK\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        foreach (DataGridViewRow fila in dtgEnsamblador.Rows)
                        {
                            if (!fila.IsNewRow) // Verifica si la fila no es la fila nueva de ingreso de datos
                            {
                                // Accede a los valores de las celdas de cada fila
                                string valorColumna1 = fila.Cells["Variable"].Value.ToString();
                                string valorColumna2 = fila.Cells["Dato 1"].Value.ToString();
                                string valorColumna3 = fila.Cells["Dato 2"].Value.ToString();
                                if (contador == 1)
                                {
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + ".DATA\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "ascii db 5,0,0,0,0,0,0\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "decimal db 0,0,0,0\n";
                                    Variable = valorColumna3;
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna3 + " DW ? ;VARIABLE\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "BASE DW 10\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "POSICION DW 1 \n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MS1 DB '"+ IM1 +" $' ; Mensaje para imprimir\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MS2 DB '"+ IM2 +" $' ; Mensaje\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MS3 DB '" + IM3 + " $' ; Mensaje\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "buffer db 6 dup(?)\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "SALTO  DB \"\",0AH,0DH,\"$\" ;Salto\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                }
                                if (valorColumna2 == "AX" && contador == 1)
                                {
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + ".CODE\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MAIN PROC\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "; Imprimir mensaje para ingresar\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AX, @DATA\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV DS, AX\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 09H\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "LEA DX, MS1\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "CALL CAPNUM ;LLAMADA\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";

                                }
                                if (valorColumna1 == "MOV" && valorColumna2 != "AX")
                                {
                                    ValorComparacion = valorColumna3;
                                }
                                //NO ASIGNADO
                                if (valorColumna1 == "CMP" && contador == 3)
                                {
                                    CMP1 = valorColumna1 + " " + Variable + ", " + ValorComparacion;
                                }
                                contador++;
                            }
                        }
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + CMP1 + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JGE CMP1\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ";Si se cumplen\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "; Imprimir mensaje\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 09H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "LEA DX, MS2\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JMP END_PROGRAM\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "CMP1:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ";Si no se cumple\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "; Imprimir mensaje\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 09H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "LEA DX, MS3\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JMP END_PROGRAM\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MAIN ENDP\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "capnum proc\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "lea dx, ascii\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov ah, 0ah\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "int 21h\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov ah,09h\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "lea dx, salto\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "int 21h\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov bh,0\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov bl,ascii+1\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov cx,bx\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov si,3\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "convierte:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov al,[ascii+bx+1]\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "sub al,30h\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov [decimal+si],al\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "dec bx\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "dec si\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "loop convierte\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov cx,4\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov si,3\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "ciclo:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov ax,posicion\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov bl,decimal+si\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov bh,0\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mul bx\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "add A,ax\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov ax,base\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mul posicion\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "mov posicion,ax\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "dec si\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "loop ciclo\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "ret\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "capnum endp\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "END_PROGRAM:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 4CH\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "END MAIN\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                    }
                    else
                    {
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ".STACK 100H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        CondicionesEnsamblador(txtCondicionV.Text);
                        CondicionV = Salida;
                        CondicionesEnsamblador(txtCondicionF.Text);
                        CondicionF = Salida;

                        foreach (DataGridViewRow fila in dtgEnsamblador.Rows)
                        {
                            if (!fila.IsNewRow) // Verifica si la fila no es la fila nueva de ingreso de datos
                            {
                                // Accede a los valores de las celdas de cada fila
                                string valorColumna1 = fila.Cells["Variable"].Value.ToString();
                                string valorColumna2 = fila.Cells["Dato 1"].Value.ToString();
                                string valorColumna3 = fila.Cells["Dato 2"].Value.ToString();
                                if (contador == 1)
                                {
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + ".DATA\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna3 + " DW ? ;Cambiar valor\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                }
                                if (valorColumna2 == "AX" && contador == 1)
                                {
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + ".CODE\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AX, @DATA\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV DS, AX\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna1 + " " + valorColumna2
                                        + ", " + valorColumna3 + "\n";
                                }
                                if (valorColumna1 == "MOV" && valorColumna2 != "AX")
                                {
                                    rtbEnsamblador.Text = rtbEnsamblador.Text + valorColumna1 + " " + valorColumna2 + ", "
                                        + valorColumna3 + "\n";
                                }
                                //NO ASIGNADO
                                if (valorColumna1 == "CMP" && contador == 3)
                                {
                                    CMP1 = valorColumna1 + " " + valorColumna2 + ", " + valorColumna3;
                                }
                                if (valorColumna1 == "CMP" && contador == 5)
                                {
                                    CMP2 = valorColumna1 + " " + valorColumna2 + ", " + valorColumna3;
                                }
                                contador++;
                            }
                        }
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + CMP1 + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JGE CMP1\n";
                        if (CMP2 != "")
                        {
                            rtbEnsamblador.Text = rtbEnsamblador.Text + CMP2 + "\n";
                            rtbEnsamblador.Text = rtbEnsamblador.Text + "JLE CMP2\n";
                            Existe = true;
                        }
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ";Si se cumplen ambas\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + CondicionV + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JMP END_PROGRAM\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "CMP1:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ";Si no se cumple la primera condicion\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + CondicionF + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JMP END_PROGRAM\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        if (Existe == true)
                        {
                            rtbEnsamblador.Text = rtbEnsamblador.Text + "CMP2:\n";
                            rtbEnsamblador.Text = rtbEnsamblador.Text + ";Si no se cumple la segunda condicion\n";
                            rtbEnsamblador.Text = rtbEnsamblador.Text + CondicionF + "\n";
                            rtbEnsamblador.Text = rtbEnsamblador.Text + "JMP END_PROGRAM\n";
                            rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        }
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "END_PROGRAM:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ";Mostrar el valor en decimal\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV BX, 10\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV CX, 0\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "CONVERT_LOOP:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "XOR DX, DX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "DIV BX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "PUSH DX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INC CX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "CMP AX, 0\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JNZ CONVERT_LOOP\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "DISPLAY_LOOP:\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + ";Convertir a ASCII\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "POP DX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "ADD DL, '0'\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 02H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "DEC CX\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "JNZ DISPLAY_LOOP\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "MOV AH, 4CH\n";
                        rtbEnsamblador.Text = rtbEnsamblador.Text + "INT 21H\n";
                    }
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivos ASM (*.asm)|*.asm|Todos los archivos (*.*)|*.*";
            saveFileDialog.Title = "Guardar archivo ASM";
            saveFileDialog.DefaultExt = "asm";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Guardar el contenido en el archivo seleccionado
                string filePath = saveFileDialog.FileName;
                SaveAsmFile(filePath, rtbEnsamblador.Text);
                Console.WriteLine("Archivo guardado exitosamente en: " + filePath);
            }
        }

        static void SaveAsmFile(string filePath, string content)
        {
            try
            {
                // Escribir el contenido en el archivo
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al guardar el archivo: " + ex.Message);
            }
        }

        private void label28_Click(object sender, EventArgs e)
        {

        }

        private void btnTriplo_Click(object sender, EventArgs e)
        {
            TriploAritmetico.Clear();
            listaDatos.Clear();
            TriploLR.Clear();
            dtgALR.Rows.Clear();
            TriploEnsamblador.Clear();
            if (ExisteDoWhile == true)
            {
                Cadena = txtCondicionWhile.Text;
                ExpresionLR();
                VLR = VLR + LRaux;
                SepararLR();
                AgregarLR();
            }
            if(ExisteDoWhile == false)
            {
                if (ExisteLR == false)
                {
                    Cadena = txtExpresionInfija.Text;
                    ExpresionAritmetica();
                    Global = Global + Inicio;
                    SepararAritmetico();
                    AgregarAritmetico();
                }
                if (ExisteLR == true)
                {
                    Cadena = txtExpresionLR.Text;
                    ExpresionLR();
                    VLR = VLR + LRaux;
                    SepararLR();
                    AgregarLR();
                }
            }
            MessageBox.Show("Generado", "Triplo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}