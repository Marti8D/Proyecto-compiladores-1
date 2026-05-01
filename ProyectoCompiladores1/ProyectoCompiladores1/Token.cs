namespace ProyectoCompiladores1.Models
{
    /// <summary>
    /// Representa un token reconocido durante el análisis léxico.
    /// </summary>
    public class Token
    {
        public string Lexema { get; set; }
        public string Tipo { get; set; }
        public string Valor { get; set; }
        public int Fila { get; set; }
        public int Columna { get; set; }

        public Token(string lexema, string tipo, string valor, int fila, int columna)
        {
            Lexema = lexema;
            Tipo = tipo;
            Valor = valor;
            Fila = fila;
            Columna = columna + 1;
        }

        public override string ToString()
        {
            return $"[{Tipo}] '{Lexema}' en ({Fila},{Columna})";
        }
    }

    /// <summary>
    /// Representa un error léxico detectado durante el análisis.
    /// </summary>
    public class ErrorLexico
    {
        public string Caracter { get; set; }
        public int Fila { get; set; }
        public int Columna { get; set; }
        public string Descripcion { get; set; }

        public ErrorLexico(string caracter, int fila, int columna, string descripcion)
        {
            Caracter = caracter;
            Fila = fila;
            Columna = columna + 1;
            Descripcion = descripcion;
        }

        public override string ToString()
        {
            return $"Error en ({Fila},{Columna}): '{Caracter}' - {Descripcion}";
        }
    }
}
