
namespace ProcessorServiceManager
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BillgenixProcessorMonitoringProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.BillgenixProcessorMonitoringInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // BillgenixProcessorMonitoringProcessInstaller1
            // 
            this.BillgenixProcessorMonitoringProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.BillgenixProcessorMonitoringProcessInstaller1.Password = null;
            this.BillgenixProcessorMonitoringProcessInstaller1.Username = null;
            // 
            // BillgenixProcessorMonitoringInstaller
            // 
            this.BillgenixProcessorMonitoringInstaller.Description = "The service maintanin all processor program run auto and sheduled";
            this.BillgenixProcessorMonitoringInstaller.DisplayName = "Billgenix Processor Monitoring";
            this.BillgenixProcessorMonitoringInstaller.ServiceName = "Billgenix Processor Monitoring";
            this.BillgenixProcessorMonitoringInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.BillgenixProcessorMonitoringProcessInstaller1,
            this.BillgenixProcessorMonitoringInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller BillgenixProcessorMonitoringProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller BillgenixProcessorMonitoringInstaller;
    }
}