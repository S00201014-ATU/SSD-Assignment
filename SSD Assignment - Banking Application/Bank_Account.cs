using System;

namespace Banking_Application
{
    public abstract class Bank_Account
    {
        public string accountNo;
        public string name;
        public string address_line_1;
        public string address_line_2;
        public string address_line_3;
        public string town;
        public double balance;

        public Bank_Account()
        {
           
        }

        public Bank_Account(string name, string address_line_1, string address_line_2, string address_line_3, string town, double balance)
        {
            this.accountNo = Guid.NewGuid().ToString();
            this.name = name;
            this.address_line_1 = address_line_1;
            this.address_line_2 = address_line_2;
            this.address_line_3 = address_line_3;
            this.town = town;
            this.balance = balance;
        }

        public void lodge(double amountIn)
        {
            balance += amountIn;
        }

        public abstract bool withdraw(double amountToWithdraw);
        public abstract double getAvailableFunds();

        public override string ToString()
        {
            return "\nAccount No: " + accountNo + "\n" +
                   "Name: " + name + "\n" +
                   "Address Line 1: " + address_line_1 + "\n" +
                   "Address Line 2: " + address_line_2 + "\n" +
                   "Address Line 3: " + address_line_3 + "\n" +
                   "Town: " + town + "\n" +
                   "Balance: €" + balance.ToString("F2") + "\n";
        }
    }
}
