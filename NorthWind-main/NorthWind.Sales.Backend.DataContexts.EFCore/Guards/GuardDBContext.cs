using NorthWind.Exceptions.Entities.Exceptions;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.Guards;

internal static class GuardDBContext
{
    public static async Task AgainstSaveChangesErrorAsync(DbContext context)
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Log detallado en consola para diagnóstico rápido
            Console.WriteLine("== DbUpdateException atrapada en GuardDBContext ==");
            Console.WriteLine(ex.ToString());
            if (ex.InnerException != null)
            {
                Console.WriteLine("-- InnerException --");
                Console.WriteLine(ex.InnerException.ToString());
                if (ex.InnerException.InnerException != null)
                {
                    Console.WriteLine("-- InnerInnerException --");
                    Console.WriteLine(ex.InnerException.InnerException.ToString());
                }
            }
            throw new UpdateException(ex, ex.Entries.Select(e => e.Entity.GetType().Name));
        }
        catch (Exception)
        {
            throw;
        }
    }
}
