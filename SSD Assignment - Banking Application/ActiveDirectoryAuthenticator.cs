using System;
using System.DirectoryServices.AccountManagement;

namespace Banking_Application
{
    internal class ActiveDirectoryAuthenticator
    {
        private PrincipalContext _context;

        public ActiveDirectoryAuthenticator()
        {
            _context = new PrincipalContext(ContextType.Domain, "ITSLIGO.LAN");
        }

        public bool Login(string username, string password)
        {
            bool isValid = _context.ValidateCredentials(username, password);

            // TODO: Log this attempt to Event Log — will add this in next step

            return isValid;
        }

        // This method will be used later for group checks
        public bool IsUserInGroup(string username, string groupName)
        {
            using (UserPrincipal user = UserPrincipal.FindByIdentity(_context, IdentityType.SamAccountName, username))
            {
                if (user == null)
                    return false;

                using (PrincipalSearchResult<Principal> groups = user.GetGroups())
                {
                    foreach (Principal p in groups)
                    {
                        if (p.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}
