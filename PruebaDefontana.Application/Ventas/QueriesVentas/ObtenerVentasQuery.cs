using Microsoft.EntityFrameworkCore;
using PruebaDefontana.Domain.Entities;
using PruebaDefontana.Infraestructure.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaDefontana.Application.Ventas.QueriesVentas
{
    public class ObtenerVentasQuery
    {
        private readonly ApplicationDbContext _context;

        public ObtenerVentasQuery(ApplicationDbContext context) 
        {
            _context = context;
        }

        public IQueryable<Ventum> ObtenerVentas()
        {
            return _context.Venta
                .Include(x => x.IdLocalNavigation)
                .Include(x => x.VentaDetalles)
                .ThenInclude(y => y.IdProductoNavigation)
                .ThenInclude(z => z.IdMarcaNavigation);
        }
    }
}
