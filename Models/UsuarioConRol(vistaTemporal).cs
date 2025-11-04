namespace Ezel_Market.Models;
public class UsuarioConRol
{
    public string Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string Rol { get; set; }

    public string UserName { get; set; }    // ← Se usa (en el small)
}