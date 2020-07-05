using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Gridify.Tests {
    public class GridifyMapperShould {

        private GridifyMapper<TestClass> _sut;
        public GridifyMapperShould()
        {
            _sut = new GridifyMapper<TestClass>();
        }

        [Fact]
        public void GenerateMappings () {
            _sut.GenerateMappings ();

            Assert.Equal (typeof (TestClass).GetProperties ().Count (), _sut.Mappings.Count);
            Assert.True (_sut.Mappings.ContainsKey ("Id"));
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
            _sut.AddMap (nameof (TestClass.Name), p => p.Name);
            Assert.Single (_sut.Mappings);
        }

        [Fact]
        public void RemoveMap () {
            _sut.Mappings.Add ("name", q => q.Name);
            _sut.Mappings.Add ("Id", q => q.Id);
            _sut.RemoveMap (nameof (TestClass.Name));
            Assert.Single (_sut.Mappings);
        }

        [Fact]
        public void GetExpression () {
           
            var actual = _sut.GetExpression ("Name");
            Expression<Func<TestClass, object>> expected = Param_0 => (object)Param_0.Name;

            Assert.IsType (expected.GetType() , actual);
            Assert.Equal( expected.Body.ToString() ,actual.Body.ToString());

        }
    }
}