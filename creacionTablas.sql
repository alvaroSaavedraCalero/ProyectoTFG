

create table CLIENTES (
	IdCliente uniqueidentifier not null default newid() primary key,
	IdCuenta varchar(max) not null,
	Rol varchar(50) not null default 'Cliente',
	Nombre varchar(50) not null,
	Apellidos varchar(50) not null,
	Telefono varchar(9),
	FechaNacimiento date default getdate(),
	Genero varchar(40),
	Nif varchar(9)
);


create table CUENTAS (
	IdCuenta uniqueidentifier not null default newid() primary key,
	LoginName varchar(50),
	Email varchar(200) not null,
	PassW varchar(50) not null,
	CuentaActiva bit not null default 0,
	ImagenAvatarBase64 varchar(max) not null
);


create table DIRECCIONES (
	IdDireccion uniqueidentifier not null default newid() primary key,
	IdCliente varchar(max) not null,
	NombreEmpresa varchar(100),
	NombreContacto varchar(100),
	ApellidosContacto varchar(100),
	TelefonoContacto varchar(9),
	Calle varchar(400),
	Numero int,
	CP int,
	Provincia varchar(max)
);

create table PEDIDOS (
	IdPedido uniqueidentifier not null default newid() primary key,
	IdCliente varchar(max) not null,
	SubTotal decimal not null,
	GastosEnvio int not null,
	Total decimal not null,
	IdDireccionEnvio varchar(max),
	IdDireccionFacturacion varchar(max),
	Fecha datetime not null
);

create table ITEMSPEDIDOS (
	IdItemPedido uniqueidentifier not null default newid() primary key,
	IdPedido varchar(max) not null,
	IdProducto varchar(max) not null,
	Cantidad int not null
);

create table AUX_PEDIDOS (
	IdPedido varchar(max) not null,
	IdItemsPedido varchar(max) not null,
	primary key (IdPedido, IdItemsPedido)
);

create table PRODUCTOS_LISTA_DESEOS (
	IdProductoListaDeseos uniqueidentifier not null default newid() primary key,
	IdCliente varchar(max) not null,
	IdProducto varchar(max) not null
);

create table COMENTARIOS (
	IdComentario uniqueidentifier not null default newid() primary key,
	IdCliente varchar(max) not null,
	ImagenCliente varchar(max) not null,
	IdProducto varchar(max) not null,
	NombreCliente varchar(max) not null,
	Comentario varchar(max) not null
);

create table PAYPAL_PEDIDOS_INFO (
	IdPaypalPedido uniqueidentifier not null default newid() primary key,
	IdCliente varchar(max) not null,
	IdCargo varchar(max) not null,
	PayPalContextClient varchar(max) not null,
	IdPedido varchar(max) not null,
);

create table GOOGLE_SESSION (
	IdGoogleSession uniqueidentifier not null default newid() primary key,
	IdGoogle varchar(max) not null,
	IdCliente varchar(max) not null
);