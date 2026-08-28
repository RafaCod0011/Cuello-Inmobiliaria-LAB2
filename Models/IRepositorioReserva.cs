using System;
using System.Collections.Generic;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public interface IRepositorioReserva : IRepositorio<Reserva>
    {
        bool EstaDisponible(int idInmueble, DateTime fechaInicio, DateTime fechaFin, int? idReservaExcluir = null);
        IList<Reserva> ObtenerPorInmueble(int idInmueble);
        IList<Reserva> ObtenerPorInquilino(int idInquilino);
        IList<Reserva> ObtenerVigentes();
        IList<Reserva> ObtenerPorTerminarEn(int dias);
        IList<Reserva> BuscarPorFechas(DateTime desde, DateTime hasta);
        void TerminarAnticipadamente(int idReserva, DateTime fechaTerminacion, int idUsuarioTerminacion);
    }
}