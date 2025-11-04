using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ezel_Market.Models
{
    public class AsignarRoles
    {
        public List<UsuarioConRol> Usuarios { get; set; }
        public List<SelectListItem> RolesDisponibles { get; set; }
        public AgregarUsuario NuevoUsuario { get; set; }
    }
}