using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AuthorizeNet.Api.Controllers.Bases;
using AuthorizeNet.Api.Contracts.V1;
using System.Diagnostics;
using ANetEmvDesktopSdk.Services;
using ANetEmvDesktopSdk.UI;
using ANetEmvDesktopSdk.Common;
using ANetEmvDesktopSdk.SDK;
using ANetEmvDesktopSdk.Models;

namespace ANetEmvDesktopSdk.Sample
{
    /// <summary>
    /// Interaction logic for MainController.xaml
    /// </summary>
    public partial class MainController : Window, SdkListener
    {
        public static string merchantName = null;
        public static string merchantID = null;
        private string sessionToken = null;
        private string deviceID = null;
        private AuthorizeNet.Environment sdkEnvironment = AuthorizeNet.Environment.SANDBOX;
        private string currencyCode = null;
        private string terminalID = null;
        bool skipSignature = false;
        private ListWindow listWindow;
        private bool showReceipt = true;


        private SdkLauncher launcher = null;
        public MainController()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Application.Current.Exit += new ExitEventHandler(this.OnApplicationExit);
        }

        public MainController(AuthorizeNet.Environment iEnvironment,
            string iCurrencyCode,
            string iTerminalID,
            bool iSkipSignature,
            bool iShowReceipt,
            string iDeviceID,
            string iSessionToken)
        {
            InitializeComponent();
            Application.Current.Exit += new ExitEventHandler(this.OnApplicationExit);
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.sdkEnvironment = iEnvironment;
            this.currencyCode = iCurrencyCode;
            this.terminalID = iTerminalID;
            this.skipSignature = iSkipSignature;
            this.showReceipt = iShowReceipt;
            this.sessionToken = iSessionToken;
            this.deviceID = iDeviceID;

            Debug.Write("Session Token in Constructor" + iSessionToken);
            Random random = new Random();
            this.amount.Text = (random.Next(1, 1000)).ToString();
            this.launcher = new SdkLauncher(iEnvironment, iCurrencyCode, iTerminalID, iSkipSignature, iShowReceipt);
            this.launcher.setMerchantInfo(merchantName, merchantID);
            this.launcher.enableLogging();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            Debug.Write("OnApplicationExit");
            try
            {
                // Ignore any errors that might occur while closing the file handle.
                this.launcher.stopUSBConnection();
            }
            catch
            {
            }
        }
        private void emvTransaction(object sender, RoutedEventArgs e)
        {
            this.launcher.setTerminalMode((this.terminalMode.IsChecked == true) ? TerminalMode.Swipe : TerminalMode.Insert_or_swipe);
            this.launcher.setConnection((this.connectionMode.IsChecked == true) ? ConnectionMode.Bluetooth : ConnectionMode.USB);
            this.launcher.startEMVTransaction(this.getRequest(), SDKTransactionType.GOODS, this);
        }

        private void processCard(object sender, RoutedEventArgs e)
        {
            this.launcher.setTerminalMode((this.terminalMode.IsChecked == true) ? TerminalMode.Swipe : TerminalMode.Insert_or_swipe);
            this.launcher.setConnection((this.connectionMode.IsChecked == true) ? ConnectionMode.Bluetooth : ConnectionMode.USB);
            this.launcher.processCardInBackground(this, SDKTransactionType.GOODS);
        }

        private void discardCard(object sender, RoutedEventArgs e)
        {
            this.launcher.discardProcessedCardData();
            this.transactionStatus.Text = "Discarded processed card.";
        }

        private void quickChipTransaction(object sender, RoutedEventArgs e)
        {
            this.launcher.setTerminalMode((this.terminalMode.IsChecked == true) ? TerminalMode.Swipe : TerminalMode.Insert_or_swipe);
            this.launcher.setConnection((this.connectionMode.IsChecked == true) ? ConnectionMode.Bluetooth : ConnectionMode.USB);
            this.launcher.setReadername((this.readerName.IsChecked == true) ? ReaderName.IDTech_Augusta : ReaderName.AnywhereCommerce_Walker);
            this.launcher.startQuickChipTransaction(this.getRequest(), SDKTransactionType.GOODS, this);
        }

