namespace BackendSGI.Models
{
    public class Documento
    {
        public int IdDocumento { get; set; }
        public string Nombre { get; set; }
        public byte[] Archivo { get; set; }
    }
}
