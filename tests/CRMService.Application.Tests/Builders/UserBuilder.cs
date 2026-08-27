using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Tests.Builders;

public class UserBuilder
{
    private Guid id = Guid.NewGuid();
    private string name = "Test User";
    private string login = "test.user";
    private string password = "password";
    private bool active = true;

    public UserBuilder WithId(Guid value)
    {
        id = value;
        return this;
    }

    public UserBuilder WithLogin(string value)
    {
        login = value;
        return this;
    }

    public UserBuilder WithPassword(string value)
    {
        password = value;
        return this;
    }

    public UserBuilder Inactive()
    {
        active = false;
        return this;
    }

    public User Build()
    {
        return new User
        {
            Id = id,
            Name = name,
            Login = login,
            Password = password,
            Active = active
        };
    }
}
