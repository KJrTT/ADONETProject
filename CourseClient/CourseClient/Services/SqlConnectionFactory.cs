using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CourseClient.Data;

namespace CourseClient.Services
{
    public interface ISqlConnectionFactory
    {
        Task<SqlConnection> CreateOpenConnectionAsync();
    }

    public sealed class SqlConnectionFactory : ISqlConnectionFactory
    {
        public async Task<SqlConnection> CreateOpenConnectionAsync()
        {
            using var db = new AppDbContext();
            var connString = db.Database.GetDbConnection().ConnectionString;
            var connection = new SqlConnection(connString);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }
    }
}


