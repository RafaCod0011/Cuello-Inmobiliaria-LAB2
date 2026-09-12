using System.Collections.Generic;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public interface IRepositorioInmueble : IRepositorio<Inmueble>
    {
        IList<Inmueble> ObtenerPorPropietario(int idPropietario);
        IList<Inmueble> ObtenerPorTipo(int idTipo);
        IList<Inmueble> ObtenerDisponibles();
        IList<Inmueble> BuscarPorDireccion(string parteDireccion);
        IList<Inmueble> BuscarDisponibles(DateTime desde, DateTime hasta, int? idTipo, int? cupoMin, int pagina, int tamano);
        int ContarDisponibles(DateTime desde, DateTime hasta, int? idTipo, int? cupoMin);
        bool ExisteDireccion(string direccion, int? idExcluir = null);
        // IList<Inmueble> ObtenerDisponiblesEntreFechas(DateTime desde, DateTime hasta);
    }
}