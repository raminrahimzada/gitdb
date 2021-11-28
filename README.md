# GitDb
File database based on Git backend written in C#

# Supports:
- CRUD
- Entire change(including delete) history
- Abilty to undo any operation (including undo operation)

# How it works
GitDb creates a folder for each collection and a file for each entity<br/>
When you do CRUD operations it is changing file/folder and making brand new commit

# Example
```cs
var _db = new GitDatabase("C:\GitDb");


var id = Guid.NewGuid().ToString();
var person = new Person(id, name:"Nikola", surname:"Tesla", age:86);


//========= inserting
_db.Insert(id, person);


//========= updating
var operationUpdate = _db.Update(id, person);


//========= deleting
var operationDelete = _db.Delete<Person>(id);


//========= undoing delete
var revertDelete = _db.Revert(operation);


//========= undoing update
var revertUpdate = _db.Revert(operationUpdate);


//========= get entity history
IEnumerable<ChangeHistory<Person>> history = _db.GetChanges<Person>(id);

```

# Note:
This is not a production ready database, Since its speed is not very efficient (50-100 operation/second in my laptop ssd) and causes a lot of IO operation<br/>
You can use it for less changing but critical cases such as app-config changes or database schema changes-such as source view,stored procedure and function structures </br>
Latest case is my case- I am currently working on it using this library

