using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Ait.ADManagement.Core.Entities;

namespace Ait.ADManagement.Core.Services
{
    public class UserService
    {
        public static List<Group> GetUserGroupMemberShip(UserPrincipal userPrincipal)
        {
            List<Group> groups = new List<Group>();

            // onderstaande try catch werd toegevoegd omdat net nieuw toegevoegde gebruikers hier een error op gaven
            try
            {
                foreach (GroupPrincipal groupPrincipal in userPrincipal.GetGroups())
                {
                    groups.Add(new Group(groupPrincipal.SamAccountName));
                }
            }
            catch (Exception fout)
            {
                string bericht = fout.Message;
            }
            return groups;
        }
        public static bool AddUserToGroup(GroupPrincipal groupPrincipal, UserPrincipal userPrincipal)
        {
            try
            {
                groupPrincipal.Members.Add(userPrincipal);
                groupPrincipal.Save();
                return true;
            }
            catch(Exception fout)
            {
                return false;
            }
        }
        public static bool RemoveUserFromGroup(GroupPrincipal groupPrincipal, UserPrincipal userPrincipal)
        {
            try
            {
                groupPrincipal.Members.Remove(userPrincipal);
                groupPrincipal.Save();
                return true;
            }
            catch (Exception fout)
            {
                return false;
            }
        }

        public static User CreateUser(OU targetOU, string firstname, string lastname, string loginName, string password, bool isEnabled, DateTime? accountExpirationDate)
        {
            // onderstaande zou moeten werken (= gebruiker meteen in correcte OU plaatsen) maar werkt niet
            //PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, AD.ADDomainNameShort, targetOU.Path);
            // dan maar nieuwe gebruiker in de OU in de "CN=Users,DC=ait,DC=local" plaatsen en achteraf verplaatsen naar targetOU
            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain);  
            UserPrincipal userPrincipal = new UserPrincipal(principalContext);
            userPrincipal.GivenName = firstname;
            userPrincipal.Surname = lastname;
            userPrincipal.DisplayName = firstname + " " + lastname;
            userPrincipal.SamAccountName = loginName;
            userPrincipal.UserPrincipalName = loginName + AD.ADDomainEmail;
            userPrincipal.SetPassword(password);
            userPrincipal.Enabled = isEnabled;
            userPrincipal.AccountExpirationDate = accountExpirationDate;
            try
            {
                userPrincipal.Save();
                User user = new User(userPrincipal.SamAccountName);
                OUService.MovePrincipal(user, targetOU);
                return user;
            }
            catch(Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        public static bool UpdateUser(User user, OU targetOU, string firstname, string lastname, string loginName, string password, bool isEnabled, DateTime? accountExpirationDate)
        {
            user.UserPrincipal.GivenName = firstname;
            user.UserPrincipal.Surname = lastname;
            user.UserPrincipal.DisplayName = firstname + " " + lastname;
            user.UserPrincipal.SamAccountName = loginName;
            user.UserPrincipal.UserPrincipalName = loginName + AD.ADDomainEmail;

            if (password.Trim() != "")
                user.UserPrincipal.SetPassword(password);
            user.UserPrincipal.Enabled = isEnabled;
            user.UserPrincipal.AccountExpirationDate = accountExpirationDate;
            try
            {
                user.UserPrincipal.Save();
                user.SamAccountName = loginName;
            }
            catch (Exception fout)
            {
                return false;
            }
            if(targetOU.Path != user.DirectoryEntry.Path)
            {
                OUService.MovePrincipal(user, targetOU);
            }
            return true;
        }
        public static bool DeleteUser(User user)
        {
            try
            {
                user.UserPrincipal.Delete();
                user = null;
                return true;
            }
            catch
            {
                return false;
            }
            
        }

    }
}
