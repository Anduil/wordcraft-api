using Helpers;
using Models;
using Repositories;
using System;
using System.Collections.Generic;

namespace Services
{
    public interface IUserService
    {
        User Authenticate(string email, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User Create(string email, string password);
        void Update(User user, string password = null);
        void Delete(int id);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public User Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;
            var user = userRepository.FindByEmail(email);
            // Check if email exists.
            if (user == null)
                return null;
            // Check if password is correct.
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;
            // Authentication successful.
            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return userRepository.GetAll();
        }

        public User GetById(int id)
        {
            return userRepository.Get(id);
        }

        public User Create(string email, string password)
        {
            // Validation.
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");
            if (userRepository.FindByEmail(email) != null)
                throw new AppException($"Username {email} is already taken");

            // Add.
            CreatePasswordHash(password, out var passwordHash, out var passwordSalt);
            var user = new User
            {
                Email = email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
            userRepository.Add(user);
            return user;
        }

        public void Update(User userParam, string password = null)
        {
            var user = userRepository.Get(userParam.Id);
            
            // Validation.
            if (user == null)
                throw new AppException("User not found");
            if (!string.IsNullOrWhiteSpace(userParam.Email) && userParam.Email != user.Email)
            {
                // Email has changed so check if the new email is already taken.
                if (userRepository.FindByEmail(userParam.Email) != null) 
                    throw new AppException($"Username {userParam.Email} is already taken");
            }

            // Update user properties.
            user.Email = userParam.Email;

            // Update password if it was entered.
            if (!string.IsNullOrWhiteSpace(password))
            {
                CreatePasswordHash(password, out var passwordHash, out var passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            userRepository.Update(user);
        }

        public void Delete(int id)
        {
            var user = userRepository.Get(id);
            if (user != null)
                userRepository.Remove(user.Id);
        }


        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (var i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}
