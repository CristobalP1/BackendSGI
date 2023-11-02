using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using BackendSGI.Models;

using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace BackendSGI.Controllers
{
    [EnableCors("ruler")]
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class HojaDeRutaController : ControllerBase
    {
        private readonly string cadenaSQL;

        public HojaDeRutaController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL");
        }

        [HttpGet]
        [Route("listaHr/{idUsuario}")]
        public async Task<IActionResult> ListaHr(int idUsuario)
        {
            List<HojaDeRuta> hojaDeRuta = new List<HojaDeRuta>();

            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("sp_ListarHojasDeRutaUsuario", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.Add(new SqlParameter("@Id_usuario", idUsuario));

                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        while (await rd.ReadAsync())
                        {
                            hojaDeRuta.Add(new HojaDeRuta()
                            {
                                IdHojaRuta = rd.GetInt32("Id_hoja_ruta"),
                                Destino = rd["Destino"].ToString(),
                                Fecha = rd["Fecha"].ToString(),
                                Tracto = rd["Tracto"].ToString(),
                                Rampla = rd["Rampla"].ToString(),
                                Estado = rd["Estado"].ToString(),
                            });
                        }
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { message = "OK", response = hojaDeRuta });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = error.Message, response = hojaDeRuta });
            }
        }


        [HttpGet]
        [Route("listarNotaVenta/{idHojaRuta}")]
        public async Task<IActionResult> ListaNotaVenta(int idHojaRuta)
        {
            List<NotaDeVenta> notaVenta = new List<NotaDeVenta>();

            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("sp_ListarNotasVentaPorHR", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.Add(new SqlParameter("@Id_hoja_ruta", idHojaRuta));

                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        while (await rd.ReadAsync())
                        {
                            notaVenta.Add(new NotaDeVenta()
                            {
                                IdNotaVenta = rd.GetInt32("Id_notaVenta"),
                                Fecha = rd["Fecha"].ToString(),
                                Cliente = rd["Cliente"].ToString(),
                                Rut = rd["Rut"].ToString(),
                                TipoViaje = rd["TipoViaje"].ToString(),
                                Direccion = rd["Dirección"].ToString(),
                                Estado = rd["Estado"].ToString(),
                            });
                        }
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { message = "OK", response = notaVenta });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = error.Message, response = notaVenta });
            }
        }


        [HttpGet]
        [Route("listarSolicitudes/{idNotaVenta}")]
        public async Task<IActionResult> ListaSolicitudes(int idNotaVenta)
        {
            List<SolicitudNotaVenta> solicitudes = new List<SolicitudNotaVenta>();

            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("sp_ListarSolicitudesPorNotaVenta", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.Add(new SqlParameter("@Id_notaVenta", idNotaVenta));

                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        while (await rd.ReadAsync())
                        {
                            solicitudes.Add(new SolicitudNotaVenta()
                            {
                                IdSolicitud = rd.GetInt32("Id_solicitud"),
                                Fecha = rd["Fecha"].ToString(),
                                Concepto = rd["Concepto"].ToString(),
                                Monto = Convert.ToInt32(rd.GetDecimal("Monto")),
                                Estado = rd["Estado"].ToString(),
                                Aprobador = rd["Aprobador"].ToString(),
                                Comentario = rd["Comentario"].ToString(),
                            });
                        }
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { message = "OK", response = solicitudes });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = error.Message, response = solicitudes });
            }
        }


        [HttpPost]
        [Route("insertarSolicitud")]
        public async Task<IActionResult> InsertarSolicitud([FromBody] SolicitudModel solicitud)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("sp_InsertarSolicitud", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.Add(new SqlParameter("@Fecha", solicitud.Fecha));
                    cmd.Parameters.Add(new SqlParameter("@Id_concepto", solicitud.IdConcepto));
                    cmd.Parameters.Add(new SqlParameter("@Monto", solicitud.Monto));
                    cmd.Parameters.Add(new SqlParameter("@Id_estado", solicitud.IdEstado));
                    cmd.Parameters.Add(new SqlParameter("@Aprobador", solicitud.Aprobador));
                    cmd.Parameters.Add(new SqlParameter("@Comentario", solicitud.Comentario));
                    cmd.Parameters.Add(new SqlParameter("@Id_notaVenta", solicitud.IdNotaVenta));

                    await cmd.ExecuteNonQueryAsync();
                }

                return StatusCode(StatusCodes.Status200OK, new { message = "OK", response = "Solicitud insertada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("insertarEvidencia")]
        public async Task<IActionResult> InsertarEvidencia([FromForm] InsertarEvidencia insertar)
        {
            try
            {
                byte[] photoBytes;
                using (var ms = new MemoryStream())
                {
                    await insertar.Photo.CopyToAsync(ms);
                    photoBytes = ms.ToArray();
                }

                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("sp_InsertarEvidencia", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Id_solicitud", insertar.IdSolicitud));
                    cmd.Parameters.Add(new SqlParameter("@Photo", photoBytes));

                    cmd.ExecuteNonQuery();
                }

                return StatusCode(StatusCodes.Status200OK, new { message = "OK", response = "Evidencia insertada correctamente" });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = error.Message });
            }
        }

        [HttpPost]
        [Route("insertarDocumento")]
        public async Task<IActionResult> InsertarDocumento([FromForm] InsertarDocumento documento)
        {
            try
            {
                byte[] archivoBytes;
                using (var ms = new MemoryStream())
                {
                    await documento.Archivo.CopyToAsync(ms);
                    archivoBytes = ms.ToArray();
                }

                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("sp_InsertarDocumento", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Nombre", documento.Nombre));
                    cmd.Parameters.Add(new SqlParameter("@Archivo", archivoBytes));
                    cmd.Parameters.Add(new SqlParameter("@Id_notaVenta", documento.IdNotaVenta));

                    cmd.ExecuteNonQuery();
                }

                return StatusCode(StatusCodes.Status200OK, new { message = "OK", response = "Documento insertado correctamente" });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = error.Message });
            }
        }

        [HttpPost]
        [Route("cambiarEstadoARevision")]
        public async Task<IActionResult> CambiarEstadoNotaVentaARevision([FromBody] CambiarEstadoNotaVenta nv)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("sp_CambiarEstadoNotaVentaARevision", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Id_notaVenta", nv.IdNotaVenta));

                    await cmd.ExecuteNonQueryAsync();
                }

                return StatusCode(StatusCodes.Status200OK, new { message = "OK", response = "Estado de la nota de venta actualizado a revisión" });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = error.Message });
            }
        }

        [HttpPost]
        [Route("cambiarEstadoNotaVentaASolicitado")]
        public async Task<IActionResult> CambiarEstadoNotaVentaASolicitado([FromBody] CambiarEstadoNotaVenta nv)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("sp_CambiarEstadoNotaVentaASolicitado", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Id_notaVenta", nv.IdNotaVenta));

                    await cmd.ExecuteNonQueryAsync();
                }

                return StatusCode(StatusCodes.Status200OK, new { message = "OK", response = "Estado de nota de venta actualizado a 'Solicitado'" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("verDocumentoNotaVenta/{idNotaVenta}")]
        public async Task<IActionResult> VerDocumentoNotaVenta(int idNotaVenta)
        {
            try
            {
                Documento documento = null;

                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("sp_VerDocumentoNotaVenta", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Id_notaVenta", idNotaVenta));

                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        if (await rd.ReadAsync())
                        {
                            documento = new Documento()
                            {
                                IdDocumento = rd.GetInt32("Id_documento"),
                                Nombre = rd["Nombre"].ToString(),
                                Archivo = rd["Archivo"] as byte[]
                            };
                        }
                    }
                }

                if (documento != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new { message = "OK", response = documento });
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "Documento no encontrado" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("verEvidenciaSolicitud/{idSolicitud}")]
        public async Task<IActionResult> VerEvidenciaSolicitud(int idSolicitud)
        {
            try
            {
                List<Evidencia> listaEvidencias = new List<Evidencia>();

                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("sp_VerEvidenciaSolicitud", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Id_solicitud", idSolicitud));

                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        while (rd.Read())
                        {
                            var evidencia = new Evidencia()
                            {
                                IdEvidencia = rd.GetInt32("Id_evidencia"),
                                Photo = (byte[])rd["Photo"]
                            };

                            listaEvidencias.Add(evidencia);
                        }
                    }
                }

                if (listaEvidencias.Count > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new { message = "OK", response = listaEvidencias });
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "Evidencia no encontrada" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }


        [HttpDelete]
        [Route("eliminarDocumento/{idNotaVenta}")]
        public async Task<IActionResult> EliminarDocumentoAsync(int idNotaVenta)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    await conexion.OpenAsync();
                    using (var cmd = new SqlCommand("sp_EliminarDocumentoNotaVenta", conexion))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Id_notaVenta", idNotaVenta));

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Documento eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

    }
}
