Overview:
=========

The Authorize.Net Windows EMV SDK enables merchant application to accept secure chip-card payments.
The SDK is currently a certified solution with TSYS. Before you use the Windows SDK, we advise that you read about the Authorize.Net C# SDK documentation. The C# SDK provides the interface to communicate with the Authorize.Net system. 

Supported Encrypted Readers:
===================
[Supported reader devices can be obtained from Authorize.Net from POS Portal](https://partner.posportal.com/authorizenet/auth/
                                                                              )

Integration Steps
===================

1. Download the SDK from GIT.

2. Copy the SDK folder into the project folder.

3. Open the project in Visual Studio.

4. Add a reference to `ANetEmvDesktop.dll`.

5. Add the following references from NuGet Manager. Tap on Tools -&gt;
NuGet Package Manager -&gt; Manage NuGet Package Manager.
```
i.  AuthorizeNet.dll
ii. BouncyCastle.Crypto.dll
iii.Microsoft.Bcl
iv. Microsft.Bcl.Async
v.  Microsoft.Bcl.Build
```
6. Perform this step is only if your application is processing transactions with the IDTech Augusta reader device. Add the following references from the `IDTechSdk` folder to your project. Right-click your project and add the DLLs below as
existing items. 
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

7. Initialize the `AuthorizeNet` SDK and authenticate the user to
generate the session token. Your application must authenticate the user or login before posting a transaction. 

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

8. Implement the `SdkListener` interface and
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

Initializing `ANetEmvDesktopSdk`
=============================

```
launcher = new SdkLauncher(iEnvironment, iCurrencyCode, iTerminalID,
iSkipSignature, iShowReceipt); 
```

>     iEnvironment: There are two environments: SANDBOX for testing your integration and LIVE for processing real transactions.
>     iCurrencyCode: Currency code of the country for e.g USA currency code is 840
>     iTerminalID: Terminal ID of the merchant terminal
>     iSkipSignature: If merchant does not want customer signature verfication then pass this value as true
>     iShowReceipt: Bool to enable/disble the receipt screen in transaction flow

Seting the Reader Device Type
===========================

 The SDK supports two devices: AnywhereCommerce Walker and IDTech Augusta.
 AnywhereCommerce Walker is select by default.
>   public void setReadername(ReaderName readerName) Refer: SDKLauncher

Seting the Terminal Mode
======================

SDK allows Swipe or Insert_or_swipe. Insert_or_swipe accepts chip-based transactions as well as Swipe/MSR transaction; Swipe accepts only MSR/Swipe transactions. Insert_or_swipe is selected by default. 

>   public void setTerminalMode(TerminalMode iTerminalCapability)

Refer to the `SDKLauncher` file and the sample app for more details.

Setting the Reader Device Connection Type
=====================================

Only the AnywhereCommerce Walker device supports two types of connection: USB and Bluetooth. The IDTech Augusta only supports USB connection. 

>   public void setReadername(ReaderName readerName) Refer: SDKLauncher

Setting Up the Bluetooth Connection
==============================

1. Set the connection type by calling the method below.

> public void setConnection(ConnectionMode iConnectionMode) Refer: SDKLauncher

2. Call the method below to discover nearby devices and present the list to the user.

>   public void establishBTConnectionAndRetrieveNearByDevices(SdkListener iListener)

3. On selection, call the method below to establish a connection with the device.

>   public void connectBTAtIndex(int iSelectedIndex) Refer: SDKLauncher

4. Implement the following methods of `SDKListener: Refer SDKListener`

>   void BTPairedDevicesScanResult(List<BTDeviceInfo> iPairedDevicesList);  Callback method which returns the near by devices
>   void BTConnected(BTDeviceInfo iDeviceInfo);  Callback on Successful Bluetooth connection with the selected device
>   void BTConnectionFailed();  Callback on failure of Bluetooth connection


Processing Transactions
========================

