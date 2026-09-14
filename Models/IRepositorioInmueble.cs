using System.Collections.Generic;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public interface IRepositorioInmueble : IRepositorio<Inmueble>
    {
        IList<Inmueble> ObtenerPorPropietario(int idPropietario);
        IList<Inmueble> ObtenerPorPropietario(int idPropietario, int pagina, int tamano);
        int ContarPorPropietario(int idPropietario);  
        IList<InmuebleReservado> ObtenerMasReservados(int dias, int pagina, int tamano);
        int ContarMasReservados(int dias); 
        IList<Inmueble> ObtenerSinReservas(int dias, int pagina, int tamano);
        int ContarSinReservas(int dias); 
        IList<Inmueble> ObtenerPorTipo(int idTipo);
        IList<Inmueble> ObtenerDisponibles();
        IList<Inmueble> BuscarPorDireccion(string parteDireccion);
        IList<Inmueble> BuscarDisponibles(DateTime desde, DateTime hasta, int? idTipo, int? cupoMin, int pagina, int tamano);
        int ContarDisponibles(DateTime desde, DateTime hasta, int? idTipo, int? cupoMin);
        bool ExisteDireccion(string direccion, int? idExcluir = null);
        // IList<Inmueble> ObtenerDisponiblesEntreFechas(DateTime desde, DateTime hasta);

        IList<Inmueble> BuscarConFiltros(string? direccion, string? estado, int? idTipo, int pagina, int tamano);
        int ContarConFiltros(string? direccion, string? estado, int? idTipo);
    }
}