using Microsoft.AspNetCore.Components;
using NorthWind.Membership.Frontend.RazorViews.ViewModels.UserLogin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.Membership.Frontend.RazorViews.Components
{
    public partial class UserLoginComponent
    {
        [Parameter]
        public UserLoginViewModel ViewModel { get; set; }
    }
}
