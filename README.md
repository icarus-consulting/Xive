[![Build status](https://ci.appveyor.com/api/projects/status/9nf2jot9hyy4jphb/branch/master?svg=true)](https://ci.appveyor.com/project/icarus-consulting/xive/branch/master)
[![codecov](https://codecov.io/gh/icarus-consulting/Xive/branch/master/graph/badge.svg)](https://codecov.io/gh/icarus-consulting/Xive)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com)
[![Commitizen friendly](https://img.shields.io/badge/commitizen-friendly-brightgreen.svg?style=flat-square)](http://commitizen.github.io/cz-cli/)
[![EO principles respected here](http://www.elegantobjects.org/badge.svg)](http://www.elegantobjects.org)

# Xive
A true Object Oriented data storage for .NET

Xive is designed to solve everyday persistence tasks with a best-practice approach that covers many common requirements of software projects.

It follows all the rules suggested in the two "[Elegant Objects](https://www.amazon.de/Elegant-Objects-Yegor-Bugayenko/dp/1519166915)" books.

Some of them:
- [no null](http://www.yegor256.com/2014/05/13/why-null-is-bad.html),
- [no getters or setters](http://www.yegor256.com/2014/09/16/getters-and-setters-are-evil.html)
- [no code execution in ctors](http://www.yegor256.com/2015/05/07/ctors-must-be-code-free.html)
- [no mutable objects](http://www.yegor256.com/2014/06/09/objects-should-be-immutable.html),
- [no static methods](http://www.yegor256.com/2014/05/05/oop-alternative-to-utility-classes.html),
- [no type casting](http://www.yegor256.com/2015/04/02/class-casting-is-anti-pattern.html),
- [implementation inheritance](http://www.yegor256.com/2016/09/13/inheritance-is-procedural.html),
- [no DTO](http://www.yegor256.com/2016/07/06/data-transfer-object.html),
- Four member variables maximum
- Five public methods maximum
- Strict method naming
- Every type is an interface
- No code execution in constructors
- No class inheritance, except for design pattern "Envelopes"
- [and more](http://www.yegor256.com/2014/09/10/anti-patterns-in-oop.html)

## What Xive can do
- Save data in files
- Save data in memory
- Simplify xml reading and writing
- Provide threadsafe access to files
- Simplify your entity-unit testing


## What Xive cannot do
- Replace a relational database

  

## Designing Apps using Xive

Xive helps you to practice a design which uses Elegant Objects. One of the main purposes of EO is to have small and simple classes. It also propagates to get rid of DTO and instead encapsulates its data. You do not build DTO's and therefore use ORM and "Manager" classes, you build smart [Objects which read and store data but not "leak" it](https://www.yegor256.com/2014/12/01/orm-offensive-anti-pattern.html).

So instead of having something like
```csharp
class MyClass
{
    void AddReminder(ReminderDTO data);
    TodoDTO GetTodo(string dataId);
    AddTodo(TodoDTO todo);
    TodoExists(string todoId);
    ...//100 more to come
}

class ReminderDTO
{
	string ID;
	DateTime Time;
	string description;
}
```

You build it like this:

```csharp
//Data root
var xive = new FileXive("c://my-organizer-app");

//Create todo using a "Smart class". The following line stores a new todo in the data root completely inside itself.
new Todos(xive).Add("work", "Get new laptop", DateTime.Now + new TimeSpan(1,0,0));

//Create an appointment using a "XML speaking" "Smart class", same as above.
var appointments = new Appointments(xive);
appointments.Add("private", "Lunch with Bob", "2019-10-15");

//Iterate through existing appointments
foreach(var apt in appointments.Between(DateTime.Now, DateTime.Now + new TimeSpan(14,0,0)))
{
    Console.WriteLine(appointments.Title(apt));
	//Add subscribers using another "XML speaking" "Smart class"
    new Subscribers(xive, apt).Add("bob@internet.org");
}

//Unit testing made simple by Xive:
public TestMethod()
{
    //for testing, use the RamHive which exists only in memory.
    var memory = new RamHive();
    Assert.Equal(new Todos(memory).ExpectedStuff());
}
```

Key points:
- The full storage is injected into the smart classes as interface IXive
- Small classes represent specific behaviour of your entity (entity here: appointment)
- Simple unit testing possible by using a in-memory xive/comb/cell


# Design Overview

Xive organzies data in so called hives. These Hives consist have three logical layers. 

## Cell
Lets start with the smallest, a cell.
A cell is some data which you can read or write.


### Example for an application whose complexity is a fit for a cell:

```
TodoApp
'- todos.xml (One Single Cell)
```

### Usage

```csharp
//A cell which lives in memory:
ICell cell = new RamCell();

//A cell which lives permanently in the filesystem:
ICell cell = new FileCell("C://temp/my-file.txt");

//Update data:

var data = new InputOf("A am a content");
cell.Update(data);

//Read data:
byte[] data = cell.Content();
```

Note: InputOf is part of [Yaapii.Atoms](https://github.com/icarus-consulting/Yaapii.Atoms), a port of cactoos library which provides true object oriented classes.

There is no need to create a file or test if the cell exists - just use it, Xive takes care of the rest. If it is not there yet, it will be created. If you write a content of zero to it, it will vanish automatically

## HoneyComb

A honeycomb is a bunch of data, it provides you multiple cells. If one file is not enough for your application, you should consider using a HoneyComb.

### Example for an application whose complexity is a fit for a honeycomb:

```
TodoApp
'- Category "Work" (A honeycomb)
|  '- title.jpg (A title picture for Work)
|  '- todos.xml
'- Category "Home" (Another honeycomb)
   '- title.jpg (A title picture for Home)
   '- todos.xml
```

### Usage

```csharp

//A honeycomb which lives in memory:
IHoneyComb comb = new RamComb();

//A honeycomb which lives persistent in the filesystem:
IHoneyComb comb = new FileComb("C:/temp/todoapp");

//Store properties in the comb
workTodos.Props()
        .Refined("has-larva", "true")
        .Refined("created", DateTime.Now.ToString());

var hasLarva = workTodos.Props().Value("has-larva");

//Get access to raw cell data
var titlePicture = comb.Cell("title.jpg")

//Get simple access to XML
var todos = comb.Xocument("todos.xml");

```

Note: A Xocument is a simple way to:
- Read data using [xpath](https://en.wikipedia.org/wiki/XPath)
- Update data using [Xambly](https://github.com/icarus-consulting/Yaapii.Xambly)

## Hive

A hive is the next steo in complexity. A hive can:
- Contain multiple combs
- Keep track of which combs exist by using a internal xml document "catalog.xml" as its index
- Be "shifted" to another scope - for example from the scope "todo" to the scope "appointments"



### Example for an application whose complexity is a fit for a hive 
(which of course might or might not make sense, depending on how you design your application)

```

OrganizerApp
|-"cockpit" (A big app needs control informations - we put that in the "cockpit")
|  |
|  |- "access" (area holding role based restrictions)
|  |  '- roles.xml (cell which user restrictions inside)
|  '- "sources" (area with )
|      '-external.xml
|
|-"appointments"
|  |
|  |-"HQ"
|  | '- catalog.xml (An index of which years exist)
|  |
|  |-"work"
|  |
|  | '- 2019.xml
|  | '- 2018.xml
|  | '- 1999.xml
|  |
|  '-"private"
|     '- 2019.xml
|
'-"todos"
  |
  '- "work" (A honeycomb)
  |
  |  '- title.jpg (A title picture for Work)
  |  '- todos.xml
  |
  '- "private" (Another honeycomb)
     |
     '- title.jpg (A title picture for Home)
     '- todos.xml
```

### Usage 

```csharp
// A hive which exists in memory
var hive = new RamHive();

// A hive which exists physically as files
var hive = new FileHive("C://organizer-app");

//Get to a specific hive scope
IHive appointments = hive.Shifted("Appointments");
```

### How does the hive know which combs it has?
The hive internally uses itself to write this into a file "catalog.xml". 

```xml
<catalog>
    <todos id="work"></todos>
    <todos id="private"></todos>
</catalog>

```
For easy use, this can be done with the class Catalog:

```csharp
var todosHive = new RamHive().Shifted("todos");

//1. Create a comb, before you can query it
todosHive.Catalog().Add("work");
todosHive.Catalog().Create("private");

//2. Get the comb by id:
IHoneyComb workTodos = hive.Comb("work");

//3. Store properties in the comb:
workTodos.Props()
        .Refined("has-larva", "true")
        .Refined("created", DateTime.Now.ToString())

//4. Work with an XML inside the comb:
var xoc = workTodos.Xocument("example.xml");

//4. Work with bytes inside the comb:
var xoc = workTodos.Cell("bytes.dat");

```

# Mnemonic
This is the memory of the xive. It is used in every layer (cell, comb, hive).
The memory contains xmls, cells, properties and if needed the indexcatalog of the hive.

Mnemonic can be in files, or in ram.
Mnemonic can be cached (with options to max size of cached cells and a blacklist for items not to cache)



Please note that a mnemonic by default does not do any caching.

So if you use *RamMnemonic* or *FileMnemonic*, the data is always read when you access it.

### Why a cache for Ram?

You might ask yourself why a RamMnemonic needs a cache when it is already in memory. The reason is that the RamMnemonic stores its data as byte[]. If you want to access it as an XML, the bytes need to be parsed to have an XML document. This parsing can be avoided by using a cache. The cache remembers the parsed XML document.

### Build a cached mnemonic

This is an example for a Cached Mnemonic which has a list of ignored files which will not be cached:

```csharp
var customXive =
	new MemorizedHive("demo",
		new CachedMnemonic(
            new RamMnemonic(),
            "a/*/ignored/*",
            "b/*/ignored/*"
            "c/*/ignored/*"
        )
	);
```

