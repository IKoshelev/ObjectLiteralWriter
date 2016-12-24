# ObjectLiteralWriter
Convert arbitrary .NET objects into strings containing their construction literal in C#

Ever had a case when you need to prepare data for a unit test, you have suitable object during runtime (i.e. you got if from DB) and you need to produce its literal for use in that test? ObjectLiteralWriter solves this problem.

It will handle primitive values, complex types, nested objects, collections,  etc...
```C#
var subj = new object[]
{
    new Test1()
    {
        Foo = 1.1m,
        Bar = null
    },
    null,
    1,
    2D,
    3M,
    true,
    new object(),
};

var literal = new ObjectLiteralWriter().GetLiteral(subj);

/*
@"new Object[]
{
new Test1()
{
Foo = 1.1M,
Bar = null,
},
null,
1,
2D,
3M,
true,
new object(),
}"*/
```

```C#
var subj = new Test1[]
{
    new Test1()
    {
        Foo = 1.1m,
        Bar = new Test1()
        {
            Foo = -3.3m
        }
    }
    ,
    null
};

var literal = new ObjectLiteralWriter().GetLiteral(subj);

/*
@"new Test1[]
{
new Test1()
{
Foo = 1.1M,
Bar = new Test1()
{
Foo = -3.3M,
Bar = null,
},
},
null,
}");
*/
```

Description of more advanced features coming soon. 
