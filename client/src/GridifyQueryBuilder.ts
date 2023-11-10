import { ConditionalOperator, LogicalOperator } from "./GridifyOperator";
import IGridifyQuery from "./IGridifyQuery";

interface IExpression {
   value: string;
   type: "filter" | "op" | "startGroup" | "endGroup";
}

class GridifyQueryBuilder {
   private query: IGridifyQuery = {
      page: 1,
      pageSize: 20,
      orderBy: "",
      filter: "",
   };

   private filteringExpressions: IExpression[] = [];

   setPage(page: number): GridifyQueryBuilder {
      this.query.page = page;
      return this;
   }

   setPageSize(pageSize: number): GridifyQueryBuilder {
      this.query.pageSize = pageSize;
      return this;
   }

   addOrderBy(field: string, descending: boolean = false): GridifyQueryBuilder {
      const orderBy = `${field.trim()} ${descending ? "desc" : ""}`.trim();
      if (this.query.orderBy) {
         this.query.orderBy += `, ${orderBy}`;
      }
      this.query.orderBy = orderBy;
      return this;
   }

   addCondition(
      field: string,
      operator: ConditionalOperator,
      value: string | number | boolean,
      caseSensitive: boolean = true,
      escapeValue: boolean = true
   ): GridifyQueryBuilder {
      let filterValue = value;
      if (escapeValue && typeof value === "string") {
         filterValue = value.replace(/([(),|]|\/i)/g, "\\$1");
      }
      filterValue = caseSensitive ? filterValue.toString() : `${filterValue.toString()}/i`;

      var filterExpression = `${field.trim()}${operator}${filterValue}`;
      this.filteringExpressions.push({
         value: filterExpression,
         type: "filter",
      });
      return this;
   }

   startGroup(): GridifyQueryBuilder {
      this.filteringExpressions.push({ value: "(", type: "startGroup" });
      return this;
   }

   endGroup(): GridifyQueryBuilder {
      this.filteringExpressions.push({ value: ")", type: "endGroup" });
      return this;
   }

   and(): GridifyQueryBuilder {
      this.filteringExpressions.push({
         value: LogicalOperator.And,
         type: "op",
      });
      return this;
   }

   or(): GridifyQueryBuilder {
      this.filteringExpressions.push({ value: LogicalOperator.Or, type: "op" });
      return this;
   }

   build(): IGridifyQuery {
      this.filteringExpressions.forEach((exp) => {
         // not valid if:
         // 1- two `filter` in a row (filters should have an operator between them)
         // 2- total startGroup and endGroup doesn't match
         // 3- startGroup and endGroup without any filter inside
         // 4- group started but it is not ended

         // we need to support and validate nested groups
         // we should throw exception if something is not correct

         this.query.filter += exp.value;
      });

      return this.query;
   }
}

export default GridifyQueryBuilder;
