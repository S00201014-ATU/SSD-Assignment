using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Banking_Application
{
    public class Data_Access_Layer
    {

        public static String databaseName = "Banking Database.db";
        private static Data_Access_Layer instance = new Data_Access_Layer();

        private Data_Access_Layer()//Singleton Design Pattern (For Concurrency Control) - Use getInstance() Method Instead.
        {
            
        }

        public static Data_Access_Layer getInstance()
        {
            return instance;
        }

        private SqliteConnection getDatabaseConnection()
        {

            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Data_Access_Layer.databaseName,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            return new SqliteConnection(databaseConnectionString);

        }

        private void initialiseDatabase()
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Bank_Accounts(    
                        accountNoHMAC TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        address_line_1 TEXT,
                        address_line_2 TEXT,
                        address_line_3 TEXT,
                        town TEXT NOT NULL,
                        balance TEXT NOT NULL,
                        accountType TEXT NOT NULL,
                        overdraftAmount TEXT,
                        interestRate TEXT
                    ) WITHOUT ROWID
                ";

                command.ExecuteNonQuery();
                
            }
        }

        public string addBankAccount(Bank_Account ba)
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();

                string accountNoHMAC = CryptoHelper.CalculateHMAC(ba.AccountNo);

                command.CommandText = @"
INSERT INTO Bank_Accounts 
(accountNoHMAC, name, address_line_1, address_line_2, address_line_3, town, balance, accountType, overdraftAmount, interestRate) 
VALUES (@accountNoHMAC, @name, @address1, @address2, @address3, @town, @balance, @accountType, @overdraft, @interest)";

                command.Parameters.AddWithValue("@accountNoHMAC", accountNoHMAC);
                command.Parameters.AddWithValue("@name", CryptoHelper.Encrypt(ba.Name));
                command.Parameters.AddWithValue("@address1", CryptoHelper.Encrypt(ba.AddressLine1));
                command.Parameters.AddWithValue("@address2", CryptoHelper.Encrypt(ba.AddressLine2));
                command.Parameters.AddWithValue("@address3", CryptoHelper.Encrypt(ba.AddressLine3));
                command.Parameters.AddWithValue("@town", CryptoHelper.Encrypt(ba.Town));
                command.Parameters.AddWithValue("@balance", CryptoHelper.Encrypt(ba.Balance.ToString("F2")));
                int acctType = (ba is Current_Account ? 1 : 2);
                command.Parameters.AddWithValue("@accountType", CryptoHelper.Encrypt(acctType.ToString()));

                if (ba is Current_Account ca)
                {
                    command.Parameters.AddWithValue("@overdraft", CryptoHelper.Encrypt(ca.overdraftAmount.ToString("F2")));
                    command.Parameters.AddWithValue("@interest", DBNull.Value);
                }
                else if (ba is Savings_Account sa)
                {
                    command.Parameters.AddWithValue("@overdraft", DBNull.Value);
                    command.Parameters.AddWithValue("@interest", CryptoHelper.Encrypt(sa.interestRate.ToString("F2")));
                }

                command.ExecuteNonQuery();
            }

            return ba.AccountNo;
        }

        public Bank_Account findBankAccountByAccNo(string accNo)
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                string accountNoHMAC = CryptoHelper.CalculateHMAC(accNo);

                command.CommandText = "SELECT * FROM Bank_Accounts WHERE accountNoHMAC = @accountNoHMAC";
                command.Parameters.AddWithValue("@accountNoHMAC", accountNoHMAC);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int accountType = Convert.ToInt32(CryptoHelper.Decrypt(reader.GetString(7)));

                        if (accountType == Account_Type.Current_Account)
                        {
                            return new Current_Account(
                                accNo,  // FIX: pass original accountNo, not reader.GetString(0)
                                CryptoHelper.Decrypt(reader.GetString(1)),  // name
                                CryptoHelper.Decrypt(reader.GetString(2)),  // address_line_1
                                CryptoHelper.Decrypt(reader.GetString(3)),  // address_line_2
                                CryptoHelper.Decrypt(reader.GetString(4)),  // address_line_3
                                CryptoHelper.Decrypt(reader.GetString(5)),  // town
                                Convert.ToDouble(CryptoHelper.Decrypt(reader.GetString(6))),  // balance
                                Convert.ToDouble(CryptoHelper.Decrypt(reader.GetString(8)))   // overdraftAmount
                            );
                        }
                        else
                        {
                            return new Savings_Account(
                                accNo,  // FIX: pass original accountNo
                                CryptoHelper.Decrypt(reader.GetString(1)),  // name
                                CryptoHelper.Decrypt(reader.GetString(2)),  // address_line_1
                                CryptoHelper.Decrypt(reader.GetString(3)),  // address_line_2
                                CryptoHelper.Decrypt(reader.GetString(4)),  // address_line_3
                                CryptoHelper.Decrypt(reader.GetString(5)),  // town
                                Convert.ToDouble(CryptoHelper.Decrypt(reader.GetString(6))),  // balance
                                Convert.ToDouble(CryptoHelper.Decrypt(reader.GetString(9)))   // interestRate
                            );
                        }
                    }
                }
            }

            return null;
        }

        public bool closeBankAccount(string accNo)
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();

                string accountNoHMAC = CryptoHelper.CalculateHMAC(accNo);

                // First check if account exists
                var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = "SELECT COUNT(*) FROM Bank_Accounts WHERE accountNoHMAC = @accountNoHMAC";
                checkCommand.Parameters.AddWithValue("@accountNoHMAC", accountNoHMAC);
                long count = (long)checkCommand.ExecuteScalar();

                if (count == 0)
                    return false;

                // If exists, delete it
                var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM Bank_Accounts WHERE accountNoHMAC = @accountNoHMAC";
                deleteCommand.Parameters.AddWithValue("@accountNoHMAC", accountNoHMAC);
                deleteCommand.ExecuteNonQuery();

                return true;
            }
        }

        public bool lodge(string accNo, double amountToLodge)
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();

                string accountNoHMAC = CryptoHelper.CalculateHMAC(accNo);

                // Retrieve current balance
                var getCommand = connection.CreateCommand();
                getCommand.CommandText = "SELECT balance FROM Bank_Accounts WHERE accountNoHMAC = @accountNoHMAC";
                getCommand.Parameters.AddWithValue("@accountNoHMAC", accountNoHMAC);

                object result = getCommand.ExecuteScalar();

                if (result == null)
                    return false;

                double currentBalance = Convert.ToDouble(CryptoHelper.Decrypt(result.ToString()));
                double newBalance = currentBalance + amountToLodge;

                // Update balance
                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNoHMAC = @accountNoHMAC";
                updateCommand.Parameters.AddWithValue("@balance", CryptoHelper.Encrypt(newBalance.ToString("F2")));
                updateCommand.Parameters.AddWithValue("@accountNoHMAC", accountNoHMAC);
                updateCommand.ExecuteNonQuery();

                return true;
            }
        }


        public bool withdraw(string accNo, double amountToWithdraw)
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();

                string accountNoHMAC = CryptoHelper.CalculateHMAC(accNo);

                // Get current balance
                var getCommand = connection.CreateCommand();
                getCommand.CommandText = "SELECT balance FROM Bank_Accounts WHERE accountNoHMAC = @accountNoHMAC";
                getCommand.Parameters.AddWithValue("@accountNoHMAC", accountNoHMAC);

                object result = getCommand.ExecuteScalar();
                if (result == null)
                    return false;

                double currentBalance = Convert.ToDouble(CryptoHelper.Decrypt(result.ToString()));
                if (currentBalance < amountToWithdraw)
                    return false;

                double newBalance = currentBalance - amountToWithdraw;

                // Update balance
                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNoHMAC = @accountNoHMAC";
                updateCommand.Parameters.AddWithValue("@balance", CryptoHelper.Encrypt(newBalance.ToString("F2")));
                updateCommand.Parameters.AddWithValue("@accountNoHMAC", accountNoHMAC);
                updateCommand.ExecuteNonQuery();

                return true;
            }
        }

    }
}