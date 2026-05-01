namespace ProyectoCompiladores1.Models
{
    /// <summary>
    /// Representa un estado dentro de un Autómata Finito No Determinista (AFN).
    /// </summary>
    public class Estado
    {
        private static int _contador = 0;

        public int Id { get; private set; }
        public bool EsAceptacion { get; set; }

        public Estado()
        {
            Id = _contador++;
            EsAceptacion = false;
        }

        /// <summary>
        /// Reinicia el contador global de IDs (útil al construir un nuevo AFN).
        /// </summary>
        public static void ReiniciarContador()
        {
            _contador = 0;
        }

        public override string ToString()
        {
            return $"q{Id}{(EsAceptacion ? "*" : "")}";
        }
    }
}
