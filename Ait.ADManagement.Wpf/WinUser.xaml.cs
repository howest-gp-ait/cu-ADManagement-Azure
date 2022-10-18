using System;
using System.Collections.Generic;
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
    /// Interaction logic for winUser.xaml
    /// </summary>
    public partial class WinUser : Window
    {
        public WinUser()
        {
            InitializeComponent();
        }
        public bool isNew;
        public User ActiveUser;
        public OU ActiveOU;
        public bool isRefreshRequired = false;

        private List<Group> AvailableGroups;
        private List<Group> AllGroups;
        private List<Group> MemberShipGroups;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AllGroups = ADService.GetAllGroups();
            cmbOUs.ItemsSource = ADService.GetAllOUs();

            for(int r = 0; r < cmbOUs.Items.Count; r++)
            {
                if( ((OU)cmbOUs.Items[r]).Path == ActiveOU.Path)
                {
                    cmbOUs.SelectedIndex = r;
                    break;
                }
            }
            //cmbOUs.SelectedItem = ActiveOU;
            PopulateMemberShipGroups();
            PopulateAvailableGroups();
            DisplayMemberShipGroups();
            DisplayAvailableGroups();
            if (isNew)
            {
                ClearControls();

            }
            else
            {
                FillControls();
            }

        }
        private void PopulateMemberShipGroups()
        {
            if (isNew)
                MemberShipGroups = new List<Group>();
            else
                MemberShipGroups = UserService.GetUserGroupMemberShip(ActiveUser.UserPrincipal);
        }
        private void PopulateAvailableGroups()
        {
            AvailableGroups = new List<Group>();
            foreach (Group group in AllGroups)
            {
                if (!MemberShipGroups.Exists(g => g.SamAccountName == group.SamAccountName))
                {
                    AvailableGroups.Add(group);
                }
            }
        }
        private void DisplayMemberShipGroups()
        {
            lstMemberOff.Items.Clear();
            foreach(Group group in MemberShipGroups)
            {
                lstMemberOff.Items.Add(group);
            }
        }
        private void DisplayAvailableGroups()
        {
            lstAvailable.Items.Clear();
            foreach(Group group in AvailableGroups)
            {
                lstAvailable.Items.Add(group);
            }
        }

        private void ClearControls()
        {
            txtFirstName.Text = "";
            txtLastname.Text = "";
            txtPassword.Text = "";
            txtUserName.Text = "";
            rdbExpiresNever.IsChecked = true;
            rdbExpiresAt.IsChecked = false;
            dtpExpirationDate.Visibility = Visibility.Hidden;
            dtpExpirationDate.SelectedDate = DateTime.Now.AddDays(365);
            chkEnabled.IsChecked = true;
            lstMemberOff.Items.Clear();
        }
        private void FillControls()
        {
            txtFirstName.Text = ActiveUser.UserPrincipal.GivenName;
            txtLastname.Text = ActiveUser.UserPrincipal.Surname;
            txtPassword.Text = "";
            txtUserName.Text = ActiveUser.UserPrincipal.SamAccountName;
            if (ActiveUser.UserPrincipal.Enabled == true)
                chkEnabled.IsChecked = true;
            else
                chkEnabled.IsChecked = false;

            if (ActiveUser.UserPrincipal.AccountExpirationDate == null)
            {
                rdbExpiresNever.IsChecked = true;
                rdbExpiresAt.IsChecked = false;
                dtpExpirationDate.Visibility = Visibility.Hidden;
            }
            else
            {
                rdbExpiresNever.IsChecked = false;
                rdbExpiresAt.IsChecked = true;
                dtpExpirationDate.Visibility = Visibility.Visible;
                dtpExpirationDate.SelectedDate = (DateTime)ActiveUser.UserPrincipal.AccountExpirationDate;
            }

        }
        private void rdbExpiresNever_Checked(object sender, RoutedEventArgs e)
        {
            dtpExpirationDate.Visibility = Visibility.Hidden;
        }

        private void rdbExpiresAt_Checked(object sender, RoutedEventArgs e)
        {
            dtpExpirationDate.Visibility = Visibility.Visible;
        }
        private void btnAddToGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstAvailable.SelectedIndex == -1) return;
            lstMemberOff.Items.Add(lstAvailable.SelectedItem);
            lstAvailable.Items.Remove(lstAvailable.SelectedItem);
            // user wordt nog niet echt member gemaakt : dat gebeurt pas bij het opslaan
        }
        private void btnRemoveFromGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstMemberOff.SelectedIndex == -1) return;
            lstAvailable.Items.Add(lstMemberOff.SelectedItem);
            lstMemberOff.Items.Remove(lstMemberOff.SelectedItem);
            // user wordt nog niet echt uit de groep gehaald : dat gebeurt pas bij het opslaan
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            isRefreshRequired = false;
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            OU targetOU = (OU)cmbOUs.SelectedItem;
            string firstname = txtFirstName.Text.Trim();
            string lastname = txtLastname.Text.Trim();
            string loginname = txtUserName.Text.Trim();
            if(loginname == "")
            {
                MessageBox.Show("Gebruikersnaam kan niet leeg zijn !", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string password = txtPassword.Text.Trim();
            bool isEnabled = false;
            if (chkEnabled.IsChecked == true)
                isEnabled = true;
            DateTime? accountExpirationDate = null;
            if (rdbExpiresAt.IsChecked == true)
            {
                accountExpirationDate = dtpExpirationDate.SelectedDate;
            }
            if(isNew)
            {
                try
                {
                    ActiveUser = UserService.CreateUser(targetOU, firstname, lastname, loginname, password, isEnabled, accountExpirationDate);
                }
                catch (Exception error)
                {
                    MessageBox.Show("Nieuwe gebruiker werden niet aangemaakt !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                isRefreshRequired = true;
                System.Threading.Thread.Sleep(1000); // even AD de tijd geven vooraleer we verder doen anders problemen met plaatsen in groepen;
            }
            else
            {
                if (UserService.UpdateUser(ActiveUser, targetOU, firstname, lastname, loginname, password, isEnabled, accountExpirationDate))
                {
                    isRefreshRequired = true;
                }
                else
                {
                    MessageBox.Show("Wijzigingen werden niet weggeschreven !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }                
            }

            // eerst ActiveUser uit alle groepen halen en vervolgens in alle groepen plaatsen die in lstMemberOff zitten

            foreach (Group group in UserService.GetUserGroupMemberShip(ActiveUser.UserPrincipal))
            {
                UserService.RemoveUserFromGroup(group.GroupPrincipal, ActiveUser.UserPrincipal);
            }

            foreach (var item in lstMemberOff.Items)
            {
                UserService.AddUserToGroup(((Group)item).GroupPrincipal, ActiveUser.UserPrincipal);
            }

            this.Close();

        }


    }
}
