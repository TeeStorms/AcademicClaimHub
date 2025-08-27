using System.Windows;

namespace AcademicClaimHub
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Default page
            GoToLecturer(this, new RoutedEventArgs());
        }

        private void GoToLecturer(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.LecturerDashboard();
            txtCurrentPage.Text = "Lecturer Dashboard";
        }

        private void GoToCoordinator(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.ProgrammeCoordinatorDashboard();
            txtCurrentPage.Text = "Programme Coordinator Dashboard";
        }

        private void GoToManager(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.AcademicManagerDashboard();
            txtCurrentPage.Text = "Academic Manager Dashboard";
        }
    }
}
