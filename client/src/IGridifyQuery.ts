export interface IGridifyQuery
   extends IGridifyPagination,
      IGridifyFiltering,
      IGridifyOrdering {}

export interface IGridifyFiltering {
   filter?: string;
}

export interface IGridifyOrdering {
   orderBy?: string;
}

export interface IGridifyPagination {
   page: number;
   pageSize: number;
}
