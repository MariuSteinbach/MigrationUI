using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace UpdateDB
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // All Subdirectories of MyDocuments
        List<string> _vsDirectories = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)).ToList();
        // Filtered to Visual Studio YYYY
        List<string> vsDirectories = new List<string>();
        // All Subdirectories of all Project-Subdirectories in vsDirectories
        List<string> projectDirectories = new List<string>();
        // All Contexts in selected Project
        List<string> contexts = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            // Add all Visual Studio Subdirectories to vsDirectories
            foreach (var dir in _vsDirectories)
            {
                if (dir.Split('\\').Last().Split(' ')[0] == "Visual" && dir.Split('\\').Last().Split(' ')[1] == "Studio")
                {
                    vsDirectories.Add(dir);
                }
            }

            // Add all Projects-Subfolders to projectDirectories
            foreach (var dir in vsDirectories)
            {
                projectDirectories.AddRange(Directory.GetDirectories(dir + "\\Projects\\"));
            }

            // Add All ProjectFolderNames to cbProject
            foreach (var proj in projectDirectories)
            {
                cbProject.Items.Add(proj.Split('\\').Last());
            }
        }

        private void btAddMigration_Click(object sender, RoutedEventArgs e)
        {
            string migrationName = tbMigrationName.Text;
            string contextName = cbContext.Text;
            if (contextName == "")
            {
                Process.Start("cmd", "/k dotnet ef migrations add " + migrationName);
            }
            else
            {
                Process.Start("cmd", "/k dotnet ef migrations add " + migrationName + " --context " + contextName);
            }
        }

        private void btUpdateDatabase_Click(object sender, RoutedEventArgs e)
        {
            string contextParameter = " --context " + cbContext.Text;
            if (cbContext.Text == "")
            {
                contextParameter = "";
            }
            string cmd = "dotnet ef database update" + contextParameter;

            Process process = new Process();

            process.StartInfo.Arguments = cmd;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            tbOutput.Text = output;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string DataDirectory = projectDirectories[cbProject.SelectedIndex] + "\\" + cbProject.SelectedItem.ToString() + "\\Data\\";
            try
            {
                contexts.AddRange(Directory.GetFiles(DataDirectory));
            }
            catch
            {
                MessageBox.Show("There is no Data Folder in this Project.", "Error!");
            }
            foreach (var context in contexts)
            {
                cbContext.Items.Add(context.Split('\\').Last().Split('.').First());
            }
        }

        private void btProject_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cofd = new CommonOpenFileDialog();
            cofd.InitialDirectory = "C:\\";
            cofd.IsFolderPicker = true;
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                projectDirectories.Add(cofd.FileName);
                cbProject.Items.Add(cofd.FileName.Split('\\').Last());
                cbProject.SelectedIndex = cbProject.Items.Count - 1;
            }
        }

        private void btContext_Click(object sender, RoutedEventArgs e)
        {
            cbContext.Items.Add(Microsoft.VisualBasic.Interaction.InputBox("Please enter a Name for your Context.", "Custom Context", ""));
            cbContext.SelectedIndex = cbContext.Items.Count - 1;
        }
    }
}
