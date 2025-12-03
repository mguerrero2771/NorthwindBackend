using NorthWind.Membership.Backend.Core.Dtos;
using NorthWind.Membership.Entities.Dtos.UserRegistration;
using NorthWind.Membership.Entities.UserLogin;
using NorthWind.Result.Entities;
using NorthWind.Validation.Entities.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.Membership.Backend.Core.Interfaces.Common
{
    public interface IMembershipService
    {
        Task<Result<IEnumerable<ValidationError>>> Register(
       UserRegistrationDto userData);
        Task<UserDto> GetUserByCredentials(UserCredentialsDto userData);
    }
    }
