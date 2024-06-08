import { GridifyQueryBuilder } from "../src/GridifyQueryBuilder";
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

describe("GridifyQueryBuilder Validation", () => {
   it("should allow balanced parentheses", () => {
      const query = new GridifyQueryBuilder()
         .startGroup()
         .addCondition("field", op.Equal, "value")
         .endGroup()
         .build();

      expect(query.filter).toEqual("(field=value)");
   });

   it("should allow nested balanced parentheses", () => {
      const query = new GridifyQueryBuilder()
         .startGroup()
            .addCondition("field", op.Equal, "value")
            .and()
            .startGroup()
               .addCondition("field", op.Equal, "value")
               .or()
               .addCondition("field", op.Equal, "value")
            .endGroup()
         .endGroup()
         .build();

      expect(query.filter).toEqual("(field=value,(field=value|field=value))");
   });

   it("should throw an error for unbalanced parentheses", () => {
      expect(() => {
         new GridifyQueryBuilder()
            .startGroup()
            .addCondition("field", op.Equal, "value")
            .build();
      }).toThrow("Group not properly closed");
   });

   it("should allow and must have logical operators between conditions", () => {
      const query = new GridifyQueryBuilder()
         .addCondition("field1", op.Equal, "value1")
         .and()
         .addCondition("field2", op.Equal, "value2")
         .build();

      expect(query.filter).toEqual("field1=value1,field2=value2");
   });

   it("should throw an error when logical operators are misplaced", () => {
      expect(() => {
         new GridifyQueryBuilder()
            .and()
            .addCondition("field", op.Equal, "value")
            .build();
      }).toThrow("expression cannot start with a logical operator");
   });

   it("should allow logical operators after a group", () => {
      const query = new GridifyQueryBuilder()
         .startGroup()
         .addCondition("field", op.Equal, "value")
         .endGroup()
         .and()
         .addCondition("anotherField", op.Equal, "anotherValue")
         .build();

      expect(query.filter).toEqual("(field=value),anotherField=anotherValue");
   });

   it("should throw an error for logical operators immediately after starting a group", () => {
      expect(() => {
         new GridifyQueryBuilder()
            .startGroup()
            .or()
            .addCondition("field", op.Equal, "value")
            .endGroup()
            .build();
      }).toThrow("logical operator immediately after startGroup is not allowed");
   });

   it("should throw an error for consecutive logical operators", () => {
      expect(() => {
         new GridifyQueryBuilder()
            .addCondition("field1", op.Equal, "value1")
            .and()
            .or()
            .addCondition("field2", op.Equal, "value2")
            .build();
      }).toThrow("consecutive operators are not allowed, consider adding a filter");
   });

   it("should throw an error for consecutive conditions", () => {
      expect(() => {
         new GridifyQueryBuilder()
            .addCondition("field1", op.Equal, "value1")
            .addCondition("field2", op.Equal, "value2")
            .build();
      }).toThrow();
   });


   it("should not include case-insensitive operator if there is no value", () => {
      const query = new GridifyQueryBuilder()
         .addCondition("name", op.Equal, "", false)
         .build();

      expect(query.filter).toEqual("name=");
   });

});
