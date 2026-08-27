using System.Collections.Generic;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public interface IRepositorioImagen : IRepositorio<ImagenInmueble>
    {
        IList<ImagenInmueble> BuscarPorInmueble(int inmuebleId);
    }
}