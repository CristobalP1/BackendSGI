namespace BackendSGI.Models
{
    public class SolicitudNotaVenta
    {
        public int IdSolicitud { get; set; }
        public string Fecha { get; set; }
        public string Concepto { get; set; }
        public int Monto { get; set; }
        public string Estado { get; set; }
        public string Aprobador { get; set; }
        public string Comentario { get; set; }
}
}
