# Guía para crear manualmente el proyecto `SYJBD.Web`

Esta guía resume los pasos para clonar la parte "sitio público" del proyecto MVC actual en un nuevo proyecto `SYJBD.Web`, creado manualmente en Visual Studio (o con `dotnet new`). El objetivo es aislar la capa de presentación pública sin tocar la aplicación interna existente.

## 1. Crear la biblioteca compartida `SYJBD.Core`

1. **Nuevo proyecto Class Library (.NET)** llamado `SYJBD.Core` dentro de la misma solución.
2. Mueve (o copia inicialmente) estas carpetas desde el proyecto actual (`SYJBD`):
   - `Models/`
   - `Data/` con `ErpDbContext` y configuraciones de EF Core.
   - `Services/` y cualquier helper o clase de lógica de negocio.
3. Asegúrate de mantener los mismos *namespaces* (por ejemplo, `namespace SYJBD.Models`) para que los controladores existentes no requieran refactors masivos.
4. Agrega los paquetes NuGet necesarios al nuevo `.csproj` (por ejemplo, `Microsoft.EntityFrameworkCore`, `Pomelo.EntityFrameworkCore.MySql`, `AutoMapper`, etc.). Puedes copiarlos desde el `<ItemGroup>` del `.csproj` original.
5. Referencia `SYJBD.Core` en el proyecto interno (`SYJBD`) y en el nuevo proyecto web:
   ```xml
   <ProjectReference Include="..\SYJBD.Core\SYJBD.Core.csproj" />
   ```

## 2. Crear manualmente el proyecto `SYJBD.Web`

En el asistente de Visual Studio elige:
- **Plantilla:** "Aplicación web de ASP.NET Core (Modelo-Vista-Controlador)".
- **Framework:** la misma versión de .NET que usa el proyecto actual (por ejemplo, `.NET 8 (Soporte a largo plazo)`).
- **Autenticación:** "Ninguno" (la web pública suele ser anónima).
- Desmarca "Configurar para HTTPS" si tu proxy (NGINX/Apache) ya se encargará del certificado en el VPS.
- No habilites contenedor ni Docker de momento.

El asistente genera un proyecto con una estructura básica. Para que funcione con tu código:

1. Elimina los controladores y vistas de ejemplo (`HomeController`, `Views/Home/*`).
2. Copia desde el proyecto original solo las carpetas que pertenecen al sitio público:
   - `Controllers/Website` o los controladores públicos.
   - `Views/<Área Pública>` y vistas compartidas necesarias (`Views/Shared/_Layout.cshtml`, parciales, etc.).
   - `wwwroot/` con los assets (CSS, JS, imágenes) del sitio público. Si hay assets compartidos con la app interna, déjalos en `SYJBD.Core` o mantenlos duplicados temporalmente.
3. Copia `appsettings.json` y `appsettings.Development.json`. Ajusta únicamente las secciones específicas de la web (por ejemplo, `Logging`). Mantén la cadena de conexión `ConnectionStrings:Default` apuntando a la misma base de datos.
4. Añade referencias a `SYJBD.Core` y a los mismos paquetes NuGet del proyecto original.

## 3. Configurar `Program.cs` en `SYJBD.Web`

1. Registra el `DbContext` compartido:
   ```csharp
   builder.Services.AddDbContext<ErpDbContext>(options =>
       options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
   ```
2. Registra los servicios de negocio que la web utiliza (`builder.Services.AddScoped<UsuariosService>()`, etc.).
3. Configura MVC sin la política global de autorización, permitiendo acceso anónimo:
   ```csharp
   builder.Services.AddControllersWithViews();
   ```
4. Ajusta la ruta por defecto:
   ```csharp
   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Website}/{action=Index}/{id?}");
   ```
5. Habilita `UseStaticFiles()` si vas a servir contenido de `wwwroot`.

## 4. Sincronizar configuración y recursos

- Copia los archivos de recursos necesarios (`appsettings.*`, `Views/_ViewImports.cshtml`, `Views/_ViewStart.cshtml`).
- Si usas `Areas`, mantén la misma estructura y registra las rutas correspondientes en `Program.cs`.
- Verifica que los *Tag Helpers* y `ViewComponents` estén en `SYJBD.Core` o referenciados correctamente.

## 5. Verificación

1. Ejecuta `dotnet build SYJBD.sln` para comprobar que los tres proyectos (`SYJBD`, `SYJBD.Core`, `SYJBD.Web`) compilan.
2. Desde Visual Studio, configura soluciones de inicio múltiple para levantar ambas aplicaciones y validar que cada una apunta a la misma base.
3. Ajusta gradualmente la app interna eliminando los controladores y vistas públicas una vez que confirmes que el nuevo proyecto funciona.

## 6. Publicación

- Publica cada proyecto por separado usando `dotnet publish` o los perfiles de publicación de Visual Studio.
- Configura tu VPS (Hostinger) con dos sitios o aplicaciones: uno para la app interna y otro para la web pública. Ambos desplegarán el ensamblado `SYJBD.Core` compartido.

Siguiendo estos pasos podrás crear manualmente `SYJBD.Web`, copiar únicamente los archivos necesarios y garantizar que las dependencias (NuGet y servicios) coincidan con la aplicación original sin romperla.
