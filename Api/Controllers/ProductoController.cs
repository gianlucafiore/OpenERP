using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api
{
    public class Producto
    {
        [Key]
        public int IdProducto{get;set;}
        public string NombreProducto{get;set;}
        public double PrecioVenta{get;set;}
        public DateTime FechaAlta{get;set;}
        public DateTime FechaBaja{get;set;}
    }
    public class Stock
    {
        [Key]
        public int IdStock{get;set;}
        public int IdProducto{get;set;}
        public int Cantidad{get;set;}
        public string Lote{get;set;}
        public DateTime Vencimiento{get;set;}
        public DateTime Fecha{get;set;}

    }
    [Route("[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductoController : Controller
    {
        Db db;
        public ProductoController(Db dbinjected)
        {
            db = dbinjected;
        }

        [HttpGet]
        public object GetProductos()
        {
            var sumaProductos = db.Producto.GroupBy(p => p.IdProducto)
                                .Select(p => p.Select(pr => new{
                                    Nombre = pr.NombreProducto,
                                    Stock = db.Stock.Where(s => s.IdProducto == pr.IdProducto)
                                                    .Sum(s => s.Cantidad) -
                                                    (from iv in db.ItemVenta
                                                     join v in db.Venta on iv.IdVenta equals v.IdVenta
                                                     where iv.IdProducto == pr.IdProducto &&
                                                      v.FechaEsperadaEntrega < DateTime.Now &&
                                                      v.FechaBaja > DateTime.Now
                                                     select iv.Cantidad
                                                    ).Sum()
                                                    // db.ItemVenta.Where(iv => iv.IdProducto == pr.IdProducto)
                                                    //             .Sum(iv => iv.Cantidad)
                                }).FirstOrDefault());
                                
            return sumaProductos.ToList();
        }
        [HttpGet("reducido")]
        public List<Producto> GetProductosReducido()
        {
            return db.Producto.ToList();
        }

        [HttpGet("{id}")]
        public object GetProductoById(int id)
        {
            var producto = db.Producto.FirstOrDefault(p => p.IdProducto == id);
            
            return new{
                IdProducto = producto.IdProducto,
                NombreProducto = producto.NombreProducto,
                //PrecioAdquirido = producto.PrecioAdquirido,
                PrecioVenta = producto.PrecioVenta,
                FechaAlta = producto.FechaAlta,
                //Stock = producto.Stock
            };
        }     
        [HttpGet("registros")]
        public IActionResult GetRegistros()
        {
            return Ok(
                        from s in db.Stock.OrderByDescending(st => st.IdStock).Take(20)
                        join p in db.Producto on s.IdProducto equals p.IdProducto
                        select new {
                            IdProducto = s.IdStock,
                            Nombre = p.NombreProducto,
                            Cantidad = s.Cantidad,
                            Lote = s.Lote,
                            Vencimiento = s.Vencimiento.ToString("dd/MM/yyyy"),
                            FechaCarga = s.Fecha.ToString("dd/MM/yyyy"),
                            Boton = $"<button class='btn btn-danger btn-sm' onclick='thisDom.DeleteRegistro({s.IdStock})'><i class='fa fa-trash'></i></button>"
                        });
        }
        [HttpDelete("registro/{id}")]
        public IActionResult RemoveRegistro(int id)
        {
            db.Stock.Remove(db.Stock.FirstOrDefault(s => s.IdStock == id));
            db.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public IActionResult PostStock(Stock data)
        {
            if(db.Producto.Count(p => p.IdProducto == data.IdProducto) < 0)
                throw new Exception("Producto inválido");

            //data.Fecha = DateTime.Now;            
            db.Stock.Add(data);            
            db.SaveChanges();
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult BajaProducto(int id)
        {
            var producto = db.Producto.FirstOrDefault(p => p.IdProducto == id);
            producto.FechaBaja = DateTime.Now;
            db.SaveChanges();
            return Ok();
        }

    }
}



