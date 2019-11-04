using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;

namespace Api
{
    public class Venta
    {
        
        public int IdVenta{get;set;}
        public DateTime FechaVenta{get;set;}
        public DateTime FechaConfirmacion{get;set;}
        public DateTime FechaFacturacion{get;set;}
        public DateTime FechaEsperadaEntrega{get;set;}
        public DateTime FechaBaja{get;set;}
        [MaxLength(20)]
        public string FormaPago{get;set;}
        public int Cliente{get;set;}
        public int UserCarga{get;set;}
        [MaxLength(60)]
        public string ReferenciaCliente{get;set;}
        public double Total{get;set;}

        public string GetEstado()
        {
            if(FechaBaja < DateTime.Now)
                return "Dada de baja";
            else if(FechaFacturacion < DateTime.Now)
                return "Facturada";
            else if (FechaConfirmacion < DateTime.Now)
                return "Confirmada";
            else return "Presupuesto";
        }
    }
    public class ItemVenta
    {
        public int IdItemVenta{get;set;}
        public int IdVenta{get;set;}
        public int IdProducto{get;set;}
        public int Cantidad{get;set;}
        public double PrecioVenta{get;set;}
        public double Impuesto{get;set;}
        public double Total {get;set;}
    }
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    //[Authorize(Policy = "OnlyAdmin")]
    [Route("[controller]")]
    public class VentaController : Controller
    {
        Db db;
        public VentaController(Db dbi)
        {
            db = dbi;
        }
        [HttpGet]
        public object GetVentas(int skip, int take, string search)
        {
            var ventas = db.Venta.OrderByDescending(v => v.FechaEsperadaEntrega).Where(v => v.FechaBaja > DateTime.Now);
            if(!string.IsNullOrEmpty(search))
            {
                var cliente = db.Cliente
                        .Where(c => c.RazonSocial.ToLower()
                        .Contains(search.ToLower())).ToList()
                        .Select(c => c.IdCliente).ToList();
                ventas = ventas.Where(v => cliente.Contains(v.Cliente));
            }
            var ventasFiltradas =             
                        (from v in ventas
                            join c in db.Cliente on v.Cliente equals c.IdCliente
                            //join p in db.Pago on c.IdCliente equals p.IdCliente
                            select new {
                                IdVenta = v.IdVenta.ToString(),
                                Fecha = v.FechaVenta.ToString("dd/MM/yyyy"),
                                FechaEntrega = v.FechaEsperadaEntrega.ToString("dd/MM/yyyy MM/dd/yyyy"),
                                Cliente = c.RazonSocial,
                                Total = v.Total.ToString(),
                                Boton = $"<a href='#/venta/{v.IdVenta}'>Ver</a>"
                            }).Skip(skip).Take(take).ToList();
            return ventasFiltradas;
        }
        // public double GetDebe(int idCliente)
        // {
        //     var sumVentas = ;
        //     var sumPagos = 
        //     return sumVentas - sumPagos;
        // }

        [HttpGet("{id}")]
        public object GetVentaById(int id)
        {            
            var venta = db.Venta.FirstOrDefault(v => v.IdVenta == id);
            var itemsVenta = (from iv in db.ItemVenta
                            join p in db.Producto on iv.IdProducto equals p.IdProducto
                            where iv.IdVenta == id
                            select new{
                                IdProducto = p.IdProducto,
                                NombreProducto = p.NombreProducto,
                                PrecioVenta = iv.PrecioVenta,
                                Impuestos = iv.Impuesto,
                                Cantidad = iv.Cantidad,
                                Total = (iv.PrecioVenta*iv.Impuesto/100+iv.PrecioVenta)*iv.Cantidad
                            });
            Console.WriteLine(db.Cliente.FirstOrDefault(c => c.IdCliente == venta.Cliente).RazonSocial);
            return new{
                Venta = new{
                    IdVenta = venta.IdVenta,
                    //FechaVenta = venta.FechaVenta,
                    RSocialCliente = db.Cliente.FirstOrDefault(c => c.IdCliente == venta.Cliente).RazonSocial,
                    Cliente = venta.Cliente,
                    ReferenciaCliente = venta.ReferenciaCliente,
                    FechaEntrega = venta.FechaEsperadaEntrega.ToString("dd/MM/yyyy"),
                    FormaPago = venta.FormaPago,
                    Total = venta.Total,
                    Estado = venta.GetEstado()
                },
                Items = itemsVenta
            };
        }
        
