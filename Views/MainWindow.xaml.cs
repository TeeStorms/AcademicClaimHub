using System.Windows;

namespace AcademicClaimHub
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Set the default page to Lecturer Dashboard when the app starts
            GoToLecturer(this, new RoutedEventArgs());
        }

        // Navigate to the Lecturer Dashboard
        private void GoToLecturer(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.LecturerDashboard();
            txtCurrentPage.Text = "Lecturer Dashboard";
        }

        // Navigate to the Programme Coordinator Dashboard
        private void GoToCoordinator(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.ProgrammeCoordinatorDashboard();
            txtCurrentPage.Text = "Programme Coordinator Dashboard";
        }

        // Navigate to the Academic Manager Dashboard
        private void GoToManager(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.AcademicManagerDashboard();
            txtCurrentPage.Text = "Academic Manager Dashboard";
        }
    }
}
