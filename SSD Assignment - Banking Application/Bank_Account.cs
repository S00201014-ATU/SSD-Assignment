using System;

namespace Banking_Application
{
    public abstract class Bank_Account
    {
        private string accountNo;
        private string name;
        private string address_line_1;
        private string address_line_2;
        private string address_line_3;
        private string town;
        private double balance;

        public string AccountNo => accountNo;
        public string Name => name;
        public string AddressLine1 => address_line_1;
        public string AddressLine2 => address_line_2;
        public string AddressLine3 => address_line_3;
        public string Town => town;
        public double Balance => balance;

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

        protected Bank_Account(string accountNo, string name, string address1, string address2, string address3, string town, double balance)
        {
            this.accountNo = accountNo;
            this.name = name;
            this.address_line_1 = address1;
            this.address_line_2 = address2;
            this.address_line_3 = address3;
            this.town = town;
            this.balance = balance;
        }

        public void lodge(double amountIn)
        {
            balance += amountIn;
        }

        public abstract bool withdraw(double amountToWithdraw);
        public abstract double getAvailableFunds();

        // NEW decrypted getters
        public string getDecryptedName()
        {
            return CryptoHelper.Decrypt(name);
        }

        public string getDecryptedAddressLine1()
        {
            return CryptoHelper.Decrypt(address_line_1);
        }

        public string getDecryptedAddressLine2()
        {
            return CryptoHelper.Decrypt(address_line_2);
        }

        public string getDecryptedAddressLine3()
        {
            return CryptoHelper.Decrypt(address_line_3);
        }

        public string getDecryptedTown()
        {
            return CryptoHelper.Decrypt(town);
        }

        public override string ToString()
        {
            return "\nAccount No: " + accountNo + "\n" +
                   "Name: " + getDecryptedName() + "\n" +
                   "Address Line 1: " + getDecryptedAddressLine1() + "\n" +
                   "Address Line 2: " + getDecryptedAddressLine2() + "\n" +
                   "Address Line 3: " + getDecryptedAddressLine3() + "\n" +
                   "Town: " + getDecryptedTown() + "\n" +
                   "Balance: €" + balance.ToString("F2") + "\n";
        }

    }
}
