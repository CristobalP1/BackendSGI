namespace BackendSGI.Models
{
    public class SolicitudModel
    {
        public DateTime Fecha { get; set; }
        public int IdConcepto { get; set; }
        public decimal Monto { get; set; }
        public int IdEstado { get; set; }
        public string Aprobador { get; set; }
        public string Comentario { get; set; }
        public int IdNotaVenta { get; set; }
    }
}
