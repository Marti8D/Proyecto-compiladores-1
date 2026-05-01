using System;
using System.Collections.Generic;
using System.Text;
using ProyectoCompiladores1.Models;

namespace ProyectoCompiladores1.Core
{
    /// <summary>
    /// Implementa el algoritmo de Thompson para convertir una expresión regular
    /// en un Autómata Finito No Determinista (AFN).
    ///
    /// Sintaxis soportada:
    ///   |       Unión
    ///   .       Concatenación (insertada automáticamente)
    ///   *       Cerradura de Kleene
    ///   +       Una o más repeticiones
    ///   ?       Cero o una vez (opcional)
    ///   ()      Agrupación
    ///   \X      Escape de carácter especial X  (ej: \+ \* \( \) \| \. \?)
    ///   [abc]   Clase de caracteres explícita   → (a|b|c)
    ///   [a-z]   Clase de caracteres con rango   → (a|b|...|z)
    ///   [^abc]  Clase negada (no soportada aún, se ignora el ^)
    /// </summary>
    public static class Thompson
    {
        // ─────────────────────────────────────────────
        //  PUNTO DE ENTRADA PÚBLICO
        // ─────────────────────────────────────────────

        public static AFN ConstruirAFN(string regex)
        {
            Estado.ReiniciarContador();

            // Expandir clases de caracteres [...]  antes de tokenizar
            string expandida = ExpandirClases(regex);

            List<RegexToken> tokens = Tokenizar(expandida);
            List<RegexToken> conCat = InsertarConcatenacion(tokens);
            List<RegexToken> postfija = InfijaAPostfija(conCat);
            return ConstruirDesdePostfija(postfija);
        }

        // ─────────────────────────────────────────────
        //  PRE-PASO – EXPANDIR CLASES DE CARACTERES [...]
        // ─────────────────────────────────────────────

        /// <summary>
        /// Convierte notaciones de clase en uniones explícitas entre paréntesis.
        /// Ejemplos:
        ///   [a-z]    →  (a|b|c|...|z)
        ///   [0-9]    →  (0|1|2|...|9)
        ///   [aeiou]  →  (a|e|i|o|u)
        ///   [a-zA-Z] →  (a|b|...|z|A|B|...|Z)
        /// </summary>
        private static string ExpandirClases(string regex)
        {
            var resultado = new StringBuilder();
            int i = 0;

            while (i < regex.Length)
            {
                char c = regex[i];

                if (c == '\\' && i + 1 < regex.Length)
                {
                    // Escape: pasar ambos caracteres tal cual
                    resultado.Append(c);
                    resultado.Append(regex[i + 1]);
                    i += 2;
                    continue;
                }

                if (c == '[')
                {
                    // Buscar el cierre ']'
                    int cierre = regex.IndexOf(']', i + 1);
                    if (cierre < 0)
                        throw new InvalidOperationException("Clase de caracteres '[' sin cerrar con ']'.");

                    string contenido = regex.Substring(i + 1, cierre - i - 1);
                    string expansion = ExpandirContenidoClase(contenido);
                    resultado.Append(expansion);
                    i = cierre + 1;
                    continue;
                }

                resultado.Append(c);
                i++;
            }

            return resultado.ToString();
        }

