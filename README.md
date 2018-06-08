Overview
=========

The In-Person Windows SDK enables your payment application to securely submit chip-card payments to Authorize.Net. Before you use this SDK, we recommend that you familizarize yourself with [Authorize.Net's C# SDK](https://github.com/AuthorizeNet/sample-code-csharp), which provides an interface to communicate with Authorize.Net. 

This SDK is currently a certified solution with TSYS. To determine which processor you use, you can submit an API call to [getMerchantDetailsRequest](https://developer.authorize.net/api/reference/#transaction-reporting-get-merchant-details). The response contains a `processors` object master.

For a list of frequently asked questions, see [our EMV FAQ page](https://support.authorize.net/s/article/Merchant-EMV-Chip-FAQs).

Supported Encrypted Readers
===================
Encrypted card readers supported by this SDK can be obtained from our [POS Portal](https://partner.posportal.com/authorizenet/auth/).

Integrating the SDK With Your Application
===================

1. Download the SDK.

2. Copy the SDK folder into your project folder.

3. Open the project in Visual Studio.

4. Add a reference to `ANetEmvDesktop.dll`.

5. Add the following references from NuGet Manager. Tap on Tools -&gt;
NuGet Package Manager -&gt; Manage NuGet Package Manager

```
i.  AuthorizeNet.dll
ii. BouncyCastle.Crypto.dll
iii.Microsoft.Bcl
iv. Microsft.Bcl.Async
v.  Microsoft.Bcl.Build
```

6. If you use the IDTech_Augusta card reader, add the .dll references shown below from the _IDTechSdk_ folder to your project. Then right-click your project and add the .dll references as existing items. 

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

7. Initialize the `AuthorizeNet` SDK and authenticate the user to generate the session token. Your application must log in or authenticate the user before posting transactions. 

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

8. Your application must implement the `SdkListener` interface and respond to all the methods as shown below.

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

Initializing ANetEmvDesktopSdk
=============================

```
launcher = new SdkLauncher(iEnvironment, iCurrencyCode, iTerminalID,
iSkipSignature, iShowReceipt); 
```

>     iEnvironment: There are two environments - SANDBOX for testing your integration and LIVE for processing real transactions.
>     iCurrencyCode: Currency code of the country. For example, the USA currency code is 840.
>     iTerminalID: Terminal ID of the merchant terminal.
>     iSkipSignature: Set to true to skip the signature during checkout.
>     iShowReceipt: Boolean to enable ordisble the receipt screen in the transaction flow.

Set Reader Device Type 
===========================

```
 SDK supports two devices: AnywhereCommerce_Walker and IDTech_Augusta
 AnywhereCommerce_Walker is selected by default.
```

>   public void setReadername(ReaderName readerName) Refer: SDKLauncher

Set Terminal Mode
======================

```
SDK allows Swipe or Insert_or_swipe. Insert_or_swipe accepts chio-based transactions as well as Swipe/MSR transaction; Swipe accepts only MSR/Swipe transactions. 
```

>   public void setTerminalMode(TerminalMode iTerminalCapability)

```
  Insert_or_swipe is selected by default.
  Refer to the SDKLauncher file and the sample app for more details.
```

Set Reader Device Connection Type
=====================================

```
Only AnywhereCommerce_Walker device supports two types of connection: USB and Bluetooth. IDTech_Augusta only supports USB connection. 
```

>   public void setReadername(ReaderName readerName) Refer: SDKLauncher

Set Up the Bluetooth Connection
==============================

``` 
 Set the connection type by calling the following method:
 public void setConnection(ConnectionMode iConnectionMode) Refer: SDKLauncher
 Call the method below to discover the nearby devices and present the list to the user.
```

>   public void establishBTConnectionAndRetrieveNearByDevices(SdkListener iListener)

```
On selection, call the following method to establish the connection with the device.
```

>   public void connectBTAtIndex(int iSelectedIndex) Refer: SDKLauncher

```
Implement the following methods of SDKListener: Refer SDKListener.
```

>   void BTPairedDevicesScanResult(List<BTDeviceInfo> iPairedDevicesList);  Callback method which returns the near by devices
>   void BTConnected(BTDeviceInfo iDeviceInfo);  Callback on Successful Bluetooth connection with the selected device
>   void BTConnectionFailed();  Callback on failure of Bluetooth connection

Transaction Processing
========================

`ANetEmvDesktopSdk` can post transactions using two different options. In the first option, the SDK takes control and presents its own UI. In the second option, the SDK doesn’t show any UI; the SDK triggers the event about the transaction progress, and your application responds to these events.

Steps to post a transaction: 
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

2. Once the transaction object is populated, call one of the following
methods to post a transaction, passing the transaction object,
transaction type and `SDKListener`. Currently SDK only supports
GOODS, SERVICES, TRANSFER and PAYMENTS transaction type.

### Quick Chip Transaction With SDK-Provided UI

>     public void startQuickChipTransaction(createTransactionRequest iTransactionRequest, SDKTransactionType iTransactionType, SdkListener iListener);   

#### Listener/Callback Methods

>     void transactionCompleted(createTransactionResponse response, bool isSuccess, string customerSignature, ErrorResponse errorResponse);
>     void transactionCanceled();

### Quick Chip Transaction With No UI from the SDK

>     public void startQuickChipWithoutUI(createTransactionRequest iTransactionRequest, SDKTransactionType iTransactionType, SdkListener >    iListener);


#### Listener/Callback Methods

>     void transactionCompleted(createTransactionResponse response, bool isSuccess, string customerSignature, ErrorResponse errorResponse);
>     void transactionStatus(TransactionStatus iTransactionStatus);
>     void requestSelectApplication(List<string> appList);
>     void hideCancelTransaction();

### Cancel the Transaction
This method only applies when you use a Quick Chip transaction with no UI from the SDK. Your application can call this method to cancel the transaction. If the returned value is true then the transaction was canceled succesfully, else the SDK could not cancel the transaction.

>     public bool cancelTransaction()

### Process a Card
The SDK's Quick Chip funtionality enables your application to process the card data even before the final amount is ready. However, processing the card does not authorize or capture the transaction. It retrives the card data and stores it in in-flight mode inside the SDK. When your application is ready with the final amount, it must initiate a Quick Chip transaction to capture the processed card data. When your application calls the process card method, the following Quick Chip transaction charges the processed card data.

### Process Card with Predetermined Amount
>     public void processCardInBackground(SdkListener iListener, SDKTransactionType iTransactionType)

#### Listener/Callback Methods

>     void processCardProgress(TransactionStatus iProgress);
>     void processCardCompletedWithStatus(bool iStatus);
>     void requestSelectApplication(List<string> appList);

### Discard Processed Card Data
If the transaction is cancelled, your application must discard the processed card data.

>     public void discardProcessedCardData()

Firmware and Configuration Update
==================================

The SDK is capable of updating the reader device firmware and configuration. Your application can check if the reader device is up to
date or not by calling the following method of `SdkLauncher` class.

###   Check for Firmware or Configuration Updates

>     public void checkForAnywhereReaderDeviceUpdates(SdkListener iListener, bool isTestReader)

#### Listener/Callback Methods

>     Void OTAUpdateRequired(Tuple<TAUpdateResult, OTAUpdateResult> iCheckUpdateStatus, string iErrorMessage); 

## Update Required 

If an update is required, your application can call one of the following methods to update the configuration or firmware.

###   Start Firmware/Configuration Update With SDK-Provided UI

>     public void startOTAUpdate(SdkListener iListener, bool isTestReader)

#### Listener/Callback Methods

>     void OTAUpdateCompleted(Tuple<OTAUpdateResult, OTAUpdateResult> iUpdateStatus, string iErrorMessage);

### Start Firmware/Configuration Update With No UI From SDK

>     public void startOTAUpdateWithNoUI(SdkListener iListener, bool isTestReader)

#### Listener/Callback Methods

>     void OTAUpdateProgress(double iPercentage, OTAUpdateType
>     iOTAUpdateType);

>   //item1 Config update
>   //item2 Firmware update

>     void OTAUpdateCompleted(Tuple<OTAUpdateResult, OTAUpdateResult> iUpdateStatus, string iErrorMessage);


## Notes
For every SDK operation, the SDK has call back methods. The SDK sends notifications about the progress or completion of the operation in callback methods. Callbacks methods are listed under each operation. 


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
