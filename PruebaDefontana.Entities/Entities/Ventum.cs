using System;
using System.Collections.Generic;

namespace PruebaDefontana.Domain.Entities
{
    public partial class Ventum
    {
        public Ventum()
        {
            VentaDetalles = new HashSet<VentaDetalle>();
            
        }

        public long IdVenta { get; set; }
        public int Total { get; set; }
        public DateTime Fecha { get; set; }
        public long IdLocal { get; set; }

        public Local IdLocalNavigation { get; set; } = null!;
        public virtual ICollection<VentaDetalle> VentaDetalles { get; set; }
    }
}
