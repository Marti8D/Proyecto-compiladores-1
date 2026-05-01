using System;
using System.Collections.Generic;
using ProyectoCompiladores1.Models;

namespace ProyectoCompiladores1.Core
{
    /// <summary>
    /// Analizador léxico que recorre una cadena de entrada, identifica tokens
    /// usando los AFN construidos por Thompson y reporta errores léxicos.
    /// </summary>
    public class AnalizadorLexico
    {
        // Pares (AFN, nombre de tipo) registrados en el orden de prioridad
        private readonly List<(AFN afn, string tipo)> _reglas;

        // Tabla de símbolos acumulada entre análisis
        private readonly List<Token> _tablaSimbolos;

        public AnalizadorLexico()
        {
            _reglas       = new List<(AFN, string)>();
            _tablaSimbolos = new List<Token>();
        }

        // ─────────────────────────────────────────────
        //  API PÚBLICA
        // ─────────────────────────────────────────────

        /// <summary>
        /// Registra una expresión regular con su tipo de token asociado.
        /// Las reglas se evalúan en el orden en que se agregan (mayor prioridad primero).
        /// </summary>
        public void AgregarRegla(string regex, string tipoToken)
        {
            if (string.IsNullOrWhiteSpace(regex))
                throw new ArgumentException("La expresión regular no puede estar vacía.");

            AFN afn = Thompson.ConstruirAFN(regex);
            _reglas.Add((afn, tipoToken));
        }

        /// <summary>
        /// Limpia las reglas registradas (no limpia la tabla de símbolos).
        /// </summary>
        public void LimpiarReglas()
        {
            _reglas.Clear();
        }

        /// <summary>
        /// Limpia la tabla de símbolos acumulada.
        /// </summary>
        public void LimpiarTablaSimbolos()
        {
            _tablaSimbolos.Clear();
        }

        /// <summary>
        /// Retorna una copia de la tabla de símbolos actual.
        /// </summary>
        public List<Token> ObtenerTablaSimbolos()
        {
            return new List<Token>(_tablaSimbolos);
        }

        /// <summary>
        /// Analiza la cadena de entrada y retorna los tokens reconocidos y
        /// los errores léxicos encontrados.
        /// </summary>
        public (List<Token> tokens, List<ErrorLexico> errores) Analizar(string entrada)
        {
            var tokens = new List<Token>();
            var errores = new List<ErrorLexico>();

            if (string.IsNullOrEmpty(entrada))
                return (tokens, errores);

            int pos = 0;
            int fila = 1;
            int columnaBase = 1; // columna del inicio de la línea actual

            while (pos < entrada.Length)
            {
                // Saltar espacios y saltos de línea
                if (entrada[pos] == '\n')
                {
                    fila++;
                    columnaBase = pos + 2; // próxima columna 1 en la nueva línea
                    pos++;
                    continue;
                }

                if (entrada[pos] == '\r')
                {
                    pos++;
                    continue;
                }

                if (entrada[pos] == ' ' || entrada[pos] == '\t')
                {
                    pos++;
                    continue;
                }

                int columna = pos - columnaBase + 1;

                // Intentar reconocer el token más largo posible
                int mejorFin = -1;
                string mejorTipo = null;

                foreach (var (afn, tipo) in _reglas)
                {
                    int fin = afn.SimularMaximo(entrada, pos);
                    if (fin > mejorFin)
                    {
                        mejorFin  = fin;
                        mejorTipo = tipo;
                    }
                }

                if (mejorFin > pos)
                {
                    string lexema = entrada.Substring(pos, mejorFin - pos);
                    var token = new Token(lexema, mejorTipo, lexema, fila, columna);
                    tokens.Add(token);
                    AgregarATabla(token);
                    pos = mejorFin;
                }
                else
                {
                    // Carácter no reconocido → error léxico
                    string caracter = entrada[pos].ToString();
                    errores.Add(new ErrorLexico(caracter, fila, columna,
                        $"Carácter '{caracter}' no reconocido por ninguna regla"));
                    pos++;
                }
            }

            return (tokens, errores);
        }

        // ─────────────────────────────────────────────
        //  PRIVADOS
        // ─────────────────────────────────────────────

        /// <summary>
        /// Agrega el token a la tabla de símbolos solo si no existe un entrada
        /// idéntica (mismo lexema y tipo).  De lo contrario, actualiza posición.
        /// La tabla es dinámica: se actualiza con cada análisis.
        /// </summary>
        private void AgregarATabla(Token token)
        {
            // Buscar si ya existe el lexema en la tabla
            bool existe = false;
            for (int i = 0; i < _tablaSimbolos.Count; i++)
            {
                if (_tablaSimbolos[i].Lexema == token.Lexema &&
                    _tablaSimbolos[i].Tipo   == token.Tipo)
                {
                    existe = true;
                    break;
                }
            }

            if (!existe)
            {
                _tablaSimbolos.Add(token);
            }
        }
    }
}
