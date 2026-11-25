using Bugtrack.Api.Classes;

public interface ITokenService
{
    LoginResponse Create(User user);
}