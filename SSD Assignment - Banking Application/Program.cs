using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Banking_Application
{
    [SupportedOSPlatform("windows")]
    public class Program
    {
        public static bool HasSufficientDiskSpace(long requiredBytes = 1024 * 1024) // 1MB by default
        {
            try
            {
                string rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
                DriveInfo drive = new DriveInfo(rootPath);
                return drive.AvailableFreeSpace >= requiredBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disk space check failed: " + ex.Message);
                return false;
            }
        }

        public static void Main(string[] args)
        {

            Data_Access_Layer dal = Data_Access_Layer.getInstance();
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
                        if (!HasSufficientDiskSpace())
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

                        String name = "";
                        loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID NAME ENTERED - PLEASE TRY AGAIN (Letters, spaces, apostrophes and hyphens only)");

                            Console.WriteLine("Enter Name: ");
                            string rawName = InputValidator.SanitizeString(Console.ReadLine());
                            name = rawName;

                            loopCount++;
                        } while (!InputValidator.IsValidName(name));

                        name = CryptoHelper.Encrypt(name);

                        String addressLine1 = "";
                        loopCount = 0;
                        string rawAddress1 = "";

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID ADDRESS LINE 1 ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Address Line 1: ");
                            rawAddress1 = InputValidator.SanitizeString(Console.ReadLine());
                            addressLine1 = rawAddress1;

                            loopCount++;
                        } while (!InputValidator.IsValidAddress(addressLine1, true));

                        addressLine1 = CryptoHelper.Encrypt(addressLine1);

                        string addressLine2 = "";
                        Console.WriteLine("Enter Address Line 2: ");
                        string rawAddress2 = InputValidator.SanitizeString(Console.ReadLine());

                        if (!InputValidator.IsValidAddress(rawAddress2, false))
                        {
                            Console.WriteLine("Invalid characters in Address Line 2 - clearing field");
                            addressLine2 = "";
                        }
                        else
                        {
                            addressLine2 = CryptoHelper.Encrypt(rawAddress2);
                        }

                        String addressLine3 = "";
                        Console.WriteLine("Enter Address Line 3: ");
                        string rawAddress3 = InputValidator.SanitizeString(Console.ReadLine());

                        if (!InputValidator.IsValidAddress(rawAddress3, false))
                        {
                            Console.WriteLine("Invalid characters in Address Line 3 - clearing field");
                            addressLine3 = "";
                        }
                        else
                        {
                            addressLine3 = CryptoHelper.Encrypt(rawAddress3);
                        }

                        String town = "";
                        loopCount = 0;
                        string rawTown = "";

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID TOWN ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Town: ");
                            rawTown = InputValidator.SanitizeString(Console.ReadLine());
                            town = rawTown;

                            loopCount++;
                        } while (!InputValidator.IsValidTown(town));

                        town = CryptoHelper.Encrypt(town);

                        double balance = -1;
                        loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID OPENING BALANCE ENTERED - PLEASE TRY AGAIN (Must be 0 or greater)");

                            Console.WriteLine("Enter Opening Balance: ");
                            String balanceString = Console.ReadLine();

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
                                String overdraftAmountString = Console.ReadLine();

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

                            ba = new Current_Account(name, addressLine1, addressLine2, addressLine3, town, balance, overdraftAmount);
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
                                String interestRateString = Console.ReadLine();

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

                            ba = new Savings_Account(name, addressLine1, addressLine2, addressLine3, town, balance, interestRate);
                        }

                        String accNo = dal.addBankAccount(ba);

                        Console.WriteLine("New Account Number Is: " + accNo);

                        string tellerName = Environment.UserName;

                        EventLogger.LogTransaction(
                            TransactionType.AccountCreation,
                            tellerName,
                            accNo,
                            ba.name
                        );

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
                                        tellerName = Environment.UserName; // Placeholder
                                        ba.name = CryptoHelper.Decrypt(ba.name);

                                        EventLogger.LogTransaction(
                                            TransactionType.AccountClosure,
                                            tellerName,
                                            closeAccNo,
                                            ba.name
                                        );

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
                            // Decrypt all PII fields before display
                            ba.name = CryptoHelper.Decrypt(ba.name);
                            ba.address_line_1 = CryptoHelper.Decrypt(ba.address_line_1);
                            ba.address_line_2 = string.IsNullOrEmpty(ba.address_line_2) ? "" : CryptoHelper.Decrypt(ba.address_line_2);
                            ba.address_line_3 = string.IsNullOrEmpty(ba.address_line_3) ? "" : CryptoHelper.Decrypt(ba.address_line_3);
                            ba.town = CryptoHelper.Decrypt(ba.town);

                            Console.WriteLine(ba.ToString());

                            tellerName = Environment.UserName; // Placeholder

                            EventLogger.LogTransaction(
                                TransactionType.BalanceQuery,
                                tellerName,
                                viewAccNo,
                                ba.name
                            );
                        }
                        break;

                    case "4": //Lodge
                        if (!HasSufficientDiskSpace())
                        {
                            Console.WriteLine("There isn't enough space on the computer to lodge money into an account.");
                            break;
                        }

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

                            tellerName = Environment.UserName;
                            ba.name = CryptoHelper.Decrypt(ba.name);

                            EventLogger.LogTransaction(
                                TransactionType.Lodgement,
                                tellerName,
                                lodgeAccNo,
                                ba.name,
                                amountToLodge,
                                reason
                            );

                            Console.WriteLine($"Successfully lodged €{amountToLodge:F2}");
                        }
                        break;

                    case "5": //Withdraw
                        if (!HasSufficientDiskSpace())
                        {
                            Console.WriteLine("There isn't enough space on the computer to withdraw money from an account.");
                            break;
                        }

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
                                tellerName = Environment.UserName; // Placeholder
                                ba.name = CryptoHelper.Decrypt(ba.name);
                                EventLogger.LogTransaction(
                                    TransactionType.Withdrawal,
                                    tellerName,
                                    withdrawAccNo,
                                    ba.name,
                                    amountToWithdraw,
                                    reason
                                );
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