import { ConditionalOperator, LogicalOperator } from "./GridifyOperator";
import { GridifyQueryBuilderOptions } from "./GridifyQueryBuilderOptions";
import { IGridifyQuery } from "./IGridifyQuery";

export class GridifyQueryBuilder {
   protected query: IGridifyQuery = {
      page: 1,
      pageSize: 20,
      orderBy: "",
      filter: "",
   };

   protected filteringExpressions: IExpression[] = [];

   constructor(protected readonly options: GridifyQueryBuilderOptions = {}) {
      if (options.from) {
         const builder = options.from;

         this.query = { ...builder.query };
         this.filteringExpressions = builder.filteringExpressions.map((exp) => ({ ...exp }));
      }
   }

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
      this.query.orderBy = this.query.orderBy
         ? `${this.query.orderBy}, ${orderBy}`
         : orderBy;
      return this;
   }

   addCondition(
      field: string,
      operator: ConditionalOperator | string,
      value: string | number | boolean,
      caseSensitive: boolean = true,
      escapeValue: boolean = true
   ): GridifyQueryBuilder {
      let filterValue = value;
      if (escapeValue && typeof value === "string") {
         filterValue = value.replace(/([(),|]|\/i)/g, "\\$1");
      }

      if (!caseSensitive && value) {
         filterValue = `${filterValue.toString()}/i`;
      }

      if (typeof operator === "string" && !Object.values(ConditionalOperator).includes(operator as ConditionalOperator)) {
         if (!operator.startsWith('#')) {
            throw new Error(`Custom operators must start with the '#' character. Received: ${operator}`);
         }
      }

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

   and(optional?: boolean): GridifyQueryBuilder {
      this.filteringExpressions.push({
         value: LogicalOperator.And,
         type: "op",
         optional
      });
      return this;
   }

   or(optional?: boolean): GridifyQueryBuilder {
      this.filteringExpressions.push({
         value: LogicalOperator.Or,
         type: "op",
         optional
      });
      return this;
   }

   build(): IGridifyQuery {
      let previousType: "filter" | "op" | "startGroup" | "endGroup" | null =
         null;
      let groupCounter = 0;
      this.filteringExpressions.forEach((exp) => {
         if (exp.type === "startGroup") {
            groupCounter++;
         }
         if (exp.type === "endGroup") {
            groupCounter--;
         }

         //,
         if (previousType === null && exp.type === "op") {
            if (exp.optional) {
               return
            }
            throw new Error("expression cannot start with a logical operator");
         }

         // filter filter
         if (previousType === "filter" && exp.type === "filter") {
            throw new Error(
               "consecutive conditions are not allowed, consider adding a logical operator"
            );
         }

         // ,,
         if (previousType === "op" && exp.type === "op") {
            if (exp.optional) {
               return
            }
            throw new Error(
               "consecutive operators are not allowed, consider adding a filter"
            );
         }

         // (,
         if (previousType === "startGroup" && exp.type === "op") {
            if (exp.optional) {
               return
            }
            throw new Error(
               "logical operator immediately after startGroup is not allowed"
            );
         }

         // )filter
         if (previousType === "endGroup" && exp.type === "filter") {
            throw new Error("Missing logical operator after endGroup");
         }

         // ()
         if (!this.options.allowEmptyGroups) {
            if (previousType === "startGroup" && exp.type === "endGroup") {
               throw new Error("Empty groups are not allowed");
            }
         }

         // )(
         if (previousType === "endGroup" && exp.type === "startGroup") {
            throw new Error("Missing a logical operator between groups");
         }

         previousType = exp.type;
         this.query.filter += exp.value;
      });

      if (groupCounter != 0) {
         throw new Error("Group not properly closed");
      }

      // postprocess
      this.query.filter = this.query.filter?.replace(/[,|]?\(\)/gi, "");

      return this.query;
   }
}

interface IExpression {
   value: string;
   type: "filter" | "op" | "startGroup" | "endGroup";
   optional?: boolean
}
