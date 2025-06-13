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
            dal.loadBankAccounts();
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
                            name = InputValidator.SanitizeString(Console.ReadLine());

                            loopCount++;

                        } while (!InputValidator.IsValidName(name));

                        String addressLine1 = "";
                        loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID ADDRESS LINE 1 ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Address Line 1: ");
                            addressLine1 = InputValidator.SanitizeString(Console.ReadLine());

                            loopCount++;

                        } while (!InputValidator.IsValidAddress(addressLine1, true));

                        String addressLine2 = "";
                        Console.WriteLine("Enter Address Line 2: ");
                        addressLine2 = InputValidator.SanitizeString(Console.ReadLine());

                        // Validate optional address line 2
                        if (!InputValidator.IsValidAddress(addressLine2, false))
                        {
                            Console.WriteLine("Invalid characters in Address Line 2 - clearing field");
                            addressLine2 = "";
                        }

                        String addressLine3 = "";
                        Console.WriteLine("Enter Address Line 3: ");
                        addressLine3 = InputValidator.SanitizeString(Console.ReadLine());

                        // Validate optional address line 3
                        if (!InputValidator.IsValidAddress(addressLine3, false))
                        {
                            Console.WriteLine("Invalid characters in Address Line 3 - clearing field");
                            addressLine3 = "";
                        }

                        String town = "";
                        loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID TOWN ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Town: ");
                            town = InputValidator.SanitizeString(Console.ReadLine());

                            loopCount++;

                        } while (!InputValidator.IsValidTown(town));

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
                                    balance = -1; // Force retry
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
                                        overdraftAmount = -1; // Force retry
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
                                        interestRate = -1; // Force retry
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

                        string tellerName = Environment.UserName; // Placeholder for now

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

                            dal.lodge(lodgeAccNo, amountToLodge);
                            tellerName = Environment.UserName;
                            string reason = amountToLodge > 10000 ? "Large lodgement above €10,000" : null;

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

                            bool withdrawalOK = dal.withdraw(withdrawAccNo, amountToWithdraw);

                            if (withdrawalOK == false)
                            {
                                Console.WriteLine("Insufficient Funds Available.");
                            }
                            else
                            {
                                Console.WriteLine($"Successfully withdrew €{amountToWithdraw:F2}");
                                tellerName = Environment.UserName; // Placeholder
                                string reason = amountToWithdraw > 10000 ? "Withdrawal above €10,000" : null;

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