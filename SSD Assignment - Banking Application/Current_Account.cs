using System;

namespace Banking_Application
{
    public class Current_Account : Bank_Account
    {
        public double overdraftAmount;

        public Current_Account() : base()
        {
        }

        public Current_Account(string name, string address_line_1, string address_line_2, string address_line_3, string town, double balance, double overdraftAmount)
            : base(name, address_line_1, address_line_2, address_line_3, town, balance)
        {
            this.overdraftAmount = overdraftAmount;
        }

        // Constructor for loading from database
        public Current_Account(string accountNo, string name, string address_line_1, string address_line_2, string address_line_3, string town, double balance, double overdraftAmount)
            : base(accountNo, name, address_line_1, address_line_2, address_line_3, town, balance)
        {
            this.overdraftAmount = overdraftAmount;
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

        public override double getAvailableFunds()
        {
            return Balance + overdraftAmount;
        }

        public override string ToString()
        {
            return base.ToString() +
                   "Account Type: Current Account\n" +
                   "Overdraft Amount: €" + overdraftAmount.ToString("F2") + "\n";
        }
    }
}
