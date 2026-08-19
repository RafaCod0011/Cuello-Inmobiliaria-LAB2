namespace Cuello_Inmobiliaria_LAB2.Models

{
    public interface IRepositorioPropietario : IRepositorio<Propietario>
    {
        Propietario? ObtenerPorEmail(string email);
        IList<Propietario> BuscarPorNombre(string nombre);
    }
}