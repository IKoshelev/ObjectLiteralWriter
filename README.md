# ObjectLiteralWriter
Convert arbitrary .NET objects at runtime into strings containing their construction literal in C#

Library is available on NuGet https://www.nuget.org/packages/Object.LiteralWriter/

Ever had a case when you need to prepare data for a unit test, you have suitable object during runtime (i.e. you got it from DB or another service) and you need to produce its literal for use in that test? ObjectLiteralWriter solves this problem.

It will handle primitive values and built-in types, complex classes, nested objects, collections, tuples, etc...

```csharp
using ObjectLiteralWriter;

var subj = new object[]
{
    new Test1()
    {
        Foo = 1.1m,
        Bar = null,
        Bac = (1, 2, 3),
    },
    new DateTime(1, 2, 3, 4, 0, 0, DateTimeKind.Utc),
    null,
    1,
    2D,
    3M,
    Guid.Parse("d237f51b-61ec-4a53-a6df-56aeaee6bb68"),
    true,
    new object(),
};

var literal = new ObjectLiteralWriter().GetLiteral(subj).IndentLiteral();

/*
@"new Object[]
{
    new Test1()
    {
        Foo = 1.1M,
        Bac = (1, 2, 3),
    },
    new DateTime(1, 2, 3, 4, 0, 0, DateTimeKind.Utc),
    null,
    1,
    2D,
    3M,
    Guid.Parse("d237f51b-61ec-4a53-a6df-56aeaee6bb68"),
    true,
    new object(),
}"*/
```

