using Elastic.Clients.Elasticsearch;
using Elastic.Transport.Extensions;
using Xunit;

namespace Gridify.Elasticsearch.Tests;

public class GridifyExtensionsTests
{
   private readonly ElasticsearchClient _client = new();

   [Theory]
   // byte equals
   [InlineData("MyByte=1", """{"term":{"MyByte":{"value":1}}}""")]
   // byte does not equal
   [InlineData("MyByte!=1", """{"bool":{"must_not":{"term":{"MyByte":{"value":1}}}}}""")]
   // byte greater than
   [InlineData("MyByte>1", """{"range":{"MyByte":{"gt":1}}}""")]
   // byte greater than or equal
   [InlineData("MyByte>=1", """{"range":{"MyByte":{"gte":1}}}""")]
   // byte less than
   [InlineData("MyByte<1", """{"range":{"MyByte":{"lt":1}}}""")]
   // byte less than or equal
   [InlineData("MyByte<=1", """{"range":{"MyByte":{"lte":1}}}""")]
   // byte is null
   [InlineData("MyByte=null", """{"bool":{"must_not":{"exists":{"field":"MyByte"}}}}""")]
   // byte is not null
   [InlineData("MyByte!=null", """{"exists":{"field":"MyByte"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithByteValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // sbyte equals
   [InlineData("MySByte=1", """{"term":{"MySByte":{"value":1}}}""")]
   // sbyte does not equal
   [InlineData("MySByte!=1", """{"bool":{"must_not":{"term":{"MySByte":{"value":1}}}}}""")]
   // sbyte greater than
   [InlineData("MySByte>1", """{"range":{"MySByte":{"gt":1}}}""")]
   // sbyte greater than or equal
   [InlineData("MySByte>=1", """{"range":{"MySByte":{"gte":1}}}""")]
   // sbyte less than
   [InlineData("MySByte<1", """{"range":{"MySByte":{"lt":1}}}""")]
   // sbyte less than or equal
   [InlineData("MySByte<=1", """{"range":{"MySByte":{"lte":1}}}""")]
   // sbyte is null
   [InlineData("MySByte=null", """{"bool":{"must_not":{"exists":{"field":"MySByte"}}}}""")]
   // sbyte is not null
   [InlineData("MySByte!=null", """{"exists":{"field":"MySByte"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithSByteValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // short equals
   [InlineData("MyShort=1", """{"term":{"MyShort":{"value":1}}}""")]
   // short does not equal
   [InlineData("MyShort!=1", """{"bool":{"must_not":{"term":{"MyShort":{"value":1}}}}}""")]
   // short greater than
   [InlineData("MyShort>1", """{"range":{"MyShort":{"gt":1}}}""")]
   // short greater than or equal
   [InlineData("MyShort>=1", """{"range":{"MyShort":{"gte":1}}}""")]
   // short less than
   [InlineData("MyShort<1", """{"range":{"MyShort":{"lt":1}}}""")]
   // short less than or equal
   [InlineData("MyShort<=1", """{"range":{"MyShort":{"lte":1}}}""")]
   // short is null
   [InlineData("MyShort=null", """{"bool":{"must_not":{"exists":{"field":"MyShort"}}}}""")]
   // short is not null
   [InlineData("MyShort!=null", """{"exists":{"field":"MyShort"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithShortValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // ushort equals
   [InlineData("MyUShort=1", """{"term":{"MyUShort":{"value":1}}}""")]
   // ushort does not equal
   [InlineData("MyUShort!=1", """{"bool":{"must_not":{"term":{"MyUShort":{"value":1}}}}}""")]
   // ushort greater than
   [InlineData("MyUShort>1", """{"range":{"MyUShort":{"gt":1}}}""")]
   // ushort greater than or equal
   [InlineData("MyUShort>=1", """{"range":{"MyUShort":{"gte":1}}}""")]
   // ushort less than
   [InlineData("MyUShort<1", """{"range":{"MyUShort":{"lt":1}}}""")]
   // ushort less than or equal
   [InlineData("MyUShort<=1", """{"range":{"MyUShort":{"lte":1}}}""")]
   // ushort is null
   [InlineData("MyUShort=null", """{"bool":{"must_not":{"exists":{"field":"MyUShort"}}}}""")]
   // ushort is not null
   [InlineData("MyUShort!=null", """{"exists":{"field":"MyUShort"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithUShortValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // int equals
   [InlineData("Id=1", """{"term":{"Id":{"value":1}}}""")]
   // int does not equal
   [InlineData("Id!=1", """{"bool":{"must_not":{"term":{"Id":{"value":1}}}}}""")]
   // int greater than
   [InlineData("Id>1", """{"range":{"Id":{"gt":1}}}""")]
   // int greater than or equal
   [InlineData("Id>=1", """{"range":{"Id":{"gte":1}}}""")]
   // int less than
   [InlineData("Id<1", """{"range":{"Id":{"lt":1}}}""")]
   // int less than or equal
   [InlineData("Id<=1", """{"range":{"Id":{"lte":1}}}""")]
   // int is null
   [InlineData("Id=null", """{"bool":{"must_not":{"exists":{"field":"Id"}}}}""")]
   // int is not null
   [InlineData("Id!=null", """{"exists":{"field":"Id"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithNumber_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // uint equals
   [InlineData("MyUInt=1", """{"term":{"MyUInt":{"value":1}}}""")]
   // uint does not equal
   [InlineData("MyUInt!=1", """{"bool":{"must_not":{"term":{"MyUInt":{"value":1}}}}}""")]
   // uint greater than
   [InlineData("MyUInt>1", """{"range":{"MyUInt":{"gt":1}}}""")]
   // uint greater than or equal
   [InlineData("MyUInt>=1", """{"range":{"MyUInt":{"gte":1}}}""")]
   // uint less than
   [InlineData("MyUInt<1", """{"range":{"MyUInt":{"lt":1}}}""")]
   // uint less than or equal
   [InlineData("MyUInt<=1", """{"range":{"MyUInt":{"lte":1}}}""")]
   // uint is null
   [InlineData("MyUInt=null", """{"bool":{"must_not":{"exists":{"field":"MyUInt"}}}}""")]
   // uint is not null
   [InlineData("MyUInt!=null", """{"exists":{"field":"MyUInt"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithUIntValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // long equals
   [InlineData("MyLong=1", """{"term":{"MyLong":{"value":1}}}""")]
   // long does not equal
   [InlineData("MyLong!=1", """{"bool":{"must_not":{"term":{"MyLong":{"value":1}}}}}""")]
   // long greater than
   [InlineData("MyLong>1", """{"range":{"MyLong":{"gt":1}}}""")]
   // long greater than or equal
   [InlineData("MyLong>=1", """{"range":{"MyLong":{"gte":1}}}""")]
   // long less than
   [InlineData("MyLong<1", """{"range":{"MyLong":{"lt":1}}}""")]
   // long less than or equal
   [InlineData("MyLong<=1", """{"range":{"MyLong":{"lte":1}}}""")]
   // long is null
   [InlineData("MyLong=null", """{"bool":{"must_not":{"exists":{"field":"MyLong"}}}}""")]
   // long is not null
   [InlineData("MyLong!=null", """{"exists":{"field":"MyLong"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithLongValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // ulong equals
   [InlineData("MyULong=1", """{"term":{"MyULong":{"value":1}}}""")]
   // ulong does not equal
   [InlineData("MyULong!=1", """{"bool":{"must_not":{"term":{"MyULong":{"value":1}}}}}""")]
   // ulong greater than
   [InlineData("MyULong>1", """{"range":{"MyULong":{"gt":1}}}""")]
   // ulong greater than or equal
   [InlineData("MyULong>=1", """{"range":{"MyULong":{"gte":1}}}""")]
   // ulong less than
   [InlineData("MyULong<1", """{"range":{"MyULong":{"lt":1}}}""")]
   // ulong less than or equal
   [InlineData("MyULong<=1", """{"range":{"MyULong":{"lte":1}}}""")]
   // ulong is null
   [InlineData("MyULong=null", """{"bool":{"must_not":{"exists":{"field":"MyULong"}}}}""")]
   // ulong is not null
   [InlineData("MyULong!=null", """{"exists":{"field":"MyULong"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithULongValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // float equals
   [InlineData("MyFloat=56.7", """{"term":{"MyFloat":{"value":56.7}}}""")]
   // float does not equal
   [InlineData("MyFloat!=56.7", """{"bool":{"must_not":{"term":{"MyFloat":{"value":56.7}}}}}""")]
   // float greater than
   [InlineData("MyFloat>56.7", """{"range":{"MyFloat":{"gt":56.7}}}""")]
   // float greater than or equal
   [InlineData("MyFloat>=56.7", """{"range":{"MyFloat":{"gte":56.7}}}""")]
   // float less than
   [InlineData("MyFloat<56.7", """{"range":{"MyFloat":{"lt":56.7}}}""")]
   // float less than or equal
   [InlineData("MyFloat<=56.7", """{"range":{"MyFloat":{"lte":56.7}}}""")]
   // float is null
   [InlineData("MyFloat=null", """{"bool":{"must_not":{"exists":{"field":"MyFloat"}}}}""")]
   // float is not null
   [InlineData("MyFloat!=null", """{"exists":{"field":"MyFloat"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithFloatValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // double equals
   [InlineData("MyDouble=56.7", """{"term":{"MyDouble":{"value":56.7}}}""")]
   // double does not equal
   [InlineData("MyDouble!=56.7", """{"bool":{"must_not":{"term":{"MyDouble":{"value":56.7}}}}}""")]
   // double greater than
   [InlineData("MyDouble>56.7", """{"range":{"MyDouble":{"gt":56.7}}}""")]
   // double greater than or equal
   [InlineData("MyDouble>=56.7", """{"range":{"MyDouble":{"gte":56.7}}}""")]
   // double less than
   [InlineData("MyDouble<56.7", """{"range":{"MyDouble":{"lt":56.7}}}""")]
   // double less than or equal
   [InlineData("MyDouble<=56.7", """{"range":{"MyDouble":{"lte":56.7}}}""")]
   // double is null
   [InlineData("MyDouble=null", """{"bool":{"must_not":{"exists":{"field":"MyDouble"}}}}""")]
   // double is not null
   [InlineData("MyDouble!=null", """{"exists":{"field":"MyDouble"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithDoubleValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // decimal equals
   [InlineData("MyDecimal=56.7", """{"term":{"MyDecimal":{"value":"56.7"}}}""")]
   // decimal does not equal
   [InlineData("MyDecimal!=56.7", """{"bool":{"must_not":{"term":{"MyDecimal":{"value":"56.7"}}}}}""")]
   // decimal greater than
   [InlineData("MyDecimal>56.7", """{"range":{"MyDecimal":{"gt":"56.7"}}}""")]
   // decimal greater than or equal
   [InlineData("MyDecimal>=56.7", """{"range":{"MyDecimal":{"gte":"56.7"}}}""")]
   // decimal less than
   [InlineData("MyDecimal<56.7", """{"range":{"MyDecimal":{"lt":"56.7"}}}""")]
   // decimal less than or equal
   [InlineData("MyDecimal<=56.7", """{"range":{"MyDecimal":{"lte":"56.7"}}}""")]
   // decimal is null
   [InlineData("MyDecimal=null", """{"bool":{"must_not":{"exists":{"field":"MyDecimal"}}}}""")]
   // decimal is not null
   [InlineData("MyDecimal!=null", """{"exists":{"field":"MyDecimal"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithDecimalValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // string equals
   [InlineData("Name=Dzmitry", """{"term":{"Name.keyword":{"value":"Dzmitry"}}}""")]
   // string does not equal
   [InlineData("Name!=Dzmitry", """{"bool":{"must_not":{"term":{"Name.keyword":{"value":"Dzmitry"}}}}}""")]
   // string contains
   [InlineData("Name=*itr", """{"wildcard":{"Name.keyword":{"value":"*itr*"}}}""")]
   // string does not contain
   [InlineData("Name!*itr", """{"bool":{"must_not":{"wildcard":{"Name.keyword":{"value":"*itr*"}}}}}""")]
   // string starts with
   [InlineData("Name^Dzm", """{"wildcard":{"Name.keyword":{"value":"Dzm*"}}}""")]
   // string does not start with
   [InlineData("Name!^Dzm", """{"bool":{"must_not":{"wildcard":{"Name.keyword":{"value":"Dzm*"}}}}}""")]
   // string ends with
   [InlineData("Name$try", """{"wildcard":{"Name.keyword":{"value":"*try"}}}""")]
   // string does not end with
   [InlineData("Name!$try", """{"bool":{"must_not":{"wildcard":{"Name.keyword":{"value":"*try"}}}}}""")]
   // string is null
   [InlineData("Name=null", """{"bool":{"must_not":{"exists":{"field":"Name"}}}}""")]
   // string is not null
   [InlineData("Name!=null", """{"exists":{"field":"Name"}}""")]
   // string is empty
   [InlineData("Name=", """{"term":{"Name.keyword":{"value":""}}}""")]
   // string is not empty
   [InlineData("Name!=", """{"bool":{"must_not":{"term":{"Name.keyword":{"value":""}}}}}""")]
   // string is empty or null
   [InlineData("Name=null|Name=", """{"bool":{"should":[{"bool":{"must_not":{"exists":{"field":"Name"}}}},{"term":{"Name.keyword":{"value":""}}}]}}""")]
   // string is not empty and not null
   [InlineData("Name!=null,Name!=", """{"bool":{"must":{"exists":{"field":"Name"}},"must_not":{"term":{"Name.keyword":{"value":""}}}}}""")]
   public void ToElasticsearchQuery_WhenCalledWithStringValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // date equals
   [InlineData("MyDateTime=2021-09-01", """{"term":{"MyDateTime":{"value":"2021-09-01T00:00:00"}}}""")]
   // date and time equals
   [InlineData("MyDateTime=2021-09-01T00:00:00", """{"term":{"MyDateTime":{"value":"2021-09-01T00:00:00"}}}""")]
   [InlineData("MyDateTime=2021-09-01 00:00:00", """{"term":{"MyDateTime":{"value":"2021-09-01T00:00:00"}}}""")]
   [InlineData("MyDateTime=2021-09-01 23:15:11", """{"term":{"MyDateTime":{"value":"2021-09-01T23:15:11"}}}""")]
   // date is null
   [InlineData("MyDateTime=null", """{"bool":{"must_not":{"exists":{"field":"MyDateTime"}}}}""")]
   // date is not null
   [InlineData("MyDateTime!=null", """{"exists":{"field":"MyDateTime"}}""")]
   // date greater than
   [InlineData("MyDateTime>2021-09-01", """{"range":{"MyDateTime":{"gt":"2021-09-01T00:00:00"}}}""")]
   // date greater than or equal
   [InlineData("MyDateTime>=2021-09-01", """{"range":{"MyDateTime":{"gte":"2021-09-01T00:00:00"}}}""")]
   // date less than
   [InlineData("MyDateTime<2021-09-01", """{"range":{"MyDateTime":{"lt":"2021-09-01T00:00:00"}}}""")]
   // date less than or equal
   [InlineData("MyDateTime<=2021-09-01", """{"range":{"MyDateTime":{"lte":"2021-09-01T00:00:00"}}}""")]
   public void ToElasticsearchQuery_WhenCalledWithDateTimeValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // date only equals
   [InlineData("MyDateOnly=2021-09-01", """{"term":{"MyDateOnly":{"value":"2021-09-01"}}}""")]
   // date only is null
   [InlineData("MyDateOnly=null", """{"bool":{"must_not":{"exists":{"field":"MyDateOnly"}}}}""")]
   // date only is not null
   [InlineData("MyDateOnly!=null", """{"exists":{"field":"MyDateOnly"}}""")]
   // date only greater than
   [InlineData("MyDateOnly>2021-09-01", """{"range":{"MyDateOnly":{"gt":"2021-09-01"}}}""")]
   // date only greater than or equal
   [InlineData("MyDateOnly>=2021-09-01", """{"range":{"MyDateOnly":{"gte":"2021-09-01"}}}""")]
   // date only less than
   [InlineData("MyDateOnly<2021-09-01", """{"range":{"MyDateOnly":{"lt":"2021-09-01"}}}""")]
   // date only less than or equal
   [InlineData("MyDateOnly<=2021-09-01", """{"range":{"MyDateOnly":{"lte":"2021-09-01"}}}""")]
   public void ToElasticsearchQuery_WhenCalledWithDateOnlyValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // bool equals
   [InlineData("IsActive=true", """{"term":{"IsActive":{"value":"true"}}}""")]
   // bool does not equal
   [InlineData("IsActive!=true", """{"bool":{"must_not":{"term":{"IsActive":{"value":"true"}}}}}""")]
   // bool is null
   [InlineData("IsActive=null", """{"bool":{"must_not":{"exists":{"field":"IsActive"}}}}""")]
   // bool is not null
   [InlineData("IsActive!=null", """{"exists":{"field":"IsActive"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithBoolValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // guid equals
   [InlineData("MyGuid=69C3BB3A-3A85-4750-BA03-1F916FA5C0B1", """{"term":{"MyGuid.keyword":{"value":"69C3BB3A-3A85-4750-BA03-1F916FA5C0B1"}}}""")]
   // guid is null
   [InlineData("MyGuid=null", """{"bool":{"must_not":{"exists":{"field":"MyGuid"}}}}""")]
   // guid is not null
   [InlineData("MyGuid!=null", """{"exists":{"field":"MyGuid"}}""")]
   public void ToElasticsearchQuery_WhenCalledWithGuidValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // , operator
   [InlineData("Id=1,Name=Dzmitry", """{"bool":{"must":[{"term":{"Id":{"value":1}}},{"term":{"Name.keyword":{"value":"Dzmitry"}}}]}}""")]
   // | operator
   [InlineData("Id=1|Id=2", """{"bool":{"should":[{"term":{"Id":{"value":1}}},{"term":{"Id":{"value":2}}}]}}""")]
   // | , operators
   [InlineData("Id=1|Id=2,Name=Dzmitry", """{"bool":{"must":[{"bool":{"should":[{"term":{"Id":{"value":1}}},{"term":{"Id":{"value":2}}}]}},{"term":{"Name.keyword":{"value":"Dzmitry"}}}]}}""")]
   // , | operators
   [InlineData("Id=1,Name=Dzmitry|Name=John", """{"bool":{"should":[{"bool":{"must":[{"term":{"Id":{"value":1}}},{"term":{"Name.keyword":{"value":"Dzmitry"}}}]}},{"term":{"Name.keyword":{"value":"John"}}}]}}""")]
   // , , , operators
   [InlineData("Id=1,Name=Dzmitry,MyDateTime=2021-09-01", """{"bool":{"must":[{"term":{"Id":{"value":1}}},{"term":{"Name.keyword":{"value":"Dzmitry"}}},{"term":{"MyDateTime":{"value":"2021-09-01T00:00:00"}}}]}}""")]
   // | | | operators
   [InlineData("Id=1|Id=2|Id=3", """{"bool":{"should":[{"term":{"Id":{"value":1}}},{"term":{"Id":{"value":2}}},{"term":{"Id":{"value":3}}}]}}""")]
   // ( | ) operators
   [InlineData("(Id=1|Id=2)", """{"bool":{"should":[{"term":{"Id":{"value":1}}},{"term":{"Id":{"value":2}}}]}}""")]
   // ( , ) operators
   [InlineData("(Id=1,Id=2)", """{"bool":{"must":[{"term":{"Id":{"value":1}}},{"term":{"Id":{"value":2}}}]}}""")]
   // ( | ) , operators
   [InlineData("(Id=1|Id=2),Name=Dzmitry", """{"bool":{"must":[{"bool":{"should":[{"term":{"Id":{"value":1}}},{"term":{"Id":{"value":2}}}]}},{"term":{"Name.keyword":{"value":"Dzmitry"}}}]}}""")]
   // ( | | ) , operator
   [InlineData("(Id=1|Id=2|Id=3),Name=Dzmitry", """{"bool":{"must":[{"bool":{"should":[{"term":{"Id":{"value":1}}},{"term":{"Id":{"value":2}}},{"term":{"Id":{"value":3}}}]}},{"term":{"Name.keyword":{"value":"Dzmitry"}}}]}}""")]
   // , ( | ) operators
   [InlineData("Id=1,(Name=Dzmitry|Name=John)", """{"bool":{"must":[{"term":{"Id":{"value":1}}},{"bool":{"should":[{"term":{"Name.keyword":{"value":"Dzmitry"}}},{"term":{"Name.keyword":{"value":"John"}}}]}}]}}""")]
   // ( | ) , ( | ) operators
   [InlineData("(Name=Dzmitry|Name=John),(Id=1|Id=2)", """{"bool":{"must":[{"bool":{"should":[{"term":{"Name.keyword":{"value":"Dzmitry"}}},{"term":{"Name.keyword":{"value":"John"}}}]}},{"bool":{"should":[{"term":{"Id":{"value":1}}},{"term":{"Id":{"value":2}}}]}}]}}""")]
   // , ( , | ) | ) operators
   [InlineData("Id=1,(Name=Dzmitry,(Id=1|Id=2)|Id=3)", """{"bool":{"must":[{"term":{"Id":{"value":1}}},{"bool":{"should":[{"bool":{"must":[{"term":{"Name.keyword":{"value":"Dzmitry"}}},{"bool":{"should":[{"term":{"Id":{"value":1}}},{"term":{"Id":{"value":2}}}]}}]}},{"term":{"Id":{"value":3}}}]}}]}}""")]
   public void ToElasticsearchQuery_WhenCalledWithDifferentOperators_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // Nested class
   [InlineData("Name=Dzmitry,ChildClass.Name=Kiryl", """{"bool":{"must":[{"term":{"Name.keyword":{"value":"Dzmitry"}}},{"term":{"ChildClass.Name.keyword":{"value":"Kiryl"}}}]}}""")]
   // Double nested class
   [InlineData("Name=Sergey,ChildClass.Name=Dzmitry,ChildClass.ChildClass.Name=Kiryl", """{"bool":{"must":[{"term":{"Name.keyword":{"value":"Sergey"}}},{"term":{"ChildClass.Name.keyword":{"value":"Dzmitry"}}},{"term":{"ChildClass.ChildClass.Name.keyword":{"value":"Kiryl"}}}]}}""")]
   public void ToElasticsearchQuery_WhenCalledWithNestedClass_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   // empty query
   [InlineData("", """{"match_all":{}}""")]
   // string starts with empty
   [InlineData("Name^", """{"wildcard":{"Name.keyword":{"value":"*"}}}""")]
   // string ends with empty
   [InlineData("Name$", """{"wildcard":{"Name.keyword":{"value":"*"}}}""")]
   // string does not start with empty
   [InlineData("Name!^", """{"bool":{"must_not":{"wildcard":{"Name.keyword":{"value":"*"}}}}}""")]
   // string does not end with empty
   [InlineData("Name!$", """{"bool":{"must_not":{"wildcard":{"Name.keyword":{"value":"*"}}}}}""")]
   public void ToElasticsearchQuery_WhenCalledWithEmptyValue_ReturnsElasticsearchQuery(string query, string expected)
   {
      AssertQuery(query, expected);
   }

   [Theory]
   [InlineData("name=Dzmitry,childname=Kiryl", """{"bool":{"must":[{"term":{"Name.keyword":{"value":"Dzmitry"}}},{"term":{"ChildClass.Name.keyword":{"value":"Kiryl"}}}]}}""")]
   public void ToElasticsearchQuery_WhenCalledWithCustomMapper_ShouldUseCorrectFieldNames(string query, string expected)
   {
      var mapper = new GridifyMapper<TestClass>()
         .GenerateMappings()
         .AddMap("name", x => x.Name)
         .AddMap("childname", x => x.ChildClass.Name);

      AssertQuery(query, expected, mapper);
   }

   [Theory]
   [InlineData("Id asc", """[{"Id":{"order":"asc"}}]""")]
   [InlineData("Id desc", """[{"Id":{"order":"desc"}}]""")]
   [InlineData("Id asc, Name desc", """[{"Id":{"order":"asc"}},{"Name.keyword":{"order":"desc"}}]""")]
   [InlineData("Id asc, Name desc, MyDateTime", """[{"Id":{"order":"asc"}},{"Name.keyword":{"order":"desc"}},{"MyDateTime":{"order":"asc"}}]""")]
   [InlineData("ChildClass.Name desc", """[{"ChildClass.Name.keyword":{"order":"desc"}}]""")]
   [InlineData("", "[]")]
   public void ToSortOptions_WhenCalledWithOrdering_ReturnsElasticsearchSortOptions(string ordering, string expected)
   {
      AssertOrdering(ordering, expected);
   }

   [Theory]
   [InlineData("name asc,childname desc", """[{"Name.keyword":{"order":"asc"}},{"ChildClass.Name.keyword":{"order":"desc"}}]""")]
   public void ToSortOptions_WhenCalledWithCustomMapper_ShouldUseCorrectFieldNames(string ordering, string expected)
   {
      var mapper = new GridifyMapper<TestClass>()
         .GenerateMappings()
         .AddMap("name", x => x.Name)
         .AddMap("childname", x => x.ChildClass.Name);

      AssertOrdering(ordering, expected, mapper);
   }

   private void AssertQuery(string query, string expected, IGridifyMapper<TestClass>? mapper = null)
   {
      var result = query.ToElasticsearchQuery(mapper);

      var jsonQuery = _client.RequestResponseSerializer.SerializeToString(result);
      Assert.Equal(expected, jsonQuery);
   }

   private void AssertOrdering(string ordering, string expected, IGridifyMapper<TestClass>? mapper = null)
   {
      var result = ordering.ToElasticsearchSortOptions(mapper);

      var jsonQuery = _client.RequestResponseSerializer.SerializeToString(result);
      Assert.Equal(expected, jsonQuery);
   }
}
