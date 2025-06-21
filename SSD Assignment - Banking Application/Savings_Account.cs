using System;

namespace Banking_Application
{
    public class Savings_Account : Bank_Account
    {
        public double interestRate;

        public Savings_Account() : base()
        {
        }

        public Savings_Account(string name, string address_line_1, string address_line_2, string address_line_3, string town, double balance, double interestRate)
            : base(name, address_line_1, address_line_2, address_line_3, town, balance)
        {
            this.interestRate = interestRate;
        }

        // Constructor for loading from database
        public Savings_Account(string accountNo, string name, string address_line_1, string address_line_2, string address_line_3, string town, double balance, double interestRate)
            : base(accountNo, name, address_line_1, address_line_2, address_line_3, town, balance)
        {
            this.interestRate = interestRate;
        }

        public override double getAvailableFunds()
        {
            return Balance;
        }

        public override bool withdraw(double amountToWithdraw)
        {
            double avFunds = getAvailableFunds();

            if (avFunds >= amountToWithdraw)
            {
                lodge(-amountToWithdraw);  // Decrease balance by using lodge(-X)
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return base.ToString() +
                   "Account Type: Savings Account\n" +
                   "Interest Rate: " + interestRate.ToString("F2") + "%\n";
        }
    }
}
