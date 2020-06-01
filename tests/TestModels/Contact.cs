using KekeDataStore.Binary;
using System;

namespace TestModels
{
    [Serializable]
    public class Contact : IBaseEntity
    {
        public Guid Id { get; set; }
        public Person Person { get; set; }
        public Phone Phone { get; set; }

        public override string ToString() => $"{Id} | {Person.FirstName} {Person.LastName} | {Phone.PhoneNumber} {Phone.Type}";
    }
}
