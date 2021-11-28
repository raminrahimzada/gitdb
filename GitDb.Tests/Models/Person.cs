using System;

namespace GitDb.Tests
{
    class Person
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
        public override string ToString()
        {
            return Name + " " + Surname + " " + Age;
        }

        public Person(string id, string name, string surname, int age)
        {
            Id = id;
            Name = name;
            Surname = surname;
            Age = age;
        }

        [Obsolete("Do not use ,reflection only",true)]
        public Person()
        {
            
        }
    }
}