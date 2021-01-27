using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OPSAPI.Models
{
    public class MyAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            UserMasterRepository _repo = new UserMasterRepository();

            var user = _repo.ValidateUser(context.UserName, context.Password);
            if (user == null)
            {
                context.SetError("invalid_grant", "Provided username and password is incorrect");
                return;
            }
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Role, user.ROLEID));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.USERID));
            identity.AddClaim(new Claim("Email", user.EMAIL));
            context.Validated(identity);
        }

        //public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        //{
        //    UserMasterRepository _repo = new UserMasterRepository();

        //    var user = _repo.ValidateUser(context.UserName, context.Password);
        //    if (user == null)
        //    {
        //        context.SetError("invalid_grant", "Provided username and password is incorrect");
        //        return;
        //    }
        //    var identity = new ClaimsIdentity(context.Options.AuthenticationType);
        //    identity.AddClaim(new Claim(ClaimTypes.Role, user.UserRoles));
        //    identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
        //    identity.AddClaim(new Claim("Email", user.UserEmailID));
        //    context.Validated(identity);
        //}
    }
}