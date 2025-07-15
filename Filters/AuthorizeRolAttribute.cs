using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SistemaBodega.Filters
{
    public class AuthorizeRolAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _rol;

        public AuthorizeRolAttribute(string rol)
        {
            _rol = rol;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var rolEnSesion = context.HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(rolEnSesion) || rolEnSesion != _rol)
            {
                context.Result = new RedirectToActionResult("AccesoDenegado", "Cuenta", null);
            }
        }
    }
}

