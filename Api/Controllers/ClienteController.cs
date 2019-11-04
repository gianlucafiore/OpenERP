using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.ComponentModel.DataAnnotations;

namespace Api
{
    public class Cliente
    {
        [Key]
        public int IdCliente{get;set;}
        public string RazonSocial{get;set;}
        public string Telefono{get;set;}
        public string DireccionLocalidad{get;set;}
        public string Descripcion{get;set;}
        public DateTime FechaAlta{get;set;}
        public DateTime FechaBaja{get;set;}
        //public string RutaImagenLogo{get;set;}
    }
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    //[Authorize(Policy = "OnlyAdmin")]
    public class ClienteController : Controller
    {
        Db db;
        public ClienteController(Db dbinjected)
        {
            db = dbinjected;
        }

        [HttpGet]
        public object GetClientes(int skip, int take, bool orden, int ordenador)
        {
            var clientes = db.Cliente;
            switch(ordenador)
            {
                case 1: 
                {
                    if(orden)
                    clientes.OrderBy(c => c.IdCliente);
                    else
                    clientes.OrderByDescending(c => c.IdCliente);
                    break;
                }
                case 2: 
                {
                    if(orden)
                    clientes.OrderBy(c => c.RazonSocial);
                    else
                    clientes.OrderByDescending(c => c.RazonSocial);
                    break;
                }
            }
            var clientesFiltrados = db.Cliente.Skip(skip).Take(take);
            return clientesFiltrados;
        }

        [HttpGet("reducido")]
        public object GetListaCliente()
        {
            var clientes = db.Cliente.Where(c => c.FechaBaja > DateTime.Now).Select(c => new{
                    IdCliente = c.IdCliente,
                    RazonSocial = c.RazonSocial
                }).ToList();
            return clientes;
        }
        [HttpGet("reducido/{idCliente}")]
        public object GetEstadoCuenta(int idCliente)
        {
            var totalVenta = db.Venta.Where(v => v.Cliente == idCliente && v.FechaBaja > DateTime.Now).Sum(v => v.Total);
            var totalPago = db.Pago.Where(c => c.IdCliente == idCliente).Sum(c => c.Monto);
            return totalPago - totalVenta;
        }

        [HttpGet("{id}")]
        public object GetClientById(int id)
        {
            var cliente = db.Cliente.FirstOrDefault(c => c.IdCliente == id);
            
            return new{
                IdCliente = cliente.IdCliente,
                RazonSocial = cliente.RazonSocial,
                Telefono = cliente.Telefono,
                Descripcion = cliente.Descripcion,
                //RutaImagenLogo = cliente.RutaImagenLogo
            };
        }     
        [HttpPost]
        public IActionResult PostNewEditCliente(Cliente data)
        {
            if(string.IsNullOrEmpty(data.RazonSocial) || string.IsNullOrEmpty(data.Telefono))
                return BadRequest("Falta completar algun campo");
            if(data.IdCliente != 0)
            {
                var cliente = db.Cliente.FirstOrDefault(c => c.IdCliente == data.IdCliente);
                cliente.RazonSocial = data.RazonSocial;
                cliente.Telefono = data.Telefono;
                cliente.Descripcion = data.Descripcion;                
            }
            else
            {
                data.FechaAlta = DateTime.Now;
                data.FechaBaja = new DateTime(2999,1,1);
                db.Cliente.Add(data);
            }
            
            db.SaveChanges();
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult BajaCliente(int id)
        {
            var cliente = db.Cliente.FirstOrDefault(c => c.IdCliente == id);
            cliente.FechaBaja = DateTime.Now;
            db.SaveChanges();
            return Ok();
        }
    }
}