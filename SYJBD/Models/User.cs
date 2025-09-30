namespace SYJBD.Models
{
    public class User
    {
        public string IdUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string? Correo { get; set; }
        public string? Rol { get; set; } = "COMERCIAL";

        public string NombreCorto =>
            string.IsNullOrWhiteSpace(Apellido) ? Nombre : $"{Nombre} {Apellido.Split(' ')[0]}";
    }

    public class LoginViewModel
    {
        public string Usuario { get; set; } = "";   // id_usuario o correo
        public string Contrasena { get; set; } = "";
        public string? ReturnUrl { get; set; }
        public string? Error { get; set; }         // <-- FALTABA
    }
}
