using SSD_Assignment___Banking_Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Banking_Application
{
    [SupportedOSPlatform("windows")]
    public class Program
    {
        public static void Main(string[] args)
        {

            Data_Access_Layer dal = Data_Access_Layer.getInstance();
            dal.GetType().GetMethod("initialiseDatabase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(dal, null);
            bool running = true;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            do
            {

                Console.WriteLine("");
                Console.WriteLine("***Banking Application Menu***");
                Console.WriteLine("1. Add Bank Account");
                Console.WriteLine("2. Close Bank Account");
                Console.WriteLine("3. View Account Information");
                Console.WriteLine("4. Make Lodgement");
                Console.WriteLine("5. Make Withdrawal");
                Console.WriteLine("6. Exit");
                Console.WriteLine("CHOOSE OPTION:");
                String option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        if (!ResourceChecker.HasSufficientDiskSpace())
                        {
                            Console.WriteLine("There isn't enough space on the computer to add a new account.");
                            break;
                        }

                        String accountType = "";
                        int loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");

                            Console.WriteLine("");
                            Console.WriteLine("***Account Types***:");
                            Console.WriteLine("1. Current Account.");
                            Console.WriteLine("2. Savings Account.");
                            Console.WriteLine("CHOOSE OPTION:");
                            accountType = Console.ReadLine();

                            loopCount++;
                        } while (!(accountType.Equals("1") || accountType.Equals("2")));

                        string rawName = "";
                        loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID NAME ENTERED - PLEASE TRY AGAIN (Letters, spaces, apostrophes and hyphens only)");

                            Console.WriteLine("Enter Name: ");
                            rawName = InputValidator.SanitizeString(Console.ReadLine());

                            loopCount++;
                        } while (!InputValidator.IsValidName(rawName));

                        string rawAddress1 = "";
                        loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID ADDRESS LINE 1 ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Address Line 1: ");
                            rawAddress1 = InputValidator.SanitizeString(Console.ReadLine());

                            loopCount++;
                        } while (!InputValidator.IsValidAddress(rawAddress1, true));

                        Console.WriteLine("Enter Address Line 2: ");
                        string rawAddress2 = InputValidator.SanitizeString(Console.ReadLine());

                        Console.WriteLine("Enter Address Line 3: ");
                        string rawAddress3 = InputValidator.SanitizeString(Console.ReadLine());

                        string rawTown = "";
                        loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID TOWN ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Town: ");
                            rawTown = InputValidator.SanitizeString(Console.ReadLine());

                            loopCount++;
                        } while (!InputValidator.IsValidTown(rawTown));

                        double balance = -1;
                        loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID OPENING BALANCE ENTERED - PLEASE TRY AGAIN (Must be 0 or greater)");

                            Console.WriteLine("Enter Opening Balance: ");
                            string balanceString = Console.ReadLine();

                            try
                            {
                                balance = Convert.ToDouble(balanceString);
                                if (!InputValidator.IsValidMonetaryAmount(balance, true))
                                {
                                    balance = -1;
                                    loopCount++;
                                }
                            }
                            catch
                            {
                                balance = -1;
                                loopCount++;
                            }

                        } while (balance < 0);

                        Bank_Account ba;

                        if (Convert.ToInt32(accountType) == Account_Type.Current_Account)
                        {
                            double overdraftAmount = -1;
                            loopCount = 0;

                            do
                            {
                                if (loopCount > 0)
                                    Console.WriteLine("INVALID OVERDRAFT AMOUNT ENTERED - PLEASE TRY AGAIN (Must be 0 or greater)");

                                Console.WriteLine("Enter Overdraft Amount: ");
                                string overdraftAmountString = Console.ReadLine();

                                try
                                {
                                    overdraftAmount = Convert.ToDouble(overdraftAmountString);
                                    if (!InputValidator.IsValidMonetaryAmount(overdraftAmount, true))
                                    {
                                        overdraftAmount = -1;
                                        loopCount++;
                                    }
                                }
                                catch
                                {
                                    overdraftAmount = -1;
                                    loopCount++;
                                }
                            } while (overdraftAmount < 0);

                            ba = new Current_Account(rawName, rawAddress1, rawAddress2, rawAddress3, rawTown, balance, overdraftAmount);
                        }
                        else
                        {
                            double interestRate = -1;
                            loopCount = 0;

                            do
                            {
                                if (loopCount > 0)
                                    Console.WriteLine("INVALID INTEREST RATE ENTERED - PLEASE TRY AGAIN (Must be between 0 and 100)");

                                Console.WriteLine("Enter Interest Rate (%): ");
                                string interestRateString = Console.ReadLine();

                                try
                                {
                                    interestRate = Convert.ToDouble(interestRateString);
                                    if (!InputValidator.IsValidPercentage(interestRate))
                                    {
                                        interestRate = -1;
                                        loopCount++;
                                    }
                                }
                                catch
                                {
                                    interestRate = -1;
                                    loopCount++;
                                }
                            } while (interestRate < 0);

                            ba = new Savings_Account(rawName, rawAddress1, rawAddress2, rawAddress3, rawTown, balance, interestRate);
                        }

                        string accNo = dal.addBankAccount(ba);

                        Console.WriteLine("New Account Number Is: " + ba.getDecryptedAccountNo());
