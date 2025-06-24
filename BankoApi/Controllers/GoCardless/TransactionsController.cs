using System.Text.Json;
using System.Text.Json.Serialization;
using BankoApi.Data;
using BankoApi.Repository;
using BankoApi.Services;
using BankoApi.Services.Model;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.GoCardless;

[ApiController]
[Route("gocardless/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly BankoDbContext _dbContext;
    private readonly GoCardlessService _goCardlessService;
    private readonly TransactionsRepository _repository;

    public TransactionsController(GoCardlessService goCardlessService, BankoDbContext dbContext)
    {
        _goCardlessService = goCardlessService;
        _dbContext = dbContext;
        _repository = new TransactionsRepository();
    }

    [HttpGet("{accountId}")]
    public async Task<IActionResult> FetchAndStoreTransactions(string accountId)
    {
        // TODO(): Handle the exceptions
       //  var transactions = await _goCardlessService.GetTransactionsAsync(accountId);
       var options = new JsonSerializerOptions
       {
           PropertyNameCaseInsensitive = true,
           DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
           AllowTrailingCommas = true
       };

       var transactions = JsonSerializer.Deserialize<Transactions>(temp, options);

        if (temp  == null) return NotFound();
        _repository.StoreTransactions(_dbContext, transactions);

        await _dbContext.SaveChangesAsync();
        return Ok("Transactions stored successfully.");
    }

    private String temp = """
                            {
                              "transactions": {
                                  "booked": [
                                      {
                                          "transactionId": "enc!!K_VC-Win3ykJR11mUPlVIhZ45RWYTF1jIFKrm9lYyDMupcVrItkyN0bWyLen0VKZ",
                                          "bookingDate": "2025-02-06",
                                          "valueDate": "2025-02-06",
                                          "transactionAmount": {
                                              "amount": "-29.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "06.02 LØRENSKOG HUS RÅDHUSET LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "06.02 LØRENSKOG HUS RÅDHUSET LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "b6c9389fed30af88716dd6a175c0a552"
                                      },
                                      {
                                          "transactionId": "enc!!0tlMSG_d482oXzU1JBnqWVCg8D_vqBUzJ5ogxIPwHIgFjPXRSTG18K7wGRi9UsYO",
                                          "bookingDate": "2025-02-05",
                                          "valueDate": "2025-02-05",
                                          "transactionAmount": {
                                              "amount": "-72.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 04.02 NOK 72.00 ZETTLE_*GRAINS MATHALL Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 04.02 NOK 72.00 ZETTLE_*GRAINS MATHALL Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "63c8a855f139259f6e17e92c0113dbba"
                                      },
                                      {
                                          "transactionId": "enc!!qFHmDpMzxNRcVfVQc7C_wxhyglneVPo_j8sDcrvOxFZaG6kqB8QTTYQx79GFflLN",
                                          "bookingDate": "2025-02-05",
                                          "valueDate": "2025-02-05",
                                          "transactionAmount": {
                                              "amount": "-42.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "04.02 HOUSE OF NERDS  VULKAN 18 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "04.02 HOUSE OF NERDS  VULKAN 18 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "2dbdc0b33e3f05937bf2ea4ca1b278fd"
                                      },
                                      {
                                          "transactionId": "enc!!YNa2zLw9tT6xi0v2xikV4LfoBxq6IhsZxhk4VQzGb7fJ1yP8seaYx-LUah_1OdWN",
                                          "bookingDate": "2025-02-05",
                                          "valueDate": "2025-02-05",
                                          "transactionAmount": {
                                              "amount": "-39.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "05.02 STARBUCKS 6121  STOPERIVEIEN STRØMMEN",
                                          "remittanceInformationUnstructuredArray": [
                                              "05.02 STARBUCKS 6121  STOPERIVEIEN STRØMMEN"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "5212ca13a69b3ff393761f46be5692f9"
                                      },
                                      {
                                          "transactionId": "enc!!_qF5KtSR6jUQr0JHnP68ZJ8SN56ZWveIdVaEJZpr5BKNpE5efk4QxPqdVlLzZcOc",
                                          "bookingDate": "2025-02-05",
                                          "valueDate": "2025-02-05",
                                          "transactionAmount": {
                                              "amount": "-333.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Walter Verdese",
                                          "creditorAccount": {
                                              "iban": "NO3712247602524",
                                              "bban": "12247602524"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Walter Verdese",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Walter Verdese"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "432810e5c03bf0a431f7510c20bbff80"
                                      },
                                      {
                                          "transactionId": "enc!!QLNdOBcEyQ6yUeXq0hni0eBZVEuARF7HQLqg6Lk02AJ8sovDER8WXSVgiavxjgGf",
                                          "bookingDate": "2025-02-05",
                                          "valueDate": "2025-02-05",
                                          "transactionAmount": {
                                              "amount": "-125.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 04.02 NOK 125.00 KornDoKKi Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 04.02 NOK 125.00 KornDoKKi Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "43a3cc19cfb7b0d4d110a7e84802ec3c"
                                      },
                                      {
                                          "transactionId": "enc!!FtpW6kG9OXD25CCIxzsyPQ-ZD46pBDRBmYqoQxKH7cwT-SI-p-lRAiLoH9kpPi4Q",
                                          "bookingDate": "2025-02-05",
                                          "valueDate": "2025-02-05",
                                          "transactionAmount": {
                                              "amount": "-52.90",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "05.02 STRØMMEN STØPERIVEIEN STRØMMEN",
                                          "remittanceInformationUnstructuredArray": [
                                              "05.02 STRØMMEN STØPERIVEIEN STRØMMEN"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "551241a898ce7db23cdd0ab6dafc3719"
                                      },
                                      {
                                          "transactionId": "enc!!iyYUWTz9XB6dlvJo8rxrVCfM4t4lbLNYjlLy2ki_mnMxNbDXvB8DfV1T1dmhhPUw",
                                          "bookingDate": "2025-02-05",
                                          "valueDate": "2025-02-05",
                                          "transactionAmount": {
                                              "amount": "-40.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "04.02 HOUSE OF NERDS  VULKAN 18 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "04.02 HOUSE OF NERDS  VULKAN 18 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "dd2250002fd68a709eab3c3f69a3505d"
                                      },
                                      {
                                          "transactionId": "enc!!oQ3cxQOoQueH2fU8pXE_xyBQi-X8wIQ_7NkHnFCJDS5dHJmRBRCDitfdC3uBM8kg",
                                          "bookingDate": "2025-02-04",
                                          "valueDate": "2025-02-04",
                                          "transactionAmount": {
                                              "amount": "-240.70",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "03.02 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "03.02 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "5ed4013de59068a3c03df48a3db92f0e"
                                      },
                                      {
                                          "transactionId": "enc!!9lSLP5UX2XlTHarnMSJp-hkTMoRRlPhX3H71oKBllyEquy5yA2EsIuHDAD10nwjr",
                                          "bookingDate": "2025-02-04",
                                          "valueDate": "2025-02-04",
                                          "transactionAmount": {
                                              "amount": "-799.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*6836 04.02 NOK 799.00 BRUCE Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*6836 04.02 NOK 799.00 BRUCE Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "d9d575b54221d51428be9db697a7c8ce"
                                      },
                                      {
                                          "transactionId": "enc!!Y9CY2s25IYVfaQXW92LYqJACohmPz6I2sxgY-XJI1Q1ac5kDIR8g0lvfC5aoI_zr",
                                          "bookingDate": "2025-02-03",
                                          "valueDate": "2025-02-03",
                                          "transactionAmount": {
                                              "amount": "-170.10",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "31.01 C.ENGEBRETSEN PILESTREDET  OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "31.01 C.ENGEBRETSEN PILESTREDET  OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "5d1ce0466de02731cc3c9e9915a1d41a"
                                      },
                                      {
                                          "transactionId": "enc!!E4XvE0_WjKWD5478yJplSPjCrVxhKZ46tVVLElfCAzZQRwMiHDs4mx43vh0bRk11",
                                          "bookingDate": "2025-02-03",
                                          "valueDate": "2025-02-03",
                                          "transactionAmount": {
                                              "amount": "-39.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "31.01 STARBUCKS 6101  STERNERSGATA OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "31.01 STARBUCKS 6101  STERNERSGATA OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "21c4e3ad7aa118e2226a9eb120f6eb57"
                                      },
                                      {
                                          "transactionId": "enc!!YhctjuhIOa3kXS8syd-zkeDNBLIcz0PM1bsOb3x1soXP3a97nRhSgHRqlQHsKetp",
                                          "bookingDate": "2025-02-03",
                                          "valueDate": "2025-02-03",
                                          "transactionAmount": {
                                              "amount": "-69.90",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "31.01 CLAS OHLSON OSL STENERSGATA  OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "31.01 CLAS OHLSON OSL STENERSGATA  OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "a8259a41e98db5649f1627418a4ab439"
                                      },
                                      {
                                          "transactionId": "enc!!FuZf0hCS2CYObKLojIunWT6icxqNJU3l98SFGzU2e4xEOVJdxUzIPkBoCmfDVdWh",
                                          "bookingDate": "2025-02-03",
                                          "valueDate": "2025-02-03",
                                          "transactionAmount": {
                                              "amount": "-177.90",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "01.02 JOKER TJUVHOLME STRANDPROMEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "01.02 JOKER TJUVHOLME STRANDPROMEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "16376b8455b03f0aec0934cb53549cc1"
                                      },
                                      {
                                          "transactionId": "enc!!tE9mHLzLxIpqx8ZTCtRRTuPRVE2fhvCKb5LZ11bn6jOrFc-NDjqUdY__Vt4FKL9g",
                                          "bookingDate": "2025-02-03",
                                          "valueDate": "2025-02-03",
                                          "transactionAmount": {
                                              "amount": "-49.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 31.01 NOK 49.00 NARVESEN 837 NATIONAL VES Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 31.01 NOK 49.00 NARVESEN 837 NATIONAL VES Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "9459e21b4627af08bc63b6dc3db69ef4"
                                      },
                                      {
                                          "transactionId": "enc!!UOBDD4MDKUOD5uPOdX24xRoXRkzIv4YHT5zzjKOqxrJcXC2Q-60nVMhh__pX_Avd",
                                          "bookingDate": "2025-01-31",
                                          "valueDate": "2025-01-31",
                                          "transactionAmount": {
                                              "amount": "-2.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "MOBILGIRO M/KID, FORF. I DAG        1 TRANS(ER) TYPE 261",
                                          "remittanceInformationUnstructuredArray": [
                                              "MOBILGIRO M/KID, FORF. I DAG        1 TRANS(ER) TYPE 261"
                                          ],
                                          "bankTransactionCode": "BOOK-EXPS-BANK",
                                          "internalTransactionId": "f208e25176b242195dee72f531fb2173"
                                      },
                                      {
                                          "transactionId": "enc!!lgi7d1YiE-fdVi3XIhdEk1ZyTqfDrwZOO4rFIp_v-t646i8C92lksC36gS7osutp",
                                          "bookingDate": "2025-01-31",
                                          "valueDate": "2025-01-31",
                                          "transactionAmount": {
                                              "amount": "-35.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.01 NOK 35.00 NARVESEN 570 AKER BRYGGE Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.01 NOK 35.00 NARVESEN 570 AKER BRYGGE Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "ea23a5bcdf34bd2f785d9483fee31f33"
                                      },
                                      {
                                          "transactionId": "enc!!pmzR62wMyy2fhmv1rLT5BZH_cTm28qInG4LY58GR62nQxcXntS0V5VNRrHfMN_KH",
                                          "bookingDate": "2025-01-31",
                                          "valueDate": "2025-01-31",
                                          "transactionAmount": {
                                              "amount": "-29.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.01 NOK 29.00 NARVESEN 570 AKER BRYGGE Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.01 NOK 29.00 NARVESEN 570 AKER BRYGGE Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "a935e9fcbc917d70e31f748490fccdc8"
                                      },
                                      {
                                          "transactionId": "enc!!_zjzdJlPibklvmkPhXVbyOYOYxBjMrYFnoejidhnb4zw_mE8WsMyF9q2DLgI4m--",
                                          "bookingDate": "2025-01-31",
                                          "valueDate": "2025-01-31",
                                          "transactionAmount": {
                                              "amount": "-40.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "VISA VAREKJØP                      20 TRANS(ER) TYPE 714",
                                          "remittanceInformationUnstructuredArray": [
                                              "VISA VAREKJØP                      20 TRANS(ER) TYPE 714"
                                          ],
                                          "bankTransactionCode": "BOOK-EXPS-BANK",
                                          "internalTransactionId": "d919033d65408c05f1043860d167e47f"
                                      },
                                      {
                                          "transactionId": "enc!!zAUdqRp7xfXcxdYiMZGBKdl_Qifh2jW6Enp-v0b5C9Lg15zV1-AHkRQoB-gGxDj1",
                                          "bookingDate": "2025-01-31",
                                          "valueDate": "2025-01-31",
                                          "transactionAmount": {
                                              "amount": "-58.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.01 NOK 58.00 NARVESEN 570 AKER BRYGGE Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.01 NOK 58.00 NARVESEN 570 AKER BRYGGE Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "9c3645fb65372ead5f5a4461fafb349e"
                                      },
                                      {
                                          "transactionId": "enc!!eJ54oCVlLR06H6N-fpXZpxkR9GgC3gPTrFBS2B5o_2Cy5aM-U-KH6A6CxhhvcEOx",
                                          "bookingDate": "2025-01-31",
                                          "valueDate": "2025-01-31",
                                          "transactionAmount": {
                                              "amount": "-155.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "30.01 OUTLAND AS GRENSEN 5-7  OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "30.01 OUTLAND AS GRENSEN 5-7  OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "6d9279d2697e2d9ed9110182be9654c2"
                                      },
                                      {
                                          "transactionId": "enc!!DrxAH8fZ74i1eXpSQis5rV_EX9E1VCNDby4e8rD4ZTk7BonxvdCzA7h2ak1I186f",
                                          "bookingDate": "2025-01-31",
                                          "valueDate": "2025-01-31",
                                          "transactionAmount": {
                                              "amount": "-62.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "VAREKJØP                           31 TRANS(ER) TYPE 709",
                                          "remittanceInformationUnstructuredArray": [
                                              "VAREKJØP                           31 TRANS(ER) TYPE 709"
                                          ],
                                          "bankTransactionCode": "BOOK-EXPS-BANK",
                                          "internalTransactionId": "b1d55a43145a6c6b0a1ff9a7b9424134"
                                      },
                                      {
                                          "transactionId": "enc!!yTrGfMATAnptqPRu8g2UUetd4sBko12F7tJQdShvdxQ_0YI9mD0GO_dlM1keYxZH",
                                          "bookingDate": "2025-01-29",
                                          "valueDate": "2025-01-30",
                                          "transactionAmount": {
                                              "amount": "-555.19",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "FORTUM STRØM AS",
                                          "creditorAccount": {
                                              "iban": "NO1860050685317",
                                              "bban": "60050685317"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: FORTUM STRØM AS Betalt: 29.01.25",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: FORTUM STRØM AS Betalt: 29.01.25"
                                          ],
                                          "remittanceInformationStructuredArray": [
                                              "reference: 7277826000100668, referenceType: SCOR"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-PCID",
                                          "internalTransactionId": "db61a87f36faa269b0c199ec0868caaa"
                                      },
                                      {
                                          "transactionId": "enc!!wIcZGThplt-Q2QWXF-sm8tdmOa5urX5-0TlTMCdgyjyfYedNrKbYqs9NykDo97Rj",
                                          "bookingDate": "2025-01-29",
                                          "valueDate": "2025-01-29",
                                          "transactionAmount": {
                                              "amount": "-172.40",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "28.01 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "28.01 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "82b7fee5481bfa5b52edaf14abf3198b"
                                      },
                                      {
                                          "transactionId": "enc!!a-Osw6aL8icPRdkjX-mmtn2OFal1d4QMKhtsFSVMkmKIumrCHJHAJn4_rAgWup6Z",
                                          "bookingDate": "2025-01-28",
                                          "valueDate": "2025-01-28",
                                          "transactionAmount": {
                                              "amount": "-94.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "28.01 REMA VIKA PARKVEIEN 64 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "28.01 REMA VIKA PARKVEIEN 64 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "98bdf82eedc2c71eec65898f6b730c40"
                                      },
                                      {
                                          "transactionId": "enc!!3k906akC3milV1jH0i3rzY37_I3mwVMlCg5SAq0N9TaRbuGRTGUAi2L6sDWHKlN7",
                                          "bookingDate": "2025-01-28",
                                          "valueDate": "2025-01-28",
                                          "transactionAmount": {
                                              "amount": "-17.70",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "27.01 COOP MEGA METRO BIBLIOTEKSGA LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "27.01 COOP MEGA METRO BIBLIOTEKSGA LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "0a3324201aff4655c85dcb51e703eb90"
                                      },
                                      {
                                          "transactionId": "enc!!xjsCUvP2BNpC3DXGQlf3Fp7MyHnMQ5FJMi03O6h_JZZ1jkJfYxWVCL_msUeZWqI2",
                                          "bookingDate": "2025-01-28",
                                          "valueDate": "2025-01-28",
                                          "transactionAmount": {
                                              "amount": "-115.70",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "27.01 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "27.01 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "670da8e013fbe315d85360bd366cd409"
                                      },
                                      {
                                          "transactionId": "enc!!DHZ7J5C9GhQjMyFO3xN1B9IejMaYmlEFgb4dMisnMv_QhOstXO3P23WdI7bQIedj",
                                          "bookingDate": "2025-01-28",
                                          "valueDate": "2025-01-27",
                                          "transactionAmount": {
                                              "amount": "-100.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Vipps MobilePay",
                                          "creditorAccount": {
                                              "iban": "NO0650055000724",
                                              "bban": "50055000724"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Vipps MobilePay",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Vipps MobilePay"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "44d1c1b1d8772f9bb7c8dcf46a7223ff"
                                      },
                                      {
                                          "transactionId": "enc!!VS_Tz3--2ozZqKgWv5smHicdIjejP4ldyHmLsyVofYv3OnU-NIlIKbo7kMyUYY0h",
                                          "bookingDate": "2025-01-28",
                                          "valueDate": "2025-01-27",
                                          "transactionAmount": {
                                              "amount": "100.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Gaetano Vincenzo Zito",
                                          "creditorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "debtorName": "Rebekka Syversen",
                                          "debtorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "remittanceInformationUnstructured": "Fra: Rebekka Syversen Betalt: 27.01.25",
                                          "remittanceInformationUnstructuredArray": [
                                              "Fra: Rebekka Syversen Betalt: 27.01.25",
                                              "For being mean"
                                          ],
                                          "bankTransactionCode": "PMNT-RCDT-RETP",
                                          "internalTransactionId": "06d4a85ff6a84deba256e0d326261d57"
                                      },
                                      {
                                          "transactionId": "enc!!lJZf9WgJsnkIZLlMrE1VrUfrAIWPwgW7zAg9SWcdliNokcipW_fRUNwEv2rdOknl",
                                          "bookingDate": "2025-01-24",
                                          "valueDate": "2025-01-27",
                                          "transactionAmount": {
                                              "amount": "-280.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 22.01 NOK 280.00 STEAM PURCHASE Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 22.01 NOK 280.00 STEAM PURCHASE Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "d40682666874a20e85b931651c65249e"
                                      },
                                      {
                                          "transactionId": "enc!!8DdE6CNbZKF_AW-gAqVXwoJtWvYy-9rwwTdiwl5di9Oqpn2Dp3BfZ2UPz9fzUZ86",
                                          "bookingDate": "2025-01-24",
                                          "valueDate": "2025-01-24",
                                          "transactionAmount": {
                                              "amount": "-167.90",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "24.01 REMA VIKA PARKVEIEN 64 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "24.01 REMA VIKA PARKVEIEN 64 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "04b668341d7c5b0dac01031618947a2f"
                                      },
                                      {
                                          "transactionId": "enc!!rbgGSNutD9AlL78baxB-FQUqQhK6oVkN-Zjgwvf47zdjqxYDodLkzJvaJAA6b6j9",
                                          "bookingDate": "2025-01-24",
                                          "valueDate": "2025-01-24",
                                          "transactionAmount": {
                                              "amount": "-49.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 23.01 NOK 49.00 NARVESEN 834 NATIONALTHEA Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 23.01 NOK 49.00 NARVESEN 834 NATIONALTHEA Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "--",
                                          "internalTransactionId": "560b5ce06dc31612af616b8caa159a66"
                                      },
                                      {
                                          "transactionId": "enc!!fVUPeNecjSdZbffxBUULSUrJ1SEYAFu7wbo1QwD1XhRikiGQETvip4WqVgtcvw91",
                                          "bookingDate": "2025-01-24",
                                          "valueDate": "2025-01-24",
                                          "transactionAmount": {
                                              "amount": "-13287.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "UTLEIEMEGLEREN FROGNER AS",
                                          "creditorAccount": {
                                              "iban": "NO7516025627389",
                                              "bban": "16025627389"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: UTLEIEMEGLEREN FROGNER AS Betalt: 24.01.25",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: UTLEIEMEGLEREN FROGNER AS Betalt: 24.01.25"
                                          ],
                                          "remittanceInformationStructuredArray": [
                                              "reference: 0400000005090580, referenceType: SCOR"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-DMCT",
                                          "internalTransactionId": "6baa5adfebf9f6dbe684c53255a33402"
                                      },
                                      {
                                          "transactionId": "enc!!XLJKKA7SLC5c0M9zSy2lHy1tNmkC9FwaQyDEsTOn6YUqJ_4cHCQ_GQyAY4wlSkwM",
                                          "bookingDate": "2025-01-22",
                                          "valueDate": "2025-01-23",
                                          "transactionAmount": {
                                              "amount": "-562.37",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 22.01 EUR 46.63 AMZN Mktp DE*DY1S428L5 Kurs: 12.0603",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 22.01 EUR 46.63 AMZN Mktp DE*DY1S428L5 Kurs: 12.0603"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "ebe1ade7f1dd024f1f55cb15078b8284"
                                      },
                                      {
                                          "transactionId": "enc!!wmKHcxqXtfmXGvcIOfzX_FGQc-oGFVytWGzDGBaNv3VBSq59PFkr1Rg040ZENjkw",
                                          "bookingDate": "2025-01-22",
                                          "valueDate": "2025-01-22",
                                          "transactionAmount": {
                                              "amount": "-747.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 21.01 NOK 747.00 Vipps*Ruter Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 21.01 NOK 747.00 Vipps*Ruter Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "--",
                                          "internalTransactionId": "d844b80cc12f555b75b5f1ca64bc4d9f"
                                      },
                                      {
                                          "transactionId": "enc!!KSNFLXTsOgmyFLczX7LOPUqrS-stv1VsO5eyvQTelOi0UJaVgHLY52uGPacxx-YN",
                                          "bookingDate": "2025-01-22",
                                          "valueDate": "2025-01-22",
                                          "transactionAmount": {
                                              "amount": "-29.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 21.01 NOK 29.00 NARVESEN 870 JERNBANETORG Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 21.01 NOK 29.00 NARVESEN 870 JERNBANETORG Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "ebbbf7db1d39cbc474d4c983ff2c9e20"
                                      },
                                      {
                                          "transactionId": "enc!!hNB-Vru7BJbjLt9C1hpP_nDUjLrLOJiEdf545rIVxevPJEHAuSIvGXOnq0WoMMOR",
                                          "bookingDate": "2025-01-22",
                                          "valueDate": "2025-01-22",
                                          "transactionAmount": {
                                              "amount": "-29.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 21.01 NOK 29.00 NARVESEN 870 JERNBANETORG Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 21.01 NOK 29.00 NARVESEN 870 JERNBANETORG Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "d83fcadf6f4154674317d2a2400de11f"
                                      },
                                      {
                                          "transactionId": "enc!!uhK5J6RXGr7RFhqOtUKLHB4p2bBUPnOgpewK9mXV2ejHtzTht54TvoG3XH5_2dSD",
                                          "bookingDate": "2025-01-22",
                                          "valueDate": "2025-01-22",
                                          "transactionAmount": {
                                              "amount": "-172.40",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "21.01 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "21.01 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "ed65730be725b2dc7e244e9ce1326259"
                                      },
                                      {
                                          "transactionId": "enc!!GTgQRFwqax97xlz_wl6y_VjleavSPy5iiQZpKSqmSMlSCcdVyeOqSci6AeIgthru",
                                          "bookingDate": "2025-01-22",
                                          "valueDate": "2025-01-22",
                                          "transactionAmount": {
                                              "amount": "-203.20",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "21.01 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "21.01 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "8a75c0b8e17b80b16473c9be1b2f81b1"
                                      },
                                      {
                                          "transactionId": "enc!!t46LwmUcOHmiOUtc_zcZYydnWdeRquhhYLS8HC_RSYfHUljIAONbmtuKOkp3itAf",
                                          "bookingDate": "2025-01-21",
                                          "valueDate": "2025-01-21",
                                          "transactionAmount": {
                                              "amount": "-192.50",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "20.01 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "20.01 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "ee5c6a3b8a14e656514af223dc2e88d6"
                                      },
                                      {
                                          "transactionId": "enc!!FYCoLiQs9OIBqvtn8JqJq7S4H51rh-H5E-Uu-OdqqQQbtIOiRph0wVtzq4Gf5Vfu",
                                          "bookingDate": "2025-01-20",
                                          "valueDate": "2025-01-20",
                                          "transactionAmount": {
                                              "amount": "-86.80",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "18.01 COOP MEGA METRO BIBLIOTEKSGA LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "18.01 COOP MEGA METRO BIBLIOTEKSGA LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "22e22379d7f2a3cbe9c2c13b5f039407"
                                      },
                                      {
                                          "transactionId": "enc!!26A88z9gMI4osPIuANgqMW6-yWjT4MW-nhZawKLLDSyc0mav8UebCg4-YrXL_TLR",
                                          "bookingDate": "2025-01-20",
                                          "valueDate": "2025-01-18",
                                          "transactionAmount": {
                                              "amount": "-1.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Rebekka Syversen",
                                          "creditorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Rebekka Syversen",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Rebekka Syversen"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "400a006778153624e9bd3d9bb1987f6a"
                                      },
                                      {
                                          "transactionId": "enc!!6VMZHZ5NgbaMrHFgbxf0yIrNrCFZQ9_XWCFejtdRQw02vazmCeZBIbVR4rBkP_gr",
                                          "bookingDate": "2025-01-17",
                                          "valueDate": "2025-01-17",
                                          "transactionAmount": {
                                              "amount": "-59.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "16.01 BREWGATA AS BRUGATA 5 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "16.01 BREWGATA AS BRUGATA 5 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "d698110da5629cc9d0bb18f8ccdfe9e1"
                                      },
                                      {
                                          "transactionId": "enc!!papvnOJoSl_m2wBJ-sRFNlI-79ZhqTH3PLiHVDj3OVIozwyvQ8t5B_04qGcOQpwq",
                                          "bookingDate": "2025-01-17",
                                          "valueDate": "2025-01-17",
                                          "transactionAmount": {
                                              "amount": "-57.80",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "16.01 JOKER ROSENKRAN ROSENKRANTZG OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "16.01 JOKER ROSENKRAN ROSENKRANTZG OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "629f5a1bf3d2caf8ab3839f90f54a836"
                                      },
                                      {
                                          "transactionId": "enc!!gCe6yut2zyb-K0oKAdCR79fm74BkGWHBql_iIS1a5Q1BaQolkSgQLU7KCyx4CpPu",
                                          "bookingDate": "2025-01-17",
                                          "valueDate": "2025-01-17",
                                          "transactionAmount": {
                                              "amount": "-128.10",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 16.01 NOK 128.10 Vipps*BREWGATA Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 16.01 NOK 128.10 Vipps*BREWGATA Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "140061346339f4f37cd72601db60c21e"
                                      },
                                      {
                                          "transactionId": "enc!!Dowef2kcYcPcFtZLG5xonTZrM94JBY6U_YvpYYfpgTxTdmy3YU4Ae4vLg-EoCyu_",
                                          "bookingDate": "2025-01-16",
                                          "valueDate": "2025-01-16",
                                          "transactionAmount": {
                                              "amount": "-279.60",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "15.01 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "15.01 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "ebe520474f3bf43b15e9a0dc90ef41b9"
                                      },
                                      {
                                          "transactionId": "enc!!CBfNBbRrY2ToUqNkNRfivh7k0-4Tq3y48wzePigEETWviNakMGSCSoQNC4rsXMW1",
                                          "bookingDate": "2025-01-15",
                                          "valueDate": "2025-01-15",
                                          "transactionAmount": {
                                              "amount": "-146.60",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "15.01 REMA VIKA PARKVEIEN 64 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "15.01 REMA VIKA PARKVEIEN 64 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "3028c74feebadcea3c36bd1379912c71"
                                      },
                                      {
                                          "transactionId": "enc!!9MpCWJ_tsMoSG3bHOeVxbkMw0dssdvLq40JQSiwej295Zulgs04DYGX1TC7LGMRn",
                                          "bookingDate": "2025-01-15",
                                          "valueDate": "2025-01-15",
                                          "transactionAmount": {
                                              "amount": "-70.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 14.01 NOK 70.00 ZETTLE_*OSB03 Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 14.01 NOK 70.00 ZETTLE_*OSB03 Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "9db5b32d4e09d9eaa88896af9ab9c9f5"
                                      },
                                      {
                                          "transactionId": "enc!!LG_RkBV-vlMaurHcG2wQgBFEJWNd4BDl2KrAoj3kzCKEOcHmMhFoVxzp8unnBhj8",
                                          "bookingDate": "2025-01-15",
                                          "valueDate": "2025-01-15",
                                          "transactionAmount": {
                                              "amount": "-40.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 14.01 NOK 40.00 ZETTLE_*OSB03 Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 14.01 NOK 40.00 ZETTLE_*OSB03 Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "b8d30c2539869c00b77550002c2d179a"
                                      },
                                      {
                                          "transactionId": "enc!!vzE6zAGV20hlRcGhEctFzJCYiOC2Zmd3lAUT7v0VMRsUVDGIjkZKQCpVJ4ckmftT",
                                          "bookingDate": "2025-01-15",
                                          "valueDate": "2025-01-15",
                                          "transactionAmount": {
                                              "amount": "-25.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 14.01 NOK 25.00 ZETTLE_*OSB03 Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 14.01 NOK 25.00 ZETTLE_*OSB03 Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "a32a8b86ddfaf181bdaf551aeba8f0cc"
                                      },
                                      {
                                          "transactionId": "enc!!vS7h_38JDIwamsJnpir4uyMSpnFue9NZVVQ7SiW07G1i7_5ZA55HVhS1HIV5rqkZ",
                                          "bookingDate": "2025-01-14",
                                          "valueDate": "2025-01-14",
                                          "transactionAmount": {
                                              "amount": "147320.53",
                                              "currency": "NOK"
                                          },
                                          "creditorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "debtorName": "AIRTHINGS ASA",
                                          "debtorAccount": {
                                              "iban": "NO6784502866191",
                                              "bban": "84502866191"
                                          },
                                          "remittanceInformationUnstructured": "Fra: AIRTHINGS ASA",
                                          "remittanceInformationUnstructuredArray": [
                                              "Fra: AIRTHINGS ASA"
                                          ],
                                          "bankTransactionCode": "PMNT-RCDT-SALA",
                                          "internalTransactionId": "876285c7e070b6dafc1a0adbc8217726"
                                      },
                                      {
                                          "transactionId": "enc!!EUuixgeV-pUDUDSxacYToNmP30pUrP-T2Ddm0lphZim9oBVjQYgLgr_F7H4MOYGB",
                                          "bookingDate": "2025-01-14",
                                          "valueDate": "2025-01-14",
                                          "transactionAmount": {
                                              "amount": "-68.50",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "13.01 KIWI 364 BYPORT JERNBANETORG OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "13.01 KIWI 364 BYPORT JERNBANETORG OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "46bb0d967ae6eac2dfee74d1714404e8"
                                      },
                                      {
                                          "transactionId": "enc!!syGAyLFZRfl5oC-uAA23dvRj1XMzu-9H6h4zgs2R7S4yodV1IfwQSUPyZq79empm",
                                          "bookingDate": "2025-01-14",
                                          "valueDate": "2025-01-14",
                                          "transactionAmount": {
                                              "amount": "-2.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Rebekka Syversen",
                                          "creditorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Rebekka Syversen",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Rebekka Syversen"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "ef68f6ebee517f2196b5c1544471ea52"
                                      },
                                      {
                                          "transactionId": "enc!!JokR5rJ4E7qYogbD-d2soB7khpBvbtO0PiSB7yZeI9SXLO6YfmcDVnSjfXErHScq",
                                          "bookingDate": "2025-01-13",
                                          "valueDate": "2025-01-13",
                                          "transactionAmount": {
                                              "amount": "-99.30",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "12.01 JOKER RUSELØKKA LØKKEVEIEN 1 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "12.01 JOKER RUSELØKKA LØKKEVEIEN 1 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "008222de3093999496fc29e2c9484a82"
                                      },
                                      {
                                          "transactionId": "enc!!lkP02XnO4sPgFegRAGXNRJ3KEZSWF9ocoIG9oS_cGzvXaVV5rfgTvuHNcwDRJ37i",
                                          "bookingDate": "2025-01-13",
                                          "valueDate": "2025-01-13",
                                          "transactionAmount": {
                                              "amount": "-200.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Stefano Pomarico",
                                          "creditorAccount": {
                                              "iban": "NO8518138144328",
                                              "bban": "18138144328"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Stefano Pomarico",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Stefano Pomarico"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "23815d0a4fe5bf20fd5ab14f524e9fd8"
                                      },
                                      {
                                          "transactionId": "enc!!YKyaW9Y8TdLuJuWgETC7atH_zvhBui1SB7elThhYKA_Y_ih58z9NsIlboNgBGptp",
                                          "bookingDate": "2025-01-13",
                                          "valueDate": "2025-01-13",
                                          "transactionAmount": {
                                              "amount": "-177.90",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "11.01 JOKER TJUVHOLME STRANDPROMEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "11.01 JOKER TJUVHOLME STRANDPROMEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "6c57c18f62e742b25a33968d2abb2853"
                                      },
                                      {
                                          "transactionId": "enc!!QpjpXP4IVPrdETGEGPshkGXnpH5-xlGn7VAPKibIEhOqQNcySPz02BIfkADGML6D",
                                          "bookingDate": "2025-01-13",
                                          "valueDate": "2025-01-13",
                                          "transactionAmount": {
                                              "amount": "-51.45",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 10.01 NOK 51.45 Radio Loekka P Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 10.01 NOK 51.45 Radio Loekka P Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "afead9c4d406c88881285e35205875a2"
                                      },
                                      {
                                          "transactionId": "enc!!2T6iHSp25tnZYeYlJ6yc2SUf3_UU_KsUF4i0sSCt5jDKdISr-1Q2nkmZVaL1S2hw",
                                          "bookingDate": "2025-01-13",
                                          "valueDate": "2025-01-12",
                                          "transactionAmount": {
                                              "amount": "-60.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Simone Sarcina",
                                          "creditorAccount": {
                                              "iban": "NO6612267587106",
                                              "bban": "12267587106"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Simone Sarcina",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Simone Sarcina"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "990520b7f4c9aa768de4cc414767469c"
                                      },
                                      {
                                          "transactionId": "enc!!DZ1dqJCirplLurlhSU5tG3giAvreQtMG10qKKXUo5gpBl58gH_XA3Tt3AuONCHLR",
                                          "bookingDate": "2025-01-13",
                                          "valueDate": "2025-01-11",
                                          "transactionAmount": {
                                              "amount": "-200.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Rebekka Syversen",
                                          "creditorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Rebekka Syversen",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Rebekka Syversen"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "0d68eea5bddee5a7a3d2cb3bf30cecb9"
                                      },
                                      {
                                          "transactionId": "enc!!Bca3cdWsYg9gsc_9G8oBVPNlHr75UaXLcebDQ1eAvULBODVdgN6ZdQ2BFBrv0OI2",
                                          "bookingDate": "2025-01-13",
                                          "valueDate": "2025-01-11",
                                          "transactionAmount": {
                                              "amount": "-60.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Rebekka Syversen",
                                          "creditorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Rebekka Syversen",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Rebekka Syversen"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "207c5978c7ada8c8645ae0f8d4b9af39"
                                      },
                                      {
                                          "transactionId": "enc!!11OHXol5BhQ6P9qoqa4DN8ukFH265pIrD9z78QfiUceQLYUhravti6SgQq6UEdSd",
                                          "bookingDate": "2025-01-13",
                                          "valueDate": "2025-01-11",
                                          "transactionAmount": {
                                              "amount": "-12.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Rebekka Syversen",
                                          "creditorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Rebekka Syversen",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Rebekka Syversen"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "73e5447604fa33fffa93fd5b0f9d642e"
                                      },
                                      {
                                          "transactionId": "enc!!sxLHnCb8ch-gSYmKW0rWvPwpiYLZYzU7hYjGqnyrFOkcGHJtkSGMzbr3bsmMJObF",
                                          "bookingDate": "2025-01-10",
                                          "valueDate": "2025-01-10",
                                          "transactionAmount": {
                                              "amount": "-325.40",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "09.01 COOP MEGA AKER  HOLMENSGT. 7 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "09.01 COOP MEGA AKER  HOLMENSGT. 7 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "91ea23038525e3fe905c8b30d9bee64d"
                                      },
                                      {
                                          "transactionId": "enc!!0A9VHYNf_ixhVfBV3v647pdRJHCxbRB_CspUcsS1u5ZYmOOaGHb6ROTxC6ajRSUr",
                                          "bookingDate": "2025-01-10",
                                          "valueDate": "2025-01-10",
                                          "transactionAmount": {
                                              "amount": "-222.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "10.01 TRYVANNSTUA BOMVEIEN 50  OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "10.01 TRYVANNSTUA BOMVEIEN 50  OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "010647299a9f1ca69f25bfb4a2ca557f"
                                      },
                                      {
                                          "transactionId": "enc!!Bste3XOBRH4L0jHIhw1WAQyYOzyHWk3I27W4-FrkzYaaxZHB1UMDSW3fL9oHTHc7",
                                          "bookingDate": "2025-01-10",
                                          "valueDate": "2025-01-10",
                                          "transactionAmount": {
                                              "amount": "-60.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "09.01 ØSLO AS STORGATA 7 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "09.01 ØSLO AS STORGATA 7 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "1fb71f39eecfc3cc52b118943ef63819"
                                      },
                                      {
                                          "transactionId": "enc!!7cPUQPg6NZ9DUXT3Gpbjgx3F74IAbkkrQufBdNKJZ9JakDTHaM_l9e1jXKNp6iGk",
                                          "bookingDate": "2025-01-10",
                                          "valueDate": "2025-01-10",
                                          "transactionAmount": {
                                              "amount": "-59.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "09.01 ØSLO AS STORGATA 7 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "09.01 ØSLO AS STORGATA 7 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "1faa5c5114154072b993058b71b2a7dc"
                                      },
                                      {
                                          "transactionId": "enc!!2b-2qZCRu6Dk5obUlFU3flWjq5fZxFGKVUd4uGe93EzIb3uBKGEwvq11rfyCizuh",
                                          "bookingDate": "2025-01-09",
                                          "valueDate": "2025-01-09",
                                          "transactionAmount": {
                                              "amount": "-30.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 08.01 NOK 30.00 Vipps*Deichman Bjoervika Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 08.01 NOK 30.00 Vipps*Deichman Bjoervika Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "234ba4ab2fdc6a7637aa27345b56fa54"
                                      },
                                      {
                                          "transactionId": "enc!!HNTiWcrF24VerqYnsbdbWZ5vCRXEz5tmwbWDOV_wFKRleOjbvfY2hKtc1HlllAAM",
                                          "bookingDate": "2025-01-09",
                                          "valueDate": "2025-01-09",
                                          "transactionAmount": {
                                              "amount": "-189.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "09.01 DELI DE LUCA KA KARL JOHANS  OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "09.01 DELI DE LUCA KA KARL JOHANS  OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "d297872ce661ea6fa0fe404015c620af"
                                      },
                                      {
                                          "transactionId": "enc!!Z6S1BtxqzE9xZTruYyDmNHlmFRuXNsB5n1jKAhjiedTtTKgXkijMXh6XscSi_yE2",
                                          "bookingDate": "2025-01-09",
                                          "valueDate": "2025-01-09",
                                          "transactionAmount": {
                                              "amount": "-318.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Rebekka Syversen",
                                          "creditorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Rebekka Syversen",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Rebekka Syversen"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "fdcfbfd3e608b625b2ea8fae4b364821"
                                      },
                                      {
                                          "transactionId": "enc!!6umfXMSaLJ2EvRttkcmT7eQ0MwmaNFTqsfeoNmE3-1OssczyQelE2hg6b0h10Xs0",
                                          "bookingDate": "2025-01-07",
                                          "valueDate": "2025-01-07",
                                          "transactionAmount": {
                                              "amount": "-133.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "06.01 NARVESEN 834 ABELHAUGEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "06.01 NARVESEN 834 ABELHAUGEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "b387df80fb54ca76e6d77e2dd3015d2f"
                                      },
                                      {
                                          "transactionId": "enc!!XVJH6gpEDl_qZAbSnQHwzZFP9RXmYBDWdOXbAqj00BQoBn0RaZOgCjqrlka96CY7",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-06",
                                          "transactionAmount": {
                                              "amount": "-96.60",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 03.01 NOK 96.60 Vipps*BREWGATA Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 03.01 NOK 96.60 Vipps*BREWGATA Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "0399f9e2bda39b2d133bf37d792afa76"
                                      },
                                      {
                                          "transactionId": "enc!!fG8QrnzTg6F_XdhAqoGd-gFFaO7jUU8C9nr2-QIGn5GKl9E7lQIhvyjo8e7nxtq-",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-06",
                                          "transactionAmount": {
                                              "amount": "-96.60",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 03.01 NOK 96.60 Vipps*BREWGATA Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 03.01 NOK 96.60 Vipps*BREWGATA Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "84dba90ec04c2ddf37ce7761cf8b2f19"
                                      },
                                      {
                                          "transactionId": "enc!!6VbhRVKRxZXGxaQMAe5PwFQ2MuXEhwJodhtInNqgJH5exNz6Srev5bUXf_2zKA2p",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-06",
                                          "transactionAmount": {
                                              "amount": "-27.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 05.01 NOK 27.00 Vipps*Ruter Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 05.01 NOK 27.00 Vipps*Ruter Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "4a0313411bcc0aab9a66c06f826b040b"
                                      },
                                      {
                                          "transactionId": "enc!!d30elZUcEXsJgkcHCvi3_-vVEI-C2hhqHXFRqsa3bcJkN_qwhIIUH3njT_zgs6qW",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-06",
                                          "transactionAmount": {
                                              "amount": "-27.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 05.01 NOK 27.00 Vipps*Ruter Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 05.01 NOK 27.00 Vipps*Ruter Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "d7a442d1de4e252d4d51117e13b5ed76"
                                      },
                                      {
                                          "transactionId": "enc!!2qBSKvI2igu68K3QZIIfa-vOCNSchf-KllPRIxQ7NPuEacv8EHbYgQE0uNSxz76j",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-06",
                                          "transactionAmount": {
                                              "amount": "-30.00",
                                              "currency": "NOK"
                                          },
                                          "creditorAccount": {
                                              "iban": "NO0817209510006",
                                              "bban": "17209510006"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "7990NOO07386187                                   2478446688",
                                          "remittanceInformationUnstructuredArray": [
                                              "7990NOO07386187                                   2478446688"
                                          ],
                                          "bankTransactionCode": "BOOK-EXPS-OTHR",
                                          "internalTransactionId": "f0ad962c7ab1c1367fb9d386f734f8d8"
                                      },
                                      {
                                          "transactionId": "enc!!vHNoBIJGh2Wzbt1oF0dsUxacE5iEea7xfRyXTXX5xfgo23610JFlMSMmZBHdFkE8",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-06",
                                          "transactionAmount": {
                                              "amount": "-61.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 05.01 NOK 61.00 NARVESEN 881 OSLO S SOR Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 05.01 NOK 61.00 NARVESEN 881 OSLO S SOR Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "08b3be8d897fa45ad74bdda9340aed23"
                                      },
                                      {
                                          "transactionId": "enc!!y3umRTS5Ev78a3lgLiRXmpIeIChicMCmCdJQf8FQd9-h1-1uVdP1Dq_9qwjAH42f",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-06",
                                          "transactionAmount": {
                                              "amount": "-799.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 04.01 NOK 799.00 BRUCE Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 04.01 NOK 799.00 BRUCE Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "8cdf33dcba6f46a80e54ea2787f58ae9"
                                      },
                                      {
                                          "transactionId": "enc!!VzLySSV5HMG40ElSpdkh47nbSGvsQnLkmRXDWYtkNuXM2hEf-EdPX8YoKq4l-RVJ",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-06",
                                          "transactionAmount": {
                                              "amount": "-2939.78",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Tisei Roberto",
                                          "creditorAccount": {
                                              "iban": "NO8617200070017",
                                              "bban": "17200070017"
                                          },
                                          "debtorName": "GAETANO VINCENZO ZITO",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Tisei Roberto                      EUR             250,00",
                                          "remittanceInformationUnstructuredArray": [
                                              "Tisei Roberto                      EUR             250,00"
                                          ],
                                          "bankTransactionCode": "PMNT-IICT-XBCT",
                                          "internalTransactionId": "947b8f401534ea80955ca1e10b8064f2"
                                      },
                                      {
                                          "transactionId": "enc!!a8maNGHGO-mjXinA0txiSdEGSSquK8W9210ahflR4p7Q8vjBG_DEqca5OdpQGFzQ",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-06",
                                          "transactionAmount": {
                                              "amount": "-102.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "03.01 NORMAL NO1089 O JERNBANETORG OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "03.01 NORMAL NO1089 O JERNBANETORG OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "e391aba2b472e498c42fa24387f1bbef"
                                      },
                                      {
                                          "transactionId": "enc!!Tgwdq06vUxNzZpsUNlteyBtK8og_mdjN4r6eEldhfe0iVYEWEFeqKHr3BuFdemmf",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-06",
                                          "transactionAmount": {
                                              "amount": "-130.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "03.01 BREWGATA AS BRUGATA 5 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "03.01 BREWGATA AS BRUGATA 5 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "979820ad06cff800e79ce69082b5632d"
                                      },
                                      {
                                          "transactionId": "enc!!f5yikjVk6-HtUf_SKbsee1Je4MlMcFfCn3i0jUHLh2I3c-w6ThNc43pSiA9IjZoL",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-06",
                                          "transactionAmount": {
                                              "amount": "-211.50",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "03.01 KIWI 364 BYPORT JERNBANETORG OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "03.01 KIWI 364 BYPORT JERNBANETORG OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "5270bfed92fe3e1af7d082771a0f94f6"
                                      },
                                      {
                                          "transactionId": "enc!!IuHf1pn7Z6ce8QW9VmcEwF3QFsry_o5Yfs57uUaiTYT9gKId20bH04a2c3VtQfDe",
                                          "bookingDate": "2025-01-06",
                                          "valueDate": "2025-01-03",
                                          "transactionAmount": {
                                              "amount": "92.00",
                                              "currency": "NOK"
                                          },
                                          "creditorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 03.01 NOK 92.00 Vipps*BREWGATA Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 03.01 NOK 92.00 Vipps*BREWGATA Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "c233576ba29a250ca5fc8a5d9ec64dc3"
                                      },
                                      {
                                          "transactionId": "enc!!zJlywt4vymNIR3s-WJHWKPKflFsQl4t4uSru3Ryjpe3ObcOTHdbrc8MfEHydxisK",
                                          "bookingDate": "2025-01-03",
                                          "valueDate": "2025-01-03",
                                          "transactionAmount": {
                                              "amount": "-430.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "02.01 FINSTUA AS 1 . OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "02.01 FINSTUA AS 1 . OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "e8997e14ea3f1619221b4bdb1a57931a"
                                      },
                                      {
                                          "transactionId": "enc!!mtXbtFH4eDwYEE3IzJ8z28jcDrpq51pmpSjzLBxukt4AJOQDYii4IG0vH2P0RrZE",
                                          "bookingDate": "2025-01-03",
                                          "valueDate": "2025-01-03",
                                          "transactionAmount": {
                                              "amount": "-55.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "02.01 FINSTUA AS 1 . OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "02.01 FINSTUA AS 1 . OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "cb712edf7e929e692be846c65b84e9c8"
                                      },
                                      {
                                          "transactionId": "enc!!ikdiHbdQlkm9dbLmfxUwJT8pxlR-X6ISK86gLgm19pnGZICT1NJ6TUkLGBgaid9I",
                                          "bookingDate": "2025-01-03",
                                          "valueDate": "2025-01-03",
                                          "transactionAmount": {
                                              "amount": "-153.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "03.01 OUTLAND OSLO GRENSEN 5 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "03.01 OUTLAND OSLO GRENSEN 5 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "420c22ce8c90790f0da26d80c51c8fb8"
                                      },
                                      {
                                          "transactionId": "enc!!Zmv7SSH-qM1JF_mebpdWabxH5_bujqAwh23uLj-otwoGHZK3qHhzwyDT794T47oD",
                                          "bookingDate": "2025-01-03",
                                          "valueDate": "2025-01-03",
                                          "transactionAmount": {
                                              "amount": "-195.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "02.01 NARVESEN 834 ABELHAUGEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "02.01 NARVESEN 834 ABELHAUGEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "c9bd85653bc743c26ad7e6771dd3f778"
                                      },
                                      {
                                          "transactionId": "enc!!WeXBXyh0uV1gRo5qTDLoXKZleu7RVLgsrUO9K8_ATmaWb1Tv5x8KbDt43EIlLAX5",
                                          "bookingDate": "2025-01-03",
                                          "valueDate": "2025-01-03",
                                          "transactionAmount": {
                                              "amount": "-88.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "03.01 ESPRESSO HOUSE  OLAV VS GT 2 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "03.01 ESPRESSO HOUSE  OLAV VS GT 2 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "9b2171136826d7fec7abd713999d8620"
                                      },
                                      {
                                          "transactionId": "enc!!t-BmRWCh1HLhVJCsXInF2dKcfYhb-B54xYyCzbWZWj4TGQG-HyQyCyrMtVXVrvq_",
                                          "bookingDate": "2025-01-02",
                                          "valueDate": "2025-01-02",
                                          "transactionAmount": {
                                              "amount": "-80.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Rebekka Syversen",
                                          "creditorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Rebekka Syversen",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Rebekka Syversen"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "634b140c94ac6c4c8735004d8f69f7ae"
                                      },
                                      {
                                          "transactionId": "enc!!PXKr6h7cLf7vltxcZFCnof0GzzrRtkAW8sQBxmNQQi285vd22qSCc8H8YCnA3V5b",
                                          "bookingDate": "2025-01-02",
                                          "valueDate": "2025-01-02",
                                          "transactionAmount": {
                                              "amount": "-150.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Rebekka Syversen",
                                          "creditorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Rebekka Syversen",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Rebekka Syversen"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "a18ec8ab8955b4ff41de02363755d729"
                                      },
                                      {
                                          "transactionId": "enc!!vXDFMOtxRN7KXW2d1Bny7uwhMxZpZg_kMruL4wgbqYrnsCG3iPMmScAeC4_p8_l-",
                                          "bookingDate": "2024-12-31",
                                          "valueDate": "2024-12-31",
                                          "transactionAmount": {
                                              "amount": "-88.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "VISA VAREKJØP                      44 TRANS(ER) TYPE 714",
                                          "remittanceInformationUnstructuredArray": [
                                              "VISA VAREKJØP                      44 TRANS(ER) TYPE 714"
                                          ],
                                          "bankTransactionCode": "BOOK-EXPS-BANK",
                                          "internalTransactionId": "77dc732f25b0b81f912fa9fec7631265"
                                      },
                                      {
                                          "transactionId": "enc!!U9WBLDS_pb2n5ERCJ34p16PE9oQgdLlFHUS3dqPIix0g3Mf1QBKGC9cN0p10NVu8",
                                          "bookingDate": "2024-12-31",
                                          "valueDate": "2024-12-31",
                                          "transactionAmount": {
                                              "amount": "-798.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "30.12 ELKJØP LØRENSKO SOLHEIMVEIEN LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "30.12 ELKJØP LØRENSKO SOLHEIMVEIEN LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "65228300f775e4805a99ed67211be504"
                                      },
                                      {
                                          "transactionId": "enc!!ohiwU3PyiEPcrBGPyU9W9x6O0lRaR6a5SSLBFoNfsOASfkB8ihP_HxiSgJFk1CSA",
                                          "bookingDate": "2024-12-31",
                                          "valueDate": "2024-12-31",
                                          "transactionAmount": {
                                              "amount": "-52.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "VAREKJØP                           26 TRANS(ER) TYPE 709",
                                          "remittanceInformationUnstructuredArray": [
                                              "VAREKJØP                           26 TRANS(ER) TYPE 709"
                                          ],
                                          "bankTransactionCode": "BOOK-EXPS-BANK",
                                          "internalTransactionId": "bf51a5994500a94859505c5c0f955596"
                                      },
                                      {
                                          "transactionId": "enc!!CnfRalM-qUv2Q5ArqaPAEyVZxCJXQTrwKynRbWwIs8I6DfJUFeHQFSOSCfAcMQE7",
                                          "bookingDate": "2024-12-31",
                                          "valueDate": "2024-12-31",
                                          "transactionAmount": {
                                              "amount": "-2.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "MOBILGIRO M/KID, FORF. I DAG        1 TRANS(ER) TYPE 261",
                                          "remittanceInformationUnstructuredArray": [
                                              "MOBILGIRO M/KID, FORF. I DAG        1 TRANS(ER) TYPE 261"
                                          ],
                                          "bankTransactionCode": "BOOK-EXPS-BANK",
                                          "internalTransactionId": "77878745f42b13e025c53f837706ecee"
                                      },
                                      {
                                          "transactionId": "enc!!YQGSjK9ZXNsvETbR4Q1Gq0E0y1yW6SIhA01HgIBMpokFXznbdsPOkQNoB_TvL2hj",
                                          "bookingDate": "2024-12-30",
                                          "valueDate": "2024-12-31",
                                          "transactionAmount": {
                                              "amount": "-154.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 25.12 NOK 154.00 STEAM PURCHASE Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 25.12 NOK 154.00 STEAM PURCHASE Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "d4b95dcbc47ffd5c6afb02dbad9d68f5"
                                      },
                                      {
                                          "transactionId": "enc!!5n0TPe6NaZluszw-v_WL103ICb4lvuyHyngf5Fr3-QEaOL8zZHLuExE4_xL9iml_",
                                          "bookingDate": "2024-12-31",
                                          "valueDate": "2024-12-30",
                                          "transactionAmount": {
                                              "amount": "99.00",
                                              "currency": "NOK"
                                          },
                                          "creditorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.12 NOK 99.00 Elkjoep Loerenskog Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.12 NOK 99.00 Elkjoep Loerenskog Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "e140b3e7d62523f454c64d98d5d7f316"
                                      },
                                      {
                                          "transactionId": "enc!!veEOft4vKzbtU4bgmYwzAMwnRW8GWJ8hV83ddfVHBLQgsV-wqfg37HXPuzeXyztm",
                                          "bookingDate": "2024-12-30",
                                          "valueDate": "2024-12-30",
                                          "transactionAmount": {
                                              "amount": "-119.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "29.12 LUNDS TIVOLI AS POSTBOKS 53  ÅLGÅRD",
                                          "remittanceInformationUnstructuredArray": [
                                              "29.12 LUNDS TIVOLI AS POSTBOKS 53  ÅLGÅRD"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "dc6e9390110f7cacd2af0b93bda3cce6"
                                      },
                                      {
                                          "transactionId": "enc!!79Xq16OZ_xYo-AQ8thMMTSYDGI5rArqOQMXgOKNDIGo80dnC-sxkw5gDCqNdbuP9",
                                          "bookingDate": "2024-12-30",
                                          "valueDate": "2024-12-30",
                                          "transactionAmount": {
                                              "amount": "-6.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 29.12 NOK 6.00 Dinner for 2 As Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 29.12 NOK 6.00 Dinner for 2 As Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "97a1c1490e5474077d4da09735047485"
                                      },
                                      {
                                          "transactionId": "enc!!s0h-ssAENXJQROCf_4PQZOuNIwqRwErkKVnhirz6sIMK-Lk4eEqnn7EfqmXtg-O5",
                                          "bookingDate": "2024-12-30",
                                          "valueDate": "2024-12-30",
                                          "transactionAmount": {
                                              "amount": "-321.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 28.12 NOK 321.00 Foodora Norway Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 28.12 NOK 321.00 Foodora Norway Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "7675c50c1fcd8afba896606eefb720d9"
                                      },
                                      {
                                          "transactionId": "enc!!9LQkV10bHQiTnB0QroB5a6Mm4_HnQp0YTJxPdOLbBVnOJF-SXw73srDQ8Ht3gxZT",
                                          "bookingDate": "2024-12-30",
                                          "valueDate": "2024-12-30",
                                          "transactionAmount": {
                                              "amount": "-327.30",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "27.12 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "27.12 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "17ef5ad9d6b88162688afe11b4743aa2"
                                      },
                                      {
                                          "transactionId": "enc!!3H-9sWvYgxPtGB1YQdTlju3o7Qbii_Lu8Hm8-gzWw17cs1kq1UCAYWvN2TWqp6an",
                                          "bookingDate": "2024-12-30",
                                          "valueDate": "2024-12-30",
                                          "transactionAmount": {
                                              "amount": "-196.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "29.12 LUNDS KONGSGT 20 ÅLGÅRD",
                                          "remittanceInformationUnstructuredArray": [
                                              "29.12 LUNDS KONGSGT 20 ÅLGÅRD"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "999084dae96e3dffcbc868b110f41025"
                                      },
                                      {
                                          "transactionId": "enc!!3lEvU0rzCC5PVEOxXIaslQO5MTN0hEBNcph44r3DgphFdQzzLgw__R9ZuU0lImS4",
                                          "bookingDate": "2024-12-30",
                                          "valueDate": "2024-12-30",
                                          "transactionAmount": {
                                              "amount": "-552.35",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "FORTUM STRØM AS",
                                          "creditorAccount": {
                                              "iban": "NO3581012373759",
                                              "bban": "81012373759"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: FORTUM STRØM AS Betalt: 28.12.24",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: FORTUM STRØM AS Betalt: 28.12.24"
                                          ],
                                          "remittanceInformationStructuredArray": [
                                              "reference: 7277826000100643, referenceType: SCOR"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-PCID",
                                          "internalTransactionId": "d796317b5f3207739c08cde4b76ce81b"
                                      },
                                      {
                                          "transactionId": "enc!!L3AbM8pFCLc7UJJtBkDA9vShCUAhRNcZiNUp7DT85aYs2JbZ-C8wfOne95rdkgUd",
                                          "bookingDate": "2024-12-30",
                                          "valueDate": "2024-12-30",
                                          "transactionAmount": {
                                              "amount": "-224.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 29.12 NOK 224.00 Dinner for 2 As Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 29.12 NOK 224.00 Dinner for 2 As Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "698caf7c9b501a5e32ebc0d894fd3421"
                                      },
                                      {
                                          "transactionId": "enc!!Hk4zb-kfAaViwlngHkSK26HM5hauOL67uQInAjKnORlcG40jKij8sBPWxd-I-ZpI",
                                          "bookingDate": "2024-12-30",
                                          "valueDate": "2024-12-28",
                                          "transactionAmount": {
                                              "amount": "-50.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Ivett Josefine Geiger",
                                          "creditorAccount": {
                                              "iban": "NO7912142490240",
                                              "bban": "12142490240"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Ivett Josefine Geiger",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Ivett Josefine Geiger"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "a0089f25eb25c51c0fbaafd0c408ea24"
                                      },
                                      {
                                          "transactionId": "enc!!qV-eiLvV4qUyLt0r1GhwKw5m76yh09TTLX6Epf5I4BDHcEXeMzgjsW_IG4ss-TTu",
                                          "bookingDate": "2024-12-27",
                                          "valueDate": "2024-12-27",
                                          "transactionAmount": {
                                              "amount": "-195.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "26.12 NARVESEN 834 ABELHAUGEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "26.12 NARVESEN 834 ABELHAUGEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "fae133e984250fbaadf9f78a411e9f3c"
                                      },
                                      {
                                          "transactionId": "enc!!2grf5ohToSOcYAzJEN-qRCkAmzwMGq63yLTUbh8-e-0AX0VnzhKPbhFfxo89YXe_",
                                          "bookingDate": "2024-12-27",
                                          "valueDate": "2024-12-27",
                                          "transactionAmount": {
                                              "amount": "-615.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 23.12 NOK 615.00 Villa Paradiso Munch Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 23.12 NOK 615.00 Villa Paradiso Munch Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "4734ff63c98116b9b7379f4c6666f2c3"
                                      },
                                      {
                                          "transactionId": "enc!!Lbfk1Ban9Gp5vm-dYJ-ENubyuEdzaY6hZuh1cUqn9D46u6rEg_cYgICQh_XvVvUk",
                                          "bookingDate": "2024-12-27",
                                          "valueDate": "2024-12-27",
                                          "transactionAmount": {
                                              "amount": "-289.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "23.12 NEW JAPAN TRADI KARL JOHANSG OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "23.12 NEW JAPAN TRADI KARL JOHANSG OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "e32e08ffde2c7af3edd4bb064f1aee2c"
                                      },
                                      {
                                          "transactionId": "enc!!KFc504yE4O-jBDXXGe8aRkfQFBFs1cEyU7GQjrqI3OqhHIL8_IviZPQTEmqYDOK0",
                                          "bookingDate": "2024-12-27",
                                          "valueDate": "2024-12-26",
                                          "transactionAmount": {
                                              "amount": "-397.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Walter Verdese",
                                          "creditorAccount": {
                                              "iban": "NO3712247602524",
                                              "bban": "12247602524"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Walter Verdese",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Walter Verdese"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "cced20221ad03303e7e67164d432bb6c"
                                      },
                                      {
                                          "transactionId": "enc!!Oj1oEF16GCO194sKAjUz-gBbCCohhehbYnFb0atxJ67QBAZANOp8JNYwkQyWRcXp",
                                          "bookingDate": "2024-12-23",
                                          "valueDate": "2024-12-23",
                                          "transactionAmount": {
                                              "amount": "-136.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "20.12 TILT . OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "20.12 TILT . OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "24bffb217f45a0d404fdea8e6f120bcf"
                                      },
                                      {
                                          "transactionId": "enc!!PEVkt5cJvAmSljJ7NWwal2hsHMgLr-md4ym6gK7qDBWxIZTRLTG9HxwfIIa0q0kL",
                                          "bookingDate": "2024-12-23",
                                          "valueDate": "2024-12-23",
                                          "transactionAmount": {
                                              "amount": "-136.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "20.12 TILT . OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "20.12 TILT . OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "7d4f68a219905d06192caf42a367e11b"
                                      },
                                      {
                                          "transactionId": "enc!!OFqxRVIZ9mJNVLaNZPoV3Gqwb2Rrkx77mBVJgWUj3jR-Pjipyvq7kX4ixf1AG5DR",
                                          "bookingDate": "2024-12-23",
                                          "valueDate": "2024-12-23",
                                          "transactionAmount": {
                                              "amount": "-13287.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "UTLEIEMEGLEREN FROGNER AS",
                                          "creditorAccount": {
                                              "iban": "NO7516025627389",
                                              "bban": "16025627389"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: UTLEIEMEGLEREN FROGNER AS Betalt: 23.12.24",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: UTLEIEMEGLEREN FROGNER AS Betalt: 23.12.24"
                                          ],
                                          "remittanceInformationStructuredArray": [
                                              "reference: 0400000005090580, referenceType: SCOR"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-DMCT",
                                          "internalTransactionId": "86b3e7759d3f8c30e745e97e41ce22f0"
                                      },
                                      {
                                          "transactionId": "enc!!39fJh9ozWD8CVxCTAPHhXKGNNGdYAgwtoxhc25_B2JwyYKsA6kDwdUM_bipqIIWh",
                                          "bookingDate": "2024-12-23",
                                          "valueDate": "2024-12-23",
                                          "transactionAmount": {
                                              "amount": "-27.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 21.12 NOK 27.00 Vipps*Ruter Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 21.12 NOK 27.00 Vipps*Ruter Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "8893d8cead0e8f991500e473f19c2f03"
                                      },
                                      {
                                          "transactionId": "enc!!iyQI5a7aixpd1o6x2bpjBHleChKY3vJpq2QnMp8D0wT47I-Rbo3bwSzSOnJsWOvS",
                                          "bookingDate": "2024-12-23",
                                          "valueDate": "2024-12-23",
                                          "transactionAmount": {
                                              "amount": "-122.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 20.12 NOK 122.00 Vipps*4Service Facility A Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 20.12 NOK 122.00 Vipps*4Service Facility A Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "72a0fd109653cb6b112c80d817819d43"
                                      },
                                      {
                                          "transactionId": "enc!!tBUO41hBWw7HVGOQ_enztCxHsjN1s_zTjzUIA-mIG8IqtUd-xJ3D_gKK8X9hpUbW",
                                          "bookingDate": "2024-12-23",
                                          "valueDate": "2024-12-23",
                                          "transactionAmount": {
                                              "amount": "-69.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "21.12 DELI DE LUCA TO ARBEIDERSAMF OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "21.12 DELI DE LUCA TO ARBEIDERSAMF OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "b8e98b00596913d1643ba18757f73a66"
                                      },
                                      {
                                          "transactionId": "enc!!Yvn9ikO9PJTjsQBTsYB3T1u-n4yzgwFD8U3f5bSppoqTecgb5RzSV8cm2p9bQ8GQ",
                                          "bookingDate": "2024-12-23",
                                          "valueDate": "2024-12-23",
                                          "transactionAmount": {
                                              "amount": "-69.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 21.12 NOK 69.00 NARVESEN 870 JERNBANETORG Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 21.12 NOK 69.00 NARVESEN 870 JERNBANETORG Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "fe861b017cbd7b3b23313c3c0b3a7db7"
                                      },
                                      {
                                          "transactionId": "enc!!rNR67D9vy3Ampuj099K2m0GyOENmTNi5gFxiq-QzKs9YzpAR0Jn71-1fEStH3qa4",
                                          "bookingDate": "2024-12-23",
                                          "valueDate": "2024-12-23",
                                          "transactionAmount": {
                                              "amount": "-179.10",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "22.12 REMA METRO SENT KULTURHUSGAT LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "22.12 REMA METRO SENT KULTURHUSGAT LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "6a4e63dbf30fcac55b6553a4d840cc1d"
                                      },
                                      {
                                          "transactionId": "enc!!73Vg6-6nEqhr7ERxW3pBfMpyqPxkeUKeAsSkcJs6j50op0Stn-9CWq6L3Sb0QiWA",
                                          "bookingDate": "2024-12-23",
                                          "valueDate": "2024-12-23",
                                          "transactionAmount": {
                                              "amount": "-747.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 20.12 NOK 747.00 Vipps*Ruter Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 20.12 NOK 747.00 Vipps*Ruter Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "4984e3ad28ca390b168b311089211ae4"
                                      },
                                      {
                                          "transactionId": "enc!!852rsyvke9ydL6l4s5VI4sfXoCN64W2IZt4hkZwTzIQJCUuksVRXiHVzjwLUO3GD",
                                          "bookingDate": "2024-12-23",
                                          "valueDate": "2024-12-23",
                                          "transactionAmount": {
                                              "amount": "-130.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "20.12 TILT . OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "20.12 TILT . OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "19c13cfe5a88915b20524e8ae0820aa8"
                                      },
                                      {
                                          "transactionId": "enc!!tbCGS9w0D3eal4h6_bnAfK2Uu7Y7hNuqe6kLysoy-QkEpGd3W1lN1-R6fzsXiNuW",
                                          "bookingDate": "2024-12-20",
                                          "valueDate": "2024-12-20",
                                          "transactionAmount": {
                                              "amount": "-754.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "20.12 OUTLAND AS GRENSEN 5-7  OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "20.12 OUTLAND AS GRENSEN 5-7  OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "32c38d364fb85556737bbd11b9abbb0b"
                                      },
                                      {
                                          "transactionId": "enc!!U2BRF0wNUrBeOkmSOZok8h_zZFRR1kdH3liww4I5S2kxNFS7vlMPo4XfkVcnQOZA",
                                          "bookingDate": "2024-12-20",
                                          "valueDate": "2024-12-20",
                                          "transactionAmount": {
                                              "amount": "-172.40",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "19.12 REMA VIKA PARKVEIEN 64 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "19.12 REMA VIKA PARKVEIEN 64 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "1cc68e2bed7b8d9354bdc4b59bc3cd41"
                                      },
                                      {
                                          "transactionId": "enc!!YwBztNhaalGBHcjrBjfE-2iZlHQ56am-Lnu_SXXEzEdY33rYm9Jtsb2qZsKNU9Rr",
                                          "bookingDate": "2024-12-19",
                                          "valueDate": "2024-12-19",
                                          "transactionAmount": {
                                              "amount": "-42.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 18.12 NOK 42.00 Vipps*Ruter Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 18.12 NOK 42.00 Vipps*Ruter Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "bb314b0c1fb9e03ea1a6cb1842905eba"
                                      },
                                      {
                                          "transactionId": "enc!!vb0YN_HIoycYnala3WhpEpm8xml_qCdAVixPMxDA58KEhkcIjcpjKJguXKzHOHyc",
                                          "bookingDate": "2024-12-19",
                                          "valueDate": "2024-12-19",
                                          "transactionAmount": {
                                              "amount": "-291.90",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 18.12 NOK 291.90 Brewdog Oslo Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 18.12 NOK 291.90 Brewdog Oslo Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "9a6389ba790862e2d8f32c022028e5da"
                                      },
                                      {
                                          "transactionId": "enc!!awFMEmRFPtir6bZGu__lF25W4QrCWYtMcW35-QGdOR6BTG8f4XT44Jwxlv9O66sj",
                                          "bookingDate": "2024-12-19",
                                          "valueDate": "2024-12-19",
                                          "transactionAmount": {
                                              "amount": "-69.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 18.12 NOK 69.00 Vipps*Ruter Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 18.12 NOK 69.00 Vipps*Ruter Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "f93d3efb83d45b0e0361b2478dcc4cff"
                                      },
                                      {
                                          "transactionId": "enc!!cLgSH24k7gEp3fnDDE81CccB0kJRDrEp42Qr_NSR6hEzSGkOeegLidiCGfgwpPD9",
                                          "bookingDate": "2024-12-18",
                                          "valueDate": "2024-12-18",
                                          "transactionAmount": {
                                              "amount": "-245.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "17.12 TRYVANNSTUA AS  TRYVANNSV 64 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "17.12 TRYVANNSTUA AS  TRYVANNSV 64 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "18ad991a6707d8576fe48b7ad1d8613d"
                                      },
                                      {
                                          "transactionId": "enc!!5FiY84q8oSHKjO-wYDHFIQDnjCYO6V9MoPLBITB69MBjFdXdfXQNrIjOpFin9YPl",
                                          "bookingDate": "2024-12-18",
                                          "valueDate": "2024-12-18",
                                          "transactionAmount": {
                                              "amount": "185.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Gaetano Vincenzo Zito",
                                          "creditorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "debtorName": "Rebekka Syversen",
                                          "debtorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "remittanceInformationUnstructured": "Fra: Rebekka Syversen Betalt: 18.12.24",
                                          "remittanceInformationUnstructuredArray": [
                                              "Fra: Rebekka Syversen Betalt: 18.12.24",
                                              "Vipps"
                                          ],
                                          "bankTransactionCode": "PMNT-RCDT-RETP",
                                          "internalTransactionId": "037116bbaccee2c0a2d16dcf4da9f46d"
                                      },
                                      {
                                          "transactionId": "enc!!cpNtY8OawpAitobUnBoQ-ivGADtlOtRjiW2Sh-kOCCwV1aaanAeBt8dGb5pbo4-e",
                                          "bookingDate": "2024-12-18",
                                          "valueDate": "2024-12-18",
                                          "transactionAmount": {
                                              "amount": "-434.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Walter Verdese",
                                          "creditorAccount": {
                                              "iban": "NO3712247602524",
                                              "bban": "12247602524"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Walter Verdese",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Walter Verdese"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "36bfa84ec7fcc18671272b06e2480ea2"
                                      },
                                      {
                                          "transactionId": "enc!!X9Yy9_InZ05yorFciDaeXWCi8z3XO8FSr5yB35yncrf2TWj8aDYeF5qmsr0g_pcg",
                                          "bookingDate": "2024-12-18",
                                          "valueDate": "2024-12-17",
                                          "transactionAmount": {
                                              "amount": "-100.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Rebekka Syversen",
                                          "creditorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Rebekka Syversen",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Rebekka Syversen"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "d7d788d24cb08c39ab95bf0c81c914fe"
                                      },
                                      {
                                          "transactionId": "enc!!ANDhHbz-VozhgczUDMDMHh93atL_qRUDsmL7jUA1KJ_sgjTgLR3owxAEPEEKgLLa",
                                          "bookingDate": "2024-12-16",
                                          "valueDate": "2024-12-16",
                                          "transactionAmount": {
                                              "amount": "-322.50",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "13.12 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "13.12 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "a0d3a3c19d5fa5281903eaa5bce7c0c7"
                                      },
                                      {
                                          "transactionId": "enc!!euqvApIgcxvezoLSoJ7588DCR2dVdXmXM6evraOYp0TP_HKznuvgIZo3A6Otqmhj",
                                          "bookingDate": "2024-12-16",
                                          "valueDate": "2024-12-16",
                                          "transactionAmount": {
                                              "amount": "-79.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "14.12 ESPRESSO HOUSE  STRANDEN 1 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "14.12 ESPRESSO HOUSE  STRANDEN 1 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "628e379bb63459d0448b66ef1037ffb8"
                                      },
                                      {
                                          "transactionId": "enc!!yLUFyu7pGCKM7VTVsQSJ-iGYA4WPW2g2BIFuwxYFysLUYtpiFNsk0dKgJ_zJf5PA",
                                          "bookingDate": "2024-12-16",
                                          "valueDate": "2024-12-16",
                                          "transactionAmount": {
                                              "amount": "-29.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 14.12 NOK 29.00 NARVESEN 834 NATIONALTHEA Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 14.12 NOK 29.00 NARVESEN 834 NATIONALTHEA Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "5b80ddaeffe8bbcdb1bda84c927e148b"
                                      },
                                      {
                                          "transactionId": "enc!!pOtjuXU3LUxz2IXtNx8sTpJB8fN1Zqyhsire7_bbh9r7iVwSCrEiVKAmk-LxE9ad",
                                          "bookingDate": "2024-12-16",
                                          "valueDate": "2024-12-16",
                                          "transactionAmount": {
                                              "amount": "-364.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "15.12 240 KREMMERHUSE STØPERIVEIEN STRØMMEN",
                                          "remittanceInformationUnstructuredArray": [
                                              "15.12 240 KREMMERHUSE STØPERIVEIEN STRØMMEN"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "4f132b8389a934ea41a75495b4488a00"
                                      },
                                      {
                                          "transactionId": "enc!!0HjV4w0jctNjzJKoI9vic6-VEB2klbfwbda-kjbrbKlqazweY1VFOyRUShn6CJxI",
                                          "bookingDate": "2024-12-16",
                                          "valueDate": "2024-12-16",
                                          "transactionAmount": {
                                              "amount": "-186.75",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "15.12 KITCHN STRØMMEN STORSENTER",
                                          "remittanceInformationUnstructuredArray": [
                                              "15.12 KITCHN STRØMMEN STORSENTER"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "72042f8bd8ec7921ad27c04c3bd51f27"
                                      },
                                      {
                                          "transactionId": "enc!!dBNwJwXGhZKPz6I1IhR3cxlJrmQ6KUGufA8_0H7sMEw6Fajszx_i5RjJ3YER11M1",
                                          "bookingDate": "2024-12-16",
                                          "valueDate": "2024-12-16",
                                          "transactionAmount": {
                                              "amount": "-34.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "16.12 KAFFEMAKERIET SOLHEIMSVN.  LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "16.12 KAFFEMAKERIET SOLHEIMSVN.  LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "d935c59e6a1ae4d3940dd025cc4a80ab"
                                      },
                                      {
                                          "transactionId": "enc!!LlfP5Lj9JrCuwMas1ti35g4slpWxslVb63SxTy8mDvh4hoyWKTu1K1cVhFtbH-zg",
                                          "bookingDate": "2024-12-16",
                                          "valueDate": "2024-12-15",
                                          "transactionAmount": {
                                              "amount": "-400.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Rebekka Syversen",
                                          "creditorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Rebekka Syversen",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Rebekka Syversen"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "91a80d28816627a5a4f1460d6f06737f"
                                      },
                                      {
                                          "transactionId": "enc!!1dYeTA-Qhj_0M9XDNIjE58ixocJr5U3_KxZ19ORqqXBuTGyRZ5b5bEaMeE9UlS22",
                                          "bookingDate": "2024-12-13",
                                          "valueDate": "2024-12-13",
                                          "transactionAmount": {
                                              "amount": "51248.00",
                                              "currency": "NOK"
                                          },
                                          "creditorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "debtorName": "AIRTHINGS ASA",
                                          "debtorAccount": {
                                              "iban": "NO6784502866191",
                                              "bban": "84502866191"
                                          },
                                          "remittanceInformationUnstructured": "Fra: AIRTHINGS ASA",
                                          "remittanceInformationUnstructuredArray": [
                                              "Fra: AIRTHINGS ASA"
                                          ],
                                          "bankTransactionCode": "PMNT-RCDT-SALA",
                                          "internalTransactionId": "f927cf2c266309337c69923535ce7555"
                                      },
                                      {
                                          "transactionId": "enc!!nHBpyiD872aWorNHJZFJRjgAtU4SvYyrL70hsumt04pzWEaAzb0Ia-4TV7sXiUv3",
                                          "bookingDate": "2024-12-11",
                                          "valueDate": "2024-12-11",
                                          "transactionAmount": {
                                              "amount": "-29.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "11.12 COOP PRIX POSTG BISKOP GUNNE OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "11.12 COOP PRIX POSTG BISKOP GUNNE OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "9eed83388bf97db15d693796426a64eb"
                                      },
                                      {
                                          "transactionId": "enc!!ho9CzHTjYO-t3srSKkizVvRH8f3-bD95gZzQB8fryokktw6bYooqogwwmmADRXfb",
                                          "bookingDate": "2024-12-11",
                                          "valueDate": "2024-12-11",
                                          "transactionAmount": {
                                              "amount": "-399.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "11.12 PENTAGON ARMY HUITFELDTSGT OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "11.12 PENTAGON ARMY HUITFELDTSGT OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "6a82aa22464ba6023494bd112b68ddcd"
                                      },
                                      {
                                          "transactionId": "enc!!IK1_MtC9vNcPMbBT17C9fIvkWaZ9NSgUtM9m-6z5ef1DiP6gltMwuteJYQ8s8lVp",
                                          "bookingDate": "2024-12-11",
                                          "valueDate": "2024-12-11",
                                          "transactionAmount": {
                                              "amount": "-233.10",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "11.12 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "11.12 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "e6e2fb9e1aa519af66f591d983d987f3"
                                      },
                                      {
                                          "transactionId": "enc!!EMwyPO2y1cozfR6t_S9w621ErRyoip1CZbAGKNBR_7o_Ot_NefzXpSC8CGDEEwxw",
                                          "bookingDate": "2024-12-11",
                                          "valueDate": "2024-12-11",
                                          "transactionAmount": {
                                              "amount": "-40.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "10.12 HOUSE OF NERDS  VULKAN 18 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "10.12 HOUSE OF NERDS  VULKAN 18 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "070dea3f56a3a50073a2a773c705759e"
                                      },
                                      {
                                          "transactionId": "enc!!zuKzAAHVg8qLu2ZxVt70LzmTLAtQ3sDZBK2WQW4La3w5AfeT95jSKE0kX8IAaXXJ",
                                          "bookingDate": "2024-12-11",
                                          "valueDate": "2024-12-10",
                                          "transactionAmount": {
                                              "amount": "-110.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Stefano Pomarico",
                                          "creditorAccount": {
                                              "iban": "NO8518138144328",
                                              "bban": "18138144328"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Stefano Pomarico",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Stefano Pomarico"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "b8059aaba0bf85641ca2e7808677ea54"
                                      },
                                      {
                                          "transactionId": "enc!!SaiZhuyLR9Rq-q5M_UJKiQPlL7nmuJuC8U-7mt1ZqbgmYjj6qqfKD_hBepjJZsXG",
                                          "bookingDate": "2024-12-10",
                                          "valueDate": "2024-12-10",
                                          "transactionAmount": {
                                              "amount": "-240.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "10.12 SABRURA STORTOR GRENSEN 4 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "10.12 SABRURA STORTOR GRENSEN 4 OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "efc3b40927d6004a3f9a3099eff13081"
                                      },
                                      {
                                          "transactionId": "enc!!oyMZwb_J1M-GqnFBrqUxaCPTUK2oG8iW43bd7bn92lXxyhnsFnBfhL7eoBRZp-LM",
                                          "bookingDate": "2024-12-09",
                                          "valueDate": "2024-12-09",
                                          "transactionAmount": {
                                              "amount": "-44.90",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "06.12 JOKER TJUVHOLME STRANDPROMEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "06.12 JOKER TJUVHOLME STRANDPROMEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "0b04453917b5ec8d3e2004827461380d"
                                      },
                                      {
                                          "transactionId": "enc!!hlRn36qT3_5NAwk40H2WTvfLXcta69kFpE9eU4M7qVGIdNxrFT2NAASyORsBtL7-",
                                          "bookingDate": "2024-12-09",
                                          "valueDate": "2024-12-09",
                                          "transactionAmount": {
                                              "amount": "-95.01",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 05.12 EUR 8.00 ROMA TRASTEVERE SELF SERV Kurs: 11.8763",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 05.12 EUR 8.00 ROMA TRASTEVERE SELF SERV Kurs: 11.8763"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "2bc4822ae5c8b04401f99cbefef58dbc"
                                      },
                                      {
                                          "transactionId": "enc!!VKMANll7JThs_LLDdMgT7vhL2eeyE6og7N4cYhlb15LE0aJE5jvC3CJNVZkaf2Ic",
                                          "bookingDate": "2024-12-09",
                                          "valueDate": "2024-12-09",
                                          "transactionAmount": {
                                              "amount": "-474.90",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "07.12 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "07.12 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "240176ac29b4556778f1b788cc4b2f53"
                                      },
                                      {
                                          "transactionId": "enc!!erp5FTzXkFgaAiZR6PF7n6lAiyZiS7-mJO0d6nqkpdBef1lRxkOQOv_SsMpWtNCn",
                                          "bookingDate": "2024-12-09",
                                          "valueDate": "2024-12-09",
                                          "transactionAmount": {
                                              "amount": "-17.89",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 04.12 EUR 1.50 ATAC TAP&GO Kurs: 11.9267",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 04.12 EUR 1.50 ATAC TAP&GO Kurs: 11.9267"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "9437e739926facddbb9ca3ef6be64c81"
                                      },
                                      {
                                          "transactionId": "enc!!ADN_dsM-y0bUBpZnjKjTsTokgoH3lrGsfDNaIxlDI1FO_jX0IwZluWjpjFBViGrG",
                                          "bookingDate": "2024-12-06",
                                          "valueDate": "2024-12-09",
                                          "transactionAmount": {
                                              "amount": "-32.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 05.12 NOK 32.00 Inflight Services Norwegi Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 05.12 NOK 32.00 Inflight Services Norwegi Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "0c0a083527e75e934703baddeb61fd3b"
                                      },
                                      {
                                          "transactionId": "enc!!Y5KBRDEmaZ1BIbaGP1Rs5xQmA9itL8RISf2S5K_Eitw1cWQ1mU0A8K-omAOVvmS9",
                                          "bookingDate": "2024-12-06",
                                          "valueDate": "2024-12-09",
                                          "transactionAmount": {
                                              "amount": "-368.14",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 05.12 EUR 31.00 FERROVECCHIO Kurs: 11.8755",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 05.12 EUR 31.00 FERROVECCHIO Kurs: 11.8755"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "cb99f7076488cbe7d953ffb2adce5dd9"
                                      },
                                      {
                                          "transactionId": "enc!!NmgOlfiHjGu-xoJcNxeKJ6rEHpilDJAKLMXu0OYhvQWGDBYgXn38uw_wwMBChT-c",
                                          "bookingDate": "2024-12-06",
                                          "valueDate": "2024-12-06",
                                          "transactionAmount": {
                                              "amount": "-3468.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 05.12 NOK 3468.00 Vipps*SKIMORE AS Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 05.12 NOK 3468.00 Vipps*SKIMORE AS Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "9f22bcdb3ed91e93b91a217c423c323e"
                                      },
                                      {
                                          "transactionId": "enc!!gcLk7e8wQSnmNeY4vBHUt3HOzaJUJGfiKu_fjIog9qKwxACdqzvUTJ5AElGwwpKR",
                                          "bookingDate": "2024-12-05",
                                          "valueDate": "2024-12-06",
                                          "transactionAmount": {
                                              "amount": "-1881.44",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 04.12 EUR 157.97 DECATHLON 00002569 Kurs: 11.9101",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 04.12 EUR 157.97 DECATHLON 00002569 Kurs: 11.9101"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "f82772c876adc89c5dfbc502a07047b1"
                                      },
                                      {
                                          "transactionId": "enc!!qR-T-vyRgwlw3rpRaZ6ePah3QMmq5fW9G9Rlymc4TCnAH1_KKW77zUpGPiyDAvC-",
                                          "bookingDate": "2024-12-05",
                                          "valueDate": "2024-12-06",
                                          "transactionAmount": {
                                              "amount": "-154.83",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 04.12 EUR 13.00 THE SLEEPING DOG - SRL Kurs: 11.9100",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 04.12 EUR 13.00 THE SLEEPING DOG - SRL Kurs: 11.9100"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "39dcec7e81c01c1a3d8693dcd2ea44e5"
                                      },
                                      {
                                          "transactionId": "enc!!7tsbV9ydBHCF7NlSp9WvqG8W7sXy2XfMDLHp4SiHYI1VRZNlfhOoBsIxtj2UstHc",
                                          "bookingDate": "2024-12-05",
                                          "valueDate": "2024-12-06",
                                          "transactionAmount": {
                                              "amount": "-136.13",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 04.12 EUR 11.43 ALICE OSTIENSE Kurs: 11.9099",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 04.12 EUR 11.43 ALICE OSTIENSE Kurs: 11.9099"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "ba0642e5128a15384a62981abbb641cc"
                                      },
                                      {
                                          "transactionId": "enc!!H43UjN1Q0NnX9UGHFD3_3-Q7CRRsvSK5Q0KGTnBtGkdc4Wi6B9BJX4ZAFcqkIkZi",
                                          "bookingDate": "2024-12-05",
                                          "valueDate": "2024-12-06",
                                          "transactionAmount": {
                                              "amount": "-13.10",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 04.12 EUR 1.10 PAGANELLI LUCA Kurs: 11.9091",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 04.12 EUR 1.10 PAGANELLI LUCA Kurs: 11.9091"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "5fb0cb9fbdeba2dc90f10e90d5981c52"
                                      },
                                      {
                                          "transactionId": "enc!!Yi-qINabvalzbod5TqQ46o9DlJZkOKhhOqXKM3O6chu9ycRJgHExZ_XQbgteJXdG",
                                          "bookingDate": "2024-12-04",
                                          "valueDate": "2024-12-05",
                                          "transactionAmount": {
                                              "amount": "-164.78",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 03.12 EUR 13.79 MG SUPERMERCATI Kurs: 11.9492",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 03.12 EUR 13.79 MG SUPERMERCATI Kurs: 11.9492"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "80e5cef18508ecf0830c50dc9c9f8280"
                                      },
                                      {
                                          "transactionId": "enc!!9-No4cliecfV_afXVR3sEmnW3JqJbTYoePDvkaijOM7JDmdUPf3P4B_S0zV5LnCa",
                                          "bookingDate": "2024-12-04",
                                          "valueDate": "2024-12-05",
                                          "transactionAmount": {
                                              "amount": "-109.93",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 03.12 EUR 9.20 BAR TABACCHI ANNARUMI Kurs: 11.9489",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 03.12 EUR 9.20 BAR TABACCHI ANNARUMI Kurs: 11.9489"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "ec8a5865ebac648a4478b92e0c4599c2"
                                      },
                                      {
                                          "transactionId": "enc!!Q2THkN_xy2UgbroyXo4bJwfvRdnEZlwc-NL4NcM_zIkvatrWPt1s5-UcnAhaq3vM",
                                          "bookingDate": "2024-12-04",
                                          "valueDate": "2024-12-05",
                                          "transactionAmount": {
                                              "amount": "-148.82",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 02.12 EUR 12.42 PIZZERIA RUSTICA Kurs: 11.9823",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 02.12 EUR 12.42 PIZZERIA RUSTICA Kurs: 11.9823"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "87b928e3862b3c3a9e48faa056f180fd"
                                      },
                                      {
                                          "transactionId": "enc!!_d2RrlUlMEZGqxMjh3DFlKKFesUvZS_mGEnQie13uMLEtpmrBZF0sKiYlS40GIqk",
                                          "bookingDate": "2024-12-04",
                                          "valueDate": "2024-12-04",
                                          "transactionAmount": {
                                              "amount": "-799.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 04.12 NOK 799.00 BRUCE Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 04.12 NOK 799.00 BRUCE Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "61815de65044770986a9cc92aad71d33"
                                      },
                                      {
                                          "transactionId": "enc!!71oT--wh9WUnapRSn6nlSa_qAMyV4aEZTxh8-6Y1wg_ANQ-b3ohOnXOApja5i4Ld",
                                          "bookingDate": "2024-12-03",
                                          "valueDate": "2024-12-04",
                                          "transactionAmount": {
                                              "amount": "-365.99",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 02.12 NOK 365.99 LinkedInPre *76108863 Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 02.12 NOK 365.99 LinkedInPre *76108863 Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "42f3202e346be668fbbbd56bbd7925f1"
                                      },
                                      {
                                          "transactionId": "enc!!fe0IYgbd9_WsUDzZHKmMR0pKlSCF0efmLmEdp1NncEUZagKMOnRq59RFNviyb3Q9",
                                          "bookingDate": "2024-12-03",
                                          "valueDate": "2024-12-04",
                                          "transactionAmount": {
                                              "amount": "-23.97",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 02.12 EUR 2.00 NO-TIMES SRL Kurs: 11.9850",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 02.12 EUR 2.00 NO-TIMES SRL Kurs: 11.9850"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "b791d9fa329c98dcd8c6395182a07fac"
                                      },
                                      {
                                          "transactionId": "enc!!B8OJ1FOtJzPRL38Xq3kb-fFxHF5Y6nGEwCAyrAhXfPp5qIVwYpX5DBNze-S-sZ3H",
                                          "bookingDate": "2024-12-03",
                                          "valueDate": "2024-12-04",
                                          "transactionAmount": {
                                              "amount": "-14.37",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 02.12 EUR 1.20 CAFFE' TEATRO ROMA Kurs: 11.9750",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 02.12 EUR 1.20 CAFFE' TEATRO ROMA Kurs: 11.9750"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "4988b23ccc1aedde54cb02f2785c5c37"
                                      },
                                      {
                                          "transactionId": "enc!!_9OYUoieByPMQm7er-jRyZXWkUHTmFkOxlbSLqtbl47tb4JqKuRUuzdzsDTypNZ-",
                                          "bookingDate": "2024-12-03",
                                          "valueDate": "2024-12-03",
                                          "transactionAmount": {
                                              "amount": "300.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Gaetano Vincenzo Zito",
                                          "creditorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "debtorName": "Rebekka Syversen",
                                          "debtorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "remittanceInformationUnstructured": "Fra: Rebekka Syversen Betalt: 03.12.24",
                                          "remittanceInformationUnstructuredArray": [
                                              "Fra: Rebekka Syversen Betalt: 03.12.24",
                                              "Taxi Tano"
                                          ],
                                          "bankTransactionCode": "PMNT-RCDT-RETP",
                                          "internalTransactionId": "cbd15734e1f2f3bb20566ff2a75962d8"
                                      },
                                      {
                                          "transactionId": "enc!!CxRJcqZm6EOCHKBYCUX7Bd53_iOJRw8VEqYqeeYbuAU-rSpr_svFYFNXlnBMLBQ5",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-03",
                                          "transactionAmount": {
                                              "amount": "-155.76",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 01.12 EUR 13.00 THE NAGS HEAD Kurs: 11.9815",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 01.12 EUR 13.00 THE NAGS HEAD Kurs: 11.9815"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "017cfaf007595389ddfcf73b2976a159"
                                      },
                                      {
                                          "transactionId": "enc!!pPzXpRHSAF8yAO3Rad5forCxN58BS31DO_SIrx2nmTrSyZWYxYsisNKM4QxNrsbS",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-03",
                                          "transactionAmount": {
                                              "amount": "-59.91",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 01.12 EUR 5.00 THE NAG'S HEAD Kurs: 11.9820",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 01.12 EUR 5.00 THE NAG'S HEAD Kurs: 11.9820"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "93e1e0bda55b04b03f4a835964c00365"
                                      },
                                      {
                                          "transactionId": "enc!!S_XLjf32SH-FEsUGCGku75oq5_BNBZasaoq79D-EHgJuDHJJxKZYrUuJkyerQDJQ",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-03",
                                          "transactionAmount": {
                                              "amount": "-898.64",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 01.12 EUR 75.00 ER MACELLAIO Kurs: 11.9819",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 01.12 EUR 75.00 ER MACELLAIO Kurs: 11.9819"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "82a83cc7e03d99fa5051aefa2c03318f"
                                      },
                                      {
                                          "transactionId": "enc!!qfUwoy9cfDYuphR47mrmyUHf_YpOZOqsZmZIJjTFStbhrjW_0FduQIkuJrxsmnBd",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-03",
                                          "transactionAmount": {
                                              "amount": "-83.87",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 01.12 EUR 7.00 THE NAG'S HEAD Kurs: 11.9814",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 01.12 EUR 7.00 THE NAG'S HEAD Kurs: 11.9814"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "bc0ac4d2f0091299ed1bd041efca162f"
                                      },
                                      {
                                          "transactionId": "enc!!MCLi5Yhjh1BRRdjT8kOe-jgvF28CsS07ycMoTJoa9Rv70aWTbzPV2OIMGF3KK1-o",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-03",
                                          "transactionAmount": {
                                              "amount": "-14.37",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 01.12 EUR 1.20 PEPYS BAR Kurs: 11.9750",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 01.12 EUR 1.20 PEPYS BAR Kurs: 11.9750"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "a1c732156390e8666c9be066c464467b"
                                      },
                                      {
                                          "transactionId": "enc!!7ImocM5YysH9hLDofSSw-JpNZ2aORMxoajBBQAhvrVGq0-iMzr6CbArgeQ9OrlVy",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-38.34",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.11 EUR 3.20 PAM LOCAL 2 Kurs: 11.9813",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.11 EUR 3.20 PAM LOCAL 2 Kurs: 11.9813"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "eed18699af6f90c811173816277dfcea"
                                      },
                                      {
                                          "transactionId": "enc!!IG4Ynyz8Bd0x8CBcUgVnKQG8h3YGLq3w73H7EGyyWvMEaplilG6Xs_DNzMkPGbsO",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-47.93",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.11 EUR 4.00 FLAMINIO 37 Kurs: 11.9825",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.11 EUR 4.00 FLAMINIO 37 Kurs: 11.9825"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "14a60e1b2eb300a48630ce181d21e97b"
                                      },
                                      {
                                          "transactionId": "enc!!8ftHYx7UfmdMBODuj1c773rnoypaiV7e3ltf0ivPwqEFa_9vmT8Judo9c0YA78jE",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-47.93",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.11 EUR 4.00 LAURENTINA Kurs: 11.9825",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.11 EUR 4.00 LAURENTINA Kurs: 11.9825"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "3fefb8001eeb018aa5000c7de05c0a08"
                                      },
                                      {
                                          "transactionId": "enc!!BfPy5IQ5snxhrOyDrx0Nn4FdsjuQQQ_OzCj0-Aeatm9JRLHZbMfwyctIuKJ-1yWj",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-166.55",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.11 EUR 13.90 FARMACIA GUARNACCI SNC Kurs: 11.9820",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.11 EUR 13.90 FARMACIA GUARNACCI SNC Kurs: 11.9820"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "25237209aa114892b3f319d0d865a412"
                                      },
                                      {
                                          "transactionId": "enc!!ofBPWG51oDZBs3ATgpHtBx1ckRkQhj4YWIXrFMM-cJYXebm6qTMiCqistxorTVd2",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-43.14",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.11 EUR 3.60 GOLD RESTAURANT Kurs: 11.9833",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.11 EUR 3.60 GOLD RESTAURANT Kurs: 11.9833"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "c5683ed6cb0c4a493ec45c1e953c49ef"
                                      },
                                      {
                                          "transactionId": "enc!!Re6OjiZhXX8qon_Xb2aokh2ihrQQ0jMf_TZxjHhpBNdZbuWg7oz3DVkcN3IftasL",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-144.62",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 29.11 EUR 12.10 AUTOGRILL 7204 Kurs: 11.9521",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 29.11 EUR 12.10 AUTOGRILL 7204 Kurs: 11.9521"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "59286ee67695a55a296f1c71f9678c21"
                                      },
                                      {
                                          "transactionId": "enc!!dIk2rylRXEUsroROVvakCSDbovIrg7p_dJHAtR6PLTA34MwOvTMLnXMFl-TSRda-",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-222.86",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.11 EUR 18.60 MONDOARANCINA Kurs: 11.9817",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.11 EUR 18.60 MONDOARANCINA Kurs: 11.9817"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "9881b827549a24656153fdcdff3f27f5"
                                      },
                                      {
                                          "transactionId": "enc!!csb9rZvzvR53bO8gQS6lyyzC4YqOoY00Jsm4QqMMF7jaDm9FnJ5TPTiay-oSNTIZ",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-102.79",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 29.11 EUR 8.60 ADS MAGLIANA NORD Kurs: 11.9523",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 29.11 EUR 8.60 ADS MAGLIANA NORD Kurs: 11.9523"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "dba77c75fa04e2f1232c7528e5c1309d"
                                      },
                                      {
                                          "transactionId": "enc!!Uy1smEbPgRcacyGQNMMbrsFNkmXuiQlGI6cwaGPSLdx1bpreTU4anTVR1YiBG7DA",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-196.50",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.11 EUR 16.40 INTIMISSIMI UOMO Kurs: 11.9817",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.11 EUR 16.40 INTIMISSIMI UOMO Kurs: 11.9817"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "0b96525f0af60fb3d704381089338c46"
                                      },
                                      {
                                          "transactionId": "enc!!Z4ToWVXjRzlrHansuqmmdtMuGSGs_KkzxhLxEHpFV1YhXvgGaITH1EOepUxolvSR",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-537.84",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 29.11 EUR 45.00 DOMUS SRL Kurs: 11.9520",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 29.11 EUR 45.00 DOMUS SRL Kurs: 11.9520"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "0c1ec0e08d978859f9d36017fe7ee840"
                                      },
                                      {
                                          "transactionId": "enc!!UWsYGNZEulELBKf-3SEpeR1lSaNdqzsMB1qofPAviZqdeqJBUG4nK8PEflPVQmwd",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-46.73",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 30.11 EUR 3.90 MONDOARANCINA Kurs: 11.9821",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 30.11 EUR 3.90 MONDOARANCINA Kurs: 11.9821"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "68bc4c00e1508baf1fbfc2d771604eef"
                                      },
                                      {
                                          "transactionId": "enc!!1FOD-82PWlE_0u2HDf8IxUN9gwpu1UUhGW5yIUOsmzTX5S0wXthpXfM4oSWEkd5u",
                                          "bookingDate": "2024-11-30",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-63.40",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 28.11 EUR 5.30 EMANUELE TOMASSI Kurs: 11.9623",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 28.11 EUR 5.30 EMANUELE TOMASSI Kurs: 11.9623"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "f27f39cf32a68d2a7d477ad4d8c61ab8"
                                      },
                                      {
                                          "transactionId": "enc!!yanvhXCbriUWcee4cODX0c5DqUR7BY_7XHt89YbnpOne6P3WuFA73SDtXN063Kjq",
                                          "bookingDate": "2024-11-30",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-77.76",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 28.11 EUR 6.50 ROSSINI CAFFE DI ROSSINI Kurs: 11.9631",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 28.11 EUR 6.50 ROSSINI CAFFE DI ROSSINI Kurs: 11.9631"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "9142d072a8dc8a5777bab9e8304978f5"
                                      },
                                      {
                                          "transactionId": "enc!!51h7503of90tl-8LInBl3rXRVUq2YZAs38Twh5dyfCO4y5C10qlyVtsREXfOAXXR",
                                          "bookingDate": "2024-11-30",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-71.78",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 28.11 EUR 6.00 BAL SRLS Kurs: 11.9633",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 28.11 EUR 6.00 BAL SRLS Kurs: 11.9633"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "7f10538da36f22b7c24fa62f1507940b"
                                      },
                                      {
                                          "transactionId": "enc!!5rdQLz5GaE_wo-itGEh6UpIK5eXf184_KEGdJC5KVeSIIEMChq8uPdkNIWgQlw77",
                                          "bookingDate": "2024-11-30",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-62.21",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 28.11 EUR 5.20 TWO BROTHERS DI DE MARTI Kurs: 11.9635",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 28.11 EUR 5.20 TWO BROTHERS DI DE MARTI Kurs: 11.9635"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "b59c03f5a4a76f19d4aa260e650c2d26"
                                      },
                                      {
                                          "transactionId": "enc!!adf-Qf8HkbN5-YiT__PSgkxjfpTIxJVMiOC1DbmSk_yoDswuMgwfzsi3hOZqZY3Q",
                                          "bookingDate": "2024-11-30",
                                          "valueDate": "2024-12-02",
                                          "transactionAmount": {
                                              "amount": "-233.28",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 28.11 EUR 19.50 WEISSE PUB DI TELESCA MA Kurs: 11.9631",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 28.11 EUR 19.50 WEISSE PUB DI TELESCA MA Kurs: 11.9631"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "b9c0f42dcb040203d2bb92d58309f814"
                                      },
                                      {
                                          "transactionId": "enc!!F_p4H-8WrsHak8JFdIuq1PJoStrR1JiB0TH_t-exmgFbXdlCWwoTDI0gPlnU5NYc",
                                          "bookingDate": "2024-12-02",
                                          "valueDate": "2024-11-30",
                                          "transactionAmount": {
                                              "amount": "185.75",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Gaetano Vincenzo Zito",
                                          "creditorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "debtorName": "Roberta Vaula",
                                          "debtorAccount": {
                                              "iban": "NO7612274014049",
                                              "bban": "12274014049"
                                          },
                                          "remittanceInformationUnstructured": "Fra: Roberta Vaula Betalt: 30.11.24",
                                          "remittanceInformationUnstructuredArray": [
                                              "Fra: Roberta Vaula Betalt: 30.11.24",
                                              "Grazie per lacquisto"
                                          ],
                                          "bankTransactionCode": "PMNT-RCDT-RETP",
                                          "internalTransactionId": "fea1fb4d896a36d78bc594dfdde692d9"
                                      },
                                      {
                                          "transactionId": "enc!!PDx3ohr34YrxbGh2ZK2CgSWvt1fou9Vs_lpC1NtPTEtQJpAH69ySYF-pUMGqviXc",
                                          "bookingDate": "2024-11-30",
                                          "valueDate": "2024-11-30",
                                          "transactionAmount": {
                                              "amount": "-60.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "VAREKJØP                           30 TRANS(ER) TYPE 709",
                                          "remittanceInformationUnstructuredArray": [
                                              "VAREKJØP                           30 TRANS(ER) TYPE 709"
                                          ],
                                          "bankTransactionCode": "BOOK-EXPS-BANK",
                                          "internalTransactionId": "1a0e0fb10bea713ba16663fd49b14a82"
                                      },
                                      {
                                          "transactionId": "enc!!70m0Uyjvs4fCumdQDNquSPUeUlA9DWmiqyFKuc73QxvnUu3Ar7XqF9CTX0_W2xRH",
                                          "bookingDate": "2024-11-30",
                                          "valueDate": "2024-11-30",
                                          "transactionAmount": {
                                              "amount": "-4.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "MOBILGIRO M/KID, FORF. I DAG        2 TRANS(ER) TYPE 261",
                                          "remittanceInformationUnstructuredArray": [
                                              "MOBILGIRO M/KID, FORF. I DAG        2 TRANS(ER) TYPE 261"
                                          ],
                                          "bankTransactionCode": "BOOK-EXPS-BANK",
                                          "internalTransactionId": "617d26a4cd87d49de4289df749b98fba"
                                      },
                                      {
                                          "transactionId": "enc!!2zguSYQLUjVfZmTq0XjFfD55flLG-4sd58Lf0Dx5LHiUYNZd44EDT8J-NRKdzSaO",
                                          "bookingDate": "2024-11-30",
                                          "valueDate": "2024-11-30",
                                          "transactionAmount": {
                                              "amount": "-2.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "NETTGIRO M/KID PÅ FORFALLSREG.      1 TRANS(ER) TYPE 202",
                                          "remittanceInformationUnstructuredArray": [
                                              "NETTGIRO M/KID PÅ FORFALLSREG.      1 TRANS(ER) TYPE 202"
                                          ],
                                          "bankTransactionCode": "BOOK-EXPS-BANK",
                                          "internalTransactionId": "9eea3b809d83da46005c524c432e93c1"
                                      },
                                      {
                                          "transactionId": "enc!!_F11vO0mv0pjkjT1vhjS1_FPqs6M-VPFkERLDUiCkidLlYrS8a3T6w5FlNBkuc8q",
                                          "bookingDate": "2024-11-30",
                                          "valueDate": "2024-11-30",
                                          "transactionAmount": {
                                              "amount": "-96.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "VISA VAREKJØP                      48 TRANS(ER) TYPE 714",
                                          "remittanceInformationUnstructuredArray": [
                                              "VISA VAREKJØP                      48 TRANS(ER) TYPE 714"
                                          ],
                                          "bankTransactionCode": "BOOK-EXPS-BANK",
                                          "internalTransactionId": "8e74d34e7280cbdacab38f6f84c29a0d"
                                      },
                                      {
                                          "transactionId": "enc!!8BoXsrfGf6ao70rCnMEAeR7Y6vFxQXkhcDp2B8AlXeOuG6BWo9gQfeTDQbaQyft4",
                                          "bookingDate": "2024-11-28",
                                          "valueDate": "2024-11-29",
                                          "transactionAmount": {
                                              "amount": "-251.11",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 27.11 EUR 21.00 BF *TEMA S.R.L. Kurs: 11.9576",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 27.11 EUR 21.00 BF *TEMA S.R.L. Kurs: 11.9576"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "267d5aa2d297b0c91f20c3a48a2927ed"
                                      },
                                      {
                                          "transactionId": "enc!!HR5h38CIe7qpdh-8uMvbC97v-foW9J2b7ot3D0vpmtKwdytERgZO6YpQQ-ka3fJd",
                                          "bookingDate": "2024-11-28",
                                          "valueDate": "2024-11-29",
                                          "transactionAmount": {
                                              "amount": "-141.10",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 27.11 EUR 11.80 LE DELIZIE DEL GRANO Kurs: 11.9576",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 27.11 EUR 11.80 LE DELIZIE DEL GRANO Kurs: 11.9576"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "8d3e95b4acb251b7f0045da04c31ea2f"
                                      },
                                      {
                                          "transactionId": "enc!!W_mDjmeyJ0a4lfioR9KXKhuCHud1EdC0LLiHrJIM5rahdyLxW5d495AEZRV1job9",
                                          "bookingDate": "2024-11-28",
                                          "valueDate": "2024-11-29",
                                          "transactionAmount": {
                                              "amount": "-40.66",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 27.11 EUR 3.40 APOGEO BAR TABACCHI Kurs: 11.9588",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 27.11 EUR 3.40 APOGEO BAR TABACCHI Kurs: 11.9588"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "d24b8666e2f4a1f294ab6b04df0a1941"
                                      },
                                      {
                                          "transactionId": "enc!!WfTX0GQPhOZ39RgaOKidhOQQQW1qXzKRkvv90QvP17K0GE8yfMuLS07sla-XM9O0",
                                          "bookingDate": "2024-11-28",
                                          "valueDate": "2024-11-29",
                                          "transactionAmount": {
                                              "amount": "-420.08",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 27.11 EUR 35.13 COMPAR SPA - BATA Kurs: 11.9579",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 27.11 EUR 35.13 COMPAR SPA - BATA Kurs: 11.9579"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "d7d1da4df7477e3bf8c70b784c17827f"
                                      },
                                      {
                                          "transactionId": "enc!!iwknv5BVUqo6-OK53KjJBNWgHrqJ3--vZNhdMTUTNpI6r08Lk1x_vQNaHIdMEnmL",
                                          "bookingDate": "2024-11-28",
                                          "valueDate": "2024-11-29",
                                          "transactionAmount": {
                                              "amount": "-119.45",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 27.11 EUR 9.99 SPORT 85 Kurs: 11.9570",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 27.11 EUR 9.99 SPORT 85 Kurs: 11.9570"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "f739e0486cf0c269e8ab729c05f4e774"
                                      },
                                      {
                                          "transactionId": "enc!!hZH7NBd1TyoBYs5PJYA1Sm51OEXPH5swObjzaaTbyPkUpdhn-J2ga00NYcO3S3Sp",
                                          "bookingDate": "2024-11-28",
                                          "valueDate": "2024-11-29",
                                          "transactionAmount": {
                                              "amount": "-125.56",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 27.11 EUR 10.50 SHEBAA Kurs: 11.9581",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 27.11 EUR 10.50 SHEBAA Kurs: 11.9581"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "8d1617139ab8e0e2b328ecdf9072c381"
                                      },
                                      {
                                          "transactionId": "enc!!az5ktj8KvZ2_0JmhuKmFYSTJI2VX9nWkm6DfhyC9ZsxPq9nSPxwg46JwZrkb7fj1",
                                          "bookingDate": "2024-11-28",
                                          "valueDate": "2024-11-28",
                                          "transactionAmount": {
                                              "amount": "-561.58",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "FORTUM STRØM AS",
                                          "creditorAccount": {
                                              "iban": "NO1860050685317",
                                              "bban": "60050685317"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: FORTUM STRØM AS Betalt: 28.11.24",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: FORTUM STRØM AS Betalt: 28.11.24"
                                          ],
                                          "remittanceInformationStructuredArray": [
                                              "reference: 7277826000100627, referenceType: SCOR"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-PCID",
                                          "internalTransactionId": "f1e5a34d7e0a325c92331a75e6b6a5de"
                                      },
                                      {
                                          "transactionId": "enc!!2I056a-Yy04A3Is51G2MCezVlk4ztAf6SuuqXzujE_7rbx7Q1l5JK_ow7qzhRPMs",
                                          "bookingDate": "2024-11-27",
                                          "valueDate": "2024-11-28",
                                          "transactionAmount": {
                                              "amount": "-178.43",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 26.11 EUR 15.00 SUMUP  *NEW FASHION DI FA Kurs: 11.8953",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 26.11 EUR 15.00 SUMUP  *NEW FASHION DI FA Kurs: 11.8953"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "4e92ae2f27325180c17883c5712ad6e5"
                                      },
                                      {
                                          "transactionId": "enc!!CTTm1wGr1eODAq2EDbmpMQIvlF4vBVD-x1l4v-rEg7Jxf4g1OfoX30evXyri3Rhv",
                                          "bookingDate": "2024-11-27",
                                          "valueDate": "2024-11-28",
                                          "transactionAmount": {
                                              "amount": "-267.64",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 26.11 EUR 22.50 LA SARTORIA Kurs: 11.8951",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 26.11 EUR 22.50 LA SARTORIA Kurs: 11.8951"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "c20f208d0ac1ffe797e58c423b99558f"
                                      },
                                      {
                                          "transactionId": "enc!!Dg5uBKT05KhXbGjwvZOAg0u9_PA47NH-UaqYk2lpbgPMke-MIliFacOrfRbMaI_t",
                                          "bookingDate": "2024-11-27",
                                          "valueDate": "2024-11-28",
                                          "transactionAmount": {
                                              "amount": "-73.75",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 26.11 EUR 6.20 EMANUELE TOMASSI Kurs: 11.8952",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 26.11 EUR 6.20 EMANUELE TOMASSI Kurs: 11.8952"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "101526fe7ce361c6afb90282fb8c01c3"
                                      },
                                      {
                                          "transactionId": "enc!!9URi5V5eVawFjxL_T364E2nZ0NyYTu46nFR9oESZiey0Z8dbaXb_69Rtj3xg4YMD",
                                          "bookingDate": "2024-11-26",
                                          "valueDate": "2024-11-27",
                                          "transactionAmount": {
                                              "amount": "-228.77",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 25.11 EUR 19.30 COLLI CAFFE Kurs: 11.8534",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 25.11 EUR 19.30 COLLI CAFFE Kurs: 11.8534"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "40697f21866e0e36f564e814182cd8c7"
                                      },
                                      {
                                          "transactionId": "enc!!ITc1KAUX7C-diU_ahdRnWr6P2GDZxnM_yV1l-kJXUAeXiNjYjQLJxTDZUCTG5wT5",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-26",
                                          "transactionAmount": {
                                              "amount": "-46.22",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 24.11 EUR 3.90 EMANUELE TOMASSI Kurs: 11.8513",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 24.11 EUR 3.90 EMANUELE TOMASSI Kurs: 11.8513"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "2ff9929005dfc6613a31811c57f01c91"
                                      },
                                      {
                                          "transactionId": "enc!!CeHGKBEfdNS2v0Wbw48RqGSrfpTN0q3Sx9ljooBBi_DBlx_iTDC16K5CrF1n_Yoq",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-26",
                                          "transactionAmount": {
                                              "amount": "-112.60",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 24.11 EUR 9.50 ALMAGAIA Kurs: 11.8526",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 24.11 EUR 9.50 ALMAGAIA Kurs: 11.8526"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "f406d84606587d3259f2b3be019ad8d5"
                                      },
                                      {
                                          "transactionId": "enc!!9Y7C74Dv9xbVMFDpsfBCQVM5NypPXlAO30YM2qic3KTJH6QZRQXwYchDtp3FvOKh",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-26",
                                          "transactionAmount": {
                                              "amount": "-67.56",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 24.11 EUR 5.70 CAOS CAFF Kurs: 11.8526",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 24.11 EUR 5.70 CAOS CAFF Kurs: 11.8526"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "9edb65b146633319a270f1311ec3ae99"
                                      },
                                      {
                                          "transactionId": "enc!!k_Lvz51Y9yqaQYhke7nu89Z9YNwAPnyHnbVW1CvErcbYmwLqFIqIpxKQLCheBsmE",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-26",
                                          "transactionAmount": {
                                              "amount": "-73.49",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 24.11 EUR 6.20 MANDUCA Kurs: 11.8532",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 24.11 EUR 6.20 MANDUCA Kurs: 11.8532"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "2d62e84e9f2eafebeb022bf22b2f95c6"
                                      },
                                      {
                                          "transactionId": "enc!!3NjEu5myRPXN8PjEY_HebANi_RaMtHx54R2HzFwZ89L3mIVFpRTQAEyCI2KLNb20",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-26",
                                          "transactionAmount": {
                                              "amount": "-891.34",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 24.11 EUR 75.20 DAR BOTTAROLO APRILIA -DE Kurs: 11.8529",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 24.11 EUR 75.20 DAR BOTTAROLO APRILIA -DE Kurs: 11.8529"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "d7c2424387f813abf251aab4226ba2f8"
                                      },
                                      {
                                          "transactionId": "enc!!Z49XDOxVeC_rVWIzdHQPAb76Y-DCSa8OBJWFjgrr5CdsKG0BAs6hskeypb-qRFZV",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-26",
                                          "transactionAmount": {
                                              "amount": "-32.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 24.11 NOK 32.00 Inflight Services Norwegi Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 24.11 NOK 32.00 Inflight Services Norwegi Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "7aa7257187e556d70db68c0a2e3958c2"
                                      },
                                      {
                                          "transactionId": "enc!!uSRlWc8--JiD3sAvTsYzyJpEOspnMmq-_EBVw_3tzuBKv_0GhZjtFCZ5AekSNkvl",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-25",
                                          "transactionAmount": {
                                              "amount": "-195.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "22.11 NARVESEN 834 ABELHAUGEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "22.11 NARVESEN 834 ABELHAUGEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "ec079385b8821162dbedd52d0828b8b7"
                                      },
                                      {
                                          "transactionId": "enc!!HAhp2dK5b3X8ZtF0zsO-zJlz2-GT-22a26Agpyah1S9wxEXNMUHm1xwqQGc0Xzin",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-25",
                                          "transactionAmount": {
                                              "amount": "-13884.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "UTLEIEMEGLEREN FROGNER AS",
                                          "creditorAccount": {
                                              "iban": "NO7516025627389",
                                              "bban": "16025627389"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: UTLEIEMEGLEREN FROGNER AS Betalt: 25.11.24",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: UTLEIEMEGLEREN FROGNER AS Betalt: 25.11.24"
                                          ],
                                          "remittanceInformationStructuredArray": [
                                              "reference: 0400000005090580, referenceType: SCOR"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-PCID",
                                          "internalTransactionId": "ece5120f40c9f888ba2fccef26dcc6c2"
                                      },
                                      {
                                          "transactionId": "enc!!N7ObKI6OSbE8XUo6JdyoRNeCK_XJXciwsYbTALNWtRWvU24AxmjaRasfHp8qKVqB",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-25",
                                          "transactionAmount": {
                                              "amount": "-52.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "24.11 STARBUCK (A364) 5019040 GARDERMOEN",
                                          "remittanceInformationUnstructuredArray": [
                                              "24.11 STARBUCK (A364) 5019040 GARDERMOEN"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "75d42e428d1a50050a93a48a4ccc3339"
                                      },
                                      {
                                          "transactionId": "enc!!zyOojY3C40n1u0hAK2m0BzBe1v3AZieXKmBGozsJi1o6qCiGv1rac2W_2_WpbF0-",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-25",
                                          "transactionAmount": {
                                              "amount": "-122.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 22.11 NOK 122.00 Vipps*4Service Facility A Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 22.11 NOK 122.00 Vipps*4Service Facility A Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "645e59f1ec32d539fdcd084a110344b6"
                                      },
                                      {
                                          "transactionId": "enc!!EdJG6AqIwY4oTDu0JevxEVMpq9U4a3hfeFRIQEy9hfNR9iMvmXXUbtDkzpVvXOQ6",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-25",
                                          "transactionAmount": {
                                              "amount": "-90.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "24.11 BIT UNION - PIL 10 SCHWEIGAA OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "24.11 BIT UNION - PIL 10 SCHWEIGAA OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "071f0b58bd341394c7a0916428cb8aec"
                                      },
                                      {
                                          "transactionId": "enc!!nmRfljy-CrIWUS-EsBJ6TTNGJtHoDZQItNIjN4JGb1yEE83O9xlWsrIN69snLkAV",
                                          "bookingDate": "2024-11-22",
                                          "valueDate": "2024-11-25",
                                          "transactionAmount": {
                                              "amount": "-12.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 21.11 NOK 12.00 APPLE.COM/BILL Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 21.11 NOK 12.00 APPLE.COM/BILL Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "a7ac0a47612c0a98932c8dccdf0bb51b"
                                      },
                                      {
                                          "transactionId": "enc!!3o41AP9Y1rVL-m4xoJIbirI05A6DPNfudNGNd7PsC9kpa3Jb2noeS4svK1Y72aN_",
                                          "bookingDate": "2024-11-25",
                                          "valueDate": "2024-11-22",
                                          "transactionAmount": {
                                              "amount": "-300.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Walter Verdese",
                                          "creditorAccount": {
                                              "iban": "NO3712247602524",
                                              "bban": "12247602524"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Walter Verdese",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Walter Verdese"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "cfb7ca5503c213ae19983fd41c439e74"
                                      },
                                      {
                                          "transactionId": "enc!!ASmu_Th6WEZHuu4Eaf43F0p_P2gzHsXhiU4zItC3PdyNBZucNwIC3qK1m3j59BOY",
                                          "bookingDate": "2024-11-21",
                                          "valueDate": "2024-11-21",
                                          "transactionAmount": {
                                              "amount": "-248.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 20.11 NOK 248.00 JOE  THE JUICE NORGE AS Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 20.11 NOK 248.00 JOE  THE JUICE NORGE AS Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "993485193c17dc752df5479b8b5ab6fc"
                                      },
                                      {
                                          "transactionId": "enc!!boISERPUCrNt6AY0nEv392YOzKTQubx9dAXPfhgPv0n1XLIGPnfwhfmo5rtbznVf",
                                          "bookingDate": "2024-11-20",
                                          "valueDate": "2024-11-21",
                                          "transactionAmount": {
                                              "amount": "-39.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 19.11 NOK 39.00 TGTG yqsv24gvsse50 Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 19.11 NOK 39.00 TGTG yqsv24gvsse50 Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "1bec6a61b05725af6ea84f3c316c731b"
                                      },
                                      {
                                          "transactionId": "enc!!-pTR1cBqeQsEksCW8rxW35UchPpaQYzSTTOdRuFy4zUCAh5cljaK1kIBxPmnsCrU",
                                          "bookingDate": "2024-11-20",
                                          "valueDate": "2024-11-20",
                                          "transactionAmount": {
                                              "amount": "-70.30",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "19.11 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "19.11 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "578fd1d39d54907797cbb4d7a54c3cbb"
                                      },
                                      {
                                          "transactionId": "enc!!OGDbuyu8WDuTk5CE_Dl0RHjud5DdT2DBdn4xYVqCUKPvqZWbw4Hc9lJzYiVKfUPw",
                                          "bookingDate": "2024-11-20",
                                          "valueDate": "2024-11-20",
                                          "transactionAmount": {
                                              "amount": "-5000.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Rebekka Syversen",
                                          "creditorAccount": {
                                              "iban": "NO2310802644022",
                                              "bban": "10802644022"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Rebekka Syversen",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Rebekka Syversen"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "edcea3fbce6a1cf96f68c3dc8726a744"
                                      },
                                      {
                                          "transactionId": "enc!!Rw__nkEoGE-_A543YWvhuLIuCEjJptGO-4zbLuR-11koT1f1UuYCYwOlMYCXY7Lc",
                                          "bookingDate": "2024-11-20",
                                          "valueDate": "2024-11-20",
                                          "transactionAmount": {
                                              "amount": "-58.80",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "20.11 MENY VIKA RUSELØKKVEIE OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "20.11 MENY VIKA RUSELØKKVEIE OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "56537a4a88b5a7e1b60683f5c690ecf1"
                                      },
                                      {
                                          "transactionId": "enc!!AXQPyO9SKlqmNBboe7IcymXqnpL8bLIW5-4fPaSlyQdjVJwYzH7pLqy3grj3faDx",
                                          "bookingDate": "2024-11-20",
                                          "valueDate": "2024-11-20",
                                          "transactionAmount": {
                                              "amount": "-35.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "19.11 DELI DE LUCA TO ARBEIDERSAMF OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "19.11 DELI DE LUCA TO ARBEIDERSAMF OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "f8f3349334d226ec200bc6afbf3acb42"
                                      },
                                      {
                                          "transactionId": "enc!!_RH4vZIYOuhGQIQJ-xqW_QCYWGmF-_25PxR9ElsndoMS2GejROH0NYqV8dGJbg7T",
                                          "bookingDate": "2024-11-20",
                                          "valueDate": "2024-11-20",
                                          "transactionAmount": {
                                              "amount": "-74.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 19.11 NOK 74.00 ZETTLE_*OSB03 Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 19.11 NOK 74.00 ZETTLE_*OSB03 Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "86baf69b75f9913ed146794e82651ca5"
                                      },
                                      {
                                          "transactionId": "enc!!2qAa2g72V0CAu0NE8WgSLJBTGQiGb3OKDOGeFjMOX6acdoWw_AHcPUIcy0ZrxT5Y",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-19",
                                          "transactionAmount": {
                                              "amount": "-5538.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Utleiemegleren Frogner AS",
                                          "creditorAccount": {
                                              "iban": "NO7516025627389",
                                              "bban": "16025627389"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Utleiemegleren Frogner AS Betalt: 18.11.24",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Utleiemegleren Frogner AS Betalt: 18.11.24"
                                          ],
                                          "remittanceInformationStructuredArray": [
                                              "reference: 0404057425090588, referenceType: SCOR"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-PCID",
                                          "internalTransactionId": "128cf117c084c6b089e57eced7074c12"
                                      },
                                      {
                                          "transactionId": "enc!!U_qEGqHLfHgd6ci9vlrxk8oH3JVyIZ5Fk4InNfonA2OXWQHTRVVODBafBtxE8ZRm",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-114.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "18.11 FINSTUA AS 1 . OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "18.11 FINSTUA AS 1 . OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "25efa875cc53cd3bef83ce84354b4a87"
                                      },
                                      {
                                          "transactionId": "enc!!CHO-jhKQB-qRXzy7rGLn6OXl2-7xkcTo8PUPxkWkfVJV1Idmn-f5I7tlc4_tAw15",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-87.80",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "16.11 COOP MEGA METRO BIBLIOTEKSGA LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "16.11 COOP MEGA METRO BIBLIOTEKSGA LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "a0148ae1a7b35d7fd5ee8c13cfe9b0e4"
                                      },
                                      {
                                          "transactionId": "enc!!5FB9TN0DLOcacW-VL5hvXWKUcX_EKc_csMRhOOBZgLs54dkKtRafsH_vbrNF8eh7",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-27.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 17.11 NOK 27.00 Vipps*Ruter Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 17.11 NOK 27.00 Vipps*Ruter Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "a9621498e299800cd6092a6a2761cd9c"
                                      },
                                      {
                                          "transactionId": "enc!!_mmO8KEgJ4Dn_aNA9W-ggNt74-DM7WdGnPHD127QQwgex66s-8jZzDgMu1hjau1t",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-27.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 17.11 NOK 27.00 Vipps*Ruter Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 17.11 NOK 27.00 Vipps*Ruter Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "e806c6f677fcc52019ceb7b14a6b4838"
                                      },
                                      {
                                          "transactionId": "enc!!fiGCTpu0lctmSy-kKc488nJyIZUZaXA2ou0D8tA184OsWhsVqBjkOVwGMxQN3P9Z",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-201.70",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "16.11 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "16.11 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "976576ba0db7248652738641fd8c4efc"
                                      },
                                      {
                                          "transactionId": "enc!!2nGNCT_YImChBTnSlmQylizGs6hYKf2TIX6EAuHnSiCF-VsuQUjaHH7XRdpMM_8Z",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-215.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "18.11 FINSTUA AS 1 . OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "18.11 FINSTUA AS 1 . OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "ecc959bd0731c7bebfed97ec36698fb1"
                                      },
                                      {
                                          "transactionId": "enc!!xHrga3X7yhFSciPPZo1TSt6dl3pZ45IN0pRohUKXh0v2uBKkamu5DU2dtZ4zluQE",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-301.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 17.11 NOK 301.00 Paa Sno Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 17.11 NOK 301.00 Paa Sno Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "691af5d7564294cfdbae630a73c260b0"
                                      },
                                      {
                                          "transactionId": "enc!!-VtBCChkRIqDiVZAcwCZs_NCgt9uQI5znEG8-kM4P6iMKyWBnQ3g8yyD4uMU3R5s",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-747.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 18.11 NOK 747.00 Vipps*Ruter Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 18.11 NOK 747.00 Vipps*Ruter Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "bb664fb03cb511e9e55f68b5c4519205"
                                      },
                                      {
                                          "transactionId": "enc!!57qwI-FOfipmOGn_QUqsuaIziB01zfvf8Wk91WOpfZZNGL_reOt7gh0cjK5HrDEu",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-38.80",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "16.11 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "16.11 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "8b90b08753bb051b39241be183e5a6fc"
                                      },
                                      {
                                          "transactionId": "enc!!gkTvjSwFfX4ZWYvLUI3S7AH0x8YedPn4EB8UA46Db6Mr_lrquOat3oumBdZki4Zd",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-2000.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "18.11 TryvannWyller A TRYVANN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "18.11 TryvannWyller A TRYVANN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "36959dff36d31059a365257d0a67fbc6"
                                      },
                                      {
                                          "transactionId": "enc!!MIZiEZiZe0WP9HDiLNsCW76XpItmiv7HGK9SdchzswIYq8xWI4hC8zKDhM4CjB-b",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-111.30",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "16.11 FEEL METRO METRO SENTER LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "16.11 FEEL METRO METRO SENTER LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "6f6facdb51ae782c52541ee6188bae42"
                                      },
                                      {
                                          "transactionId": "enc!!bG0uX_gRFj5mfwFQU1-zRgJ7qCAJlCCw4317qOpmSvk8NnQnP_0szElkY6ztq0_O",
                                          "bookingDate": "2024-11-15",
                                          "valueDate": "2024-11-18",
                                          "transactionAmount": {
                                              "amount": "-49.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 14.11 NOK 49.00 APPLE.COM/BILL Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 14.11 NOK 49.00 APPLE.COM/BILL Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-XBPO",
                                          "internalTransactionId": "64c2afdac11dec34d7b4b7f2dcb1ec34"
                                      },
                                      {
                                          "transactionId": "enc!!qgIdLrrQA5Ir7X77zeW6NSGgJ-qJGrjatKd80Bg3YmePnTAq_2NWoTP0e_v3h5JW",
                                          "bookingDate": "2024-11-18",
                                          "valueDate": "2024-11-17",
                                          "transactionAmount": {
                                              "amount": "-200.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Walter Verdese",
                                          "creditorAccount": {
                                              "iban": "NO3712247602524",
                                              "bban": "12247602524"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Walter Verdese",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Walter Verdese"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "f99df6d48ce4fbd76db56971454979c4"
                                      },
                                      {
                                          "transactionId": "enc!!t3oljS336Tz0_IKVZaX62TSmy5OPDcmpqMnKBDO16DR138KlftoA9vFi19SJVAzs",
                                          "bookingDate": "2024-11-15",
                                          "valueDate": "2024-11-15",
                                          "transactionAmount": {
                                              "amount": "-423.50",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "14.11 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "14.11 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "727f078f080bed439d4453958e4a4a5b"
                                      },
                                      {
                                          "transactionId": "enc!!CKmr68yaIFOPpIpsVgKHyRTLNO2NsznD96xaegyx1MBNmjjom5nFpnMpoULyzcNE",
                                          "bookingDate": "2024-11-14",
                                          "valueDate": "2024-11-14",
                                          "transactionAmount": {
                                              "amount": "-100.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "13.11 CHURROS 03 . ÅLGÅRD",
                                          "remittanceInformationUnstructuredArray": [
                                              "13.11 CHURROS 03 . ÅLGÅRD"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "74cc15222cbc706370a1ef3c801a75b7"
                                      },
                                      {
                                          "transactionId": "enc!!bkB_acDAmnQ63EawrlMwxXCcyl6h6KY9c8Tmp2z_Yg4zJnEw3lcC8k36dH1QtbQm",
                                          "bookingDate": "2024-11-14",
                                          "valueDate": "2024-11-14",
                                          "transactionAmount": {
                                              "amount": "-98.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 13.11 NOK 98.00 7-ELEVEN 7060 PALEET Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 13.11 NOK 98.00 7-ELEVEN 7060 PALEET Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "3554ef5952a69c1a484195da249ef531"
                                      },
                                      {
                                          "transactionId": "enc!!qccVPEFDLcUkwpf12jDNYY6syhXwb9YhtQHe5M-F2V9SUBD32UKtpKBZpT4M7feo",
                                          "bookingDate": "2024-11-14",
                                          "valueDate": "2024-11-14",
                                          "transactionAmount": {
                                              "amount": "41245.00",
                                              "currency": "NOK"
                                          },
                                          "creditorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "debtorName": "AIRTHINGS ASA",
                                          "debtorAccount": {
                                              "iban": "NO6784502866191",
                                              "bban": "84502866191"
                                          },
                                          "remittanceInformationUnstructured": "Fra: AIRTHINGS ASA",
                                          "remittanceInformationUnstructuredArray": [
                                              "Fra: AIRTHINGS ASA"
                                          ],
                                          "bankTransactionCode": "PMNT-RCDT-SALA",
                                          "internalTransactionId": "c9e61324b030edfe4cb7cb527557ddf0"
                                      },
                                      {
                                          "transactionId": "enc!!w7zE9rleMchuwy2unYTfRSJ-BV6SnlwppGJIDbalKNkM5RKkVPDeKhvh8rdWQcJx",
                                          "bookingDate": "2024-11-14",
                                          "valueDate": "2024-11-14",
                                          "transactionAmount": {
                                              "amount": "-32.90",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "13.11 JOKER ROSENKRAN ROSENKRANTZG OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "13.11 JOKER ROSENKRAN ROSENKRANTZG OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "9b7a253a196fc4e84270d9840dc45bd7"
                                      },
                                      {
                                          "transactionId": "enc!!ZI6k2ZakrdBtgJDnB1Gd6LTUQadsRHDsviFN6YcquDRriBWCD3xXZuzI7LVZiUyF",
                                          "bookingDate": "2024-11-13",
                                          "valueDate": "2024-11-13",
                                          "transactionAmount": {
                                              "amount": "-119.70",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "12.11 MATKROKEN FRIDT FRIDTJOF NAN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "12.11 MATKROKEN FRIDT FRIDTJOF NAN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "f11e4787a35d7bafb412663157c36e86"
                                      },
                                      {
                                          "transactionId": "enc!!yfBGgAMy7LBhbHDrWqC2H6TL87pwgHnakHkHqn0HHoaBdoOyM4Gr0E-olu7zdwGI",
                                          "bookingDate": "2024-11-12",
                                          "valueDate": "2024-11-12",
                                          "transactionAmount": {
                                              "amount": "-700.00",
                                              "currency": "NOK"
                                          },
                                          "creditorName": "Walter Verdese",
                                          "creditorAccount": {
                                              "iban": "NO3712247602524",
                                              "bban": "12247602524"
                                          },
                                          "debtorName": "Gaetano Vincenzo Zito",
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "Til: Walter Verdese",
                                          "remittanceInformationUnstructuredArray": [
                                              "Til: Walter Verdese"
                                          ],
                                          "bankTransactionCode": "PMNT-ICDT-RETP",
                                          "internalTransactionId": "caba364fad538480b39f02bef39a9c24"
                                      },
                                      {
                                          "transactionId": "enc!!LkVxfOh8j2M3pucZL6FjuQiV_lgd2Jurb-Ma3j7eUTcPOx9pCgA-cQ8FvvZhvkJe",
                                          "bookingDate": "2024-11-12",
                                          "valueDate": "2024-11-12",
                                          "transactionAmount": {
                                              "amount": "-266.40",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "11.11 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "11.11 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "e60431e9bcaa2a57318be4428fb09868"
                                      },
                                      {
                                          "transactionId": "enc!!7LbV6l3ezWj40GtHSzN2x9kcymAz3rEqFicNYfFxQTsOyQ3hfiRE047YWyiG-dhz",
                                          "bookingDate": "2024-11-12",
                                          "valueDate": "2024-11-12",
                                          "transactionAmount": {
                                              "amount": "-53.80",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "12.11 BACKSTUBE SANDV OTTO SVERDRU SANDVIKA",
                                          "remittanceInformationUnstructuredArray": [
                                              "12.11 BACKSTUBE SANDV OTTO SVERDRU SANDVIKA"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "3769654b9107535dfc32bde837e87db4"
                                      },
                                      {
                                          "transactionId": "enc!!zd-T9sxIlT-YQikIfE9Jo4HA4CYI45baEaXDLkqlse_fyYV9yeZTew-nk5mvqTGx",
                                          "bookingDate": "2024-11-11",
                                          "valueDate": "2024-11-11",
                                          "transactionAmount": {
                                              "amount": "-109.80",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "09.11 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "09.11 KIWI 301 SOLHEI SKÅRERSLETTA LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "e0108a660d8725915cc79172fb24fd24"
                                      },
                                      {
                                          "transactionId": "enc!!9WEcL0rE_wODl9LtyfDIRKRpidlM2hgiLpJh95H_AVkgiU00hatQAiiBHw9e07Fq",
                                          "bookingDate": "2024-11-11",
                                          "valueDate": "2024-11-11",
                                          "transactionAmount": {
                                              "amount": "-558.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "09.11 SABRURA TRIADEN GAMLEVEIEN 8 LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "09.11 SABRURA TRIADEN GAMLEVEIEN 8 LØRENSKOG"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "5915a605361f4a0334874cbdfc6ace3f"
                                      },
                                      {
                                          "transactionId": "enc!!jeRFq93bsSseETrUGbCuZzeTcVGWnBocTZS8NLaoddIdPYIb_nr9o6V-On9CuU4R",
                                          "bookingDate": "2024-11-11",
                                          "valueDate": "2024-11-11",
                                          "transactionAmount": {
                                              "amount": "-30.90",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "08.11 MENY VIKA RUSELØKKVEIE OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "08.11 MENY VIKA RUSELØKKVEIE OSLO"
                                          ],
                                          "bankTransactionCode": "CARD-NCRD-POSD",
                                          "internalTransactionId": "8518185dae968fb3ebfd59862e0bbc42"
                                      },
                                      {
                                          "transactionId": "enc!!5NcMvtpPhFVrABQLQvAfsGqBiK1t8U8qamVEQOeHxqLwMTqF5olZsLa348X6xQZJ",
                                          "bookingDate": "2024-11-11",
                                          "valueDate": "2024-11-11",
                                          "transactionAmount": {
                                              "amount": "-153.00",
                                              "currency": "NOK"
                                          },
                                          "debtorAccount": {
                                              "iban": "NO9317202972616",
                                              "bban": "17202972616"
                                          },
                                          "remittanceInformationUnstructured": "*8098 08.11 NOK 153.00 JOE  THE JUICE NORGE AS Kurs: 1.0000",
                                          "remittanceInformationUnstructuredArray": [
                                              "*8098 08.11 NOK 153.00 JOE  THE JUICE NORGE AS Kurs: 1.0000"
                                          ],
                                          "bankTransactionCode": "CARD-CCRD-POSD",
                                          "internalTransactionId": "6f304faee7b380eccc80803324f01b71"
                                      }
                                  ],
                                  "pending": [
                                      {
                                          "bookingDate": "2025-02-07",
                                          "valueDate": "2025-02-07",
                                          "transactionAmount": {
                                              "amount": "-347.00",
                                              "currency": "NOK"
                                          },
                                          "remittanceInformationUnstructured": "07.02 BURGER KING MET METROSENTERE LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "07.02 BURGER KING MET METROSENTERE LØRENSKOG"
                                          ]
                                      },
                                      {
                                          "bookingDate": "2025-02-07",
                                          "valueDate": "2025-02-06",
                                          "transactionAmount": {
                                              "amount": "-75.00",
                                              "currency": "NOK"
                                          },
                                          "remittanceInformationUnstructured": "06.02 SUBWAY METRO SE BIBLIOTEKGT  LØRENSKOG",
                                          "remittanceInformationUnstructuredArray": [
                                              "06.02 SUBWAY METRO SE BIBLIOTEKGT  LØRENSKOG"
                                          ]
                                      },
                                      {
                                          "bookingDate": "2025-02-07",
                                          "valueDate": "2025-02-06",
                                          "transactionAmount": {
                                              "amount": "-201.00",
                                              "currency": "NOK"
                                          },
                                          "remittanceInformationUnstructured": "06.02 KIWI 373 SOLLI  HENRIK IBSEN OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "06.02 KIWI 373 SOLLI  HENRIK IBSEN OSLO"
                                          ]
                                      },
                                      {
                                          "bookingDate": "2025-02-07",
                                          "valueDate": "2025-02-06",
                                          "transactionAmount": {
                                              "amount": "-241.70",
                                              "currency": "NOK"
                                          },
                                          "remittanceInformationUnstructured": "06.02 REMA VIKA PARKVEIEN 64 OSLO",
                                          "remittanceInformationUnstructuredArray": [
                                              "06.02 REMA VIKA PARKVEIEN 64 OSLO"
                                          ]
                                      }
                                  ]
                              }
                          }
                          """;
}