        private void quickChipHeadless(object sender, RoutedEventArgs e)
        {

            if(this.connectionMode.IsChecked == true)
                this.transactionStatus.Text = "Scanning bluetooth devices...";  //@Parth
            Debug.Write("quickChipHeadless" + this.button4 + this.launcher);
            this.launcher.setTerminalMode((this.terminalMode.IsChecked == true) ? TerminalMode.Swipe : TerminalMode.Insert_or_swipe);
            this.launcher.setConnection((this.connectionMode.IsChecked == true) ? ConnectionMode.Bluetooth : ConnectionMode.USB);
            this.launcher.setReadername((this.readerName.IsChecked == true) ? ReaderName.IDTech_Augusta : ReaderName.AnywhereCommerce_Walker);
            this.launcher.startQuickChipWithoutUI(this.getRequest(), SDKTransactionType.GOODS, this);
        }

        private void quickChipWithTipAmount(object sender, RoutedEventArgs e)
        {
        }

        private void quickChipWithTipOptions(object sender, RoutedEventArgs e)
        {

        }

        private void getDeviceInfo(object sender, RoutedEventArgs e)
        {
            Debug.Write("MainController:getDeviceInfo");
            this.transactionStatus.Text = "Getting device info...";
            this.launcher.setReadername((this.readerName.IsChecked == true) ? ReaderName.IDTech_Augusta : ReaderName.AnywhereCommerce_Walker);
            this.launcher.setConnection((this.connectionMode.IsChecked == true) ? ConnectionMode.Bluetooth : ConnectionMode.USB);
            this.launcher.getAnyWhereReaderInfo(this);
        }

        private void checkForUpdates(object sender, RoutedEventArgs e)
        {
            Debug.Write("MainController:checkForUpdates");
            this.transactionStatus.Text = "Checking for updates...";
            this.launcher.setConnection((this.connectionMode.IsChecked == true) ? ConnectionMode.Bluetooth : ConnectionMode.USB);
            this.launcher.checkForAnywhereReaderDeviceUpdates(this, this.isTestReader.IsChecked ?? false);
        }

        private void OTAUpdates(object sender, RoutedEventArgs e)
        {
            Debug.Write("MainController:OTAUpdates");
            this.launcher.setConnection((this.connectionMode.IsChecked == true) ? ConnectionMode.Bluetooth : ConnectionMode.USB);
            this.launcher.startOTAUpdate(this, this.isTestReader.IsChecked ?? false);
        }

        private void OTAUpdatesHeadless(object sender, RoutedEventArgs e)
        {
            Debug.Write("MainController:OTAUpdatesHeadless");
            this.transactionStatus.Text = "Updating...";
            this.launcher.setConnection((this.connectionMode.IsChecked == true) ? ConnectionMode.Bluetooth : ConnectionMode.USB);
            this.launcher.startOTAUpdateWithNoUI(this, this.isTestReader.IsChecked ?? true);
            this.transactionStatus.Text = "Updating Config";
        }

        private void IsAugustaConnected(object sender, RoutedEventArgs e)
        {
            Debug.Write("MainController:IsAugustaConnected");
            this.transactionStatus.Text = "Checking...";
            this.launcher.setReadername(ReaderName.IDTech_Augusta);
            this.launcher.isAugustaReaderDeviceConnected(this);
            this.transactionStatus.Text = "";
        }

        private void close(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            this.amount.Text = (random.Next(1, 1000)).ToString();
        }

        private createTransactionRequest getRequest()
        {
            Debug.Write("Session Token" + this.sessionToken);
            Random random = new Random();

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                Item = this.sessionToken,
                mobileDeviceId = this.deviceID,
                ItemElementName = ItemChoiceType.sessionToken
            };

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = this.sdkEnvironment;

