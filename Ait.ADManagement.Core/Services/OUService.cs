using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Ait.ADManagement.Core.Entities;

namespace Ait.ADManagement.Core.Services
{
    public class OUService
    {
        public static List<OU> GetChildOUs(string parentPath)
        {
            List<OU> oUs = new List<OU>();
            DirectorySearcher directorySearcher = new DirectorySearcher(new DirectoryEntry(parentPath))
            {
                Filter = "(objectCategory=organizationalUnit)",
                SearchScope = SearchScope.OneLevel
            };
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                oUs.Add(new OU(searchResult.Path));
            }
            return oUs;
        }
        public static List<User> GetUsers(DirectoryEntry directoryEntry)
        {
          
            List<User> users = new List<User>();
            DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry)
            {
                Filter = "(objectCategory=person)",
                SearchScope = SearchScope.OneLevel
            };
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                users.Add(new User(searchResult.Properties["sAMAccountName"][0].ToString()));
            }
            return users;
        }
        public static List<Group> GetGroups(DirectoryEntry directoryEntry)
        {

            List<Group> groups = new List<Group>();
            DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry)
            {
                Filter = "(objectCategory=group)",
                SearchScope = SearchScope.OneLevel
            };
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                groups.Add(new Group(searchResult.Properties["sAMAccountName"][0].ToString()));
            }
            return groups;
        }

        public static void MovePrincipal(User user, OU destinationOU)
        {
            DirectoryEntry currentDirectoryEntry = user.DirectoryEntry;
            DirectoryEntry destinationDirectory = destinationOU.DirectoryEntry;
            currentDirectoryEntry.MoveTo(destinationDirectory);
        }
        public static void MovePrincipal(Group group, OU destinationOU)
        {
            DirectoryEntry currentDirectoryEntry = group.DirectoryEntry;
            DirectoryEntry destinationDirectory = destinationOU.DirectoryEntry;
            currentDirectoryEntry.MoveTo(destinationDirectory);
        }

    }
}
