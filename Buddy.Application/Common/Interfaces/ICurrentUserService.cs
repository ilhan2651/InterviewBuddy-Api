namespace Buddy.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }      
        string? Username { get; } 
        string? Email { get; }    
    }
}