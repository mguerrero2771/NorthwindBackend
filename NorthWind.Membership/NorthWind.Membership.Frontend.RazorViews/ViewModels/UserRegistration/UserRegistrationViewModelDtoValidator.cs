using NorthWind.Membership.Entities.Dtos.UserRegistration;
using NorthWind.Validation.Entities.Abstractions;
using NorthWind.Validation.Entities.Enums;
using NorthWind.Validation.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.Membership.Frontend.RazorViews.ViewModels.UserRegistration
{
    internal class UserRegistrationViewModelDtoValidator(
 IModelValidatorHub<UserRegistrationDto> validator) :
 AbstractViewModelValidator<UserRegistrationDto, UserRegistrationViewModel>(
validator, ValidationConstraint.AlwaysValidate)
    {
    }
}
