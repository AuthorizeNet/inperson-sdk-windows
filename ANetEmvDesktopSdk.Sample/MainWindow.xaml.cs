using System;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using System.ComponentModel;
using System.Xml;

namespace ANetEmvDesktopSdk.Sample
{
    public partial class MainWindow : Window
    {
        mobileDeviceLoginResponse response;
        private System.ComponentModel.BackgroundWorker backgroundWorkerLogin;
        private AuthorizeNet.Environment sdkEnvironment = AuthorizeNet.Environment.PRODUCTION;
        private bool skipSignatureValue = false;
        private bool showReceiptValue = true;

        public MainWindow()
        {
            InitializeComponent();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //this.txtUsername.Text = "SandboxQATest2";
            //this.txtPassword.Password = "Authnet101d";

            this.txtUsername.Text = "qafdctest1";
            this.txtPassword.Password = "FDCAuthnet#2018a";

            //this.txtUsername.Text = "qalynk1owner";
            //this.txtPassword.Password = "LynkAuthnet#2017";

            backgroundWorkerLogin = new BackgroundWorker();
            this.backgroundWorkerLogin.DoWork += new DoWorkEventHandler(this.backgroundWorkerLogin_GetAuthenticationToken);
            this.backgroundWorkerLogin.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorkerLogin_GetAuthenticationTokenProgress);
            this.backgroundWorkerLogin.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerLogin_GetAuthenticationTokenCompleted);
        }

        private void OnPayClicked(object context, RoutedEventArgs state)
        {
            this.btnPay.IsEnabled = false;
            this.progressBar.IsIndeterminate = true;
            string username = this.txtUsername.Text;
            string password = this.txtPassword.Password;
            string url = null;
            this.skipSignatureValue = this.skipSignature.IsChecked ?? false;
            this.showReceiptValue = this.showReceipt.IsChecked ?? false;
            string environment = "PROD";

            if (this.sandBox.IsChecked == true)
            {
                environment = "SANDBOX";
                this.sdkEnvironment = AuthorizeNet.Environment.SANDBOX;
            }
            else if (this.custom.IsChecked == true)
            {
                XmlTextReader reader = new XmlTextReader("Env.xml");

                Stack<string> stack = new Stack<string>();
                Dictionary<string, string> keyValue = new Dictionary<string, string>();

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            Debug.Write("<" + reader.Name);
                            keyValue.Add(reader.Name, "");
                            stack.Push(reader.Name);
                            while (reader.MoveToNextAttribute()) // Read the attributes.
                                Console.Write(" " + reader.Name + "='" + reader.Value + "'");
                            Debug.WriteLine(">");
                            break;
                        case XmlNodeType.Text: //Display the text in each element.
                            Debug.WriteLine(reader.Value);
                            string node = stack.Pop();
                            keyValue[node] = reader.Value;
                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.
                            Debug.Write("</" + reader.Name);
                            Debug.WriteLine(">");
                            break;
                    }
                }

                username = keyValue["username"];
                password = keyValue["password"];
                environment = "CUSTOM";
                url = keyValue["url"];
                AuthorizeNet.Environment.createEnvironment(url, url, url);
                this.sdkEnvironment = AuthorizeNet.Environment.CUSTOM;
            }

            if (String.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("You must specify a username.", "Input Error", MessageBoxButton.OK);
                return;
            }

            if (String.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("You must specify a password.", "Input Error", MessageBoxButton.OK);
                return;
            }

            List<object> arguments = new List<object>();
            arguments.Add(username);
            arguments.Add(password);
            arguments.Add("123456789WINSDK");

            string[] workerArguments = new string[] { username, password, "123456789WINSDK", environment };
            backgroundWorkerLogin.RunWorkerAsync(workerArguments);
        }

        private void backgroundWorkerLogin_GetAuthenticationToken(object sender, DoWorkEventArgs e)
        {
            string[] workerArguments = e.Argument as string[];

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = workerArguments[0],
                Item = workerArguments[1],
                ItemElementName = ItemChoiceType.password
            };

            if (workerArguments[3].Equals("SANDBOX"))
            {
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
            }
            else if (workerArguments[3].Equals("CUSTOM"))
            {
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.CUSTOM;
            }
            else
            {
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
            }

            mobileDeviceLoginRequest request = new mobileDeviceLoginRequest()
            {
                merchantAuthentication = new merchantAuthenticationType()
                {
                    name = workerArguments[0],
                    Item = workerArguments[1],
                    mobileDeviceId = workerArguments[2],
                    ItemElementName = ItemChoiceType.password
                }
            };

            BackgroundWorker worker = sender as BackgroundWorker;
            try
            {
                mobileDeviceLoginController controller = new mobileDeviceLoginController(request);
                e.Result = controller.ExecuteWithApiResponse();
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Execption Message" + exception.Message);
            }
            finally
            {

            }
        }

        private void backgroundWorkerLogin_GetAuthenticationTokenProgress(object sender, ProgressChangedEventArgs e)
        {
        }

        private void backgroundWorkerLogin_GetAuthenticationTokenCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mobileDeviceLoginResponse response = e.Result as mobileDeviceLoginResponse;
            MainController mainController = null;

            if (!object.ReferenceEquals(response, null) && !object.ReferenceEquals(response.sessionToken, null))
            {
                this.response = response;
                Debug.WriteLine("RunnerBackground completed" + response.sessionToken);
                //this.Hide();

                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        MainController.merchantName = this.response.merchantContact.merchantName;  // optional
                        MainController.merchantID = "********0108 "; // optional
                        mainController = new MainController(this.sdkEnvironment,
                            "840",
                            "00222273",
                            this.skipSignatureValue,
                            this.showReceiptValue,
                            "123456789WINSDK",
                            this.response.sessionToken);
                        Application.Current.MainWindow = mainController;
                        this.Close();
                        mainController.ShowDialog();
                    });
                }
                catch
                (Exception exception)
                {
                    Debug.Write("Exception in main window" + exception.Message + " " + mainController);
                }
                finally
                {
                }
            }
            else
            {
                Debug.WriteLine("RunnerBackground completed with some error");
                this.btnPay.IsEnabled = true;
                ErrorScreen errorScreen = new ErrorScreen();
                errorScreen.ShowDialog();
            }
        }
    }
}
