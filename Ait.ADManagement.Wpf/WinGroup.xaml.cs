using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ait.ADManagement.Core.Entities;
using Ait.ADManagement.Core.Services;

namespace Ait.ADManagement.Wpf
{
    /// <summary>
    /// Interaction logic for WinGroup.xaml
    /// </summary>
    public partial class WinGroup : Window
    {
        public WinGroup()
        {
            InitializeComponent();
        }

        public bool isNew;
        public Group activeGroup;
        public OU activeOU;
        public bool isRefreshRequired = false;

        private List<User> usersInActiveGroup = new List<User>();
        private List<User> usersNotInActiveGroup = new List<User>();
        private List<User> allADUsers = new List<User>();
        private List<Group> groupsInActiveGroup = new List<Group>();
        private List<Group> groupsNotInActiveGroup = new List<Group>();
        private List<Group> allADGroups = new List<Group>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


            allADGroups = ADService.GetAllGroups();
            allADUsers = ADService.GetAllUsers();
            if (isNew)
            {
                activeGroup = new Group();
                txtGroupName.Text = "";
            }
            else
            {
                txtGroupName.Text = activeGroup.SamAccountName;
                usersInActiveGroup = GroupService.GetUsersInGroup(activeGroup.GroupPrincipal).OrderBy(g=>g.SamAccountName).ToList();
                groupsInActiveGroup = GroupService.GetGroupsInGroup(activeGroup.GroupPrincipal).OrderBy(g => g.SamAccountName).ToList();
            }
            cmbOUs.ItemsSource = ADService.GetAllOUs();
            for (int r = 0; r < cmbOUs.Items.Count; r++)
            {
                if (((OU)cmbOUs.Items[r]).Path == activeOU.Path)
                {
                    cmbOUs.SelectedIndex = r;
                    break;
                }
            }

            PopulateUsersNotInGroups();
            PopulateGroupsNotInGroups();
            DisplayPopulations();



        }
        private void PopulateUsersNotInGroups()
        {
            foreach (User user in allADUsers)
            {
                if (!usersInActiveGroup.Exists(u => u.SamAccountName == activeGroup.SamAccountName))
                {
                    usersNotInActiveGroup.Add(user);
                }
            }
            usersNotInActiveGroup = usersNotInActiveGroup.OrderBy(g => g.SamAccountName).ToList();
        }
        private void PopulateGroupsNotInGroups()
        {
            foreach (Group group in allADGroups)
            {
                if (!groupsInActiveGroup.Exists(g => g.SamAccountName == activeGroup.SamAccountName))
                {
                    groupsNotInActiveGroup.Add(group);
                }
            }
            groupsNotInActiveGroup = groupsNotInActiveGroup.OrderBy(g => g.SamAccountName).ToList();
        }
        private void DisplayPopulations()
        {
            lstUsersWel.Items.Clear();
            lstUsersNiet.Items.Clear();
            lstGroupsWel.Items.Clear();
            lstGroupsNiet.Items.Clear();
            foreach (User user in usersInActiveGroup)
            {
                lstUsersWel.Items.Add(user);
            }
            foreach (User user in usersNotInActiveGroup)
            {
                lstUsersNiet.Items.Add(user);
            }
            foreach (Group group in groupsInActiveGroup)
            {
                lstGroupsWel.Items.Add(group);
            }
            foreach (Group group in groupsNotInActiveGroup)
            {
                lstGroupsNiet.Items.Add(group);
            }
        }

        private void btnAddUserToGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsersNiet.SelectedIndex == -1) return;
            lstUsersWel.Items.Add(lstUsersNiet.SelectedItem);
            lstUsersNiet.Items.Remove(lstUsersNiet.SelectedItem);
            // user wordt nog niet echt member gemaakt : dat gebeurt pas bij het opslaan
        }

        private void btnRemoveUserFromGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsersWel.SelectedIndex == -1) return;
            lstUsersNiet.Items.Add(lstUsersWel.SelectedItem);
            lstUsersWel.Items.Remove(lstUsersWel.SelectedItem);
            // user wordt nog niet echt uit de groep gehaald : dat gebeurt pas bij het opslaan
        }

        private void btnAddGroupToGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstGroupsNiet.SelectedIndex == -1) return;
            lstGroupsWel.Items.Add(lstGroupsNiet.SelectedItem);
            lstGroupsNiet.Items.Remove(lstGroupsNiet.SelectedItem);
            // group wordt nog niet echt member gemaakt : dat gebeurt pas bij het opslaan
        }

        private void btnRemoveGroupFromGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstGroupsWel.SelectedIndex == -1) return;
            lstGroupsNiet.Items.Add(lstGroupsWel.SelectedItem);
            lstGroupsWel.Items.Remove(lstGroupsWel.SelectedItem);
            // group wordt nog niet echt uit de groep gehaald : dat gebeurt pas bij het opslaan
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            isRefreshRequired = false;
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            isRefreshRequired = false;
            OU targetOU = (OU)cmbOUs.SelectedItem;
            string groupName = txtGroupName.Text.Trim();
            if (groupName == "")
            {
                MessageBox.Show("Groepsnaam kan niet leeg zijn !", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (isNew)
            {
                try
                {
                    activeGroup = GroupService.CreateGroup(targetOU, groupName);
                }
                catch (Exception error)
                {
                    MessageBox.Show("Nieuwe groep werden niet aangemaakt !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                try
                {
                    activeGroup = GroupService.UpdateGroup(activeGroup, targetOU, groupName);                    
                }
                catch (Exception error)
                {
                    MessageBox.Show("Wijzigingen werden niet weggeschreven !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            isRefreshRequired = true;

            foreach (User user in usersInActiveGroup)
            {
                UserService.RemoveUserFromGroup(activeGroup.GroupPrincipal, user.UserPrincipal);
            }
            foreach (var item in lstUsersWel.Items)
            {
                UserService.AddUserToGroup(activeGroup.GroupPrincipal, ((User)item).UserPrincipal);
            }
            foreach (Group group in groupsInActiveGroup)
            {
                GroupService.RemoveGroupFromGroup(group.GroupPrincipal, activeGroup.GroupPrincipal);
            }
            foreach (var item in lstGroupsWel.Items)
            {
                GroupService.AddGroupToGroup(((Group)item).GroupPrincipal, activeGroup.GroupPrincipal);
            }

            this.Close();
        }
    }
}
