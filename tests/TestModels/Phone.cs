using System;

namespace TestModels
{
    [Serializable]
    public enum PhoneType : byte
    {
        WORK,
        CELLPHONE,
        HOME
    }

    [Serializable]
    public class Phone
    {
        public string PhoneNumber { get; set; }
        public PhoneType Type { get; set; }
    }
}
