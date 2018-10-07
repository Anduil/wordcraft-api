using Dapper;
using Microsoft.Extensions.Configuration;
using Models;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        User FindByEmail(string email);
    }

    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;

        public UserRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetValue<string>("DBInfo:ConnectionString");
        }

        private IDbConnection Connection => new NpgsqlConnection(connectionString);

        public void Add(User item)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT INTO users (email,passwordHash,passwordSalt) VALUES(@Email,@PasswordHash,@PasswordSalt)", item);
            }

        }

        public IEnumerable<User> GetAll()
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<User>("SELECT * FROM users");
            }
        }

        public User FindByEmail(string email)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<User>("SELECT * FROM users WHERE email = @Email", new { Email = email }).FirstOrDefault();
            }
        }

        public User Get(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<User>("SELECT * FROM users WHERE id = @Id", new { Id = id }).FirstOrDefault();
            }
        }

        public void Remove(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute("DELETE FROM users WHERE Id = @Id", new { Id = id });
            }
        }

        public void Update(User item)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Query("UPDATE users SET email = @Email, passwordHash = @PasswordHash, passwordSalt = @PasswordSalt WHERE id = @Id", item);
            }
        }
    }
}