            transactionRequestType transaction = new transactionRequestType()
            {
                amount = Convert.ToDecimal(this.amount.Text, CultureInfo.InvariantCulture),
                transactionSettings = new settingType[] {
                },
                retail = new transRetailInfoType()
                {
                    deviceType = "7",
                    marketType = "2",
                },
                order = new orderType()
                {
                    description = "Windows SDK Order",
                    invoiceNumber = Convert.ToString(random.Next(1999999, 999999999))
                }
            };
            transaction.terminalNumber = this.terminalID;
            createTransactionRequest request = new createTransactionRequest()
            {
                transactionRequest = transaction
            };
            Debug.Write("Session Token" + this.sessionToken);
            request.merchantAuthentication = new merchantAuthenticationType()
            {

                Item = this.sessionToken,
                mobileDeviceId = this.deviceID,
                ItemElementName = ItemChoiceType.sessionToken
            };
            return request;
        }


        private void updateTransactionStatus(TransactionStatus iStatus)
        {
            Debug.Write("updateTransactionStatus" + iStatus);
            switch (iStatus)
            {
                case TransactionStatus.NoUSBSwiperDeviceConnected:
                    this.transactionStatus.Text = "No swiper device connected.";
                    break;
                case TransactionStatus.WaitingForCard:
                    this.transactionStatus.Text = (this.terminalMode.IsChecked == true) ? "Please swipe the card." : "Please insert or swipe card.";
                    break;
                case TransactionStatus.CardReadError:
                    this.transactionStatus.Text = "Could not read the card, please remove the card and press ok to try again.";
                    break;
                case TransactionStatus.RetrySwipe:
                    this.transactionStatus.Text = "Please swipe again.";
                    break;
                case TransactionStatus.SwipeCard:
                    this.transactionStatus.Text = "Please swipe..";
                    break;
                case TransactionStatus.ICCCard:
                    this.transactionStatus.Text = "Please insert the card.";
                    break;
                case TransactionStatus.NotICCCard:
                    this.transactionStatus.Text = "Non Chip Card, please swipe.";
                    break;
                case TransactionStatus.ProcessingTransaction:
                    this.transactionStatus.Text = "Processing transaction...";
                    break;
                case TransactionStatus.RemoveCard:
                    this.transactionStatus.Text = "Please remove the card";
                    break;
            }
        }

        void SdkListener.transactionCompleted(createTransactionResponse response, bool isSuccess, string customerSignature, ErrorResponse errorResponse)
        {
            Debug.Write("\n MainController:transactionCompleted" + "\n" + response + "\n" + isSuccess + "\n" + errorResponse);

            this.Dispatcher.Invoke(() =>
            {
                if (isSuccess)
                {
                    Random random = new Random();
                    this.amount.Text = (random.Next(1, 1000)).ToString();
                    this.transactionStatus.Text = string.Format("Transaction Approved \n Transaction ID {0}", response.transactionResponse.transId);
                }
                else
                {
                    this.transactionStatus.Text = string.Format("Transaction Failed \n {0}", errorResponse.errorMessage);
                }
            });
        }

        void SdkListener.transactionStatus(TransactionStatus iTransactionStatus)
        {
            Debug.Write("MainController:transactionStatus" + "\n" + iTransactionStatus);

            this.Dispatcher.Invoke(() =>
            {
                if (iTransactionStatus == TransactionStatus.CardReadError)
                {
                    CardReaderErrorWindow anErrorWindow = new CardReaderErrorWindow("Could not read the card, please remove the card and press ok to try again");
                    anErrorWindow.okButtonEvent += new SDKEventHandler(readerErrorOkAction);
                    anErrorWindow.cancelButtonEvent += new SDKEventHandler(readerErrorCancelAction);
                    anErrorWindow.ShowDialog();
                }
                this.updateTransactionStatus(iTransactionStatus);
            });
        }

        private void readerErrorOkAction(object sender, EventArgs e)
        {
            Debug.Write("MainController:readerErrorOkAction");
            this.launcher.retryCheckCard();
        }

        private void readerErrorCancelAction(object sender, EventArgs e)
        {
            Debug.Write("MainController:readerErrorCancelAction");
            this.launcher.cancelCheckCard();
        }

        void SdkListener.transactionCanceled()
        {
            Debug.Write("MainController:transactionCanceled");
            this.Dispatcher.Invoke(() =>
            {
                this.transactionStatus.Text = "Transaction Canceled.";
            });
        }

        void SdkListener.hideCancelTransaction()
        {
            Debug.Write("MainController:hideCancelTransaction");

        }

        void SdkListener.processCardProgress(TransactionStatus iProgress)
        {
            Debug.Write("MainController:processCardProgress" + "\n" + iProgress);
            this.Dispatcher.Invoke(() =>
            {
                this.updateTransactionStatus(iProgress);
            });
        }

        void SdkListener.processCardCompletedWithStatus(bool iStatus)
        {
            Debug.Write("MainController:processCardCompletedWithStatus" + "\n" + iStatus);

            this.Dispatcher.Invoke(() =>
            {
                if (iStatus)
                {
                    this.transactionStatus.Text = "Processed card successfully.";
                }
                else
                {
                    this.transactionStatus.Text = "Process card failed.";
                }
            });
        }

        void SdkListener.requestSelectApplication(List<string> appList)
        {
            Debug.Write("MainController:requestSelectApplication");
            this.Dispatcher.Invoke(() =>
            {
                ApplicationSelectionWindow applicationSelectionWindow = new ApplicationSelectionWindow(appList);
                applicationSelectionWindow.selectionApplicationEvent += new ApplicationSelectionEventHandler(selectApplicationAction);
                applicationSelectionWindow.cancelEvent += new SDKEventHandler(cancelApplicationSelectionAction);
                applicationSelectionWindow.ShowDialog();
            });
        }

        void SdkListener.readerDeviceInfo(Dictionary<string, string> iDeviceInfo)
        {
            Debug.Write("MainController.readerDeviceInfo" + iDeviceInfo);

            this.Dispatcher.Invoke(() =>
            {
                string s = "Could not retrieve device info.";

                if (!object.Equals(iDeviceInfo, null))
                {
                    s = string.Join(";", iDeviceInfo.Select(x => x.Key + "=" + x.Value).ToArray());
                }

                MessageWindow messageWindow = new MessageWindow(s);
                messageWindow.ShowDialog();
            });
        }

        void SdkListener.OTAUpdateRequired(Tuple<OTAUpdateResult, OTAUpdateResult> iCheckUpdateStatus, string iErrorMessage)
        {
            Debug.Write("MainController.OTAUpdateRequired" + iCheckUpdateStatus + iErrorMessage);
            this.Dispatcher.Invoke(() =>
            {
                if (!object.Equals(iCheckUpdateStatus, null))
                {
                    if (iCheckUpdateStatus.Item1.status == OTAUpdateStatus.SUCCESS ||
                            iCheckUpdateStatus.Item2.status == OTAUpdateStatus.SUCCESS)
                    {
                        this.transactionStatus.Text = "Update Required.";
                    }
                    else if (!object.ReferenceEquals(iErrorMessage, null))
                    {
                        this.transactionStatus.Text = iErrorMessage;
                    }
                    else
                    {
                        this.transactionStatus.Text = "Deivce is upto date.";
                    }
                }
            });
        }

        void SdkListener.OTAUpdateProgress(double iPercentage, OTAUpdateType iOTAUpdateType)
        {
            Debug.Write("MainController.OTAUpdateProgress" + iPercentage + iOTAUpdateType);
            this.Dispatcher.Invoke(() =>
            {
                if (OTAUpdateType.ConfigUpdate == iOTAUpdateType)
                {
                    this.transactionStatus.Text = "Updating Config...";
                }
                else
                {
                    this.transactionStatus.Text = "Updating Firmware...";
                }
                this.OTAProgressBar.Value = iPercentage;
            });
        }

        void SdkListener.OTAUpdateCompleted(Tuple<OTAUpdateResult, OTAUpdateResult> iUpdateStatus, string iErrorMessage)
        {
            Debug.Write("MainController.OTAUpdateCompleted" + iUpdateStatus + iErrorMessage);
            this.Dispatcher.Invoke(() =>
            {
                if (iUpdateStatus.Item1.status == OTAUpdateStatus.SUCCESS && iUpdateStatus.Item2.status == OTAUpdateStatus.SUCCESS)
                {
                    this.transactionStatus.Text = "Update successful.";
                }
                else
                {
                    this.transactionStatus.Text = "Update failed.";
                }
            });
        }

        private void refreshAction(object sender, EventArgs e)
        {
            this.launcher.refreshBTDevices();
        }

        private void listWindowClosedAction(object sender, EventArgs e)
        {

        }

        private void deviceSelected(object sender, ListItemSelectedEventArgs e)
        {
            this.launcher.connectBTAtIndex(e.SelecteIndex);
            this.listWindow.Close();  
        }

        void SdkListener.BTPairedDevicesScanResult(List<BTDeviceInfo> iList)
        {
            Debug.Write("MainController.BTPairedDevicesScanResult" + iList);

            List<string> devices = Utils.BTDeviceNames(iList);

            this.Dispatcher.Invoke(() =>
            {
                if (object.ReferenceEquals(this.listWindow, null) || !this.listWindow.IsActive)
                {
                    this.listWindow = new ListWindow(devices);
                    this.listWindow.refreshEvent += new SDKEventHandler(refreshAction);
                    this.listWindow.itemSelectionEvent += new ListItemSelectionEventHandler(deviceSelected);
                    this.listWindow.windowClosedEvent += new SDKEventHandler(listWindowClosedAction);
                    this.listWindow.ShowDialog();
                }
                else
                {
                    this.listWindow.hideLoadingIndicator();
                    this.listWindow.reloadList(devices);
                }
            });
        }

        //void SdkListener.BTConnected()
        //{
        //    Debug.Write("MainController.BTConnected");

        //    this.Dispatcher.Invoke(() =>
        //    {
        //        if (!object.ReferenceEquals(this.listWindow, null))
        //        {
        //            this.listWindow.Hide();
        //            this.listWindow.Close();
        //        }
        //    });
        //}

        void SdkListener.BTConnectionFailed()
        {
            Debug.Write("MainController.BTConnectionFailed");

            this.Dispatcher.Invoke(() =>
            {
                if (!object.ReferenceEquals(this.listWindow, null))
                {
                    this.listWindow.hideLoadingIndicator();
                    MessageBox.Show("Could not connect establish the connection.");
                }
            });
        }

        void SdkListener.isAugustaReaderDeviceConnected(Dictionary<string, string> iDeviceInfo)
        {
            if (iDeviceInfo != null)
            {
                MessageBox.Show("Augusta device connected. Serial Number : " + iDeviceInfo["serialNumber"] + " Firmware Version : " + iDeviceInfo["firmwareVersion"]);
            }
            else
            {
                MessageBox.Show("Augusta device NOT connected.");
            }
        }

        private void selectApplicationAction(object sender, ApplicationSelectedEventArgs e)
        {
            Debug.Write("MainController:selectApplicationAction");
            this.launcher.selectApplication(e.SelectedApplication);
        }

        private void cancelApplicationSelectionAction(object sender, EventArgs e)
        {
            Debug.Write("MainController:cancelApplicationSelectionAction");
            this.launcher.cancelSelectApplication();
        }

        private void logout(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            this.Close();
            mainWindow.ShowDialog();
        }

        void SdkListener.BTConnected(BTDeviceInfo iDeviceInfo)
        {
            Debug.Write("MainController:SdkListener.BTConnected");
        }
    }
}