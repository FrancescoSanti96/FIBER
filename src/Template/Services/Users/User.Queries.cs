using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template.Infrastructure;
using Template.EntityModel.Models;

namespace Template.Services.Users
{
    public class UserDetailQuery
    {
        public int Id { get; set; }
    }

    public class UserDetailDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get => $"{FirstName} {LastName}"; }
        public int RoleId { get; set; }
        public bool OnboardingComplete { get; set; }
    }

    public class CheckLoginCredentialsQuery
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class GetAllProvincesQuery
    {
    }

    public class GetAllColturesQuery
    {
    }

    public class GetUserWithPreferencesQuery
    {
        public int Id { get; set; }
    }

    public class GetBulletinByIdQuery
    {
        public int Id { get; set; }
    }

    public class BulletinDetailDto
    {
        public int Id { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }
        public bool Published { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateOnly? ExpireDate { get; set; }
        public string AuthorFirstName { get; set; }
        public string AuthorLastName { get; set; }
        public string AuthorEmail { get; set; }
        public List<string> ProvinceNames { get; set; } = [];
        public List<string> ColtureNames { get; set; } = [];
    }

    public partial class UserService
    {
        /// <summary>
        /// Returns the detail of the user who matches the Id passed in the qry parameter
        /// </summary>
        /// <param name="qry"></param>
        /// <returns></returns>
        public async Task<UserDetailDTO> Query(UserDetailQuery qry)
        {
            return await _dbContext.Users
                .Where(x => x.Id == qry.Id)
                .Select(x => new UserDetailDTO
                {
                    Id = x.Id,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    RoleId = x.RoleId,
                    OnboardingComplete = x.OnboardingComplete
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Check if credentials passed in the query are valid for a user present in the database
        /// </summary>
        /// <param name="qry"></param>
        /// <returns>User data if user has been found and credentials are valid</returns>
        /// <exception cref="LoginException">Invalid credentials</exception>
        public async Task<UserDetailDTO> Query(CheckLoginCredentialsQuery qry)
        {
            var user = await _dbContext.Users
                .Where(x => x.Email == qry.Email)
                .FirstOrDefaultAsync();

            if (user == null || user.IsMatchWithPassword(qry.Password) == false)
                throw new LoginException("Email o password errate");

            return new UserDetailDTO
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleId = user.RoleId
            };
        }

        /// <summary>
        /// Returns user with preferences (provinces and coltures)
        /// </summary>
        /// <param name="qry"></param>
        /// <returns></returns>
        public async Task<UserDto> Query(GetUserWithPreferencesQuery qry)
        {
            var user = await _dbContext.Users
                .Include(u => u.Provinces)
                .Include(u => u.Coltures)
                .Where(x => x.Id == qry.Id)
                .FirstOrDefaultAsync();

            return user != null ? new UserDto(user) : null;
        }

        /// <summary>
        /// Returns a specific bulletin with all related data
        /// </summary>
        /// <param name="qry"></param>
        /// <returns></returns>
        public async Task<BulletinDetailDto> Query(GetBulletinByIdQuery qry)
        {
            var bulletin = await _dbContext.Bulletins
                .Include(b => b.User)
                .Include(b => b.Provinces)
                .Include(b => b.Coltures)
                .Where(b => b.Id == qry.Id)
                .FirstOrDefaultAsync();

            if (bulletin == null)
                return null;

            return new BulletinDetailDto
            {
                Id = bulletin.Id,
                Summary = bulletin.Summary,
                Body = bulletin.Body,
                Published = bulletin.Published,
                PublishDate = bulletin.PublishDate,
                ExpireDate = bulletin.ExpireDate,
                AuthorFirstName = bulletin.User?.FirstName ?? "",
                AuthorLastName = bulletin.User?.LastName ?? "",
                AuthorEmail = bulletin.User?.Email ?? "",
                ProvinceNames = bulletin.Provinces?.Select(p => p.Name).ToList() ?? [],
                ColtureNames = bulletin.Coltures?.Select(c => c.Name).ToList() ?? []
            };
        }
    }
}
