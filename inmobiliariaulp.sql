-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 19-08-2026 a las 02:08:02
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
-- Estructura de tabla para la tabla `inquilino`
--

CREATE TABLE `inquilino` (
  `IdInquilino` int(11) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Dni` varchar(20) NOT NULL,
  `Telefono` varchar(30) DEFAULT NULL,
  `Email` varchar(150) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `inquilino`
--

INSERT INTO `inquilino` (`IdInquilino`, `Nombre`, `Apellido`, `Dni`, `Telefono`, `Email`) VALUES
(1, 'Lucas', 'Sanchez', '40123456', '2664123001', 'lucas.sanchez@gmail.com'),
(2, 'Ana', 'Lopez', '41234567', '2664234002', 'ana.lopez@gmail.com'),
(3, 'Martin', 'Diaz', '38987654', '2664345003', 'martin.diaz@gmail.com'),
(4, 'Sofia', 'Torres', '42345678', '2664456004', 'sofia.torres@gmail.com'),
(5, 'Diego', 'Ramirez', '37456789', '2664567005', 'diego.ramirez@gmail.com');

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
  `Email` varchar(150) NOT NULL,
  `Clave` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `propietario`
--

INSERT INTO `propietario` (`IdPropietario`, `Nombre`, `Apellido`, `Dni`, `Telefono`, `Email`, `Clave`) VALUES
(1, 'Juan', 'Perez', '30123456', '2664123456', 'juan.perez@gmail.com', '123456'),
(2, 'Maria', 'Gonzalez', '28456789', '2664234567', 'maria.gonzalez@gmail.com', '123456'),
(3, 'Carlos', 'Rodriguez', '32789123', '2664345678', 'carlos.rodriguez@gmail.com', '123456'),
(4, 'Laura', 'Fernandez', '35987654', '2664456789', 'laura.fernandez@gmail.com', '123456'),
(5, 'Pedro', 'Martinez', '27654321', '2664567890', 'pedro.martinez@gmail.com', '123456');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `inquilino`
--
ALTER TABLE `inquilino`
  ADD PRIMARY KEY (`IdInquilino`);

--
-- Indices de la tabla `propietario`
--
ALTER TABLE `propietario`
  ADD PRIMARY KEY (`IdPropietario`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `inquilino`
--
ALTER TABLE `inquilino`
  MODIFY `IdInquilino` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT de la tabla `propietario`
--
ALTER TABLE `propietario`
  MODIFY `IdPropietario` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
