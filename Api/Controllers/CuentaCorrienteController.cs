using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api
{
    public class CuentaCorriente
    {
        public int IdPago{get;set;}
        public int NroPedido{get;set;}
        public DateTime Fecha{get;set;}
        public double Debe{get;set;}
        public double Haber{get;set;}
        public string FechaStr{get;set;}
        public string Metodo{get;set;}
        public string Observacion{get;set;}
    }
    public class Pago
    {
        [Key]
        public int IdPago{get;set;}
        public int IdCliente{get;set;}
        public double Monto{get;set;}
        public DateTime Fecha{get;set;}
        public string Metodo{get;set;}
        public  string Observacion{get;set;}
    }
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CuentaCorrienteController : Controller
    {
        Db db;
        public CuentaCorrienteController(Db dbInjected)
        {
            db = dbInjected;
        }

        [HttpGet("{idCliente}")]
        public object GetCtaCte(int idCliente, DateTime fechaDesde, DateTime fechaHasta)
        {
            //var file = new StreamReader("").ReadToEnd();
            var pagos = db.Pago.Where(p => p.IdCliente == idCliente && 
                                p.Fecha >= fechaDesde && 
                                p.Fecha <= fechaHasta)
                            .OrderBy(p => p.IdPago);//.Take(100);
            var ventas = 
                db.Venta.Where(v => v.Cliente == idCliente && 
                                v.FechaBaja > DateTime.Now && 
                                v.FechaVenta.Date >= fechaDesde && 
                                v.FechaVenta.Date <= fechaHasta)
                        .OrderByDescending(v => v.IdVenta)
                        //.Take(100)
                        .Select(v => new {
                            v.IdVenta,
                            v.FechaVenta,
                            v.Total
                        });
            var cc = new List<CuentaCorriente>();
            foreach(var venta in ventas)
            {
                cc.Add(new CuentaCorriente{
                    NroPedido = venta.IdVenta,
                    Fecha = venta.FechaVenta.Date,
                    FechaStr = venta.FechaVenta.ToString("dd/MM/yyyy"),
                    Haber = 0,
                    Debe = venta.Total,
                    Metodo = "",
                    Observacion = ""
                });
            }
            foreach(var pago in pagos)
            {
                cc.Add(new CuentaCorriente{
                    //IdPago = pago.IdPago,
                    Fecha = pago.Fecha,
                    FechaStr = pago.Fecha.ToString("dd/MM/yyyy"),
                    Haber = pago.Monto,
                    Debe = 0,
                    Metodo = pago.Metodo,
                    Observacion = pago.Observacion
                });
            }
            

            cc = cc.OrderBy(c => c.Fecha).ToList();
            var diferencia = db.Venta.Where(v => v.Cliente == idCliente && v.FechaBaja > DateTime.Now).Sum(v => v.Total) - 
                            db.Pago.Where(p => p.IdCliente == idCliente).Sum(p => p.Monto);
            return new {
                Cuenta = cc,
                Diferencia = diferencia,
                Cliente = db.Cliente.FirstOrDefault(c => c.IdCliente == idCliente)
            };
        }
        [HttpGet("pagos/{idCliente}")]
        public IActionResult GetPagos(int idCliente)
        {
            return Ok(db.Pago.Where(p => p.IdCliente == idCliente)
                    .OrderByDescending(p => p.Fecha)
                    .Take(10)
                    .OrderBy(p => p.Fecha)
                    .Select(p => new {
                        IdPago = p.IdPago,
                        Fecha = p.Fecha.ToString("dd/MM/yyyy"),
                        Monto = p.Monto,
                        Metodo = p.Metodo
                    })
                    .ToList());
        }
        [HttpGet("pago/{id}")]
        public Pago GetDetallePago(int id)
        {
            return db.Pago.FirstOrDefault(p => p.IdPago == id);
        }

        [HttpPost("pago")]
        public IActionResult PostPago(Pago data)
        {
            //data.Fecha = DateTime.Now;

            db.Add(data);
            db.SaveChanges();
            return Ok();
        }
        [HttpDelete("pago")]
        public IActionResult EliminarPago(int idPago)
        {
            db.Pago.Remove(db.Pago.FirstOrDefault(p => p.IdPago == idPago));
            db.SaveChanges();
            return Ok();
        }
    }
}