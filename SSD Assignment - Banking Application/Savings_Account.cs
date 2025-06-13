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

        public override double getAvailableFunds()
        {
            return balance;
        }

        public override bool withdraw(double amountToWithdraw)
        {
            double avFunds = getAvailableFunds();

            if (avFunds >= amountToWithdraw)
            {
                balance -= amountToWithdraw;
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
