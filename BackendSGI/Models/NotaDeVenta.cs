namespace BackendSGI.Models
{
    public class NotaDeVenta
    {
        public int IdNotaVenta { get; set; }
        public string Fecha { get; set;}
        public string Cliente { get; set;}
        public string Rut { get; set;}
        public string TipoViaje { get; set;}
        public string Direccion { get; set;}
        public string Estado { get; set;}
    }
}
