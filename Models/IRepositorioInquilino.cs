using System.Collections.Generic;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public interface IRepositorioInquilino : IRepositorio<Inquilino>
    {
        Inquilino? ObtenerPorEmail(string email);
        IList<Inquilino> BuscarPorNombre(string nombre);
    }
}