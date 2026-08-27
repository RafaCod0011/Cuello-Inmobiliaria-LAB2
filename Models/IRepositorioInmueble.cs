using System.Collections.Generic;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public interface IRepositorioInmueble : IRepositorio<Inmueble>
    {
        IList<Inmueble> ObtenerPorPropietario(int idPropietario);
        IList<Inmueble> ObtenerPorTipo(int idTipo);
        IList<Inmueble> ObtenerDisponibles();
        IList<Inmueble> BuscarPorDireccion(string parteDireccion);
        bool ExisteDireccion(string direccion, int? idExcluir = null);
        // IList<Inmueble> ObtenerDisponiblesEntreFechas(DateTime desde, DateTime hasta);
    }
}