        /// <summary>
        /// Expande el contenido interno de una clase (sin los corchetes).
        /// Soporta rangos X-Y y caracteres individuales.
        /// </summary>
        private static string ExpandirContenidoClase(string contenido)
        {
            var chars = new List<char>();
            int i = 0;

            // Ignorar negación '^' al inicio (no implementada, se trata igual)
            if (i < contenido.Length && contenido[i] == '^')
                i++;

            while (i < contenido.Length)
            {
                char actual = contenido[i];

                // Rango: X-Y
                if (i + 2 < contenido.Length && contenido[i + 1] == '-')
                {
                    char desde = actual;
                    char hasta = contenido[i + 2];

                    if (desde > hasta)
                        throw new InvalidOperationException(
                            $"Rango inválido en clase de caracteres: '{desde}-{hasta}'.");

                    for (char ch = desde; ch <= hasta; ch++)
                        chars.Add(ch);

                    i += 3;
                }
                else
                {
                    chars.Add(actual);
                    i++;
                }
            }

            if (chars.Count == 0)
                throw new InvalidOperationException("Clase de caracteres vacía '[]'.");

            if (chars.Count == 1)
                return EscaparSiEspecial(chars[0]).ToString();

            // Construir (c1|c2|...|cn)
            var sb = new StringBuilder("(");
            for (int j = 0; j < chars.Count; j++)
            {
                if (j > 0) sb.Append('|');
                sb.Append(EscaparSiEspecial(chars[j]));
            }
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// Devuelve la representación del carácter en la regex expandida,
        /// escapando los que son operadores del motor.
        /// </summary>
        private static string EscaparSiEspecial(char c)
        {
            switch (c)
            {
                case '(':
                case ')':
                case '|':
                case '*':
                case '+':
                case '?':
                case '.':
                case '\\':
                    return "\\" + c;
                default:
                    return c.ToString();
            }
        }

        // ─────────────────────────────────────────────
        //  REPRESENTACIÓN INTERNA DE TOKENS DE REGEX
        // ─────────────────────────────────────────────

        private class RegexToken
        {
            public char Simbolo { get; }
            public bool EsLiteral { get; }

            public RegexToken(char simbolo, bool esLiteral)
            {
                Simbolo = simbolo;
                EsLiteral = esLiteral;
            }

            public bool EsOperando => EsLiteral || !EsOperadorGlobal(Simbolo);
            public bool EsOperador => !EsLiteral && EsOperadorGlobal(Simbolo);
            public bool EsApertura => !EsLiteral && Simbolo == '(';
            public bool EsCierre => !EsLiteral && Simbolo == ')';
        }

        private static bool EsOperadorGlobal(char c)
        {
            return c == '|' || c == '.' || c == '*' || c == '+' || c == '?' || c == '(' || c == ')';
        }

        // ─────────────────────────────────────────────
        //  PASO 1 – TOKENIZAR (maneja escape \)
        // ─────────────────────────────────────────────

        private static List<RegexToken> Tokenizar(string regex)
        {
            var lista = new List<RegexToken>();
            int i = 0;
            while (i < regex.Length)
            {
                char c = regex[i];
                if (c == '\\' && i + 1 < regex.Length)
                {
                    lista.Add(new RegexToken(regex[i + 1], true));
                    i += 2;
                }
                else
                {
                    bool esLiteral = !EsOperadorGlobal(c);
                    lista.Add(new RegexToken(c, esLiteral));
                    i++;
                }
            }
            return lista;
        }

        // ─────────────────────────────────────────────
        //  PASO 2 – INSERTAR CONCATENACIÓN EXPLÍCITA
        // ─────────────────────────────────────────────

        private static List<RegexToken> InsertarConcatenacion(List<RegexToken> tokens)
        {
            var resultado = new List<RegexToken>();
            var concat = new RegexToken('.', false);

            for (int i = 0; i < tokens.Count; i++)
            {
                var actual = tokens[i];
                resultado.Add(actual);

                if (i + 1 < tokens.Count)
                {
                    var siguiente = tokens[i + 1];

                    bool actualTermina = actual.EsOperando || actual.EsCierre ||
                                          (!actual.EsLiteral && (actual.Simbolo == '*' || actual.Simbolo == '+' || actual.Simbolo == '?'));
                    bool siguienteInicia = siguiente.EsOperando || siguiente.EsApertura;

                    if (actualTermina && siguienteInicia)
                        resultado.Add(concat);
                }
            }
            return resultado;
        }

        // ─────────────────────────────────────────────
        //  PASO 3 – INFIJA A POSTFIJA (Shunting Yard)
        // ─────────────────────────────────────────────

        private static int Precedencia(RegexToken tok)
        {
            if (tok.EsLiteral) return 0;
            switch (tok.Simbolo)
            {
                case '|': return 1;
                case '.': return 2;
                case '*':
                case '+':
                case '?': return 3;
                default: return 0;
            }
        }

        private static List<RegexToken> InfijaAPostfija(List<RegexToken> tokens)
        {
            var salida = new List<RegexToken>();
            var pila = new Stack<RegexToken>();

            foreach (var tok in tokens)
            {
                if (tok.EsOperando)
                {
                    salida.Add(tok);
                }
                else if (tok.EsApertura)
                {
                    pila.Push(tok);
                }
                else if (tok.EsCierre)
                {
                    while (pila.Count > 0 && !pila.Peek().EsApertura)
                        salida.Add(pila.Pop());

                    if (pila.Count == 0)
                        throw new InvalidOperationException("Paréntesis desbalanceados en la expresión regular.");

                    pila.Pop();
                }
                else if (tok.EsOperador)
                {
                    while (pila.Count > 0 && !pila.Peek().EsApertura &&
                           Precedencia(pila.Peek()) >= Precedencia(tok))
                    {
                        salida.Add(pila.Pop());
                    }
                    pila.Push(tok);
                }
            }

            while (pila.Count > 0)
            {
                var top = pila.Pop();
                if (top.EsApertura)
                    throw new InvalidOperationException("Paréntesis desbalanceados en la expresión regular.");
                salida.Add(top);
            }

            return salida;
        }

        // ─────────────────────────────────────────────
        //  PASO 4 – CONSTRUIR AFN DESDE POSTFIJA
        // ─────────────────────────────────────────────

        private static AFN ConstruirDesdePostfija(List<RegexToken> postfija)
        {
            var pila = new Stack<AFN>();

            foreach (var tok in postfija)
            {
                if (tok.EsOperando)
                {
                    pila.Push(AFNBasico(tok.Simbolo));
                }
                else
                {
                    switch (tok.Simbolo)
                    {
                        case '.':
                            if (pila.Count < 2) throw new InvalidOperationException("Expresión mal formada: faltan operandos para concatenación.");
                            var b = pila.Pop();
                            var a = pila.Pop();
                            pila.Push(Concatenar(a, b));
                            break;

                        case '|':
                            if (pila.Count < 2) throw new InvalidOperationException("Expresión mal formada: faltan operandos para unión.");
                            var d = pila.Pop();
                            var e2 = pila.Pop();
                            pila.Push(Union(e2, d));
                            break;

                        case '*':
                            if (pila.Count < 1) throw new InvalidOperationException("Expresión mal formada: falta operando para *.");
                            pila.Push(CerraduraKleene(pila.Pop()));
                            break;

                        case '+':
                            if (pila.Count < 1) throw new InvalidOperationException("Expresión mal formada: falta operando para +.");
                            pila.Push(UnaMas(pila.Pop()));
                            break;

                        case '?':
                            if (pila.Count < 1) throw new InvalidOperationException("Expresión mal formada: falta operando para ?.");
                            pila.Push(Opcional(pila.Pop()));
                            break;

                        default:
                            throw new InvalidOperationException($"Operador desconocido: '{tok.Simbolo}'");
                    }
                }
            }

            if (pila.Count != 1)
                throw new InvalidOperationException($"Expresión regular mal formada (elementos en pila: {pila.Count}).");

            return pila.Pop();
        }

        // ─────────────────────────────────────────────
        //  CONSTRUCCIONES BÁSICAS DE THOMPSON
        // ─────────────────────────────────────────────

        private static AFN AFNBasico(char simbolo)
        {
            var afn = new AFN();
            var inicio = new Estado();
            var fin = new Estado { EsAceptacion = true };

            afn.AgregarEstado(inicio);
            afn.AgregarEstado(fin);
            afn.AgregarTransicion(new Transicion(inicio, simbolo, fin));

            afn.EstadoInicial = inicio;
            afn.EstadoAceptacion = fin;
            return afn;
        }

        private static AFN Concatenar(AFN m1, AFN m2)
        {
            m1.EstadoAceptacion.EsAceptacion = false;
            m1.AgregarEpsilon(m1.EstadoAceptacion, m2.EstadoInicial);
            foreach (var e in m2.Estados) m1.AgregarEstado(e);
            foreach (var t in m2.Transiciones) m1.AgregarTransicion(t);
            m1.EstadoAceptacion = m2.EstadoAceptacion;
            return m1;
        }

        private static AFN Union(AFN m1, AFN m2)
        {
            var afn = new AFN();
            var inicio = new Estado();
            var fin = new Estado { EsAceptacion = true };

            afn.AgregarEstado(inicio);
            afn.AgregarEstado(fin);
            foreach (var e in m1.Estados) afn.AgregarEstado(e);
            foreach (var t in m1.Transiciones) afn.AgregarTransicion(t);
            foreach (var e in m2.Estados) afn.AgregarEstado(e);
            foreach (var t in m2.Transiciones) afn.AgregarTransicion(t);

            afn.AgregarEpsilon(inicio, m1.EstadoInicial);
            afn.AgregarEpsilon(inicio, m2.EstadoInicial);
            m1.EstadoAceptacion.EsAceptacion = false;
            m2.EstadoAceptacion.EsAceptacion = false;
            afn.AgregarEpsilon(m1.EstadoAceptacion, fin);
            afn.AgregarEpsilon(m2.EstadoAceptacion, fin);

            afn.EstadoInicial = inicio;
            afn.EstadoAceptacion = fin;
            return afn;
        }

        private static AFN CerraduraKleene(AFN m)
        {
            var afn = new AFN();
            var inicio = new Estado();
            var fin = new Estado { EsAceptacion = true };

            afn.AgregarEstado(inicio);
            afn.AgregarEstado(fin);
            foreach (var e in m.Estados) afn.AgregarEstado(e);
            foreach (var t in m.Transiciones) afn.AgregarTransicion(t);

            afn.AgregarEpsilon(inicio, m.EstadoInicial);
            afn.AgregarEpsilon(inicio, fin);
            m.EstadoAceptacion.EsAceptacion = false;
            afn.AgregarEpsilon(m.EstadoAceptacion, m.EstadoInicial);
            afn.AgregarEpsilon(m.EstadoAceptacion, fin);

            afn.EstadoInicial = inicio;
            afn.EstadoAceptacion = fin;
            return afn;
        }

        private static AFN UnaMas(AFN m)
        {
            AFN copia = ClonarAFN(m);
            return Concatenar(m, CerraduraKleene(copia));
        }

        private static AFN Opcional(AFN m)
        {
            var afn = new AFN();
            var inicio = new Estado();
            var fin = new Estado { EsAceptacion = true };

            afn.AgregarEstado(inicio);
            afn.AgregarEstado(fin);
            foreach (var e in m.Estados) afn.AgregarEstado(e);
            foreach (var t in m.Transiciones) afn.AgregarTransicion(t);

            afn.AgregarEpsilon(inicio, m.EstadoInicial);
            afn.AgregarEpsilon(inicio, fin);
            m.EstadoAceptacion.EsAceptacion = false;
            afn.AgregarEpsilon(m.EstadoAceptacion, fin);

            afn.EstadoInicial = inicio;
            afn.EstadoAceptacion = fin;
            return afn;
        }

        // ─────────────────────────────────────────────
        //  UTILITARIO: CLONAR AFN
        // ─────────────────────────────────────────────

        private static AFN ClonarAFN(AFN original)
        {
            var mapa = new Dictionary<Estado, Estado>();
            var clon = new AFN();

            foreach (var e in original.Estados)
            {
                var nuevo = new Estado { EsAceptacion = e.EsAceptacion };
                mapa[e] = nuevo;
                clon.AgregarEstado(nuevo);
            }

            foreach (var t in original.Transiciones)
                clon.AgregarTransicion(new Transicion(mapa[t.Origen], t.Simbolo, mapa[t.Destino]));

            clon.EstadoInicial = mapa[original.EstadoInicial];
            clon.EstadoAceptacion = mapa[original.EstadoAceptacion];
            return clon;
        }
    }
}