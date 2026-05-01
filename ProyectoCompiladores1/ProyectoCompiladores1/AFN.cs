using System.Collections.Generic;

namespace ProyectoCompiladores1.Models
{
    /// <summary>
    /// Autómata Finito No Determinista construido con el algoritmo de Thompson.
    /// </summary>
    public class AFN
    {
        public Estado EstadoInicial { get; set; }
        public Estado EstadoAceptacion { get; set; }
        public List<Estado> Estados { get; private set; }
        public List<Transicion> Transiciones { get; private set; }

        public AFN()
        {
            Estados = new List<Estado>();
            Transiciones = new List<Transicion>();
        }

        /// <summary>
        /// Agrega un estado al AFN.
        /// </summary>
        public void AgregarEstado(Estado estado)
        {
            Estados.Add(estado);
        }

        /// <summary>
        /// Agrega una transición al AFN.
        /// </summary>
        public void AgregarTransicion(Transicion transicion)
        {
            Transiciones.Add(transicion);
        }

        /// <summary>
        /// Agrega una transición épsilon entre dos estados.
        /// </summary>
        public void AgregarEpsilon(Estado origen, Estado destino)
        {
            Transiciones.Add(new Transicion(origen, '\0', destino));
        }

        /// <summary>
        /// Cierre épsilon: todos los estados alcanzables desde el conjunto dado
        /// usando solo transiciones épsilon.
        /// </summary>
        public HashSet<Estado> CierreEpsilon(HashSet<Estado> estados)
        {
            var resultado = new HashSet<Estado>(estados);
            var pila = new Stack<Estado>(estados);

            while (pila.Count > 0)
            {
                Estado actual = pila.Pop();
                foreach (var trans in Transiciones)
                {
                    if (trans.Origen == actual && trans.EsEpsilon)
                    {
                        if (!resultado.Contains(trans.Destino))
                        {
                            resultado.Add(trans.Destino);
                            pila.Push(trans.Destino);
                        }
                    }
                }
            }
            return resultado;
        }

        /// <summary>
        /// Cierre épsilon desde un único estado.
        /// </summary>
        public HashSet<Estado> CierreEpsilon(Estado estado)
        {
            return CierreEpsilon(new HashSet<Estado> { estado });
        }

        /// <summary>
        /// Mueve: dado un conjunto de estados y un símbolo, retorna el conjunto
        /// de estados alcanzables mediante ese símbolo.
        /// </summary>
        public HashSet<Estado> Mover(HashSet<Estado> estados, char simbolo)
        {
            var resultado = new HashSet<Estado>();
            foreach (var estado in estados)
            {
                foreach (var trans in Transiciones)
                {
                    if (trans.Origen == estado && trans.Simbolo == simbolo)
                    {
                        resultado.Add(trans.Destino);
                    }
                }
            }
            return resultado;
        }

        /// <summary>
        /// Simula el AFN con una cadena de entrada.
        /// Retorna true si la cadena es aceptada.
        /// </summary>
        public bool Simular(string entrada)
        {
            HashSet<Estado> actuales = CierreEpsilon(EstadoInicial);

            foreach (char c in entrada)
            {
                HashSet<Estado> siguientes = Mover(actuales, c);
                actuales = CierreEpsilon(siguientes);
                if (actuales.Count == 0) return false;
            }

            // Verifica si algún estado actual es de aceptación
            foreach (var estado in actuales)
            {
                if (estado.EsAceptacion) return true;
            }
            return false;
        }

        /// <summary>
        /// Simula el AFN sobre una subcadena de la entrada y retorna cuántos
        /// caracteres fueron consumidos (el prefijo más largo aceptado), o -1 si ninguno.
        /// </summary>
        public int SimularMaximo(string entrada, int inicio)
        {
            HashSet<Estado> actuales = CierreEpsilon(EstadoInicial);
            int ultimaAceptacion = -1;

            for (int i = inicio; i < entrada.Length; i++)
            {
                char c = entrada[i];
                HashSet<Estado> siguientes = Mover(actuales, c);
                actuales = CierreEpsilon(siguientes);

                if (actuales.Count == 0) break;

                foreach (var estado in actuales)
                {
                    if (estado.EsAceptacion)
                    {
                        ultimaAceptacion = i + 1; // posición exclusiva
                        break;
                    }
                }
            }

            return ultimaAceptacion; // -1 si nunca aceptó
        }
    }
}
