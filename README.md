# Sistema de Reservas Temporales

Sistema web para la gestión de una inmobiliaria dedicada al alquiler temporario de propiedades, desarrollado en ASP.NET Core MVC con MySQL. Esta entrega implementa el ABM (Alta, Baja y Modificación) de Propietarios e Inquilinos.

---

## 👥 Integrantes del Grupo

- **Cuello Rafael Nicolas - nicolascuello12@gmail.com - [RafaCod0011](https://github.com/RafaCod0011)**

---

## 📐 Modelo de Datos

### Diagrama Entidad-Relación (DER)

Diagrama de clases provisional, falta hacer la implementacion final.

```mermaid
erDiagram

    PROPIETARIO {
        int IdPropietario PK
        string Nombre
        string Apellido
        string Dni
        string Telefono
        string Email
        string Clave
    }

    INQUILINO {
        int IdInquilino PK
        string Nombre
        string Apellido
        string Dni
        string Telefono
        string Email
    }

    INMUEBLE {
        int IdInmueble PK
        string Direccion
        int Cupo
        decimal PrecioPorDia
        decimal PorcentajeReserva
        string Estado
        decimal Latitud
        decimal Longitud
        int IdPropietario FK
        int IdTipo FK
    }

    TIPO_INMUEBLE {
        int IdTipo PK
        string Nombre
    }

    RESERVA {
        int IdReserva PK
        date FechaInicio
        date FechaFin
        decimal MontoDiario
        date FechaCreacion
        date FechaTerminacionAnticipada
        int IdInmueble FK
        int IdInquilino FK
        int IdUsuarioCreacion FK
        int IdUsuarioTerminacion FK
    }

    PAGO {
        int IdPago PK
        string Concepto
        date FechaPago
        decimal Importe
        bool Anulado
        date FechaCreacion
        date FechaAnulacion
        int IdReserva FK
        int IdUsuarioCreacion FK
        int IdUsuarioAnulacion FK
    }

    USUARIO {
        int IdUsuario PK
        string Email
        string Clave
        string Rol
        string Nombre
        string Apellido
        string Avatar
    }

    IMAGEN_INMUEBLE {
        int IdImagen PK
        string Ruta
        int Orden
        int IdInmueble FK
    }

    PROPIETARIO ||--o{ INMUEBLE : posee
    TIPO_INMUEBLE ||--o{ INMUEBLE : clasifica
    INMUEBLE ||--o{ RESERVA : tiene
    INMUEBLE ||--o{ IMAGEN_INMUEBLE : contiene
    INQUILINO ||--o{ RESERVA : realiza
    USUARIO ||--o{ RESERVA : crea
    USUARIO ||--o{ RESERVA : termina
    RESERVA ||--o{ PAGO : registra
    USUARIO ||--o{ PAGO : crea
    USUARIO ||--o{ PAGO : anula
```

---

## 🗄️ Levantar la Base de Datos

El proyecto utiliza **MySQL** como sistema gestor de base de datos.

El script de creación de la base de datos se encuentra en la **raíz del proyecto**:

La base de datos actualmente solo cuenta con dos tablas: Propietario e Inquilino.

```text
inmobiliariaulp.sql
```

### Requisitos

- MySQL
- MySQL Workbench, DBeaver o cualquier cliente compatible con MySQL

### Pasos

1. Clonar el repositorio.

2. Abrir el archivo `inmobiliaria.sql` ubicado en la raíz del proyecto.

3. Ejecutar el script completo desde **MySQL Workbench**, **DBeaver** u otro cliente MySQL.

4. El script creará la base de datos, sus tablas y datos iniciales correspondientes.

5. Verificar que la base de datos haya sido creada correctamente con sus datos antes de ejecutar el proyecto.

> **Importante:** La configuración de conexión del proyecto debe utilizar los datos correspondientes a la instalación local de MySQL.
