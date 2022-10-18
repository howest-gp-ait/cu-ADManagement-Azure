using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Ait.ADManagement.Core.Entities;

namespace Ait.ADManagement.Core.Services
{
    public class ADService
    {
        public static List<Group> GetAllGroups()
        {
            List<Group> groups = new List<Group>();
            DirectorySearcher directorySearcher = new DirectorySearcher()
            {
                Filter = "(objectCategory=group)",
                SearchScope = SearchScope.Subtree
            };
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                groups.Add(new Group(searchResult.Properties["sAMAccountName"][0].ToString()));
            }
            return groups;
        }
        public static List<Group> GetAllGroups(string[] OUPaths)
        {
            List<Group> groups = new List<Group>();
            foreach (string OUPath in OUPaths)
            {
                DirectorySearcher directorySearcher = new DirectorySearcher(new DirectoryEntry(OUPath))
                {
                    Filter = "(objectCategory=group)",
                    SearchScope = SearchScope.Subtree
                };
                foreach (SearchResult searchResult in directorySearcher.FindAll())
                {
                    groups.Add(new Group(searchResult.Properties["sAMAccountName"][0].ToString()));
                }
            }
            return groups;
        }
        public static List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            DirectorySearcher directorySearcher = new DirectorySearcher()
            {
                Filter = "(objectCategory=person)",
                SearchScope = SearchScope.Subtree
            };
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                users.Add(new User(searchResult.Properties["sAMAccountName"][0].ToString()));
            }
            return users;
        }
        public static List<User> GetAllUsers(string[] OUPaths)
        {
            List<User> users = new List<User>();
            foreach (string OUPath in OUPaths)
            {
                DirectorySearcher directorySearcher = new DirectorySearcher(new DirectoryEntry(OUPath))
                {
                    Filter = "(objectCategory=person)",
                    SearchScope = SearchScope.Subtree
                };
                foreach (SearchResult searchResult in directorySearcher.FindAll())
                {
                    users.Add(new User(searchResult.Properties["sAMAccountName"][0].ToString()));
                }
            }
            return users;
        }

        public static List<OU> GetAllOUs()
        {

            List<OU> oUs = new List<OU>();
            DirectorySearcher zoeker = new DirectorySearcher(AD.LDAPLong)
            {
                Filter = "(objectCategory=organizationalUnit)",
                SearchScope = SearchScope.Subtree
            };
            foreach (SearchResult searchResult in zoeker.FindAll())
            {
                oUs.Add(new OU(searchResult.Path));
            }
            return oUs;
        }
        public static List<OU> GetBaseOUs()
        {
            List<OU> oUs = new List<OU>();
            DirectorySearcher directorySearcher = new DirectorySearcher(AD.LDAPLong)
            {
                Filter = "(objectCategory=organizationalUnit)",
                SearchScope = SearchScope.OneLevel
            };
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                OU ou = new OU(searchResult.Path);
                oUs.Add(ou);
            }
            return oUs;
        }
    }
}
