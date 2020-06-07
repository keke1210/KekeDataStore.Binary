using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using TestModels;

namespace KekeDataStore.Binary.Test
{
    [TestFixture]
    public class BinaryDataStore_Tests
    {
        private readonly string _dataPath = @"C:\Users\Keke\Desktop\MyNuGet\KekeDataStore\tests\KekeDataStore.Binary.Test\Database";
        private readonly string _visualFilePath = @"C:\Users\Keke\Desktop\MyNuGet\KekeDataStore\tests\KekeDataStore.Binary.Test\OutputFiles\Contacts_Test.txt";
        private readonly IBinaryDataStore<Contact> _dataStore;

        public BinaryDataStore_Tests()
        {
            _dataStore = new BinaryDataStore<Contact>(_dataPath, "BinaryDataStore_Tests_Db");
        }

        [Test]
        public void GetAll_Test()
        {
            var contacts = _dataStore.GetAll();

            // Write data to txt file on the specified path to see the data visually
            contacts.VisualizeToFile(_visualFilePath);

            Assert.AreEqual(contacts.Count(), _dataStore.Count);
        }

        [Test]
        public void Get_Test()
        {
            var contacts = _dataStore.Get(x => x.Person.FirstName == "Skerdi");

            Assert.IsNotNull(contacts);
        }

        [Test]
        public void GetSingle_Test()
        {
            var contact = _dataStore.GetSingle(x => x.Id == new Guid("bba9dcb0-563e-42b6-a2ff-6554ebba87f2"));

            Assert.IsNotNull(contact);
        }

        [Test]
        public void Create_WithSpecifiedId_Test()
        {
            // Contact with Id
            var newContact = new Contact
            {
                Id = Guid.NewGuid(),
                Person = new Person { FirstName = "Test1", LastName = "Test1" },
                Phone = new Phone { PhoneNumber = "0684594960", Type = PhoneType.CELLPHONE }
            };

            var createdContact = _dataStore.Create(newContact);
            var changesSaved = _dataStore.SaveChanges();

            Assert.IsNotNull(createdContact);
            Assert.IsTrue(changesSaved);
        }

        /// <summary>
        /// Pass the model without a specified Id(Guid)
        /// </summary>
        [Test]
        public void Create_WithoutSpecifiedId_Test()
        {
            // Contact without Id
            var newContact = new Contact {
                Person = new Person { FirstName = "Test1", LastName = "Test1" },
                Phone = new Phone { PhoneNumber = "0684594960", Type = PhoneType.CELLPHONE }
            };

            var createdContact =  _dataStore.Create(newContact);
            var changesSaved =  _dataStore.SaveChanges();

            Assert.IsNotNull(createdContact);
            Assert.IsTrue(changesSaved);
        }


        [Test]
        [TestCase("4b6d70f9-3f8d-477f-a870-55f8dd11980d")]
        public void Delete_Contact_Test(string id)
        {
            var createdContact = _dataStore.Delete(id);
            Assert.IsTrue(createdContact);

            var getContactById = _dataStore.GetById(id);
            Assert.IsNull(getContactById);
        }

        [Test]
        public void Update_Contact_Test()
        {
            string id = Guid.NewGuid().ToString();

            var newItemTest = new Contact
            {
                Id = new Guid(id),
                Person = new Person { FirstName = "SkerdiNotUpdated", LastName = "BerberiNotUpdated" },
                Phone = new Phone { PhoneNumber = "11", Type = PhoneType.HOME }
            };
            // Create new eleemnt with the specified Id
            _dataStore.Create(newItemTest);

            var updateItem = new Contact
            {
                Person = new Person { FirstName = "FirstNameUpdated", LastName = "LastNameUpdated" },
                Phone = new Phone { PhoneNumber = "11", Type = PhoneType.HOME }
            };

            var updatedContact = _dataStore.Update(id, updateItem);
            Assert.IsNotNull(updatedContact);
            Assert.IsNotNull("FirstNameUpdated", updatedContact.Person.FirstName);
            Assert.IsNotNull("LastNameUpdated", updatedContact.Person.LastName);

            var getContactAfterUpdate = _dataStore.GetById(id);
            Assert.IsNotNull(getContactAfterUpdate);

            Assert.AreNotEqual(newItemTest, getContactAfterUpdate);
        }


        [Test]
        [TestCase("bba9dcb0-563e-42b6-a2ff-6554ebba87f2")]
        public void GetById_Test(string Id)
        {
            var contact = _dataStore.GetById(Id);

            Assert.IsNotNull(contact);
            Assert.AreEqual(contact.Id.ToString(), Id);
        }

        [Test]
        public void GetByFirstName()
        {
            var contacts = _dataStore.Get(x=>x.Person.FirstName == "Test1");

            var firstEl = contacts.FirstOrDefault();
            Assert.IsNotNull(firstEl);

            Assert.AreEqual(firstEl.Person.FirstName, "Test1");
        }

        [Test]
        public void OrderBy_Test()
        {
            var myContactsOrder = _dataStore.AsQueryable().OrderBy("Person.FirstName").ThenBy("Person.LastName");
            var linqContactsOrder = _dataStore.GetAll().OrderBy(x=>x.Person.FirstName).ThenBy(x=>x.Person.LastName);

            Assert.IsNotNull(myContactsOrder);
            Assert.IsNotNull(linqContactsOrder);
            Assert.AreEqual(myContactsOrder, linqContactsOrder);
        }

        [Test]
        public void OrderByDescending_Test()
        {
            var myContactsOrder = _dataStore.AsQueryable().OrderByDescending("Person.FirstName").ThenBy("Person.LastName");
            var linqContactsOrder = _dataStore.GetAll().OrderByDescending(x => x.Person.FirstName).ThenBy(x => x.Person.LastName);

            Assert.IsNotNull(myContactsOrder);
            Assert.IsNotNull(linqContactsOrder);
            Assert.AreEqual(myContactsOrder, linqContactsOrder);
        }

        //[Test]
        //public void Truncate_Test()
        //{
        //    var truncated = _dataStore.Truncate();
        //    Assert.IsTrue(truncated);
        //    Assert.AreEqual(0, _dataStore.Count);
        //}

       
    }
}