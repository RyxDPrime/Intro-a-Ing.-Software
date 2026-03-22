// Eslabón 1: AutenticacionHandler
// Verifica que el usuario esté activo en el sistema y tenga
// permiso para registrar ventas.
// Rol "Asistente" y "Administrador" pueden registrar ventas — tu documento.

namespace AguaMinami.Application.Sales.Handlers;

public class AutenticacionHandler : VentaHandler
{
    private readonly IUsuarioRepository _usuarios;

    public AutenticacionHandler(IUsuarioRepository usuarios) =>
        _usuarios = usuarios;

    public override async Task Manejar(VentaContext ctx)
    {
        // 1. Verifica que el usuario exista y esté activo
        var usuario = await _usuarios.ObtenerPorNombreAsync(ctx.Usuario);

        if (usuario is null || !usuario.Activo)
            throw new ValidacionVentaException(
                codigo:  "AUTH_INACTIVO",
                eslabón: "Autenticacion",
                mensaje: $"Usuario '{ctx.Usuario}' no existe o está inactivo."
            );

        // 2. Verifica que el rol tenga permiso para ventas
        var rolesPermitidos = new[] { "Administrador", "Asistente" };
        if (!rolesPermitidos.Contains(ctx.Rol))
            throw new ValidacionVentaException(
                codigo:  "AUTH_ROL",
                eslabón: "Autenticacion",
                mensaje: $"El rol '{ctx.Rol}' no tiene permiso para registrar ventas.",
                detalle: new { RolActual = ctx.Rol, RolesPermitidos = rolesPermitidos }
            );

        // ✓ Validación superada — pasa al siguiente eslabón
        await Continuar(ctx);
    }
}