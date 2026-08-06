# DevTi POS Lite

DevTi POS Lite es un sistema de punto de venta (POS) desarrollado en .NET con Windows Forms para facilitar la gestión de ventas, productos, usuarios y operaciones básicas de caja en entornos pequeños o medianos.

## Descripción general

Este proyecto ofrece una interfaz sencilla para:

- Registrar ventas y devoluciones
- Gestionar productos, categorías y usuarios
- Administrar roles y permisos
- Controlar aperturas y cierres de caja
- Generar reportes básicos

## Uso y licencia

Este software se ofrece como un sistema de uso gratuito para fines de evaluación, educativos, personales o de uso interno.

No está autorizado para su comercialización, reventa, distribución comercial o uso empresarial con fines de lucro sin el permiso expreso del creador.

## Requisitos

- Windows 10 o 11
- .NET SDK 10.0 o superior
- Visual Studio 2022 o Visual Studio Code con la extensión de C#

## Instalación

1. Clona este repositorio:
   ```bash
   git clone <url-del-repositorio>
   cd devti-poslite-winforms
   ```

2. Restaura las dependencias:
   ```bash
   dotnet restore DevtiPosLite.slnx
   ```

3. Compila la solución:
   ```bash
   dotnet build DevtiPosLite.slnx
   ```

4. Ejecuta la aplicación:
   ```bash
   dotnet run --project DevtiPosLite.UI
   ```

La base de datos local se configura mediante el archivo [DevtiPosLite.UI/appsettings.json](DevtiPosLite.UI/appsettings.json) y por defecto usa un archivo SQLite llamado poslite.db.

## Estructura del proyecto

- [DevtiPosLite.Core](DevtiPosLite.Core): modelos, DTOs e interfaces compartidos.
- [DevtiPosLite.Services](DevtiPosLite.Services): lógica de negocio y servicios del sistema.
- [DevtiPosLite.UI](DevtiPosLite.UI): formularios Windows Forms y capa de interfaz de usuario.
- [DevtiPosLite.Data](DevtiPosLite.Data): acceso a datos y persistencia.

## Desarrollo

Para trabajar en el proyecto:

- Abre la solución [DevtiPosLite.slnx](DevtiPosLite.slnx)
- Ajusta la configuración en [DevtiPosLite.UI/appsettings.json](DevtiPosLite.UI/appsettings.json)
- Puedes usar la app para probar flujos de venta, usuarios y caja

## Licencia

Este proyecto se distribuye bajo una licencia MIT modificada para uso gratuito y no comercial.

La licencia permite usar, copiar, modificar y redistribuir el código únicamente para fines gratuitos, personales, educativos o de evaluación.

Queda expresamente prohibido el uso comercial, la reventa, la distribución con fines de lucro o la adaptación para productos comerciales sin autorización previa y por escrito del creador.

Consulta el archivo [LICENSE](LICENSE) para más detalles.
