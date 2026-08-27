using System.Collections.Generic;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public interface IRepositorioTipoInmueble
    {
        IList<TipoInmueble> ObtenerLista();
        TipoInmueble? ObtenerPorId(int id);
    }
}