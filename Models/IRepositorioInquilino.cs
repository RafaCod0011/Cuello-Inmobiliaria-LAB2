using System.Collections.Generic;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public interface IRepositorioInquilino : IRepositorio<Inquilino>
    {
        Inquilino? ObtenerPorEmail(string email);
        Inquilino? ObtenerPorDni(string dni);
        IList<Inquilino> BuscarPorNombre(string nombre);
    }
}