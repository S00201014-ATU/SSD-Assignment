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
                        accountNo TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        address_line_1 TEXT,
                        address_line_2 TEXT,
                        address_line_3 TEXT,
                        town TEXT NOT NULL,
                        balance REAL NOT NULL,
                        accountType INTEGER NOT NULL,
                        overdraftAmount REAL,
                        interestRate REAL
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

                command.CommandText = @"
            INSERT INTO Bank_Accounts 
            (accountNo, name, address_line_1, address_line_2, address_line_3, town, balance, accountType, overdraftAmount, interestRate) 
            VALUES (@accountNo, @name, @address1, @address2, @address3, @town, @balance, @accountType, @overdraft, @interest)";

                command.Parameters.AddWithValue("@accountNo", ba.accountNo);
                command.Parameters.AddWithValue("@name", ba.name);
                command.Parameters.AddWithValue("@address1", ba.address_line_1);
                command.Parameters.AddWithValue("@address2", ba.address_line_2);
                command.Parameters.AddWithValue("@address3", ba.address_line_3);
                command.Parameters.AddWithValue("@town", ba.town);
                command.Parameters.AddWithValue("@balance", ba.balance);
                command.Parameters.AddWithValue("@accountType", ba is Current_Account ? 1 : 2);

                if (ba is Current_Account ca)
                {
                    command.Parameters.AddWithValue("@overdraft", ca.overdraftAmount);
                    command.Parameters.AddWithValue("@interest", DBNull.Value);
                }
                else if (ba is Savings_Account sa)
                {
                    command.Parameters.AddWithValue("@overdraft", DBNull.Value);
                    command.Parameters.AddWithValue("@interest", sa.interestRate);
                }

                command.ExecuteNonQuery();
            }

            return ba.accountNo;
        }

        public Bank_Account findBankAccountByAccNo(string accNo)
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Bank_Accounts WHERE accountNo = @accountNo";
                command.Parameters.AddWithValue("@accountNo", accNo);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int accountType = reader.GetInt16(7);

                        if (accountType == Account_Type.Current_Account)
                        {
                            return new Current_Account
                            {
                                accountNo = reader.GetString(0),
                                name = reader.GetString(1),
                                address_line_1 = reader.GetString(2),
                                address_line_2 = reader.GetString(3),
                                address_line_3 = reader.GetString(4),
                                town = reader.GetString(5),
                                balance = reader.GetDouble(6),
                                overdraftAmount = reader.GetDouble(8)
                            };
                        }
                        else
                        {
                            return new Savings_Account
                            {
                                accountNo = reader.GetString(0),
                                name = reader.GetString(1),
                                address_line_1 = reader.GetString(2),
                                address_line_2 = reader.GetString(3),
                                address_line_3 = reader.GetString(4),
                                town = reader.GetString(5),
                                balance = reader.GetDouble(6),
                                interestRate = reader.GetDouble(9)
                            };
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

                // First check if account exists
                var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = "SELECT COUNT(*) FROM Bank_Accounts WHERE accountNo = @accountNo";
                checkCommand.Parameters.AddWithValue("@accountNo", accNo);

                long count = (long)checkCommand.ExecuteScalar();
                if (count == 0)
                    return false;

                // If exists, delete it
                var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = @accountNo";
                deleteCommand.Parameters.AddWithValue("@accountNo", accNo);
                deleteCommand.ExecuteNonQuery();

                return true;
            }
        }

        public bool lodge(string accNo, double amountToLodge)
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();

                // Retrieve current balance
                var getCommand = connection.CreateCommand();
                getCommand.CommandText = "SELECT balance FROM Bank_Accounts WHERE accountNo = @accountNo";
                getCommand.Parameters.AddWithValue("@accountNo", accNo);

                object result = getCommand.ExecuteScalar();

                if (result == null)
                    return false;

                double currentBalance = Convert.ToDouble(result);
                double newBalance = currentBalance + amountToLodge;

                // Update balance
                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accountNo";
                updateCommand.Parameters.AddWithValue("@balance", newBalance);
                updateCommand.Parameters.AddWithValue("@accountNo", accNo);
                updateCommand.ExecuteNonQuery();

                return true;
            }
        }


        public bool withdraw(string accNo, double amountToWithdraw)
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();

                // Get current balance
                var getCommand = connection.CreateCommand();
                getCommand.CommandText = "SELECT balance FROM Bank_Accounts WHERE accountNo = @accountNo";
                getCommand.Parameters.AddWithValue("@accountNo", accNo);

                object result = getCommand.ExecuteScalar();
                if (result == null)
                    return false;

                double currentBalance = Convert.ToDouble(result);
                if (currentBalance < amountToWithdraw)
                    return false;

                double newBalance = currentBalance - amountToWithdraw;

                // Update balance
                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accountNo";
                updateCommand.Parameters.AddWithValue("@balance", newBalance);
                updateCommand.Parameters.AddWithValue("@accountNo", accNo);
                updateCommand.ExecuteNonQuery();

                return true;
            }
        }

    }
}