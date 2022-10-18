using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ait.ADManagement.Core.Entities;
using Ait.ADManagement.Core.Services;

namespace Ait.ADManagement.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BuildTreeView();
            grpGroup.Margin = grpUser.Margin;
            lblOUPath.Content = "";
            //List<Group> groups = ADService.GetAllGroups();  LDAP://OU=OUGroepen,DC=ait,DC=local
            grpGroup.Visibility = Visibility.Hidden;
            grpUser.Visibility = Visibility.Hidden;
        }
        private void BuildTreeView()
        {
            List<OU> oUs = ADService.GetBaseOUs();
            TVOU.Items.Clear();
            foreach(OU ou in oUs)
            {
                TreeViewItem treeViewItem = new TreeViewItem();
                treeViewItem.Tag = ou;
                treeViewItem.Header = ou.DirectoryEntry.Name;
                BuildTreeViewRecursive(treeViewItem, ou);
                TVOU.Items.Add(treeViewItem);
            }
        }
        private void BuildTreeViewRecursive(TreeViewItem parentItem, OU parentOU)
        {
            List<OU> oUs = OUService.GetChildOUs(parentOU.Path);
            foreach (OU ou in oUs)
            {
                TreeViewItem treeViewItem = new TreeViewItem();
                treeViewItem.Tag = ou;
                treeViewItem.Header = ou.DirectoryEntry.Name;
                BuildTreeViewRecursive(treeViewItem, ou);
                parentItem.Items.Add(treeViewItem);
            }

        }

        private void TVOU_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            grpUser.Visibility = Visibility.Hidden;

            lstGroups.ItemsSource = null;
            lstUsers.ItemsSource = null;

            lblOUPath.Content = "";

            if (TVOU.SelectedItem == null) return;

            TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
            OU ou = (OU)itm.Tag;
            lstGroups.ItemsSource = OUService.GetGroups(ou.DirectoryEntry);
            lstUsers.ItemsSource = OUService.GetUsers(ou.DirectoryEntry);

            lblOUPath.Content = ou.Path;

        }

        private void lstUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            grpUser.Visibility = Visibility.Hidden;
            lstUserMemberOff.ItemsSource = null;

            if (lstUsers.SelectedItem == null) return;

            grpUser.Visibility = Visibility.Visible;
            User user = (User)lstUsers.SelectedItem;
            lblUserDisplayName.Content = user.UserPrincipal.DisplayName;
            lblUserFirstName.Content = user.UserPrincipal.GivenName;
            lblUserLastName.Content = user.UserPrincipal.Surname;
            lblUserUserName.Content = user.UserPrincipal.UserPrincipalName;
            if (user.UserPrincipal.AccountExpirationDate == null)
                lblUserExpirationDate.Content = "Account never expires";
            else
                lblUserExpirationDate.Content = "Expires on " +((DateTime) user.UserPrincipal.AccountExpirationDate).ToString("dd/MM/yyyy");
            lblUserEnabled.Foreground = Brushes.White;
            if (user.UserPrincipal.Enabled == true)
            {
                lblUserEnabled.Content = "User is ENABLED";
                lblUserEnabled.Background = Brushes.ForestGreen;
            }
            else
            {
                lblUserEnabled.Content = "User is DISABLED";
                lblUserEnabled.Background = Brushes.Tomato;
            }

            lstUserMemberOff.ItemsSource = UserService.GetUserGroupMemberShip(user.UserPrincipal);
        }

        private void lstGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            grpGroup.Visibility = Visibility.Hidden;
            lstUsersInGroup.ItemsSource = null;
            lstGroupsInGroup.ItemsSource = null;
            lstGroupMemberOf.ItemsSource = null;

            if (lstGroups.SelectedIndex == -1) return;

            grpGroup.Visibility = Visibility.Visible;
            Group group = (Group)lstGroups.SelectedItem;
            lblGroupName.Content = group.GroupPrincipal.Name;
            lstUsersInGroup.ItemsSource = GroupService.GetUsersInGroup(group.GroupPrincipal);
            lstGroupsInGroup.ItemsSource = GroupService.GetGroupsInGroup(group.GroupPrincipal);
            lstGroupMemberOf.ItemsSource = GroupService.GetGroupMemberShip(group.GroupPrincipal);
        }

        private void btnNewUser_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een OU in de boomstructuur links.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            WinUser winUser = new WinUser();
            winUser.isNew = true;

            TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
            winUser.ActiveOU = (OU)(itm.Tag);

            winUser.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winUser.ShowDialog();
            if (winUser.isRefreshRequired)
            {
                TVOU_SelectedItemChanged(null, null);
                try
                {
                    // --- geen idee waarom onderstaande niet werkt???
                    // lstUsers.SelectedItem = winUser.ActiveUser;
                    // --- alternatief
                    int indeks = 0;
                    foreach (var user in lstUsers.Items)
                    {
                        if (((User)user).SamAccountName == winUser.ActiveUser.SamAccountName)
                        {
                            lstUsers.SelectedIndex = indeks;
                            break;
                        }
                        indeks++;
                    }
                    lstUsers_SelectionChanged(null, null);
                    grpUser.Visibility = Visibility.Visible;
                }
                catch
                {

                }
            }
        }


        private void btnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem == null)
                return;
            if (TVOU.SelectedItem == null)
                return;

            WinUser winUser = new WinUser();
            winUser.isNew = false;
            winUser.ActiveUser = (User)lstUsers.SelectedItem;
            TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
            winUser.ActiveOU = (OU)(itm.Tag);

            winUser.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winUser.ShowDialog();
            if(winUser.isRefreshRequired)
            {
                TVOU_SelectedItemChanged(null, null);
                try
                {
                    // --- geen idee waarom onderstaande niet werkt???
                    // lstUsers.SelectedItem = winUser.ActiveUser;
                    // --- alternatief
                    int indeks = 0;
                    foreach(var user in lstUsers.Items)
                    {
                        if(((User)user).SamAccountName == winUser.ActiveUser.SamAccountName)
                        {
                            lstUsers.SelectedIndex = indeks;
                            break;
                        }
                        indeks++;
                    }
                    lstUsers_SelectionChanged(null, null);
                    grpUser.Visibility = Visibility.Visible;
                }
                catch
                {

                }
            }
        }


        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem == null)
                return;
            User victim = (User)lstUsers.SelectedItem;
            UserService.DeleteUser(victim);
            TVOU_SelectedItemChanged(null, null);
        }


        private void btnNewGroup_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een OU in de boomstructuur links.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            WinGroup winGroup = new WinGroup();
            winGroup.isNew = true;

            TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
            winGroup.activeOU = (OU)(itm.Tag);

            winGroup.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winGroup.ShowDialog();
            if (winGroup.isRefreshRequired)
            {

                TVOU_SelectedItemChanged(null, null);
                try
                {
                    // --- geen idee waarom onderstaande niet werkt???
                    // lstUsers.SelectedItem = winUser.ActiveUser;
                    // --- alternatief
                    int indeks = 0;
                    foreach (var group in lstGroups.Items)
                    {
                        if (((Group)group).SamAccountName == winGroup.activeGroup.SamAccountName)
                        {
                            lstGroups.SelectedIndex = indeks;
                            break;
                        }
                        indeks++;
                    }
                    lstGroups_SelectionChanged(null, null);
                    grpGroup.Visibility = Visibility.Visible;
                }
                catch
                {
                }
            }
        }
        private void btnEditGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstGroups.SelectedItem == null)
                return;
            if (TVOU.SelectedItem == null)
                return;

            WinGroup winGroup = new WinGroup();
            winGroup.isNew = false;
            winGroup.activeGroup = (Group)lstGroups.SelectedItem;
            TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
            winGroup.activeOU = (OU)(itm.Tag);

            winGroup.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winGroup.ShowDialog();
            if (winGroup.isRefreshRequired)
            {
                
                TVOU_SelectedItemChanged(null, null);
                try
                {
                    // --- geen idee waarom onderstaande niet werkt???
                    // lstUsers.SelectedItem = winUser.ActiveUser;
                    // --- alternatief
                    int indeks = 0;
                    foreach (var group in lstGroups.Items)
                    {
                        if (((Group)group).SamAccountName == winGroup.activeGroup.SamAccountName)
                        {
                            lstGroups.SelectedIndex = indeks;
                            break;
                        }
                        indeks++;
                    }
                    lstGroups_SelectionChanged(null, null);
                    grpGroup.Visibility = Visibility.Visible;
                }
                catch
                {

                }
            }

        }

        private void btnDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstGroups.SelectedItem == null)
                return;
            Group victim = (Group)lstGroups.SelectedItem;
            GroupService.DeleteGroup(victim);
            TVOU_SelectedItemChanged(null, null);
        }


    }
}
