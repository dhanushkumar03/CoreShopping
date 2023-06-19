using Core_Web_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Core_Web_API.Repository
{
    public interface IAccountRepository
    {
        Task<ValidationModel> Login(UserModel user);
        Task<ValidationModel> SignUp(SignUpTemp user);
    }
}
