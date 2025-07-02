using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Template.EntityModel;
using Template.EntityModel.Models;

namespace Template.Services.Users
{
    public class UserDto
    {
        public int Id { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string DisplayName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
        public string Email { get; init; }
        public bool OnboardingComplete { get; init; }
        public List<Colture> Coltures { get; init; }
        public List<Province> Provinces { get; init; }

        public UserDto() { }
        public UserDto(User entity)
        {
            Id = entity.Id;
            FirstName = entity.FirstName;
            LastName = entity.LastName;
            OnboardingComplete = entity.OnboardingComplete;
            Coltures = entity.Coltures.ToList() ?? [];
            Provinces = entity.Provinces.ToList() ?? [];
        }
    }

    public partial class UserService
    {
        private readonly FiberDbContext _dbContext;

        public UserService(FiberDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
