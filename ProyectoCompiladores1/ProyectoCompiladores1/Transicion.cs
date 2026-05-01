namespace ProyectoCompiladores1.Models
{
    /// <summary>
    /// Representa una transición entre dos estados del AFN.
    /// El símbolo '\0' representa una transición épsilon (ε).
    /// </summary>
    public class Transicion
    {
        public Estado Origen { get; set; }
        public char Simbolo { get; set; }   // '\0' = épsilon
        public Estado Destino { get; set; }

        public Transicion(Estado origen, char simbolo, Estado destino)
        {
            Origen = origen;
            Simbolo = simbolo;
            Destino = destino;
        }

        public bool EsEpsilon => Simbolo == '\0';

        public override string ToString()
        {
            string sym = EsEpsilon ? "ε" : Simbolo.ToString();
            return $"{Origen} --{sym}--> {Destino}";
        }
    }
}
