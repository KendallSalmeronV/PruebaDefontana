using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PruebaDefontana.Application.Ventas.QueriesVentas;
using PruebaDefontana.Domain.Entities;
using PruebaDefontana.Infraestructure.Persistance;

internal class Program
{

    /* Prueba Defontana
     * Se realiza lo solicitado en https://defontanacl-my.sharepoint.com/:w:/g/personal/gpuelles_defontana_com/EcLfnWyynQxOrY7K72U7Ey4BBUYmNPKOL9Huw4II1_j1hQ?e=XjmO2b
     * Autor: Kendal Salmeron Valverde
     * Fecha Inicio: 16/02/2024
     * Se culmina: 19/02/2024
     * Solución GitHub: 
     */
    public static void Main(string[] args)
    {
        var services = CreateServices();

        MostrarData(services);

    }
 
    private static void MostrarData(ServiceProvider provider)
    {
        var obtenerVentasUery = new ObtenerVentasQuery(provider.GetService<ApplicationDbContext>());
        var data = obtenerVentasUery.ObtenerVentas();
        var total = ObtenerTotalVentas(data);
        var ventaMasAlta = VentaMasAlta(data);
        var productoMasVentas = ProductoMasVentas(data);
        var localMayorVentas = LocalMayorVentas(data);
        var marca = MarcaMayorMargenGanancias(data);

        Console.WriteLine($"El total de ventas de los últimos 30 días es: {total.cantidadVentas} y el monto es de: {total.montoTotal} \n" +
            $"La venta con el monto más alto se realizó el día: {ventaMasAlta.fecha} y el monto fue de: {ventaMasAlta.monto}\n" +
            $"El producto con mayor monto total de ventas es: {productoMasVentas}\n" +
            $"El local con mayor monto de ventas es: {localMayorVentas}\n" +
            $"La Marca con mayor margen de ganancias es {marca}\n");

        ObtenerProductoPorLocal(data);


        Console.WriteLine("Hola Data");
    }

    /// <summary>
    /// Se obtiene los totales de las ventas en 30 días 
    /// </summary>
    /// <param name="ventas"></param>
    /// <returns></returns>
    private static (double montoTotal, int cantidadVentas) ObtenerTotalVentas(IQueryable<Ventum> ventas)
    {
        ventas = ventas.Where(x => x.Fecha >= DateTime.Now.AddDays(-30));
        return (ventas.Sum(x => x.Total), ventas.Count());   
    }

    /// <summary>
    /// Se obtiene la fecha y monto de la venta más alta
    /// </summary>
    /// <param name="ventas"></param>
    /// <returns></returns>
    private static (DateTime fecha, int monto)VentaMasAlta(IQueryable<Ventum> ventas)
    {
        var venta = ventas.OrderByDescending(x => x.Total).FirstOrDefault();
        return (venta.Fecha, venta.Total);
    }

    /// <summary>
    /// Se obtiene el producto con más ventas
    /// </summary>
    /// <param name="ventas"></param>
    /// <returns></returns>
    private static string ProductoMasVentas(IQueryable<Ventum> ventas)
    {
        // Contamos cuántas veces aparece cada producto en todos los detalles de venta y luego seleccionamos 
        var productosMasVendidos = ventas
            .SelectMany(x => x.VentaDetalles).ToList()
            .GroupBy(p => p.IdProductoNavigation)
            .Select(g => new { Producto = new Producto { Nombre = g.Key.Nombre }, TotalVentas = g.Count() })
            .OrderByDescending(v => v.TotalVentas);

        return productosMasVendidos.FirstOrDefault().Producto.Nombre;
    }

    /// <summary>
    /// Se obtiene el local con mayor venta
    /// </summary>
    /// <param name="ventas"></param>
    /// <returns></returns>
    private static string LocalMayorVentas(IQueryable<Ventum> ventas)
    {
        var local = ventas
            .GroupBy(l => l.IdLocalNavigation.Nombre)
            .Select(g => new { Local = g.Key, MotoTotal = g.Sum(v => v.Total)})
            .OrderByDescending(x => x.MotoTotal).FirstOrDefault();

        return local?.Local;
    }

    /// <summary>
    /// Se obttiene la marca que vende más
    /// </summary>
    /// <param name="ventas"></param>
    /// <returns></returns>
    private static string MarcaMayorMargenGanancias(IQueryable<Ventum> ventas)
    {
        var gananciasPorMarca = ventas
           .SelectMany(x => x.VentaDetalles)
           .ToList()
           .GroupBy(z => z.IdProductoNavigation.IdMarcaNavigation)
           .Select(g => new
           {
               Marca = g.Key,
               GananciaTotal = g.Sum(d => d.Cantidad * d.IdProductoNavigation.CostoUnitario)
           })
           .OrderByDescending(x => x.GananciaTotal);

        return gananciasPorMarca.FirstOrDefault().Marca.Nombre;
    }

    /// <summary>
    /// Se obtiene el produco más vendido por local 
    /// </summary>
    /// <param name="ventas"></param>
    private static void ObtenerProductoPorLocal(IQueryable<Ventum> ventas)
    {

        var productosPorLocal = ventas
            .GroupBy(x => x.IdLocalNavigation.Nombre)
            .Select(g => new
            {
                Local = g.Key,
                ProducToMasVendido = g.SelectMany(z => z.VentaDetalles)
                .GroupBy(b => b.IdProductoNavigation.Nombre)
                .Select(h => new
                {
                    Producto = h.Key,
                    CantidadVendida = h.Sum(d => d.Cantidad)
                })
                .OrderByDescending(l => l.CantidadVendida)
                .FirstOrDefault()
            }).ToList();


        foreach (var item in productosPorLocal)
        {
            Console.WriteLine($"En el local {item.Local} el producto más vendido es: {item.ProducToMasVendido.Producto}");
        }
    }


    /// <summary>
    /// Se configura los servicios para la inyección de dependencias
    /// </summary>
    /// <returns></returns>
    static ServiceProvider CreateServices()
    {
        var serviceProvider = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer("Data Source=lab-defontana-202310.caporvnn6sbh.us-east-1.rds.amazonaws.com;Initial Catalog=Prueba;User Id=ReadOnly;Password=d*3PSf2MmRX9vJtA5sgwSphCVQ26*T53uU");
            }).BuildServiceProvider();

        return serviceProvider;
    }


}