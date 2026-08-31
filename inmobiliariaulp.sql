-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 31-08-2026 a las 19:51:41
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `inmobiliariaulp`
--
CREATE DATABASE IF NOT EXISTS `inmobiliariaulp` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `inmobiliariaulp`;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `imageninmueble`
--

CREATE TABLE `imageninmueble` (
  `IdImagen` int(11) NOT NULL,
  `Ruta` varchar(500) NOT NULL,
  `Orden` int(11) NOT NULL DEFAULT 0,
  `IdInmueble` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `imageninmueble`
--

INSERT INTO `imageninmueble` (`IdImagen`, `Ruta`, `Orden`, `IdInmueble`) VALUES
(1, '/Uploads/Inmuebles/1/480f0055-0744-4b6d-8449-331f9830f2a2.jpg', 4, 1),
(3, '/Uploads/Inmuebles/1/e21a6eee-7ca6-493c-813e-66dbec7f6160.jpg', 0, 1),
(4, '/Uploads/Inmuebles/2/5476365a-95ee-42b8-85f6-fec1e4f13f74.jpg', 0, 2),
(5, '/Uploads/Inmuebles/1/aa7b9807-8dee-4c03-bb6b-d613226eb0f2.jpg', 3, 1),
(6, '/Uploads/Inmuebles/3/98f6696b-3376-4dde-9ee2-4952e9f15c19.jpg', 0, 3),
(7, '/Uploads/Inmuebles/4/ee58bf56-285f-4f78-bffd-98b466791357.jpg', 0, 4),
(8, '/Uploads/Inmuebles/5/b8d5e2d7-6419-4e0b-b13e-2d765e626411.jpg', 0, 5),
(9, '/Uploads/Inmuebles/6/6d1e7479-141f-4f8e-97d9-6e37b85b5b1a.jpg', 0, 6);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inmueble`
--

CREATE TABLE `inmueble` (
  `IdInmueble` int(11) NOT NULL,
  `Direccion` varchar(255) NOT NULL,
  `Cupo` int(11) NOT NULL,
  `PrecioPorDia` decimal(10,2) NOT NULL,
  `PorcentajeReserva` decimal(5,2) NOT NULL DEFAULT 30.00,
  `Estado` enum('Activo','Suspendido') NOT NULL DEFAULT 'Activo',
  `Latitud` decimal(10,8) DEFAULT NULL,
  `Longitud` decimal(11,8) DEFAULT NULL,
  `IdPropietario` int(11) NOT NULL,
  `IdTipo` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `inmueble`
--

INSERT INTO `inmueble` (`IdInmueble`, `Direccion`, `Cupo`, `PrecioPorDia`, `PorcentajeReserva`, `Estado`, `Latitud`, `Longitud`, `IdPropietario`, `IdTipo`) VALUES
(1, 'Av. San Martín 123, San Luis', 4, 150.00, 20.00, 'Activo', -33.29104900, -66.33649500, 1, 1),
(2, 'Calle 9 de Julio 456, Villa Mercedes', 6, 200.00, 25.00, 'Activo', -33.68000000, -65.46000000, 1, 2),
(3, 'Ruta 8 km 12, La Punta', 8, 300.00, 20.00, 'Suspendido', -33.18200000, -66.30800000, 2, 1),
(4, 'Calle Belgrano 789, San Luis', 2, 100.00, 40.00, 'Activo', -33.29000000, -66.33000000, 3, 3),
(5, 'Av. Pringles 1010, San Luis', 5, 180.00, 30.00, 'Activo', -33.30500000, -66.34000000, 4, 2),
(6, 'Ana Maria Galetti 719', 5, 15000.00, 23.00, 'Activo', -33.29822800, -66.34368400, 2, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inquilino`
--

CREATE TABLE `inquilino` (
  `IdInquilino` int(11) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Dni` varchar(20) NOT NULL,
  `Telefono` varchar(30) DEFAULT NULL,
  `Email` varchar(150) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `inquilino`
--

INSERT INTO `inquilino` (`IdInquilino`, `Nombre`, `Apellido`, `Dni`, `Telefono`, `Email`) VALUES
(1, 'Lucas', 'Sánchez', '40123456', '2664123001', 'lucas.sanchez@gmail.com'),
(2, 'Ana', 'López', '41234567', '2664234002', 'ana.lopez@gmail.com'),
(3, 'Martín', 'Díaz', '38987654', '2664345003', 'martin.diaz@gmail.com'),
(4, 'Sofía', 'Torres', '42345678', '2664456004', 'sofia.torres@gmail.com'),
(5, 'Diego', 'Ramírez', '37456789', '2664567005', 'diego.ramirez@gmail.com');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `pago`
--

CREATE TABLE `pago` (
  `IdPago` int(11) NOT NULL,
  `Concepto` varchar(100) NOT NULL,
  `FechaPago` date NOT NULL,
  `Importe` decimal(10,2) NOT NULL,
  `Anulado` tinyint(1) NOT NULL DEFAULT 0,
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `FechaAnulacion` datetime DEFAULT NULL,
  `IdReserva` int(11) NOT NULL,
  `IdUsuarioCreacion` int(11) NOT NULL,
  `IdUsuarioAnulacion` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `pago`
--

INSERT INTO `pago` (`IdPago`, `Concepto`, `FechaPago`, `Importe`, `Anulado`, `FechaCreacion`, `FechaAnulacion`, `IdReserva`, `IdUsuarioCreacion`, `IdUsuarioAnulacion`) VALUES
(1, 'Seña (20.00%)', '2026-08-28', 700.00, 0, '2026-08-28 16:06:07', NULL, 1, 1, NULL),
(2, 'Seña (40.00%)', '2026-08-28', 320.00, 0, '2026-08-28 16:24:25', NULL, 2, 1, NULL),
(3, 'Seña (23.00%)', '2026-08-28', 230.00, 0, '2026-08-28 16:38:47', NULL, 3, 1, NULL),
(5, 'Multa por terminación anticipada', '2026-08-31', 375.00, 0, '2026-08-31 12:00:35', NULL, 1, 1, NULL);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `propietario`
--

CREATE TABLE `propietario` (
  `IdPropietario` int(11) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Dni` varchar(20) NOT NULL,
  `Telefono` varchar(30) DEFAULT NULL,
  `Email` varchar(150) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `propietario`
--

INSERT INTO `propietario` (`IdPropietario`, `Nombre`, `Apellido`, `Dni`, `Telefono`, `Email`) VALUES
(1, 'Juan', 'Pérez', '30123456', '2664123456', 'juan.perez@gmail.com'),
(2, 'María', 'González', '28456789', '2664234567', 'maria.gonzalez@gmail.com'),
(3, 'Carlos', 'Rodríguez', '32789123', '2664345678', 'carlos.rodriguez@gmail.com'),
(4, 'Laura', 'Fernández', '35987654', '2664456789', 'laura.fernandez@gmail.com'),
(5, 'Pedro', 'Martínez', '27654321', '2664567890', 'pedro.martinez@gmail.com');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `reserva`
--

CREATE TABLE `reserva` (
  `IdReserva` int(11) NOT NULL,
  `FechaInicio` date NOT NULL,
  `FechaFin` date NOT NULL,
  `MontoDiario` decimal(10,2) NOT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `FechaTerminacionAnticipada` date DEFAULT NULL,
  `IdInmueble` int(11) NOT NULL,
  `IdInquilino` int(11) NOT NULL,
  `IdUsuarioCreacion` int(11) NOT NULL,
  `IdUsuarioTerminacion` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `reserva`
--

INSERT INTO `reserva` (`IdReserva`, `FechaInicio`, `FechaFin`, `MontoDiario`, `FechaCreacion`, `FechaTerminacionAnticipada`, `IdInmueble`, `IdInquilino`, `IdUsuarioCreacion`, `IdUsuarioTerminacion`) VALUES
(1, '2026-08-29', '2026-09-05', 500.00, '2026-08-28 16:06:07', NULL, 1, 1, 1, NULL),
(2, '2026-09-01', '2026-09-05', 200.00, '2026-08-28 16:24:25', NULL, 4, 2, 1, NULL),
(3, '2026-09-01', '2026-09-05', 200.00, '2026-08-28 16:38:47', NULL, 6, 4, 1, NULL);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tipoinmueble`
--

CREATE TABLE `tipoinmueble` (
  `IdTipo` int(11) NOT NULL,
  `Nombre` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `tipoinmueble`
--

INSERT INTO `tipoinmueble` (`IdTipo`, `Nombre`) VALUES
(1, 'Casa'),
(5, 'Chalet'),
(2, 'Departamento'),
(4, 'Loft'),
(3, 'Monoambiente');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuario`
--

CREATE TABLE `usuario` (
  `IdUsuario` int(11) NOT NULL,
  `Email` varchar(150) NOT NULL,
  `Clave` varchar(255) NOT NULL,
  `Rol` enum('Administrador','Empleado') NOT NULL DEFAULT 'Empleado',
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Avatar` varchar(500) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `usuario`
--

INSERT INTO `usuario` (`IdUsuario`, `Email`, `Clave`, `Rol`, `Nombre`, `Apellido`, `Avatar`) VALUES
(1, 'admin@inmobiliaria.com', 'admin123', 'Administrador', 'Admin', 'Sistema', NULL),
(2, 'empleado@inmobiliaria.com', 'empleado123', 'Empleado', 'Empleado', 'Demo', NULL);

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `imageninmueble`
--
ALTER TABLE `imageninmueble`
  ADD PRIMARY KEY (`IdImagen`),
  ADD KEY `FK_ImagenInmueble_Inmueble` (`IdInmueble`);

--
-- Indices de la tabla `inmueble`
--
ALTER TABLE `inmueble`
  ADD PRIMARY KEY (`IdInmueble`),
  ADD KEY `FK_Inmueble_Propietario` (`IdPropietario`),
  ADD KEY `FK_Inmueble_TipoInmueble` (`IdTipo`);

--
-- Indices de la tabla `inquilino`
--
ALTER TABLE `inquilino`
  ADD PRIMARY KEY (`IdInquilino`),
  ADD UNIQUE KEY `UK_Inquilino_Dni` (`Dni`),
  ADD UNIQUE KEY `UK_Inquilino_Email` (`Email`);

--
-- Indices de la tabla `pago`
--
ALTER TABLE `pago`
  ADD PRIMARY KEY (`IdPago`),
  ADD KEY `FK_Pago_Reserva` (`IdReserva`),
  ADD KEY `FK_Pago_UsuarioCreacion` (`IdUsuarioCreacion`),
  ADD KEY `FK_Pago_UsuarioAnulacion` (`IdUsuarioAnulacion`);

--
-- Indices de la tabla `propietario`
--
ALTER TABLE `propietario`
  ADD PRIMARY KEY (`IdPropietario`),
  ADD UNIQUE KEY `UK_Propietario_Dni` (`Dni`),
  ADD UNIQUE KEY `UK_Propietario_Email` (`Email`);

--
-- Indices de la tabla `reserva`
--
ALTER TABLE `reserva`
  ADD PRIMARY KEY (`IdReserva`),
  ADD KEY `FK_Reserva_Inmueble` (`IdInmueble`),
  ADD KEY `FK_Reserva_Inquilino` (`IdInquilino`),
  ADD KEY `FK_Reserva_UsuarioCreacion` (`IdUsuarioCreacion`),
  ADD KEY `FK_Reserva_UsuarioTerminacion` (`IdUsuarioTerminacion`);

--
-- Indices de la tabla `tipoinmueble`
--
ALTER TABLE `tipoinmueble`
  ADD PRIMARY KEY (`IdTipo`),
  ADD UNIQUE KEY `UK_TipoInmueble_Nombre` (`Nombre`);

--
-- Indices de la tabla `usuario`
--
ALTER TABLE `usuario`
  ADD PRIMARY KEY (`IdUsuario`),
  ADD UNIQUE KEY `UK_Usuario_Email` (`Email`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `imageninmueble`
--
ALTER TABLE `imageninmueble`
  MODIFY `IdImagen` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT de la tabla `inmueble`
--
ALTER TABLE `inmueble`
  MODIFY `IdInmueble` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT de la tabla `inquilino`
--
ALTER TABLE `inquilino`
  MODIFY `IdInquilino` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `pago`
--
ALTER TABLE `pago`
  MODIFY `IdPago` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT de la tabla `propietario`
--
ALTER TABLE `propietario`
  MODIFY `IdPropietario` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT de la tabla `reserva`
--
ALTER TABLE `reserva`
  MODIFY `IdReserva` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `tipoinmueble`
--
ALTER TABLE `tipoinmueble`
  MODIFY `IdTipo` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT de la tabla `usuario`
--
ALTER TABLE `usuario`
  MODIFY `IdUsuario` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `imageninmueble`
--
ALTER TABLE `imageninmueble`
  ADD CONSTRAINT `FK_ImagenInmueble_Inmueble` FOREIGN KEY (`IdInmueble`) REFERENCES `inmueble` (`IdInmueble`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Filtros para la tabla `inmueble`
--
ALTER TABLE `inmueble`
  ADD CONSTRAINT `FK_Inmueble_Propietario` FOREIGN KEY (`IdPropietario`) REFERENCES `propietario` (`IdPropietario`) ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Inmueble_TipoInmueble` FOREIGN KEY (`IdTipo`) REFERENCES `tipoinmueble` (`IdTipo`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `pago`
--
ALTER TABLE `pago`
  ADD CONSTRAINT `FK_Pago_Reserva` FOREIGN KEY (`IdReserva`) REFERENCES `reserva` (`IdReserva`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Pago_UsuarioAnulacion` FOREIGN KEY (`IdUsuarioAnulacion`) REFERENCES `usuario` (`IdUsuario`) ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Pago_UsuarioCreacion` FOREIGN KEY (`IdUsuarioCreacion`) REFERENCES `usuario` (`IdUsuario`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `reserva`
--
ALTER TABLE `reserva`
  ADD CONSTRAINT `FK_Reserva_Inmueble` FOREIGN KEY (`IdInmueble`) REFERENCES `inmueble` (`IdInmueble`) ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Reserva_Inquilino` FOREIGN KEY (`IdInquilino`) REFERENCES `inquilino` (`IdInquilino`) ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Reserva_UsuarioCreacion` FOREIGN KEY (`IdUsuarioCreacion`) REFERENCES `usuario` (`IdUsuario`) ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Reserva_UsuarioTerminacion` FOREIGN KEY (`IdUsuarioTerminacion`) REFERENCES `usuario` (`IdUsuario`) ON DELETE SET NULL ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
