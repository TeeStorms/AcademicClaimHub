using System.Windows;

namespace AcademicClaimHub
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GoToLecturer(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.LecturerDashboard();
        }

        private void GoToCoordinator(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.ProgrammeCoordinatorDashboard();
        }

        private void GoToManager(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.AcademicManagerDashboard();
        }
    }
}
