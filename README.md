Overview:
=========

Windows EMV SDK allows merchant application to accept chip card payments.
EMV SDK securely does a chip transaction; it is a certified solution
with TSYS processor. It is advisable that before using this SDK developer must read about AuthorizeNet CSharp SDK. CSharp SDK provides interface to communicate with Authorize.Net system. 

For frequently asked questions, see [the EMV FAQ page](https://www.authorize.net/support/emvfaqs/).
To determine which processor you use, you can submit an API call to [getMerchantDetailsRequest](https://developer.authorize.net/api/reference/#transaction-reporting-get-merchant-details). The response contains a `processors` object master

Supported Encrypted Readers:
===================
[Supported reader devices can be obtained from Authorize.Net from POS Portal](https://partner.posportal.com/authorizenet/auth/
                                                                              )

Steps to integrate:
===================

1. Download the SDK from GIT

2. Copy the SDK folder into project folder

3. Open the project in Visual Studio

4. Add reference to ANetEmvDesktop.dll

5. Add the following references from NuGet Manager, Tap on Tools -&gt;
NuGet Package Manager -&gt; Manage NuGet Package Manager
```
i.  AuthorizeNet.dll
ii. BouncyCastle.Crypto.dll
iii.Microsoft.Bcl
iv. Microsft.Bcl.Async
v.  Microsoft.Bcl.Build
```
6. Add the following references from IDTechSdk folder to your project, right click on your project and add the below dll's as an 
existing items. This step is only if application is processing transactions with the IDTech_Augusta reader device. 
```
i.   Augusta_config.dll
ii.  Augusta_device.dll
iii. Augusta_emv.dll
iv.  Augusta_icc.dll
v.   Augusta_KSB_config.dll
vi.  Augusta_KB_device.dll
vii. Augusta_KB_msr.dll
viii.Augusta_KB_parse.dll
ix.  Augusta_msr.dll
x.   Augusta_parse.dll
```

7. Initialize the AuthorizeNet SDK and authenticate the user to
generate the session token. Merchant application must authenticate the user or login before posting transaction. 

>     ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
>     mobileDeviceLoginRequest request = new mobileDeviceLoginRequest()
>     {
>         merchantAuthentication = new merchantAuthenticationType()
>         {
>                name = //username,
>                Item = //password,
>                mobileDeviceId = //unique device identifier,
>                ItemElementName = ItemChoiceType.password
>          }
>     };
>     mobileDeviceLoginController controller = new mobileDeviceLoginController(request);
>     mobileDeviceLoginResponse response = controller.ExecuteWithApiResponse();

8. Merchant application must implement SdkListener interface and
respond to all the methods.

>     public interface SdkListener
>     {
>         void transactionCompleted(createTransactionResponse response, bool isSuccess, string customerSignature, ErrorResponse 
>         errorResponse);
>         void transactionStatus(TransactionStatus iTransactionStatus);
>         void transactionCanceled();
>         void hideCancelTransaction();
>         void processCardProgress(TransactionStatus iProgress);
>         void processCardCompletedWithStatus(bool iStatus);
>         void requestSelectApplication(List<string> appList);
>         void readerDeviceInfo(Dictionary<string, string> iDeviceInfo);
>
>       //item1 Config update
>       //item2 Firmware update
>         void OTAUpdateRequired(Tuple<OTAUpdateResult, OTAUpdateResult> iCheckUpdateStatus, string iErrorMessage);
>         void OTAUpdateProgress(double iPercentage, OTAUpdateType iOTAUpdateType);
>
>       //item1 Config update
>       //item2 Firmware update
>         void OTAUpdateCompleted(Tuple<OTAUpdateResult, OTAUpdateResult> iUpdateStatus, string iErrorMessage);
>     }

Initialize ANetEmvDesktopSdk:
=============================

launcher = new SdkLauncher(iEnvironment, iCurrencyCode, iTerminalID,
iSkipSignature, iShowReceipt); 

>     iEnvironment: There are two environments: SANDBOX for testing your integration and LIVE for processing real transactions.
>     iCurrencyCode: Currency code of the country for e.g USA currency code is 840
>     iTerminalID: Terminal ID of the merchant terminal
>     iSkipSignature: If merchant does not want customer signature verfication then pass this value as true
>     iShowReceipt: Bool to enable/disble the receipt screen in transaction flow

Set the reader device type: 
===========================
```
 SDK supports two devices: AnywhereCommerce_Walker and IDTech_Augusta
 AnywhereCommerce_Walker is select by default
```
>   public void setReadername(ReaderName readerName) Refer: SDKLauncher

Set the terminal mode:
======================
```
SDK allows Swipe or Insert_or_swipe. Insert_or_swipe accepts CHIP Based transactions as well as Swipe/MSR transaction, Swipe accepts only MSR/Swipe transactions. 
```
>   public void setTerminalMode(TerminalMode iTerminalCapability)
```
  Insert_or_swipe is selected by default
  Refer to the SDKLauncher file and the sample app for more details.
```
Set the reader device connection type:
=====================================
```
Only AnywhereCommerce_Walker device supports two types of connection: USB and Bluetooth. IDTech_Augusta only supports USB connection. 
```
>   public void setReadername(ReaderName readerName) Refer: SDKLauncher

Setup the Bluetooth connection:
==============================
``` 
 Set the connection type by calling the below method
 public void setConnection(ConnectionMode iConnectionMode) Refer: SDKLauncher
 Call the below method to discover the near by devices and present the list to the user
```
>   public void establishBTConnectionAndRetrieveNearByDevices(SdkListener iListener)
```
On selection call the below method to establish the connection with the device
```
>   public void connectBTAtIndex(int iSelectedIndex) Refer: SDKLauncher
```
Implement the below methods of SDKListener: Refer SDKListener
```
>   void BTPairedDevicesScanResult(List<BTDeviceInfo> iPairedDevicesList);  Callback method which returns the near by devices
>   void BTConnected(BTDeviceInfo iDeviceInfo);  Callback on Successful Bluetooth connection with the selected device
>   void BTConnectionFailed();  Callback on failure of Bluetooth connection


Transaction Processing:
========================

ANetEmvDesktopSdk can post transaction in two different flavors: first
where SDK takes control and presents its own UI, second where in SDK
doesn’t show any UI; SDK triggers the event about the transaction
progress, Merchant application should respond to these events.

Steps to post a transaction: 
-----------------------------

1.  Create a transaction object in the following
>       private createTransactionRequest getRequest ()
>        {
>            Debug.Write("Session Token" + this.sessionToken);
>            Random random = new Random();
>
>            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
>            {
>                Item = this.sessionToken,
>                mobileDeviceId = this.deviceID,
>                ItemElementName = ItemChoiceType.sessionToken
>            };
>
>           ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = this.sdkEnvironment;
>
>            transactionRequestType transaction = new transactionRequestType()
>            {
>                amount = Convert.ToDecimal(this.amount.Text, CultureInfo.InvariantCulture),
>                transactionSettings = new settingType[] {
>                },
>                retail = new transRetailInfoType()
>                {
>                    deviceType = "7",
>                    marketType = "2",
>                },
>                order = new orderType()
>                {
>                    description = "Windows SDK Order",
>                    invoiceNumber = Convert.ToString(random.Next(1999999, 999999999))
>                }
>            };
>            transaction.terminalNumber = this.terminalID;
>            createTransactionRequest request = new createTransactionRequest()
>            {
>                transactionRequest = transaction
>            };
>            Debug.Write("Session Token" + this.sessionToken);
>            request.merchantAuthentication = new merchantAuthenticationType()
>            {
>                
>                Item = this.sessionToken,
>                mobileDeviceId = this.deviceID,
>                ItemElementName = ItemChoiceType.sessionToken
>            };
>            return request;
>        }

2. Once transaction object is populated, call one of the following
methods to post a transaction. Pass the transaction object,
transaction type and SDKListenere. Currently SDK only supports
GOODS, SERVICES, TRANSFER and PAYMENTS transaction type

### Quick Chip transaction with SDK provided UI:

>     public void startQuickChipTransaction(createTransactionRequest iTransactionRequest, SDKTransactionType iTransactionType, SdkListener iListener);   

#### Listener/Callback methods:

>     void transactionCompleted(createTransactionResponse response, bool isSuccess, string customerSignature, ErrorResponse errorResponse);
>     void transactionCanceled();

### Quick Chip transaction with no UI from SDK:

>     public void startQuickChipWithoutUI(createTransactionRequest iTransactionRequest, SDKTransactionType iTransactionType, SdkListener >    iListener);


#### Listener/Callback methods: 

>     void transactionCompleted(createTransactionResponse response, bool isSuccess, string customerSignature, ErrorResponse errorResponse);
>     void transactionStatus(TransactionStatus iTransactionStatus);
>     void requestSelectApplication(List<string> appList);
>     void hideCancelTransaction();

### Cancel the transaction:
This method is only applicable in case of Quick Chip transaction with no UI from SDK. Merchant application can call this method to cancel the transaction. If returned value is true then transaction was canceled succesfully else SDK could not cancel the transaction.

>     public bool cancelTransaction()

### Process card:
SDK's Quick Chip funtionality allows merchant application to process the card data even before the final amount is ready. Processing the card does not authorize or capture the transaction; however, it retrives the card data and stores in inflight mode inside the SDK. When merchant application is ready with the final amount, application must initiate a Quick Chip transaction to capture the processed card data. When merchant application calls the process card method, the following Quick Chip transaction charges the processed card data.

### Process card with predetermined amount:
>     public void processCardInBackground(SdkListener iListener, SDKTransactionType iTransactionType)

#### Listener/Callback methods:

>     void processCardProgress(TransactionStatus iProgress);
>     void processCardCompletedWithStatus(bool iStatus);
>     void requestSelectApplication(List<string> appList);

### Discard Processed Card data:
In case, Merchant application does not want to charge the processed card. Merchant application can discard the processed
card data.

>     public void discardProcessedCardData()

Firmware and Configuration update:
==================================

Windows EMV is capable of updating the reader device firmware and
configuration. Merchant application can check if reader device is up to
date or not by calling the below method of SdkLauncher class

###   Check for firmware or configuration updates:

>     public void checkForAnywhereReaderDeviceUpdates(SdkListener iListener, bool isTestReader)

#### Listener/Callback methods:

>     Void OTAUpdateRequired(Tuple<TAUpdateResult, OTAUpdateResult> iCheckUpdateStatus, string iErrorMessage); 

Update Required In case update is required then Merchant application can
call one of the following method to update the configuration or firmware

###   Start Firmware/configuration update with SDK provided UI:

>     public void startOTAUpdate(SdkListener iListener, bool isTestReader)

#### Listener/Callback methods:

>     void OTAUpdateCompleted(Tuple<OTAUpdateResult, OTAUpdateResult> iUpdateStatus, string iErrorMessage);

### Start Firmware/configuration update with no UI from SDK:
>     public void startOTAUpdateWithNoUI(SdkListener iListener, bool isTestReader)

#### Listener/Callback methods:

>     void OTAUpdateProgress(double iPercentage, OTAUpdateType
>     iOTAUpdateType);

>   //item1 Config update
>   //item2 Firmware update

>     void OTAUpdateCompleted(Tuple<OTAUpdateResult, OTAUpdateResult> iUpdateStatus, string iErrorMessage);


## Notes:
For every SDK operation, SDK has call back methods. SDK notifies about the progress or completion of the operation in callback methods. Callbacks methods are listed under each operations. 


## Error  Codes
You can view these error messages at our [Reason Response Code Tool](http://developer.authorize.net/api/reference/responseCodes.html) by entering the specific Response Reason Code into the tool. There will be additional information and suggestions there.

Field Order	| Response Code | Response Reason Code | Text
--- | --- | --- | ---
3 | 2 | 355	| An error occurred while parsing the EMV data.
3 | 2 | 356	| EMV-based transactions are not currently supported for this processor and card type.
3 | 2 | 357	| Opaque Descriptor is required.
3 | 2 | 358	| EMV data is not supported with this transaction type.
3 | 2 | 359	| EMV data is not supported with this market type.
3 | 2 | 360	| An error occurred while decrypting the EMV data.
3 | 2 | 361	| The EMV version is invalid.
3 | 2 | 362	| x_emv_version is required.
