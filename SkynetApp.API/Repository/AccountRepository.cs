﻿using SkynetApp.API.Service;
using SkynetApp.API.Data;
using SkynetApp.API.Models;
using Dapper;

namespace SkynetApp.API.Repository
{
    public class AccountRepository : IAccountService
    {
        private readonly SkynetDbContext _context;
        public AccountRepository(SkynetDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppUser>> RegisterAsync(AppUser appUser, string password)
        {
            byte[] passwordHash, passwordSalt;
            using var connection = _context.CreateConnection();
            var checkUserExists = await connection.QueryAsync<AppUser>("SELECT * FROM tblAppUser WHERE Username = @Username",
                new { Username = appUser.Username });
            if (checkUserExists.Count() > 0)
            {
                return null;
            }

            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            var result = await connection.QueryAsync<AppUser>("spRegisterUser",
                new { Id = appUser.Id, Username = appUser.Username, PasswordHash = appUser.PasswordHash, PasswordSalt = appUser.PasswordSalt });

            return result;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }


    }
}