        [HttpPost]
        public int PostVenta(string dataVenta, string items)
        {
            var itemsVenta = JsonConvert.DeserializeObject<List<ItemVenta>>(items);
            var data = JsonConvert.DeserializeObject<Venta>(dataVenta);
            if(itemsVenta.Count == 0 || data.Cliente == 0)
            {
                throw new Exception("No se ha completado algún parámetro");
            }
            itemsVenta = itemsVenta.Where(i => i.IdProducto != 0 && i.Cantidad != 0).ToList();
            int lastVenta = 0;
            try{
                lastVenta = db.Venta.LastOrDefault().IdVenta +1;
            }catch(NullReferenceException e)
            {
                Console.WriteLine(e.StackTrace);
                lastVenta = 1;
            }
            
            foreach(var itm in itemsVenta)
            {
                itm.IdVenta = lastVenta;
                itm.Total = (itm.PrecioVenta * itm.Impuesto/100+itm.PrecioVenta)*itm.Cantidad;
                //Producto.RestarStock(itm.IdProducto, itm.Cantidad, db);
            }
            data.FechaBaja = new DateTime(2999,1,1);
            data.FechaConfirmacion = new DateTime(2999,1,1);
            data.FechaVenta = DateTime.Now;
            //data.UserCarga = int.Parse(User.FindFirst("IdAcount").Value);
            data.Total = itemsVenta.Sum(i => i.Total);
            db.ItemVenta.AddRange(itemsVenta);
            db.Venta.Add(data);
            db.SaveChanges();
            return data.IdVenta;
        }

        [HttpPost("{id}")]
        public IActionResult EditarVenta(int id, string dataVenta, string items)
        {
            var data = JsonConvert.DeserializeObject<Venta>(dataVenta);
            var venta = db.Venta.FirstOrDefault(v => v.IdVenta == id);   
            
            if(venta.FechaConfirmacion < DateTime.Now)
            {
                return BadRequest("La venta se encuentra confirmada, no se puede ediar");
            }

            var itemsPrevios = db.ItemVenta.Where(i => i.IdVenta == id).ToList();
            var itemsVenta = JsonConvert.DeserializeObject<List<ItemVenta>>(items);
            for(var i = 0; i<itemsVenta.Count; i++)
            {
                itemsVenta[i].IdVenta = id;
            }
            if(itemsPrevios.Except(itemsVenta).Count() > 0 || itemsVenta.Except(itemsPrevios).Count() >0)
            {
                var remover = db.ItemVenta.Where(i => i.IdVenta == id);
                db.ItemVenta.RemoveRange(remover);
            }
            if(itemsVenta.Count == 0)
            {
                throw new Exception("No se ha completado algún parámetro");
            }
            db.ItemVenta.AddRange(itemsVenta);

            venta.Total = itemsVenta.Sum(i => i.Cantidad*(i.PrecioVenta*i.Impuesto/100+i.PrecioVenta));
            venta.ReferenciaCliente = data.ReferenciaCliente;
            venta.FechaEsperadaEntrega = data.FechaEsperadaEntrega;
            venta.FormaPago = data.FormaPago;
            //venta.Cliente = data.Cliente;
            //venta.Comentario = data.Comentario;

            db.SaveChanges();
            return Ok();
        }

        [HttpGet("factura/{id}")]
        public IActionResult Imprimir(int id)
        {
            var venta = db.Venta.Where(v => v.IdVenta == id).Select(v => new{
                IdVenta = v.IdVenta,
                ReferenciaCliente = v.ReferenciaCliente,
                FechaVenta = v.FechaVenta.ToString("dd/MM/yyy"),
                FechaEsperadaEntrega = v.FechaEsperadaEntrega.ToString("dd/MM/yyy"),
                Cliente = v.Cliente,
                Total = v.Total
            }).FirstOrDefault();
            var cliente = db.Cliente.FirstOrDefault(c => c.IdCliente == venta.Cliente);
            var items = (from i in db.ItemVenta
                        join p in db.Producto on i.IdProducto equals p.IdProducto
                        where i.IdVenta == venta.IdVenta
                        select new {
                            Producto = p.NombreProducto,
                            Cantidad = i.Cantidad,
                            Precio = i.PrecioVenta,
                            Impuesto = i.Impuesto,
                            Total = (i.PrecioVenta * i.Impuesto + i.PrecioVenta)*i.Cantidad
                        });

            return Ok(new{
                venta = venta,
                cliente = cliente,
                items = items
            });
        }

        [HttpDelete("{id}")]
        public IActionResult BajaVenta(int id)
        {
            var venta = db.Venta.FirstOrDefault(v => v.IdVenta == id);
            if(venta.FechaConfirmacion < DateTime.Now)
            {   
                var itemsVenta = db.ItemVenta.Where(i => i.IdVenta == id);      
            }
            venta.FechaBaja = DateTime.Now;
            db.SaveChanges();
            return Ok();
        }
    }
}