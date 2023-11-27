-- Creacion de tablas

create table Clientes (
	IdCliente int identity(1,1) primary key,
	Rol nvarchar(50),
	Nombre nvarchar(150),
	Apellidos nvarchar(200),
	Telefono nvarchar(9),
	FechaNacimiento date,
	Genero nvarchar(50),
	Nif nvarchar(9),
	IdCuenta int,

);