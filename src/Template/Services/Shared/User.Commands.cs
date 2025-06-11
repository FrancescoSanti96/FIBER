using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template.EntityModel.Models;

namespace Template.Services.Shared
{
    public class AddOrUpdateUserCommand
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RoleId { get; set; }
    }

    public partial class SharedService
    {
        public async Task<int> Handle(AddOrUpdateUserCommand cmd)
        {
            var user = await _dbContext.Users
                .Where(x => x.Id == cmd.Id)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                user = new EntityModel.Models.User
                {
                    Id = cmd.Id,
                    Email = cmd.Email,
                    FirstName = cmd.FirstName,
                    LastName = cmd.LastName,
                    RoleId = cmd.RoleId
                };
                _dbContext.Users.Add(user);
            }

            user.FirstName = cmd.FirstName;
            user.LastName = cmd.LastName;

            await _dbContext.SaveChangesAsync();

            return user.Id;
        }
    }
}