using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Gridify.Tests {
    public class GridifyMapperShould {

        class TestClass {
            public int Id { get; set; }
            public string Name { get; set; }
            public TestClass ClassProp { get; set; }
        }

        [Fact]
        public void GenerateMappings () {
            var sut = new GridifyMapper<TestClass> ();
            sut.GenerateMappings ();
            Assert.Equal (typeof (TestClass).GetProperties ().Count (), sut.Mappings.Count);
            Assert.True (sut.Mappings.ContainsKey ("Id"));
        }

        [Fact]
        public void CaseSensitivity () {
            var sut = new GridifyMapper<TestClass> (true);
            sut.Mappings.Add ("id", q => q.Id);

            Assert.True (sut.CaseSensitive);
            Assert.True (sut.Mappings.ContainsKey ("id"));
            Assert.False (sut.Mappings.ContainsKey ("ID"));
        }

        [Fact]
        public void AddMap () {
            var sut = new GridifyMapper<TestClass> ();
            sut.AddMap (nameof (TestClass.Name), p => p.Name);
            Assert.Single (sut.Mappings);
        }

        [Fact]
        public void RemoveMap () {
            var sut = new GridifyMapper<TestClass> ();
            sut.Mappings.Add ("name", q => q.Name);
            sut.Mappings.Add ("Id", q => q.Id);
            sut.RemoveMap (nameof (TestClass.Name));
            Assert.Single (sut.Mappings);
        }

        [Fact]
        public void GetExpression () {
            var sut = new GridifyMapper<TestClass> ();
            
            Expression<Func<TestClass, object>> expected = Param_0 => (object)Param_0.Name;
            var actual = sut.GetExpression ("Name");

            Assert.IsType (expected.GetType() , actual);
            Assert.Equal( expected.Body.ToString() ,actual.Body.ToString());

        }
    }
}