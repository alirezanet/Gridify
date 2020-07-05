using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit; 

namespace Gridify.Tests {
    public class GridifyExtentionsShould {
        
        private List<TestClass> _fakeRepository;
        public GridifyExtentionsShould()
        {
            _fakeRepository = new List<TestClass>(GetSampleData());
        }

#region "ApplyOrdering"
        [Fact]
        public void ApplyOrdering_SortBy_Ascending()
        {
            var gq = new GridifyQuery() {SortBy = "Name" , IsSortAsc = true };
            var actual = _fakeRepository.AsQueryable()
                                        .ApplyOrdering(gq)
                                        .ToList();
            var expected = _fakeRepository.OrderBy(q=>q.Name).ToList();
            Assert.Equal( expected,actual);

        }
        [Fact]
        public void ApplyOrdering_SortBy_Descending()
        {
            var gq = new GridifyQuery() {SortBy = "Name" , IsSortAsc = false };
            var actual = _fakeRepository.AsQueryable()
                                        .ApplyOrdering(gq)
                                        .ToList();
            var expected = _fakeRepository.OrderByDescending(q=>q.Name).ToList();
            Assert.Equal( expected,actual);
        }

        [Fact]
        public void ApplyOrdering_SortUsingChildClassProperty()
        {
            var gq = new GridifyQuery() {SortBy = "Child-Name" , IsSortAsc = false };
            var gm = new GridifyMapper<TestClass>()
                             .GenerateMappings()
                             .AddMap("Child-Name", q => q.ChildClass.Name);
                            

            var actual = _fakeRepository.AsQueryable().Where(q=>q.ChildClass != null)
                                        .ApplyOrdering(gq,gm)
                                        .ToList();

            var expected = _fakeRepository.Where(q=>q.ChildClass != null)
                                        .OrderByDescending(q=>q.ChildClass.Name).ToList();

            Assert.Equal( expected,actual);
        }


        [Fact]
        public void ApplyOrdering_EmptySortBy_ShouldSkip()
        {
            var gq = new GridifyQuery() { };
            var actual = _fakeRepository.AsQueryable()
                                        .ApplyOrdering(gq)
                                        .ToList();
            var expected = _fakeRepository.ToList();
            Assert.Equal( expected,actual);
        }
        [Fact]
        public void ApplyOrdering_NullGridifyQuery_ShouldSkip()
        {
            var actual = _fakeRepository.AsQueryable()
                                        .ApplyOrdering(null)
                                        .ToList();
            var expected = _fakeRepository.ToList();
            Assert.Equal( expected,actual);
        }
#endregion

#region "Data"
        private List<TestClass> GetSampleData(){
            var lst = new List<TestClass>();
            lst.Add(new TestClass(1,"John",null));
            lst.Add(new TestClass(2,"Bob",null));
            lst.Add(new TestClass(3,"Jack",(TestClass) lst[0].Clone()));
            lst.Add(new TestClass(4,"Rose",null));
            lst.Add(new TestClass(5,"Ali",null));
            lst.Add(new TestClass(6,"Hamid", (TestClass) lst[0].Clone()));
            lst.Add(new TestClass(7,"Hasan",(TestClass) lst[1].Clone()));
            lst.Add(new TestClass(8,"Farhad",(TestClass) lst[2].Clone()));
            lst.Add(new TestClass(9,"Sara", null));
            return lst;
        }

    }
#endregion
}