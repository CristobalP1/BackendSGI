namespace BackendSGI.Models
{
    public class InsertarDocumento
    {
        public IFormFile Archivo { get; set; }
        public string Nombre { get; set; }
        public int IdNotaVenta { get; set; }
    }
}
