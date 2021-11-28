using System;

namespace GitDb
{
    public class ChangeHistory<T>
    {
        public override string ToString()
        {
            if (IsUpdated)
            {
                return "updated to "+Value.ToString();
            }
            if (IsDeleted)
            {
                return "deleted";
            }

            if (IsInserted)
            {
                return "inserted "+Value.ToString();
            }

            return string.Empty;
        }

        public DateTimeOffset When { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsUpdated { get; set; }
        public bool IsInserted { get; set; }
        public T Value { get; set; }
        public string Message { get; set; }
        public string User { get; set; }

        public ChangeHistory<T> Deleted()
        {
            IsDeleted = true;
            IsUpdated = false;
            IsInserted = false;
            return this;
        }
        public ChangeHistory<T> Inserted(T t)
        {
            IsDeleted = false;
            IsUpdated = false;
            IsInserted = true;
            Value = t;
            return this;
        } 
        
        public ChangeHistory<T> Updated(T t)
        {
            IsDeleted = false;
            IsUpdated = true;
            IsInserted = false;
            Value = t;
            return this;
        }

        public ChangeHistory<T> SetValue(T t)
        {
            Value = t;
            return this;
        }

        public ChangeHistory<T> SetMessage(string message)
        {
            Message = message;
            return this;
        }

        public ChangeHistory<T> SetUser(string user)
        {
            User = user;
            return this;
        }

        public ChangeHistory<T> SetWhen(DateTimeOffset @when)
        {
            When = @when;
            return this;
        }
    }
}