`ANetEmvDesktopSdk` can post transactions in two ways:

* SDK takes control and presents its own UI.

* SDK doesn’t show any UI. SDK triggers the event about the transaction
progress. Your application should respond to these events.

To post a transaction:
-----------------------------

1.  Create a transaction object.

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

2. When the transaction object is populated, call one of the following
methods to post a transaction. Pass the transaction object,
transaction type and `SDKListener`. The SDK currently supports
GOODS, SERVICES, TRANSFER and PAYMENTS transaction types.

### Quick Chip Transaction using the SDK's UI

>     public void startQuickChipTransaction(createTransactionRequest iTransactionRequest, SDKTransactionType iTransactionType, SdkListener iListener);   

#### Listener/Callback Methods

>     void transactionCompleted(createTransactionResponse response, bool isSuccess, string customerSignature, ErrorResponse errorResponse);
>     void transactionCanceled();

### Quick Chip Transaction Without using the SDK's UI

>     public void startQuickChipWithoutUI(createTransactionRequest iTransactionRequest, SDKTransactionType iTransactionType, SdkListener >    iListener);

#### Listener/Callback Methods

>     void transactionCompleted(createTransactionResponse response, bool isSuccess, string customerSignature, ErrorResponse errorResponse);
>     void transactionStatus(TransactionStatus iTransactionStatus);
>     void requestSelectApplication(List<string> appList);
>     void hideCancelTransaction();

### Cancelling the Transaction

This method only applies when you use Quick Chip without the SDK's UI. Your application can call this method to cancel the transaction. If the value returned is true, the transaction was cancelled succesfully.

>     public bool cancelTransaction()

### Processing Card Data

The SDK's Quick Chip funtionality enables your application to process the card data before the final amount is ready. Processing the card does not authorize or capture the transaction; it retrives the card data and stores in in-flight mode inside the SDK. When your application is ready with the final amount, it must initiate a Quick Chip transaction to capture the processed card data. When your application calls the process card method, the following Quick Chip transaction charges the processed card data.

### Processing the Card using a Predetermined Amount

>     public void processCardInBackground(SdkListener iListener, SDKTransactionType iTransactionType)

#### Listener/Callback Methods

>     void processCardProgress(TransactionStatus iProgress);
>     void processCardCompletedWithStatus(bool iStatus);
>     void requestSelectApplication(List<string> appList);

### Discarding Processed Card Data

>     public void discardProcessedCardData()

Updating the Firmware and Configuration
==================================

The SDK is capable of updating the reader device's firmware and
configuration. Your application can determine whether the reader device is current by calling the method of `SdkLauncher` class.

>     public void checkForAnywhereReaderDeviceUpdates(SdkListener iListener, bool isTestReader)

#### Listener/Callback Methods

>     Void OTAUpdateRequired(Tuple<TAUpdateResult, OTAUpdateResult> iCheckUpdateStatus, string iErrorMessage); 

###   Start Firmware/configuration Update With the SDK's UI

>     public void startOTAUpdate(SdkListener iListener, bool isTestReader)

#### Listener/Callback Methods

>     void OTAUpdateCompleted(Tuple<OTAUpdateResult, OTAUpdateResult> iUpdateStatus, string iErrorMessage);

### Start Firmware/configuration Update Without the SDK's UI

>     public void startOTAUpdateWithNoUI(SdkListener iListener, bool isTestReader)

#### Listener/Callback Methods

>     void OTAUpdateProgress(double iPercentage, OTAUpdateType
>     iOTAUpdateType);

>   //item1 Config update
>   //item2 Firmware update

>     void OTAUpdateCompleted(Tuple<OTAUpdateResult, OTAUpdateResult> iUpdateStatus, string iErrorMessage);

## Notes:
For every SDK operation, the SDK has call back methods. The SDK sends notifications about the progress or completion of the operation in callback methods. Callbacks methods are listed under each operations. 

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