;

                        string tellerName = Environment.UserName;

                        EventLogger.LogTransaction(
                            TransactionType.AccountCreation,
                            tellerName,
                            accNo,
                            rawName  //  use original user input — no need to decrypt after
                        );

                        ba = null;
                        tellerName = null;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        break;

                    case "2":
                        Console.WriteLine("Enter Account Number: ");
                        String closeAccNo = InputValidator.SanitizeString(Console.ReadLine());

                        if (!InputValidator.IsValidAccountNumber(closeAccNo))
                        {
                            Console.WriteLine("Invalid Account Number Format");
                            break;
                        }

                        ba = dal.findBankAccountByAccNo(closeAccNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            Console.WriteLine(ba.ToString());

                            String ans = "";

                            do
                            {
                                Console.WriteLine("Proceed With Deletion (Y/N)?");
                                ans = Console.ReadLine();

                                switch (ans)
                                {
                                    case "Y":
                                    case "y":
                                        dal.closeBankAccount(closeAccNo);
                                        Console.WriteLine("Account Closed Successfully");

                                        string decryptedName_Close = CryptoHelper.Decrypt(ba.Name);
                                        string tellerName_Close = Environment.UserName;

                                        EventLogger.LogTransaction(
                                            TransactionType.AccountClosure,
                                            tellerName_Close,
                                            closeAccNo,
                                            decryptedName_Close
                                        );

                                        ba = null;
                                        GC.Collect();
                                        GC.WaitForPendingFinalizers();

                                        break;
                                    case "N":
                                    case "n":
                                        Console.WriteLine("Account Closure Cancelled");
                                        break;
                                    default:
                                        Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                                        break;
                                }
                            } while (!(ans.Equals("Y") || ans.Equals("y") || ans.Equals("N") || ans.Equals("n")));
                        }

                        break;

                    case "3":
                        Console.WriteLine("Enter Account Number: ");
                        String viewAccNo = InputValidator.SanitizeString(Console.ReadLine());

                        if (!InputValidator.IsValidAccountNumber(viewAccNo))
                        {
                            Console.WriteLine("Invalid Account Number Format");
                            break;
                        }

                        ba = dal.findBankAccountByAccNo(viewAccNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            Console.WriteLine("Account No: " + ba.AccountNo);
                            Console.WriteLine("Name: " + ba.getDecryptedName());
                            Console.WriteLine("Address Line 1: " + ba.getDecryptedAddressLine1());
                            Console.WriteLine("Address Line 2: " + ba.getDecryptedAddressLine2());
                            Console.WriteLine("Address Line 3: " + ba.getDecryptedAddressLine3());
                            Console.WriteLine("Town: " + ba.getDecryptedTown());
                            Console.WriteLine("Balance: €" + ba.Balance.ToString("F2"));
                            Console.WriteLine("Available Funds: €" + ba.getAvailableFunds().ToString("F2"));

                            string tellerName_View = Environment.UserName;

                            EventLogger.LogTransaction(
                                TransactionType.BalanceQuery,
                                tellerName_View,
                                viewAccNo,
                                ba.getDecryptedName()
                            );

                            // Clear sensitive decrypted data from memory
                            ba = null;
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                        break;

                    case "4": // Lodge

                        Console.WriteLine("Enter Account Number: ");
                        String lodgeAccNo = InputValidator.SanitizeString(Console.ReadLine());

                        if (!InputValidator.IsValidAccountNumber(lodgeAccNo))
                        {
                            Console.WriteLine("Invalid Account Number Format");
                            break;
                        }

                        ba = dal.findBankAccountByAccNo(lodgeAccNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            double amountToLodge = -1;
                            loopCount = 0;

                            do
                            {
                                if (loopCount > 0)
                                    Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN (Must be greater than 0)");

                                Console.WriteLine("Enter Amount To Lodge: ");
                                String amountToLodgeString = Console.ReadLine();

                                try
                                {
                                    amountToLodge = Convert.ToDouble(amountToLodgeString);
                                    if (!InputValidator.IsValidMonetaryAmount(amountToLodge, false))
                                    {
                                        amountToLodge = -1;
                                        loopCount++;
                                    }
                                }
                                catch
                                {
                                    amountToLodge = -1;
                                    loopCount++;
                                }

                            } while (amountToLodge <= 0);

                            // Prompt for reason if amount is over €10,000
                            string reason = null;
                            if (amountToLodge > 10000)
                            {
                                do
                                {
                                    Console.WriteLine("Amount exceeds €10,000. Please enter a valid reason for this transaction (at least 3 characters, must include letters):");
                                    string rawInput = Console.ReadLine();

                                    // Validate: not empty, contains letters, at least 3 characters
                                    if (string.IsNullOrWhiteSpace(rawInput) || rawInput.Length < 3 || !rawInput.Any(char.IsLetter))
                                    {
                                        Console.WriteLine("Invalid reason. It must be at least 3 characters long and contain at least one letter.");
                                        reason = null;
                                    }
                                    else
                                    {
                                        reason = InputValidator.SanitizeString(rawInput);
                                    }

                                } while (reason == null);
                            }

                            dal.lodge(lodgeAccNo, amountToLodge);

                            string decryptedName_Lodge = ba.getDecryptedName();
                            string tellerName_Lodge = Environment.UserName;

                            EventLogger.LogTransaction(
                                TransactionType.Lodgement,
                                tellerName_Lodge,
                                lodgeAccNo,
                                decryptedName_Lodge,
                                amountToLodge,
                                reason
                            );

                            Console.WriteLine($"Successfully lodged €{amountToLodge:F2}");

                            // Memory hygiene
                            ba = null;
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                        break;

                    case "5": // Withdraw

                        Console.WriteLine("Enter Account Number: ");
                        String withdrawAccNo = InputValidator.SanitizeString(Console.ReadLine());

                        if (!InputValidator.IsValidAccountNumber(withdrawAccNo))
                        {
                            Console.WriteLine("Invalid Account Number Format");
                            break;
                        }

                        ba = dal.findBankAccountByAccNo(withdrawAccNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            double amountToWithdraw = -1;
                            loopCount = 0;

                            do
                            {
                                if (loopCount > 0)
                                    Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN (Must be greater than 0)");

                                Console.WriteLine("Enter Amount To Withdraw (€" + ba.getAvailableFunds() + " Available): ");
                                String amountToWithdrawString = Console.ReadLine();

                                try
                                {
                                    amountToWithdraw = Convert.ToDouble(amountToWithdrawString);
                                    if (!InputValidator.IsValidMonetaryAmount(amountToWithdraw, false))
                                    {
                                        amountToWithdraw = -1;
                                        loopCount++;
                                    }
                                }
                                catch
                                {
                                    amountToWithdraw = -1;
                                    loopCount++;
                                }

                            } while (amountToWithdraw <= 0);

                            // Prompt for reason if over €10,000
                            string reason = null;
                            if (amountToWithdraw > 10000)
                            {
                                do
                                {
                                    Console.WriteLine("Amount exceeds €10,000. Please enter a valid reason for this transaction (at least 3 characters, must include letters):");
                                    string rawInput = Console.ReadLine();

                                    if (string.IsNullOrWhiteSpace(rawInput) || rawInput.Length < 3 || !rawInput.Any(char.IsLetter))
                                    {
                                        Console.WriteLine("Invalid reason. It must be at least 3 characters long and contain at least one letter.");
                                        reason = null;
                                    }
                                    else
                                    {
                                        reason = InputValidator.SanitizeString(rawInput);
                                    }

                                } while (reason == null);
                            }

                            bool withdrawalOK = dal.withdraw(withdrawAccNo, amountToWithdraw);

                            if (withdrawalOK == false)
                            {
                                Console.WriteLine("Insufficient Funds Available.");
                            }
                            else
                            {
                                Console.WriteLine($"Successfully withdrew €{amountToWithdraw:F2}");

                                string decryptedName_Withdraw = ba.getDecryptedName();
                                string tellerName_Withdraw = Environment.UserName;

                                EventLogger.LogTransaction(
                                    TransactionType.Withdrawal,
                                    tellerName_Withdraw,
                                    withdrawAccNo,
                                    decryptedName_Withdraw,
                                    amountToWithdraw,
                                    reason
                                );

                                // Clear decrypted and sensitive data from memory
                                ba = null;
                                reason = null;
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                            }
                        }
                        break;

                    case "6":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                        break;
                }


            } while (running != false);

        }

    }
}