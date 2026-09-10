namespace Cuello_Inmobiliaria_LAB2.Models
{
    public interface IRepositorioUsuario : IRepositorio<Usuario>
    {
        Usuario? ObtenerPorEmail(string email);
    }
}