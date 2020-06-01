using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using TestModels;

namespace KekeDataStore.Binary.Test
{
    [TestFixture]
    public class ThreadSafety_Tests
    {
        private readonly string _dataPath = @"C:\Users\Keke\Desktop\MyNuGet\KekeDataStore\tests\KekeDataStore.Binary.Test\Database";

        private readonly IBinaryDataStore<Contact> _dataStore;

        public ThreadSafety_Tests()
        {
            _dataStore = new BinaryDataStore<Contact>(_dataPath, "ThreadSafety_Tests_Db");
        }

        // Although we can't really test thread safety in depth, this is a simple way to test that locks work
        [Test]
        public void Test_Threadsafety()
        { 
            _dataStore.Truncate();

            var t1 = new Thread(Add_5000_Rows_To_File);
            var t2 = new Thread(Add_5000_Rows_To_File);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            Assert.AreEqual(10000, _dataStore.Count);

            _dataStore.Truncate();
            _dataStore.SaveChanges();
            Assert.AreEqual(0, _dataStore.Count);
        }

        [Test]
        public void Test_Threadsafety_WithParallelLoops()
        {
            _dataStore.Truncate();

            var t1 = new Thread(Add_500_Rows_Parallel_To_File);
            var t2 = new Thread(Add_500_Rows_Parallel_To_File);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            Assert.AreEqual(1000, _dataStore.Count);

            _dataStore.Truncate();
            _dataStore.SaveChanges();
            Assert.AreEqual(0, _dataStore.Count);
        }

        private void Add_5000_Rows_To_File()
        {
            for (int i = 0; i < 5000; i++)
            {
                _dataStore.Create(new Contact
                {
                    Person = new Person { FirstName ="Skerdi", LastName = "Berberi" },
                    Phone = new Phone { PhoneNumber ="11", Type = PhoneType.WORK }
                });
            }

            _dataStore.SaveChanges();
        }

        private void Add_500_Rows_Parallel_To_File()
        {
            Parallel.For(0, 500, (i) => 
            {
                _dataStore.Create(new Contact
                {
                    Person = new Person { FirstName = "Skerdi", LastName = "Berberi" },
                    Phone = new Phone { PhoneNumber = "11", Type = PhoneType.WORK }
                });
            });

            _dataStore.SaveChanges();
        }


    }
}
