using api.examenfinal.Models;
using Microsoft.EntityFrameworkCore;

namespace api.examenfinal.Data
{
    public class ExamenFinalApp : DbContext
    {
        public ExamenFinalApp(DbContextOptions<ExamenFinalApp> options) : base(options)
        {

        }
        public DbSet<Funcionario> Funcionario => Set<Funcionario>();

    }
}
