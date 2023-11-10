import GridifyQueryBuilder from "../src/GridifyQueryBuilder";
import { ConditionalOperator as op } from "../src/GridifyOperator";
describe("GridifyQueryBuilder", () => {
   it("should build a simple query", () => {
      const query = new GridifyQueryBuilder()
         .setPage(1)
         .setPageSize(20)
         .addOrderBy("name")
         .addCondition("age", op.GreaterThan, 30)
         .build();

      expect(query).toEqual({
         page: 1,
         pageSize: 20,
         orderBy: "name",
         filter: "age>30",
      });
   });

   it("should build a complex query with logical operators and groups", () => {
      const query = new GridifyQueryBuilder()
         .setPage(2)
         .setPageSize(10)
         .addOrderBy("name", true)
         .startGroup()
         .addCondition("age", op.LessThan, 50)
         .or()
         .addCondition("name", op.StartsWith, "A")
         .endGroup()
         .and()
         .addCondition("isActive", op.Equal, true)
         .build();

      expect(query).toEqual({
         page: 2,
         pageSize: 10,
         orderBy: "name desc",
         filter: "(age<50|name^A),isActive=true",
      });
   });

   it("should escape value by default", () => {
      const query = new GridifyQueryBuilder()
         .addCondition("value", op.Equal, "abcd,abcd|abcd(abcd)abcd/iabcd")
         .build();

      expect(query).toEqual(
         expect.objectContaining({
            filter: String.raw`value=abcd\,abcd\|abcd\(abcd\)abcd\/iabcd`,
         })
      );
   });
});
