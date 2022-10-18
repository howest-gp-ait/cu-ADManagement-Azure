using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Ait.ADManagement.Core.Entities;

namespace Ait.ADManagement.Core.Services
{
    public class GroupService
    {
        public static List<Group> GetGroupMemberShip(GroupPrincipal groupPrincipal)
        {
            string[] paths = { AD.LDAPShort + "OU=OUGroepen" + AD.LDAPSuffix };
            List<Group> allGroups = ADService.GetAllGroups(paths);
            List <Group> groups = new List<Group>();
            foreach(Group group in allGroups)
            {
                if(groupPrincipal.IsMemberOf(group.GroupPrincipal))
                { 
                    groups.Add(group);
                }
            }            
            return groups;
        }
        public static List<User> GetUsersInGroup(GroupPrincipal groupPrincipal)
        {
            List<User> users = new List<User>();
            foreach (Principal principal in groupPrincipal.GetMembers())
            {
                if(principal is UserPrincipal)
                {
                    User user = new User(principal.SamAccountName);
                    users.Add(user);
                }
            }
            return users;
        }
        public static List<Group> GetGroupsInGroup(GroupPrincipal groupPrincipal)
        {
            List<Group> groups = new List<Group>();
            foreach (Principal principal in groupPrincipal.GetMembers())
            {
                if (principal is GroupPrincipal)
                {
                    Group group = new Group(principal.SamAccountName);
                    groups.Add(group);
                }
            }
            return groups;
        }
        public static bool AddGroupToGroup(GroupPrincipal childGroupPrincipal, GroupPrincipal parentGroupPrincipal)
        {
            try
            {
                parentGroupPrincipal.Members.Add(childGroupPrincipal);
                parentGroupPrincipal.Save();
                return true;
            }
            catch (Exception fout)
            {
                return false;
            }
        }
        public static bool RemoveGroupFromGroup(GroupPrincipal childGroupPrincipal, GroupPrincipal parentGroupPrincipal)
        {
            try
            {
                parentGroupPrincipal.Members.Remove(childGroupPrincipal);
                parentGroupPrincipal.Save();
                return true;
            }
            catch (Exception fout)
            {
                return false;
            }
        }

        public static Group CreateGroup(OU targetOU, string groupName)
        {
            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain);
            GroupPrincipal groupPrincipal = new GroupPrincipal(principalContext);
            groupPrincipal.Name = groupName;
            groupPrincipal.SamAccountName = groupName;
            try
            {
                groupPrincipal.Save();
                Group group = new Group(groupPrincipal.SamAccountName);
                OUService.MovePrincipal(group, targetOU);
                return group;
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        public static Group UpdateGroup(Group group, OU targetOU, string groupName)
        {
            Group retourGroup = null;
            try
            {
                // dit kan wel ???
                group.GroupPrincipal.SamAccountName = groupName;
                group.GroupPrincipal.Save();

                // Name prop is readonly bij een bestaande groep, dus onderstaande werkt niet ?????
                // group.GroupPrincipal.Name = groupName;
                //
                // Wat dan wel werkt : (hierdoor wordt op AD blijkbaar wel een nieuwe group-object gemaakt, dus ik vermoed wissen en nieuw maken): 
                // ===========================================================
                DirectoryEntry directoryEntry = new DirectoryEntry(AD.LDAPShort + group.GroupPrincipal.DistinguishedName);
                directoryEntry.Rename("CN=" + groupName);
                // ===========================================================

                retourGroup = new Group(groupName);
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
            if (targetOU.Path != retourGroup.DirectoryEntry.Path)
            {
                OUService.MovePrincipal(retourGroup, targetOU);
            }
            return retourGroup;
        }
        public static bool DeleteGroup(Group group)
        {
            try
            {
                group.GroupPrincipal.Delete();
                group = null;
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
