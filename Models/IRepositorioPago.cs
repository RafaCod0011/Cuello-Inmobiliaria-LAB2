using System.Collections.Generic;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public interface IRepositorioPago : IRepositorio<Pago>
    {
        IList<Pago> ObtenerPorReserva(int idReserva);
        void Anular(int idPago, int idUsuarioAnulacion);
    }
}